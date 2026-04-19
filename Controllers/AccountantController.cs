using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Services;

namespace PayroTech.Controllers;

[Authorize]
public class AccountantController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public AccountantController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    private async Task<ApplicationUser?> GetAccountantUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Role != UserRole.Accountant) return null;
        return user;
    }

    // GET: /Accountant/Index  →  Views/Accountant/Index.cshtml
    public async Task<IActionResult> Index()
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today      = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Pending payrolls (approved, not yet paid)
        var pendingPayrolls = await _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Processed))
            .OrderByDescending(p => p.PayrollPeriod.PayDate)
            .Take(10).ToListAsync();

        // This month totals
        var monthPayrolls = await _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && p.PayrollPeriod.StartDate >= monthStart
                     && p.Status == PayrollStatus.Paid)
            .ToListAsync();

        // Today's attendance for accountant's own record
        var myEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
        var todayAtt   = myEmployee != null
            ? await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == myEmployee.Id && a.Date == today)
            : null;

        ViewBag.PendingPayrolls   = pendingPayrolls;
        ViewBag.PendingCount      = pendingPayrolls.Count;
        ViewBag.PendingTotal      = pendingPayrolls.Sum(p => p.NetPay);
        ViewBag.MonthTotal        = monthPayrolls.Sum(p => p.NetPay);
        ViewBag.MonthEmployees    = monthPayrolls.Select(p => p.EmployeeId).Distinct().Count();
        ViewBag.TodayAtt          = todayAtt;
        ViewBag.CurrentUser       = user;

        return View();
    }

    // GET: /Accountant/PendingPayrolls  →  Views/Accountant/PendingPayrolls.cshtml
    public async Task<IActionResult> PendingPayrolls(int page = 1)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        const int pageSize = 15;
        var query = _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Processed))
            .OrderByDescending(p => p.PayrollPeriod.PayDate);

        var total    = await query.CountAsync();
        var payrolls = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Payrolls    = payrolls;
        ViewBag.TotalCount  = total;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.PageSize    = pageSize;
        return View();
    }

    // GET: /Accountant/ProcessPayroll  →  Views/Accountant/ProcessPayroll.cshtml
    public async Task<IActionResult> ProcessPayroll()
    {
        if (await GetAccountantUser() == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Accountant/DistributeSalaries  →  Views/Accountant/DistributeSalaries.cshtml
    public async Task<IActionResult> DistributeSalaries()
    {
        if (await GetAccountantUser() == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Accountant/PayrollHistory  →  Views/Accountant/PayrollHistory.cshtml
    public async Task<IActionResult> PayrollHistory(int page = 1)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        const int pageSize = 15;
        var query = _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.Employee.CompanyId == user.CompanyId && p.Status == PayrollStatus.Paid)
            .OrderByDescending(p => p.PayrollPeriod.PayDate);

        var total    = await query.CountAsync();
        var payrolls = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Payrolls    = payrolls;
        ViewBag.TotalCount  = total;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.PageSize    = pageSize;
        return View();
    }

    // GET: /Accountant/Payslips  →  Views/Accountant/Payslips.cshtml
    public async Task<IActionResult> Payslips(int page = 1)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        const int pageSize = 15;
        var query = _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Employee)
            .Where(p => p.Employee.CompanyId == user.CompanyId)
            .OrderByDescending(p => p.PayrollPeriod.PayDate);

        var total    = await query.CountAsync();
        var payrolls = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Payrolls    = payrolls;
        ViewBag.TotalCount  = total;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.PageSize    = pageSize;
        return View();
    }

    // GET: /Accountant/Reports  →  Views/Accountant/Reports.cshtml
    public async Task<IActionResult> Reports()
    {
        if (await GetAccountantUser() == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Accountant/AuditLogs  →  Views/Accountant/AuditLogs.cshtml
    public async Task<IActionResult> AuditLogs(int page = 1, int days = 30)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        const int pageSize = 20;
        var fromDate = DateTime.UtcNow.AddDays(-days);

        var query = _context.AuditLogs
            .Where(l => l.UserId == user.Id && l.CreatedAt >= fromDate)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var logs  = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.AuditLogs   = logs;
        ViewBag.TotalCount  = total;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Days        = days;
        return View();
    }

    // GET: /Accountant/MyAttendance  →  Views/Accountant/MyAttendance.cshtml
    public async Task<IActionResult> MyAttendance()
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
        var today    = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        List<Attendance> attendance = new();
        if (employee != null)
        {
            attendance = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && a.Date >= monthStart)
                .OrderByDescending(a => a.Date).ToListAsync();
        }

        ViewBag.Attendance   = attendance;
        ViewBag.Employee     = employee;
        ViewBag.CurrentUser  = user;
        return View();
    }

    // GET: /Accountant/Profile  →  Views/Accountant/Profile.cshtml
    public async Task<IActionResult> Profile()
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var fullUser = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        ViewBag.CurrentUser = fullUser;
        return View();
    }
}
