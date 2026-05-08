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
    public async Task<IActionResult> ProcessPayroll(int? periodId)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        // Get all periods that are Approved (ready for accountant review before paying)
        var periods = await _context.PayrollPeriods
            .Where(p => p.CompanyId == user.CompanyId
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Processed))
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        PayrollPeriod? selectedPeriod = null;
        List<Payroll> payrolls = new();

        if (periodId.HasValue)
        {
            selectedPeriod = periods.FirstOrDefault(p => p.Id == periodId.Value);
        }
        else if (periods.Any())
        {
            selectedPeriod = periods.First();
        }

        if (selectedPeriod != null)
        {
            payrolls = await _context.Payrolls
                .Include(p => p.Employee).ThenInclude(e => e.Department)
                .Include(p => p.PayrollPeriod)
                .Where(p => p.PayrollPeriodId == selectedPeriod.Id)
                .OrderBy(p => p.Employee.LastName)
                .ToListAsync();
        }

        ViewBag.Periods        = periods;
        ViewBag.SelectedPeriod = selectedPeriod;
        ViewBag.Payrolls       = payrolls;
        ViewBag.TotalGross     = payrolls.Sum(p => p.GrossPay);
        ViewBag.TotalDeduct    = payrolls.Sum(p => p.TotalDeductions);
        ViewBag.TotalNet       = payrolls.Sum(p => p.NetPay);
        ViewBag.TotalSSS       = payrolls.Sum(p => p.SSSContribution);
        ViewBag.TotalPhilHealth = payrolls.Sum(p => p.PhilHealthContribution);
        ViewBag.TotalPagIbig   = payrolls.Sum(p => p.PagIbigContribution);
        ViewBag.TotalTax       = payrolls.Sum(p => p.WithholdingTax);
        ViewBag.TotalLate      = payrolls.Sum(p => p.LateDeduction);
        ViewBag.TotalAbsence   = payrolls.Sum(p => p.AbsenceDeduction);
        ViewBag.TotalOT        = payrolls.Sum(p => p.OvertimePay);
        ViewBag.TotalHoliday   = payrolls.Sum(p => p.HolidayPay);

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

    // POST: /Accountant/DistributeDept — distribute budget to one department
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DistributeDept(int periodId, string deptName)
    {
        var user = await GetAccountantUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var payrolls = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.PayrollPeriodId == periodId
                     && p.Employee.CompanyId == user.CompanyId
                     && (p.Employee.Department!.DepartmentName ?? "General") == deptName
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Processed))
            .ToListAsync();

        if (!payrolls.Any())
            return Json(new { success = false, message = "No pending payrolls found for this department." });

        var total = payrolls.Sum(p => p.NetPay);
        foreach (var p in payrolls) { p.Status = PayrollStatus.Paid; p.ProcessedAt = DateTime.UtcNow; }
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Distributed Department Salaries", "Payroll",
            periodId.ToString(), null,
            new { Department = deptName, Count = payrolls.Count, Total = total },
            user.CompanyId);

        return Json(new { success = true, message = $"✅ ₱{total:N2} distributed to {deptName} ({payrolls.Count} employees)." });
    }

    // POST: /Accountant/MarkDeptAsPaid — distribute budget to one department
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDeptAsPaid(int periodId, string deptName)
    {
        var user = await GetAccountantUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var payrolls = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.PayrollPeriodId == periodId
                     && p.Employee.CompanyId == user.CompanyId
                     && (p.Employee.Department!.DepartmentName ?? "General") == deptName
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Processed))
            .ToListAsync();

        if (!payrolls.Any())
            return Json(new { success = false, message = "No pending payrolls found for this department." });

        var total = payrolls.Sum(p => p.NetPay);
        foreach (var p in payrolls)
        {
            p.Status      = PayrollStatus.Paid;
            p.ProcessedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Distributed Department Salaries", "Payroll",
            periodId.ToString(), null,
            new { Department = deptName, Count = payrolls.Count, TotalAmount = total },
            user.CompanyId);

        return Json(new { success = true, count = payrolls.Count, total = total });
    }

    // POST: /Accountant/MarkAsPaid — mark a payroll record as paid (salary distributed)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var user = await GetAccountantUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id && p.Employee.CompanyId == user.CompanyId);

        if (payroll == null)
            return Json(new { success = false, message = "Payroll record not found." });

        if (payroll.Status == PayrollStatus.Paid)
            return Json(new { success = false, message = "Already marked as paid." });

        payroll.Status      = PayrollStatus.Paid;
        payroll.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Marked Payroll as Paid", "Payroll",
            payroll.Id.ToString(), null,
            new { EmployeeId = payroll.EmployeeId, NetPay = payroll.NetPay },
            user.CompanyId);

        return Json(new { success = true, message = $"Salary of ₱{payroll.NetPay:N2} marked as distributed." });
    }

    // POST: /Accountant/MarkAllAsPaid — mark all approved payrolls as paid for a period
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsPaid(int periodId)
    {
        var user = await GetAccountantUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var payrolls = await _context.Payrolls
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == periodId
                     && p.Employee.CompanyId == user.CompanyId
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Processed))
            .ToListAsync();

        if (!payrolls.Any())
            return Json(new { success = false, message = "No pending payrolls found for this period." });

        var total = payrolls.Sum(p => p.NetPay);
        foreach (var p in payrolls)
        {
            p.Status      = PayrollStatus.Paid;
            p.ProcessedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Distributed All Salaries", "PayrollPeriod",
            periodId.ToString(), null,
            new { Count = payrolls.Count, TotalAmount = total },
            user.CompanyId);

        TempData["Success"] = $"✅ {payrolls.Count} salaries totalling ₱{total:N2} marked as distributed!";
        return Json(new { success = true, count = payrolls.Count, total = total });
    }

    // GET: /Accountant/PayslipDetail?id=X — full payslip breakdown for one employee
    [HttpGet]
    public async Task<IActionResult> PayslipDetail(int id)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var payroll = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Shift)
            .Include(p => p.Employee).ThenInclude(e => e.Company)
            .Include(p => p.PayrollPeriod)
            .FirstOrDefaultAsync(p => p.Id == id && p.Employee.CompanyId == user.CompanyId);

        if (payroll == null) return NotFound();

        return Json(new
        {
            employeeName   = payroll.Employee?.FullName,
            employeeNumber = payroll.Employee?.EmployeeNumber,
            department     = payroll.Employee?.Department?.DepartmentName ?? "—",
            period         = payroll.PayrollPeriod?.PeriodName,
            payDate        = payroll.PayrollPeriod?.PayDate.ToString("MMMM dd, yyyy"),
            daysWorked     = payroll.DaysWorked,
            otHours        = payroll.OvertimeHours,
            lateHours      = payroll.LateHours,
            absentDays     = payroll.AbsentDays,
            basicPay       = payroll.BasicPay,
            overtimePay    = payroll.OvertimePay,
            holidayPay     = payroll.HolidayPay,
            grossPay       = payroll.GrossPay,
            lateDeduction  = payroll.LateDeduction,
            absenceDeduction = payroll.AbsenceDeduction,
            undertimeDeduction = payroll.UndertimeDeduction,
            sss            = payroll.SSSContribution,
            philhealth     = payroll.PhilHealthContribution,
            pagibig        = payroll.PagIbigContribution,
            tax            = payroll.WithholdingTax,
            totalDeductions = payroll.TotalDeductions,
            netPay         = payroll.NetPay,
            status         = payroll.Status.ToString()
        });
    }

    // GET: /Accountant/Reports  →  Views/Accountant/Reports.cshtml
    public async Task<IActionResult> Reports(int? year, int? month)
    {
        var user = await GetAccountantUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today    = DateTime.Today;
        var selYear  = year  ?? today.Year;
        var selMonth = month ?? today.Month;

        var periodStart = new DateTime(selYear, selMonth, 1);
        var periodEnd   = periodStart.AddMonths(1).AddDays(-1);

        // All paid payrolls for the selected month
        var payrolls = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && p.Status == PayrollStatus.Paid
                     && p.PayrollPeriod.StartDate >= periodStart
                     && p.PayrollPeriod.StartDate <= periodEnd)
            .OrderBy(p => p.Employee.LastName)
            .ToListAsync();

        // Year-to-date totals
        var ytdStart = new DateTime(selYear, 1, 1);
        var ytdPayrolls = await _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && p.Status == PayrollStatus.Paid
                     && p.PayrollPeriod.StartDate >= ytdStart
                     && p.PayrollPeriod.StartDate <= periodEnd)
            .ToListAsync();

        // Monthly breakdown for chart (all months in selected year)
        var monthlyData = await _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Where(p => p.Employee.CompanyId == user.CompanyId
                     && p.Status == PayrollStatus.Paid
                     && p.PayrollPeriod.StartDate.Year == selYear)
            .GroupBy(p => p.PayrollPeriod.StartDate.Month)
            .Select(g => new { Month = g.Key, NetTotal = g.Sum(p => p.NetPay), EmpCount = g.Select(p => p.EmployeeId).Distinct().Count() })
            .OrderBy(g => g.Month)
            .ToListAsync();

        ViewBag.Payrolls       = payrolls;
        ViewBag.SelectedYear   = selYear;
        ViewBag.SelectedMonth  = selMonth;
        ViewBag.PeriodLabel    = periodStart.ToString("MMMM yyyy");
        ViewBag.TotalGross     = payrolls.Sum(p => p.GrossPay);
        ViewBag.TotalNet       = payrolls.Sum(p => p.NetPay);
        ViewBag.TotalSSS       = payrolls.Sum(p => p.SSSContribution);
        ViewBag.TotalPhilHealth = payrolls.Sum(p => p.PhilHealthContribution);
        ViewBag.TotalPagIbig   = payrolls.Sum(p => p.PagIbigContribution);
        ViewBag.TotalTax       = payrolls.Sum(p => p.WithholdingTax);
        ViewBag.TotalLate      = payrolls.Sum(p => p.LateDeduction);
        ViewBag.TotalAbsence   = payrolls.Sum(p => p.AbsenceDeduction);
        ViewBag.TotalOT        = payrolls.Sum(p => p.OvertimePay);
        ViewBag.TotalHoliday   = payrolls.Sum(p => p.HolidayPay);
        ViewBag.EmployeeCount  = payrolls.Select(p => p.EmployeeId).Distinct().Count();
        ViewBag.YtdNet         = ytdPayrolls.Sum(p => p.NetPay);
        ViewBag.YtdSSS         = ytdPayrolls.Sum(p => p.SSSContribution);
        ViewBag.YtdPhilHealth  = ytdPayrolls.Sum(p => p.PhilHealthContribution);
        ViewBag.YtdPagIbig     = ytdPayrolls.Sum(p => p.PagIbigContribution);
        ViewBag.YtdTax         = ytdPayrolls.Sum(p => p.WithholdingTax);
        ViewBag.MonthlyData    = monthlyData;

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
