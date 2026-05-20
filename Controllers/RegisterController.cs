using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace PayroTech.Controllers;

/// <summary>
/// Public self-service company registration (online — no walk-in required).
/// Route: /Register
/// Accessible from the login page "Register Your Company Online" button.
/// </summary>
public class RegisterController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGeneratorService _passwordGenerator;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;

    public RegisterController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPasswordGeneratorService passwordGenerator,
        IEmailService emailService,
        IPaymentService paymentService)
    {
        _context          = context;
        _userManager      = userManager;
        _passwordGenerator = passwordGenerator;
        _emailService     = emailService;
        _paymentService   = paymentService;
    }

    // GET: /Register
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View("~/Views/Register/Index.cshtml");
    }

    // POST: /Register/Submit
    // Creates a pending (inactive) company + initiates PayMongo payment link.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit([FromBody] OnlineRegistrationRequest req)
    {
        if (req == null)
            return Json(new { success = false, message = "Invalid request." });

        if (string.IsNullOrWhiteSpace(req.CompanyName))
            return Json(new { success = false, message = "Company name is required." });
        if (string.IsNullOrWhiteSpace(req.AdminEmail))
            return Json(new { success = false, message = "Manager email is required." });
        if (string.IsNullOrWhiteSpace(req.AdminFirstName) || string.IsNullOrWhiteSpace(req.AdminLastName))
            return Json(new { success = false, message = "Manager name is required." });

        // Duplicate email check
        if (await _userManager.FindByEmailAsync(req.AdminEmail) != null)
            return Json(new { success = false, message = "An account with this email already exists. Please sign in." });

        // Duplicate company name check
        if (await _context.Companies.AnyAsync(c => c.CompanyName.ToLower() == req.CompanyName.ToLower()))
            return Json(new { success = false, message = "A company with this name is already registered." });

        try
        {
            // Calculate pricing
            var planName = req.PlanLevel switch { 2 => "Standard", 3 => "Professional", 4 => "Enterprise", _ => "Basic" };
            var planPrice = req.BillingCycle == "yearly"
                ? req.PlanLevel switch { 2 => 49990m, 3 => 79990m, 4 => 129990m, _ => 29990m }
                : req.PlanLevel switch { 2 => 4999m,  3 => 7999m,  4 => 12999m,  _ => 2999m  };
            var kioskCost   = req.KioskCount * (req.BillingCycle == "yearly" ? 2500m * 12 * 0.83m : 2500m);
            var totalAmount = planPrice + kioskCost;

            // Create PayMongo payment link first
            var paymentResult = await _paymentService.CreatePaymentLinkAsync(
                $"PayroTech {planName} Plan - {req.CompanyName}",
                totalAmount,
                $"{req.BillingCycle} billing, {req.KioskCount} kiosk(s)");

            if (paymentResult == null || string.IsNullOrEmpty(paymentResult.CheckoutUrl))
                return Json(new { success = false, message = "Could not create payment link. Please try again later." });

            // Create company as INACTIVE (pending payment)
            var companyCode = await GenerateCompanyCodeAsync(req.CompanyName);
            var company = new Company
            {
                CompanyCode          = companyCode,
                CompanyName          = req.CompanyName,
                Address              = req.Address ?? "",
                ContactNumber        = req.ContactNumber ?? "",
                Email                = req.AdminEmail,
                SubscriptionStart    = DateTime.UtcNow.AddDays(1),
                SubscriptionEnd      = req.BillingCycle == "yearly"
                    ? DateTime.UtcNow.AddDays(1).AddYears(1)
                    : DateTime.UtcNow.AddDays(1).AddMonths(1),
                IsSubscriptionActive = false,  // Activated after payment
                IsInitialSetupComplete = false,
                IsActive             = false   // Pending payment confirmation
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Enable modules based on plan
            var modules = req.PlanLevel switch
            {
                1 => new[] { ModuleType.Attendance, ModuleType.Payroll, ModuleType.Leave },
                2 => new[] { ModuleType.Attendance, ModuleType.Payroll, ModuleType.Leave, ModuleType.Overtime, ModuleType.GovernmentCompliance },
                3 => new[] { ModuleType.Attendance, ModuleType.Payroll, ModuleType.Leave, ModuleType.Overtime, ModuleType.GovernmentCompliance, ModuleType.Reports },
                _ => Enum.GetValues<ModuleType>()
            };
            foreach (var m in modules)
                _context.CompanyModules.Add(new CompanyModule { CompanyId = company.Id, ModuleType = m, IsEnabled = true });
            await _context.SaveChangesAsync();

            return Json(new
            {
                success     = true,
                checkoutUrl = paymentResult.CheckoutUrl,
                referenceNo = paymentResult.ReferenceNumber,
                companyId   = company.Id,
                totalAmount = totalAmount
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Registration error: " + ex.Message });
        }
    }

    // POST: /Register/ConfirmPayment
    // Called after the client clicks "I have completed payment".
    // Activates the company and creates the manager account.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest req)
    {
        if (req == null)
            return Json(new { success = false, message = "Invalid request." });

        var company = await _context.Companies.FindAsync(req.CompanyId);
        if (company == null)
            return Json(new { success = false, message = "Company not found." });

        // Activate company
        company.IsActive             = true;
        company.IsSubscriptionActive = true;

        // Create manager account
        var tempPassword = _passwordGenerator.GenerateRandomPassword(12);
        var qrHash       = KioskController.GenerateQRCodeHash(Guid.NewGuid().ToString());

        var manager = new ApplicationUser
        {
            UserName               = req.AdminEmail,
            Email                  = req.AdminEmail,
            FirstName              = req.AdminFirstName,
            LastName               = req.AdminLastName,
            CompanyId              = company.Id,
            Role                   = UserRole.CompanyAdmin,
            IsActive               = true,
            EmailConfirmed         = true,
            MustChangePassword     = true,
            RequiresFaceEnrollment = true,
            QRCodeHash             = qrHash,
            QRCodeGeneratedAt      = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(manager, tempPassword);
        if (!result.Succeeded)
            return Json(new
            {
                success = false,
                message = "Account creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description))
            });

        await _context.SaveChangesAsync();

        // Send welcome email with credentials
        await _emailService.SendWelcomeEmailAsync(
            req.AdminEmail,
            $"{req.AdminFirstName} {req.AdminLastName}",
            company.CompanyName,
            tempPassword);

        return Json(new
        {
            success  = true,
            message  = $"Welcome to PayroTech! Your account has been created. Check {req.AdminEmail} for your login credentials.",
            loginUrl = "/Auth/Login"
        });
    }

    private async Task<string> GenerateCompanyCodeAsync(string name)
    {
        var prefix = new string(name.ToUpper().Where(char.IsLetter).Take(3).ToArray()).PadRight(3, 'X');
        string code;
        int tries = 0;
        do
        {
            code = $"{prefix}{RandomNumberGenerator.GetInt32(1000, 9999)}";
            tries++;
        }
        while (await _context.Companies.AnyAsync(c => c.CompanyCode == code) && tries < 10);
        return code;
    }
}

public class OnlineRegistrationRequest
{
    public string  CompanyName    { get; set; } = "";
    public string? Address        { get; set; }
    public string? ContactNumber  { get; set; }
    public string  AdminEmail     { get; set; } = "";
    public string  AdminFirstName { get; set; } = "";
    public string  AdminLastName  { get; set; } = "";
    public int     PlanLevel      { get; set; } = 1;
    public string  BillingCycle   { get; set; } = "monthly";
    public int     KioskCount     { get; set; } = 1;
}

public class ConfirmPaymentRequest
{
    public int    CompanyId      { get; set; }
    public string AdminEmail     { get; set; } = "";
    public string AdminFirstName { get; set; } = "";
    public string AdminLastName  { get; set; } = "";
}
