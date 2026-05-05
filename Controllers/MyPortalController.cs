using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;

namespace PayroTech.Controllers;

/// <summary>
/// Employee self-service portal. Route: /MyPortal/...
/// Views live in: Views/Employee/
/// </summary>
[Authorize]
public class MyPortalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyPortalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<ApplicationUser?> GetEmployeeUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Role != UserRole.Employee) return null;
        return user;
    }

    // GET: /MyPortal/Index  →  Views/Employee/Index.cshtml
    public async Task<IActionResult> Index()
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today    = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Get employee record
        var employee = await _context.Employees
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        // Today's attendance
        var todayAtt = employee != null
            ? await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today)
            : null;

        // This month stats
        int presentCount = 0, lateCount = 0, otHours = 0;
        if (employee != null)
        {
            var monthAtt = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && a.Date >= monthStart && a.Date <= today)
                .ToListAsync();
            presentCount = monthAtt.Count(a => a.TimeIn != null);
            lateCount    = monthAtt.Count(a => a.LateMinutes > 0);
            otHours      = monthAtt.Sum(a => a.OvertimeMinutes) / 60;
        }

        // Leave balance
        var leaveBalance = employee != null
            ? await _context.LeaveBalances.FirstOrDefaultAsync(l => l.EmployeeId == employee.Id && l.Year == today.Year)
            : null;

        // Recent leaves
        var recentLeaves = employee != null
            ? await _context.Leaves
                .Where(l => l.EmployeeId == employee.Id)
                .OrderByDescending(l => l.CreatedAt).Take(3).ToListAsync()
            : new List<Leave>();

        // Last payroll
        var lastPayroll = employee != null
            ? await _context.Payrolls
                .Include(p => p.PayrollPeriod)
                .Where(p => p.EmployeeId == employee.Id && p.Status == PayrollStatus.Paid)
                .OrderByDescending(p => p.PayrollPeriod.PayDate).FirstOrDefaultAsync()
            : null;

        ViewBag.Employee      = employee;
        ViewBag.TodayAtt      = todayAtt;
        ViewBag.PresentCount  = presentCount;
        ViewBag.LateCount     = lateCount;
        ViewBag.OtHours       = otHours;
        ViewBag.LeaveBalance  = leaveBalance;
        ViewBag.RecentLeaves  = recentLeaves;
        ViewBag.LastPayroll   = lastPayroll;
        ViewBag.CurrentUser   = user;

        return View("~/Views/Employee/Index.cshtml");
    }

    // GET: /MyPortal/Attendance  →  Views/Employee/Attendance.cshtml
    public async Task<IActionResult> Attendance(int? month, int? year)
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today     = DateTime.Today;
        var viewMonth = month ?? today.Month;
        var viewYear  = year  ?? today.Year;
        var monthStart = new DateTime(viewYear, viewMonth, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);

        var employee = await _context.Employees
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        List<Attendance> monthAttendance = new();
        if (employee != null)
        {
            monthAttendance = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && a.Date >= monthStart && a.Date <= monthEnd)
                .OrderByDescending(a => a.Date).ToListAsync();
        }

        ViewBag.Employee        = employee;
        ViewBag.MonthAttendance = monthAttendance;
        ViewBag.ViewMonth       = viewMonth;
        ViewBag.ViewYear        = viewYear;
        ViewBag.MonthName       = monthStart.ToString("MMMM yyyy");
        ViewBag.PresentCount    = monthAttendance.Count(a => a.TimeIn != null && a.LateMinutes == 0);
        ViewBag.LateCount       = monthAttendance.Count(a => a.LateMinutes > 0);
        ViewBag.OtMinutes       = monthAttendance.Sum(a => a.OvertimeMinutes);

        return View("~/Views/Employee/Attendance.cshtml");
    }

    // GET: /MyPortal/Leaves  →  Views/Employee/Leaves.cshtml
    public async Task<IActionResult> Leaves()
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);

        List<Leave> leaves = new();
        LeaveBalance? balance = null;

        if (employee != null)
        {
            leaves = await _context.Leaves
                .Where(l => l.EmployeeId == employee.Id)
                .OrderByDescending(l => l.CreatedAt).ToListAsync();

            balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(l => l.EmployeeId == employee.Id && l.Year == DateTime.Today.Year);
        }

        ViewBag.Leaves   = leaves;
        ViewBag.Balance  = balance;
        ViewBag.Employee = employee;

        return View("~/Views/Employee/Leaves.cshtml");
    }

    // GET: /MyPortal/RequestLeave  →  Views/Employee/RequestLeave.cshtml
    [HttpGet]
    public async Task<IActionResult> RequestLeave()
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
        var balance  = employee != null
            ? await _context.LeaveBalances.FirstOrDefaultAsync(l => l.EmployeeId == employee.Id && l.Year == DateTime.Today.Year)
            : null;

        // Upcoming holidays
        var holidays = await _context.Holidays
            .Where(h => h.CompanyId == user.CompanyId && h.Date >= DateTime.Today)
            .OrderBy(h => h.Date).Take(5).ToListAsync();

        ViewBag.Balance   = balance;
        ViewBag.Employee  = employee;
        ViewBag.Holidays  = holidays;

        return View("~/Views/Employee/RequestLeave.cshtml");
    }

    // POST: /MyPortal/RequestLeave
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestLeave(LeaveType leaveType, DateTime startDate, DateTime endDate, string reason)
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
        if (employee == null)
        {
            TempData["Error"] = "Employee record not found.";
            return RedirectToAction(nameof(RequestLeave));
        }

        if (endDate < startDate)
        {
            TempData["Error"] = "End date cannot be before start date.";
            return RedirectToAction(nameof(RequestLeave));
        }

        var totalDays = (decimal)(endDate - startDate).TotalDays + 1;

        var leave = new Leave
        {
            EmployeeId   = employee.Id,
            LeaveType    = leaveType,
            StartDate    = startDate,
            EndDate      = endDate,
            TotalDays    = totalDays,
            Reason       = reason,
            Status       = LeaveStatus.Pending,
            CreatedAt    = DateTime.UtcNow
        };

        _context.Leaves.Add(leave);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Leave request submitted for {startDate:MMM dd} – {endDate:MMM dd, yyyy}.";
        return RedirectToAction(nameof(Leaves));
    }

    // GET: /MyPortal/Payslips  →  Views/Employee/Payslips.cshtml
    public async Task<IActionResult> Payslips(int? year)
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var viewYear = year ?? DateTime.Today.Year;
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);

        List<Payroll> payrolls = new();
        if (employee != null)
        {
            payrolls = await _context.Payrolls
                .Include(p => p.PayrollPeriod)
                .Where(p => p.EmployeeId == employee.Id && p.PayrollPeriod.StartDate.Year == viewYear)
                .OrderByDescending(p => p.PayrollPeriod.PayDate).ToListAsync();
        }

        ViewBag.Payrolls  = payrolls;
        ViewBag.Employee  = employee;
        ViewBag.ViewYear  = viewYear;
        ViewBag.YtdGross  = payrolls.Sum(p => p.GrossPay);
        ViewBag.YtdNet    = payrolls.Sum(p => p.NetPay);
        ViewBag.YtdTax    = payrolls.Sum(p => p.WithholdingTax);
        ViewBag.YtdBenefits = payrolls.Sum(p => p.SSSContribution + p.PhilHealthContribution + p.PagIbigContribution);

        return View("~/Views/Employee/Payslips.cshtml");
    }

    // GET: /MyPortal/Overtime  →  Views/Employee/Overtime.cshtml
    public async Task<IActionResult> Overtime(int? month, int? year)
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today      = DateTime.Today;
        var viewMonth  = month ?? today.Month;
        var viewYear   = year  ?? today.Year;
        var monthStart = new DateTime(viewYear, viewMonth, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);

        List<Overtime> overtimes = new();
        if (employee != null)
        {
            overtimes = await _context.Overtimes
                .Where(o => o.EmployeeId == employee.Id && o.Date >= monthStart && o.Date <= monthEnd)
                .OrderByDescending(o => o.Date).ToListAsync();
        }

        ViewBag.Overtimes  = overtimes;
        ViewBag.Employee   = employee;
        ViewBag.ViewMonth  = viewMonth;
        ViewBag.ViewYear   = viewYear;
        ViewBag.MonthName  = monthStart.ToString("MMMM yyyy");
        ViewBag.TotalOTMin = overtimes.Where(o => o.Status == LeaveStatus.Approved).Sum(o => o.TotalMinutes);
        ViewBag.PendingCount = overtimes.Count(o => o.Status == LeaveStatus.Pending);

        return View("~/Views/Employee/Overtime.cshtml");
    }

    // GET: /MyPortal/PrintPayslip?id=X — printable payslip page (opens in new tab)
    [HttpGet]
    public async Task<IActionResult> PrintPayslip(int id)
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        if (employee == null) return NotFound();

        var payroll = await _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Company)
            .FirstOrDefaultAsync(p => p.Id == id && p.EmployeeId == employee.Id);

        if (payroll == null) return NotFound();

        return View("~/Views/Employee/PrintPayslip.cshtml", payroll);
    }

    // GET: /MyPortal/GetPayslip?id=X — returns full payslip JSON for modal
    [HttpGet]
    public async Task<IActionResult> GetPayslip(int id)
    {
        var user = await GetEmployeeUser();
        if (user == null) return Unauthorized();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
        if (employee == null) return NotFound();

        var payroll = await _context.Payrolls
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Company)
            .FirstOrDefaultAsync(p => p.Id == id && p.EmployeeId == employee.Id);

        if (payroll == null) return NotFound();

        return Json(new
        {
            employeeName   = payroll.Employee?.FullName,
            employeeNumber = payroll.Employee?.EmployeeNumber,
            department     = payroll.Employee?.Department?.DepartmentName ?? "—",
            companyName    = payroll.Employee?.Company?.CompanyName ?? "—",
            period         = payroll.PayrollPeriod?.PeriodName,
            startDate      = payroll.PayrollPeriod?.StartDate.ToString("MMM dd, yyyy"),
            endDate        = payroll.PayrollPeriod?.EndDate.ToString("MMM dd, yyyy"),
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

    // GET: /MyPortal/SalaryBreakdown  →  Views/Employee/SalaryBreakdown.cshtml
    public async Task<IActionResult> SalaryBreakdown()
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        // Pull from last paid payroll for accurate figures
        Payroll? lastPayroll = null;
        if (employee != null)
        {
            lastPayroll = await _context.Payrolls
                .Include(p => p.PayrollPeriod)
                .Where(p => p.EmployeeId == employee.Id && p.Status == PayrollStatus.Paid)
                .OrderByDescending(p => p.PayrollPeriod.PayDate)
                .FirstOrDefaultAsync();
        }

        ViewBag.Employee    = employee;
        ViewBag.CurrentUser = user;
        ViewBag.LastPayroll = lastPayroll;

        return View("~/Views/Employee/SalaryBreakdown.cshtml");
    }

    // GET: /MyPortal/Profile  →  Views/Employee/Profile.cshtml
    public async Task<IActionResult> Profile()
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        var fullUser = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        ViewBag.Employee    = employee;
        ViewBag.CurrentUser = fullUser;

        return View("~/Views/Employee/Profile.cshtml");
    }

    // GET: /MyPortal/AuditLogs  →  Views/Employee/AuditLogs.cshtml
    public async Task<IActionResult> AuditLogs(int page = 1, int days = 7, string? action = null, string? search = null)
    {
        var user = await GetEmployeeUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var fromDate = DateTime.UtcNow.AddDays(-days);
        var pageSize = 20;

        var query = _context.AuditLogs
            .Where(l => l.UserId == user.Id && l.CreatedAt >= fromDate)
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(action)) query = query.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(search)) query = query.Where(l =>
            l.Action.Contains(search) || (l.NewValues != null && l.NewValues.Contains(search)));

        var totalCount = await query.CountAsync();
        var logs       = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.Days        = days;

        return View("~/Views/Employee/AuditLogs.cshtml", logs);
    }
}
