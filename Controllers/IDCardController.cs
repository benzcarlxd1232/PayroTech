using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Services;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace PayroTech.Controllers;

[Authorize]
[SupportedOSPlatform("windows")]
public class IDCardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public IDCardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    // Generate ID Card after face enrollment — all roles get full ID card
    [HttpGet]
    public async Task<IActionResult> Generate()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Load full user data with relations
        var fullUser = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Branch)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (fullUser == null) return NotFound();

        // Also load Employee record if it exists (has more complete data)
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        // Merge Employee data into ApplicationUser where ApplicationUser fields are empty
        if (employee != null)
        {
            if (string.IsNullOrEmpty(fullUser.ContactNumber) && !string.IsNullOrEmpty(employee.ContactNumber))
                fullUser.ContactNumber = employee.ContactNumber;
            if (string.IsNullOrEmpty(fullUser.Address) && !string.IsNullOrEmpty(employee.Address))
                fullUser.Address = employee.Address;
            if (fullUser.StartDate == null && employee.HireDate != default)
                fullUser.StartDate = employee.HireDate;
            if (fullUser.Department == null && employee.Department != null)
                fullUser.Department = employee.Department;
        }

        var qrCodeDataUrl = GenerateQRCodeDataUrl(fullUser.QRCodeHash ?? fullUser.Id);
        ViewBag.QRCodeDataUrl = qrCodeDataUrl;
        ViewBag.IsFirstTime = true;

        return View("~/Views/IDCard/StaffIDCard.cshtml", fullUser);
    }

    private string GetDashboardController(UserRole role)
    {
        return role switch
        {
            UserRole.HR => "HR",
            UserRole.Accountant => "Accountant",
            UserRole.Employee => "MyPortal",
            UserRole.CompanyAdmin => "Manager",
            _ => "Home"
        };
    }

    // Manager QR Code page
    [HttpGet]
    public async Task<IActionResult> ManagerQRCode()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user.Role != UserRole.CompanyAdmin) 
            return RedirectToAction("Index", "Home");

        var qrCodeDataUrl = GenerateQRCodeDataUrl(user.QRCodeHash ?? "");
        ViewBag.QRCodeDataUrl = qrCodeDataUrl;

        return View(user);
    }

    // Mark ID as printed and redirect to dashboard
    [HttpPost]
    public async Task<IActionResult> MarkAsPrinted()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        // Create or update ID request record
        var request = new IDRequest
        {
            UserId = user.Id,
            Reason = "Initial ID Card Print",
            Status = IDRequestStatus.Printed,
            IsPrinted = true,
            PrintedAt = DateTime.UtcNow,
            CompanyId = user.CompanyId ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.IDRequests.Add(request);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "ID Card Printed", "User", user.Id, null,
            new { PrintedAt = DateTime.UtcNow }, user.CompanyId);

        return Json(new { success = true });
    }

    // Request new ID form
    [HttpGet]
    public new async Task<IActionResult> Request()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user.Role == UserRole.CompanyAdmin)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // Submit ID request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public new async Task<IActionResult> Request(string reason)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user.Role == UserRole.CompanyAdmin)
            return RedirectToAction("Index", "Home");

        if (string.IsNullOrWhiteSpace(reason) || reason.Length < 10)
        {
            TempData["Error"] = "Please provide a detailed reason (at least 10 characters).";
            return View();
        }

        var request = new IDRequest
        {
            UserId = user.Id,
            Reason = reason,
            Status = Models.Enums.IDRequestStatus.Pending,
            CompanyId = user.CompanyId ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.IDRequests.Add(request);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "ID Request Submitted", "IDRequest", request.Id.ToString(), null,
            new { Reason = reason }, user.CompanyId);

        TempData["Success"] = "Your ID request has been submitted and is pending manager approval.";
        return RedirectToAction("MyRequests");
    }

    // View my ID requests
    [HttpGet]
    public async Task<IActionResult> MyRequests(string tab = "active")
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        IQueryable<IDRequest> query = _context.IDRequests
            .Include(r => r.ApprovedBy)
            .Where(r => r.UserId == user.Id);

        if (tab == "active")
        {
            query = query.Where(r => r.Status == IDRequestStatus.Pending);
        }
        else // archive
        {
            query = query.Where(r => r.Status != IDRequestStatus.Pending);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        ViewBag.ActiveTab = tab;
        return View(requests);
    }

    // Print approved ID
    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var request = await _context.IDRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (request == null || request.Status != IDRequestStatus.Approved)
        {
            TempData["Error"] = "ID request not found or not approved.";
            return RedirectToAction("MyRequests");
        }

        // Check if already printed
        if (request.IsPrinted)
        {
            TempData["Error"] = "This ID card has already been printed and cannot be accessed again.";
            return RedirectToAction("MyRequests");
        }

        // Load full user data
        var fullUser = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Branch)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (fullUser == null) return NotFound();

        // Merge Employee data
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);
        if (employee != null)
        {
            if (string.IsNullOrEmpty(fullUser.ContactNumber) && !string.IsNullOrEmpty(employee.ContactNumber))
                fullUser.ContactNumber = employee.ContactNumber;
            if (string.IsNullOrEmpty(fullUser.Address) && !string.IsNullOrEmpty(employee.Address))
                fullUser.Address = employee.Address;
            if (fullUser.StartDate == null && employee.HireDate != default)
                fullUser.StartDate = employee.HireDate;
            if (fullUser.Department == null && employee.Department != null)
                fullUser.Department = employee.Department;
        }

        // Generate QR code
        var qrCodeDataUrl = GenerateQRCodeDataUrl(fullUser.QRCodeHash ?? "");
        ViewBag.QRCodeDataUrl = qrCodeDataUrl;
        ViewBag.IsFirstTime = false;
        ViewBag.RequestId = id;

        return View("~/Views/IDCard/StaffIDCard.cshtml", fullUser);
    }

    // Get pending ID request count
    [HttpGet]
    public async Task<JsonResult> GetPendingCount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { count = 0 });

        int count = 0;
        
        if (user.Role == UserRole.CompanyAdmin)
        {
            // Manager sees all pending requests for company
            count = await _context.IDRequests
                .Where(r => r.CompanyId == user.CompanyId && 
                           r.Status == IDRequestStatus.Pending)
                .CountAsync();
        }
        else
        {
            // Others see their own pending requests
            count = await _context.IDRequests
                .Where(r => r.UserId == user.Id && 
                           r.Status == IDRequestStatus.Pending)
                .CountAsync();
        }
        
        return Json(new { count });
    }

    // GET: /IDCard/PrintReplacement/{requestId} — staff downloads replacement ID after notification
    [HttpGet]
    public async Task<IActionResult> PrintReplacement(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var request = await _context.IDRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id
                                   && r.Status == IDRequestStatus.Approved);

        if (request == null)
        {
            TempData["Error"] = "ID replacement request not found or not yet approved.";
            return RedirectToAction("Index", "Home");
        }

        // Mark notification as read
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.UserId == user.Id
                && n.Type == NotificationType.IDCardReady
                && n.RelatedEntityId == id);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        // Load full user data
        var fullUser = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Branch)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (fullUser == null) return NotFound();

        // Merge Employee data
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);
        if (employee != null)
        {
            if (string.IsNullOrEmpty(fullUser.ContactNumber) && !string.IsNullOrEmpty(employee.ContactNumber))
                fullUser.ContactNumber = employee.ContactNumber;
            if (string.IsNullOrEmpty(fullUser.Address) && !string.IsNullOrEmpty(employee.Address))
                fullUser.Address = employee.Address;
            if (fullUser.StartDate == null) fullUser.StartDate = employee.HireDate;
            if (fullUser.Department == null && employee.Department != null)
                fullUser.Department = employee.Department;
        }

        var qrCodeDataUrl = GenerateQRCodeDataUrl(fullUser.QRCodeHash ?? fullUser.Id);
        ViewBag.QRCodeDataUrl  = qrCodeDataUrl;
        ViewBag.IsFirstTime    = false;
        ViewBag.RequestId      = id;
        ViewBag.IsReplacement  = true;

        await _context.SaveChangesAsync();

        return View("~/Views/IDCard/StaffIDCard.cshtml", fullUser);
    }

    // Mark request as printed
    [HttpPost]
    public async Task<IActionResult> MarkRequestPrinted(int requestId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var request = await _context.IDRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == user.Id);

        if (request != null && request.Status == Models.Enums.IDRequestStatus.Approved)
        {
            request.Status = Models.Enums.IDRequestStatus.Printed;
            request.IsPrinted = true;
            request.PrintedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(user.Id, "ID Card Printed from Request", "IDRequest", requestId.ToString(), null,
                new { PrintedAt = DateTime.UtcNow }, user.CompanyId);
        }

        return Json(new { success = true });
    }

    // Helper method to generate QR code
    private string GenerateQRCodeDataUrl(string data)
    {
        if (string.IsNullOrEmpty(data)) return "";

        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        using var qrCodeImage = qrCode.GetGraphic(20);
        using var ms = new MemoryStream();
        qrCodeImage.Save(ms, ImageFormat.Png);
        var base64 = Convert.ToBase64String(ms.ToArray());
        return $"data:image/png;base64,{base64}";
    }
}
