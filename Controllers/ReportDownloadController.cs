using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Services;

namespace PayroTech.Controllers;

/// <summary>
/// Handles all password-protected PDF report downloads for Manager and SuperAdmin.
/// The user must verify their account password before downloading any report.
/// </summary>
[Authorize]
public class ReportDownloadController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPdfReportService _pdfService;
    private readonly IAuditLogService _auditLogService;

    public ReportDownloadController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPdfReportService pdfService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _pdfService = pdfService;
        _auditLogService = auditLogService;
    }

    // ═══════════════════════════════════════════════════════════════════
    // PASSWORD VERIFICATION
    // ═══════════════════════════════════════════════════════════════════

    [HttpPost]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid) return Json(new { success = false, message = "Incorrect password. Please try again." });

        // Store a short-lived token in session so the download can proceed
        var token = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("pdf_download_token", token);
        HttpContext.Session.SetString("pdf_download_password", request.Password);

        return Json(new { success = true, token });
    }

    // ═══════════════════════════════════════════════════════════════════
    // MANAGER REPORTS
    // ═══════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> ManagerPayrollSummary(int? year, int? month)
    {
        var (user, password) = await GetAuthorizedUser(UserRole.CompanyAdmin, UserRole.Supervisor);
        if (user == null) return Unauthorized("Verify your password first.");

        var selYear = year ?? DateTime.Today.Year;
        var selMonth = month ?? DateTime.Today.Month;
        var periodStart = new DateTime(selYear, selMonth, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var periodLabel = periodStart.ToString("MMMM yyyy");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        var payrolls = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && p.PayrollPeriod.StartDate >= periodStart
                     && p.PayrollPeriod.StartDate <= periodEnd)
            .OrderBy(p => p.Employee.LastName)
            .ToListAsync();

        var rows = payrolls.Select(p => new PayrollReportRow
        {
            EmployeeName = p.Employee?.FullName ?? "—",
            Department = p.Employee?.Department?.DepartmentName ?? "—",
            DaysWorked = p.DaysWorked,
            GrossPay = p.GrossPay,
            TotalDeductions = p.TotalDeductions,
            GovContributions = p.SSSContribution + p.PhilHealthContribution + p.PagIbigContribution,
            NetPay = p.NetPay
        }).ToList();

        var pdf = _pdfService.GeneratePayrollSummary(
            company?.CompanyName ?? "Company", rows, periodLabel, password!);

        await _auditLogService.LogAsync(user.Id, "Downloaded Payroll Summary PDF", "Report",
            null, null, new { Period = periodLabel, EmployeeCount = rows.Count }, user.CompanyId);

        return File(pdf, "application/pdf", $"PayrollSummary_{periodLabel.Replace(" ", "_")}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ManagerEmployeeMasterlist()
    {
        var (user, password) = await GetAuthorizedUser(UserRole.CompanyAdmin, UserRole.Supervisor);
        if (user == null) return Unauthorized("Verify your password first.");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive)
            .OrderBy(e => e.LastName).ToListAsync();

        var staffUsers = await _context.Users
            .Include(u => u.Department)
            .Where(u => u.CompanyId == user.CompanyId && u.IsActive
                     && (u.Role == UserRole.HR || u.Role == UserRole.Accountant))
            .ToListAsync();

        var rows = new List<EmployeeReportRow>();
        rows.AddRange(staffUsers.Select(u => new EmployeeReportRow
        {
            EmployeeNumber = u.EmployeeNumber ?? "—",
            FullName = u.FullName,
            Email = u.Email ?? "—",
            Department = u.Department?.DepartmentName ?? "—",
            Role = u.Role.ToString(),
            HireDate = u.StartDate?.ToString("MMM dd, yyyy") ?? "—"
        }));
        rows.AddRange(employees.Select(e => new EmployeeReportRow
        {
            EmployeeNumber = e.EmployeeNumber,
            FullName = e.FullName,
            Email = e.Email ?? "—",
            Department = e.Department?.DepartmentName ?? "—",
            Role = "Employee",
            HireDate = e.HireDate.ToString("MMM dd, yyyy")
        }));

        var pdf = _pdfService.GenerateEmployeeMasterlist(
            company?.CompanyName ?? "Company", rows, password!);

        await _auditLogService.LogAsync(user.Id, "Downloaded Employee Masterlist PDF", "Report",
            null, null, new { TotalStaff = rows.Count }, user.CompanyId);

        return File(pdf, "application/pdf", $"EmployeeMasterlist_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ManagerAttendanceReport(int? year, int? month)
    {
        var (user, password) = await GetAuthorizedUser(UserRole.CompanyAdmin, UserRole.Supervisor);
        if (user == null) return Unauthorized("Verify your password first.");

        var selYear = year ?? DateTime.Today.Year;
        var selMonth = month ?? DateTime.Today.Month;
        var periodStart = new DateTime(selYear, selMonth, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var periodLabel = periodStart.ToString("MMMM yyyy");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive).ToListAsync();

        var attendance = await _context.Attendances
            .Where(a => a.Employee.CompanyId == user.CompanyId
                     && a.Date >= periodStart && a.Date <= periodEnd)
            .ToListAsync();

        var workDays = Enumerable.Range(0, (periodEnd - periodStart).Days + 1)
            .Select(d => periodStart.AddDays(d))
            .Count(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);

        var rows = employees.Select(emp =>
        {
            var empAtt = attendance.Where(a => a.EmployeeId == emp.Id).ToList();
            var present = empAtt.Count(a => a.TimeIn != null);
            return new AttendanceReportRow
            {
                EmployeeName = emp.FullName,
                Department = emp.Department?.DepartmentName ?? "—",
                PresentDays = present,
                LateDays = empAtt.Count(a => a.LateMinutes > 0),
                AbsentDays = Math.Max(0, workDays - present),
                OtHours = Math.Round(empAtt.Sum(a => a.OvertimeMinutes) / 60m, 1),
                LateMinutes = empAtt.Sum(a => a.LateMinutes)
            };
        }).ToList();

        var pdf = _pdfService.GenerateAttendanceReport(
            company?.CompanyName ?? "Company", rows, periodLabel, password!);

        await _auditLogService.LogAsync(user.Id, "Downloaded Attendance Report PDF", "Report",
            null, null, new { Period = periodLabel }, user.CompanyId);

        return File(pdf, "application/pdf", $"AttendanceReport_{periodLabel.Replace(" ", "_")}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ManagerGovernmentReport(int? year, int? month)
    {
        var (user, password) = await GetAuthorizedUser(UserRole.CompanyAdmin, UserRole.Supervisor);
        if (user == null) return Unauthorized("Verify your password first.");

        var selYear = year ?? DateTime.Today.Year;
        var selMonth = month ?? DateTime.Today.Month;
        var periodStart = new DateTime(selYear, selMonth, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var periodLabel = periodStart.ToString("MMMM yyyy");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        var payrolls = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && p.PayrollPeriod.StartDate >= periodStart
                     && p.PayrollPeriod.StartDate <= periodEnd)
            .ToListAsync();

        var rows = payrolls.Select(p => new GovernmentReportRow
        {
            EmployeeName = p.Employee?.FullName ?? "—",
            Department = p.Employee?.Department?.DepartmentName ?? "—",
            SSS = p.SSSContribution,
            PhilHealth = p.PhilHealthContribution,
            PagIbig = p.PagIbigContribution,
            Tax = p.WithholdingTax
        }).ToList();

        var pdf = _pdfService.GenerateGovernmentReport(
            company?.CompanyName ?? "Company", rows, periodLabel, password!);

        await _auditLogService.LogAsync(user.Id, "Downloaded Government Compliance PDF", "Report",
            null, null, new { Period = periodLabel }, user.CompanyId);

        return File(pdf, "application/pdf", $"GovernmentCompliance_{periodLabel.Replace(" ", "_")}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ManagerAuditLogReport(int? days)
    {
        var (user, password) = await GetAuthorizedUser(UserRole.CompanyAdmin, UserRole.Supervisor);
        if (user == null) return Unauthorized("Verify your password first.");

        var daysBack = days ?? 30;
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);
        var company = await _context.Companies.FindAsync(user.CompanyId);

        var logs = await _context.AuditLogs
            .Include(l => l.User)
            .Where(l => l.CompanyId == user.CompanyId && l.CreatedAt >= fromDate)
            .OrderByDescending(l => l.CreatedAt)
            .Take(500)
            .ToListAsync();

        var rows = logs.Select(l => new AuditLogReportRow
        {
            DateTime = l.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt"),
            UserName = l.User?.FullName ?? "System",
            Action = l.Action,
            Category = l.EntityType ?? "—",
            IpAddress = l.IpAddress ?? "—"
        }).ToList();

        var periodLabel = $"Last {daysBack} days";
        var pdf = _pdfService.GenerateAuditLogReport(
            company?.CompanyName ?? "Company", rows, periodLabel, password!);

        await _auditLogService.LogAsync(user.Id, "Downloaded Audit Log PDF", "Report",
            null, null, new { Days = daysBack, LogCount = rows.Count }, user.CompanyId);

        return File(pdf, "application/pdf", $"AuditLogs_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // ═══════════════════════════════════════════════════════════════════
    // SUPERADMIN REPORTS
    // ═══════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> SuperAdminCompanyReport()
    {
        var (user, password) = await GetAuthorizedUser(UserRole.ErpSuperAdmin);
        if (user == null) return Unauthorized("Verify your password first.");

        var companies = await _context.Companies
            .Include(c => c.Employees)
            .OrderBy(c => c.CompanyName).ToListAsync();

        var rows = companies.Select(c => new CompanyReportRow
        {
            CompanyName = c.CompanyName,
            Industry = c.Address ?? "—",
            EmployeeCount = c.Employees?.Count(e => e.IsActive) ?? 0,
            Subscription = c.IsSubscriptionActive ? "Active" : "Inactive",
            RegisteredDate = c.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy"),
            Status = c.IsActive ? "Active" : "Inactive"
        }).ToList();

        var pdf = _pdfService.GenerateSuperAdminCompanyReport(rows, password!);
        return File(pdf, "application/pdf", $"CompanyDirectory_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> SuperAdminRevenueReport(int? year, int? month)
    {
        var (user, password) = await GetAuthorizedUser(UserRole.ErpSuperAdmin);
        if (user == null) return Unauthorized("Verify your password first.");

        var selYear = year ?? DateTime.Today.Year;
        var selMonth = month ?? DateTime.Today.Month;
        var periodLabel = new DateTime(selYear, selMonth, 1).ToString("MMMM yyyy");

        // Revenue is subscription-based: ₱2,500 per active company
        var companies = await _context.Companies
            .Where(c => c.IsSubscriptionActive && c.CreatedAt.Year <= selYear)
            .OrderBy(c => c.CompanyName).ToListAsync();

        var rows = companies.Select(c => new RevenueReportRow
        {
            Date = c.SubscriptionEnd?.ToLocalTime().ToString("MMM dd, yyyy") ?? "—",
            CompanyName = c.CompanyName,
            Plan = "Monthly Subscription",
            Amount = 2500m,
            Status = c.IsSubscriptionActive ? "Active" : "Expired"
        }).ToList();

        var pdf = _pdfService.GenerateSuperAdminRevenueReport(rows, periodLabel, password!);
        return File(pdf, "application/pdf", $"RevenueReport_{periodLabel.Replace(" ", "_")}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> SuperAdminActivityReport(int? days)
    {
        var (user, password) = await GetAuthorizedUser(UserRole.ErpSuperAdmin);
        if (user == null) return Unauthorized("Verify your password first.");

        var daysBack = days ?? 30;
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);

        var logs = await _context.VendorLogs
            .OrderByDescending(l => l.CreatedAt)
            .Where(l => l.CreatedAt >= fromDate)
            .Take(500)
            .ToListAsync();

        var rows = logs.Select(l => new AuditLogReportRow
        {
            DateTime = l.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt"),
            UserName = l.VendorName ?? "System",
            Action = l.Action ?? "—",
            Category = l.EntityType ?? "—",
            IpAddress = l.IpAddress ?? "—"
        }).ToList();

        var periodLabel = $"Last {daysBack} days";
        var pdf = _pdfService.GenerateSuperAdminActivityReport(rows, periodLabel, password!);
        return File(pdf, "application/pdf", $"SystemActivityReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // ═══════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private async Task<(ApplicationUser? user, string? password)> GetAuthorizedUser(params UserRole[] allowedRoles)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !allowedRoles.Contains(user.Role))
            return (null, null);

        var password = HttpContext.Session.GetString("pdf_download_password");
        var token = HttpContext.Session.GetString("pdf_download_token");
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(token))
            return (null, null);

        // Clear token after use (one-time use per verification)
        HttpContext.Session.Remove("pdf_download_token");
        HttpContext.Session.Remove("pdf_download_password");

        return (user, password);
    }
}

public class VerifyPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}
