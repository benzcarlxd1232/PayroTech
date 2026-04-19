using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Services;
using System.ComponentModel.DataAnnotations;

namespace PayroTech.Controllers;

/// <summary>
/// Incident Report — all staff (Employee, HR, Accountant) submit to CompanyAdmin.
/// Lost ID Card incidents auto-generate an approved IDRequest on manager approval.
/// </summary>
[Authorize]
public class IncidentReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;

    public IncidentReportController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService,
        IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
        _emailService = emailService;
    }

    // ── GET: /IncidentReport/Create ──────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        ViewBag.IncidentTypes = GetIncidentTypeList();
        return View();
    }

    // ── POST: /IncidentReport/Create ─────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIncidentViewModel model, string? WhenHappenedTime)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (!string.IsNullOrEmpty(WhenHappenedTime) && TimeSpan.TryParse(WhenHappenedTime, out var ts))
            model.WhenHappenedTime = ts;

        if (!ModelState.IsValid)
        {
            ViewBag.IncidentTypes = GetIncidentTypeList();
            return View(model);
        }

        // ALL staff reports go to CompanyAdmin
        string? assignedToId = await GetManagerIdAsync(user);

        var report = new IncidentReport
        {
            CompanyId    = user.CompanyId ?? 0,
            ReportedById = user.Id,
            AssignedToId = assignedToId,
            Title        = GetIncidentTitle(model.IncidentType),
            Description  = model.Description ?? string.Empty,
            Type         = model.IncidentType,
            Severity     = IncidentSeverity.Normal,
            Status       = IncidentStatus.Open,
            WhenHappened = model.WhenHappenedDate.HasValue
                ? model.WhenHappenedDate.Value.Date.Add(model.WhenHappenedTime ?? TimeSpan.Zero)
                : null,
            WhatHappened = model.WhatHappened,
            WhyHappened  = model.WhyHappened ?? string.Empty,
            CreatedAt    = DateTime.UtcNow
        };

        _context.IncidentReports.Add(report);
        await _context.SaveChangesAsync();

        // Notify the assigned manager in-app
        if (assignedToId != null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId            = assignedToId,
                Title             = $"New Incident Report: {report.Title}",
                Message           = $"{user.FullName} submitted an incident report. Please review.",
                Type              = NotificationType.IncidentReport,
                Priority          = NotificationPriority.High,
                ActionUrl         = $"/IncidentReport/Review",
                RelatedEntityType = "IncidentReport",
                RelatedEntityId   = report.Id,
                CreatedAt         = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Also send email to manager
            var manager = await _userManager.FindByIdAsync(assignedToId);
            if (manager != null)
            {
                await _emailService.SendEmailAsync(
                    manager.Email!,
                    manager.FullName,
                    $"[PayroTech] New Incident Report from {user.FullName}",
                    BuildManagerNotificationEmail(user.FullName, report.Title, model.WhatHappened));
            }
        }

        await _auditLogService.LogAsync(user.Id, "Created Incident Report", "IncidentReport",
            report.Id.ToString(), null,
            new { Type = model.IncidentType.ToString(), CompanyId = user.CompanyId },
            user.CompanyId);

        TempData["Success"] = "Incident report submitted successfully.";
        return RedirectToAction(nameof(MyReports));
    }

    // ── GET: /IncidentReport/MyReports ───────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> MyReports()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var reports = await _context.IncidentReports
            .Include(r => r.AssignedTo)
            .Where(r => r.ReportedById == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Check if any approved Lost ID reports have a downloadable IDRequest
        var approvedIdRequestIds = await _context.IDRequests
            .Where(r => r.UserId == user.Id && r.Status == IDRequestStatus.Approved)
            .Select(r => r.Id)
            .ToListAsync();

        ViewBag.ApprovedIDRequestIds = approvedIdRequestIds;
        return View(reports);
    }

    // ── GET: /IncidentReport/Review — CompanyAdmin sees all company reports ──
    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (user.Role != UserRole.CompanyAdmin)
            return RedirectToAction("Index", "Home");

        var reports = await _context.IncidentReports
            .Include(r => r.ReportedBy)
            .Where(r => r.CompanyId == user.CompanyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reports);
    }

    // ── GET: /IncidentReport/Details/{id} ────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var report = await _context.IncidentReports
            .Include(r => r.ReportedBy).ThenInclude(u => u.Company)
            .Include(r => r.AssignedTo)
            .FirstOrDefaultAsync(r => r.Id == id
                && (r.ReportedById == user.Id || r.CompanyId == user.CompanyId));

        if (report == null) return NotFound();

        // Find linked IDRequest if Lost ID and approved
        IDRequest? idRequest = null;
        if (report.Type == IncidentType.LostIDCard && report.Status == IncidentStatus.Resolved)
        {
            idRequest = await _context.IDRequests
                .FirstOrDefaultAsync(r => r.UserId == report.ReportedById
                    && r.Status == IDRequestStatus.Approved
                    && r.CompanyId == report.CompanyId);
        }

        ViewBag.IDRequest = idRequest;
        return View(report);
    }

    // ── POST: /IncidentReport/Approve/{id} ───────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user.Role != UserRole.CompanyAdmin)
            return Json(new { success = false, message = "Unauthorized" });

        var report = await _context.IncidentReports
            .Include(r => r.ReportedBy)
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == user.CompanyId);

        if (report == null)
            return Json(new { success = false, message = "Report not found" });

        report.Status     = IncidentStatus.Resolved;
        report.ResolvedAt = DateTime.UtcNow;
        report.Resolution = string.IsNullOrWhiteSpace(notes) ? "Approved by manager." : notes;

        int? idRequestId = null;

        // Lost ID Card → auto-create approved IDRequest
        if (report.Type == IncidentType.LostIDCard)
        {
            var idRequest = new IDRequest
            {
                UserId           = report.ReportedById,
                Reason           = "Lost ID Card — approved via incident report #" + report.Id,
                Status           = IDRequestStatus.Approved,
                CompanyId        = report.CompanyId,
                ApprovedByUserId = user.Id,
                ApprovedAt       = DateTime.UtcNow,
                ApprovalNotes    = report.Resolution,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            };
            _context.IDRequests.Add(idRequest);
            await _context.SaveChangesAsync();
            idRequestId = idRequest.Id;

            // In-app notification to staff
            _context.Notifications.Add(new Notification
            {
                UserId            = report.ReportedById,
                Title             = "✅ ID Card Replacement Approved",
                Message           = "Your lost ID card report has been approved. Go to My Submitted Reports to download your new ID card.",
                Type              = NotificationType.IDCardReady,
                Priority          = NotificationPriority.High,
                ActionUrl         = "/IncidentReport/MyReports",
                RelatedEntityType = "IDRequest",
                RelatedEntityId   = idRequest.Id,
                CreatedAt         = DateTime.UtcNow
            });
        }
        else
        {
            // General approval notification
            _context.Notifications.Add(new Notification
            {
                UserId            = report.ReportedById,
                Title             = "✅ Incident Report Resolved",
                Message           = $"Your incident report \"{report.Title}\" has been resolved by the manager.",
                Type              = NotificationType.IncidentReport,
                Priority          = NotificationPriority.Normal,
                ActionUrl         = $"/IncidentReport/Details/{report.Id}",
                RelatedEntityType = "IncidentReport",
                RelatedEntityId   = report.Id,
                CreatedAt         = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // Email notification to staff
        await _emailService.SendEmailAsync(
            report.ReportedBy.Email!,
            report.ReportedBy.FullName,
            "[PayroTech] Your Incident Report Has Been Approved",
            BuildStaffApprovalEmail(report.ReportedBy.FullName, report.Title, report.Resolution,
                report.Type == IncidentType.LostIDCard));

        await _auditLogService.LogAsync(user.Id, "Approved Incident Report", "IncidentReport",
            report.Id.ToString(), null, new { Notes = notes }, user.CompanyId);

        return Json(new { success = true, message = "Report approved.", idRequestId });
    }

    // ── POST: /IncidentReport/Reject/{id} ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || user.Role != UserRole.CompanyAdmin)
            return Json(new { success = false, message = "Unauthorized" });

        if (string.IsNullOrWhiteSpace(reason))
            return Json(new { success = false, message = "Rejection reason is required." });

        var report = await _context.IncidentReports
            .Include(r => r.ReportedBy)
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == user.CompanyId);

        if (report == null)
            return Json(new { success = false, message = "Report not found" });

        report.Status     = IncidentStatus.Closed;
        report.ResolvedAt = DateTime.UtcNow;
        report.Resolution = $"Rejected: {reason}";

        // In-app notification
        _context.Notifications.Add(new Notification
        {
            UserId            = report.ReportedById,
            Title             = "❌ Incident Report Rejected",
            Message           = $"Your incident report \"{report.Title}\" was rejected. Reason: {reason}",
            Type              = NotificationType.IncidentReport,
            Priority          = NotificationPriority.Normal,
            ActionUrl         = $"/IncidentReport/Details/{report.Id}",
            RelatedEntityType = "IncidentReport",
            RelatedEntityId   = report.Id,
            CreatedAt         = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Email notification
        await _emailService.SendEmailAsync(
            report.ReportedBy.Email!,
            report.ReportedBy.FullName,
            "[PayroTech] Your Incident Report Was Not Approved",
            BuildStaffRejectionEmail(report.ReportedBy.FullName, report.Title, reason));

        await _auditLogService.LogAsync(user.Id, "Rejected Incident Report", "IncidentReport",
            report.Id.ToString(), null, new { Reason = reason }, user.CompanyId);

        return Json(new { success = true, message = "Report rejected." });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string?> GetManagerIdAsync(ApplicationUser reporter)
    {
        // All staff → CompanyAdmin of same company
        var manager = await _context.Users
            .FirstOrDefaultAsync(u => u.CompanyId == reporter.CompanyId
                && u.Role == UserRole.CompanyAdmin
                && u.IsActive);
        return manager?.Id;
    }

    private static string GetIncidentTitle(IncidentType type) => type switch
    {
        IncidentType.LostIDCard          => "Lost ID Card",
        IncidentType.LateArrival         => "Late Arrival",
        IncidentType.AbsentWithoutNotice => "Absent Without Notice",
        IncidentType.EquipmentDamage     => "Equipment Damage",
        IncidentType.WorkplaceAccident   => "Workplace Accident",
        IncidentType.PayrollDelay        => "Payroll Delay",
        IncidentType.BudgetIssue         => "Budget Issue",
        IncidentType.SystemError         => "System Error",
        IncidentType.SecurityBreach      => "Security Breach",
        _                                => "Other Incident"
    };

    private static List<(int Value, string Label)> GetIncidentTypeList() =>
        Enum.GetValues<IncidentType>()
            .Select(t => ((int)t, GetIncidentTitle(t)))
            .ToList();

    private static string BuildManagerNotificationEmail(string staffName, string title, string what)
    {
        return "<div style='font-family:Inter,sans-serif;max-width:600px;margin:0 auto;padding:24px;'>" +
               "<h2 style='color:#1e293b;'>New Incident Report</h2>" +
               "<p><strong>" + staffName + "</strong> submitted an incident report that requires your review.</p>" +
               "<table style='width:100%;border-collapse:collapse;margin:16px 0;'>" +
               "<tr><td style='padding:8px;background:#f8fafc;font-weight:600;width:140px;'>Type</td>" +
               "<td style='padding:8px;border-bottom:1px solid #e2e8f0;'>" + title + "</td></tr>" +
               "<tr><td style='padding:8px;background:#f8fafc;font-weight:600;'>What Happened</td>" +
               "<td style='padding:8px;border-bottom:1px solid #e2e8f0;'>" + what + "</td></tr>" +
               "</table>" +
               "<p>Please log in to PayroTech to review and take action.</p>" +
               "</div>";
    }

    private static string BuildStaffApprovalEmail(string name, string title, string? notes, bool isLostId)
    {
        var lostIdNote = isLostId
            ? "<p style='background:#d1fae5;padding:12px;border-radius:8px;'><strong>Your replacement ID card is ready.</strong> Log in to PayroTech &rarr; My Submitted Reports to download it.</p>"
            : "";
        var notesHtml = notes != null
            ? "<p><strong>Manager's Notes:</strong> " + notes + "</p>"
            : "";
        return "<div style='font-family:Inter,sans-serif;max-width:600px;margin:0 auto;padding:24px;'>" +
               "<h2 style='color:#059669;'>Incident Report Approved</h2>" +
               "<p>Hi <strong>" + name + "</strong>,</p>" +
               "<p>Your incident report <strong>&ldquo;" + title + "&rdquo;</strong> has been approved by your manager.</p>" +
               notesHtml + lostIdNote +
               "</div>";
    }

    private static string BuildStaffRejectionEmail(string name, string title, string reason)
    {
        return "<div style='font-family:Inter,sans-serif;max-width:600px;margin:0 auto;padding:24px;'>" +
               "<h2 style='color:#dc2626;'>Incident Report Not Approved</h2>" +
               "<p>Hi <strong>" + name + "</strong>,</p>" +
               "<p>Your incident report <strong>&ldquo;" + title + "&rdquo;</strong> was not approved.</p>" +
               "<p><strong>Reason:</strong> " + reason + "</p>" +
               "<p>If you have questions, please contact your manager directly.</p>" +
               "</div>";
    }
}

// ── View Models ───────────────────────────────────────────────────────────────

public class CreateIncidentViewModel
{
    public IncidentType IncidentType { get; set; }
    public DateTime? WhenHappenedDate { get; set; } = DateTime.Today;
    public TimeSpan? WhenHappenedTime { get; set; } = DateTime.Now.TimeOfDay;

    [Required(ErrorMessage = "Please describe what happened.")]
    public string WhatHappened { get; set; } = string.Empty;
    public string? WhyHappened { get; set; }
    public string? Description { get; set; }
}
