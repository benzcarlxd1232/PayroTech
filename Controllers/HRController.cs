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
public class HRController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGeneratorService _passwordGenerator;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;
    private readonly IQRCodeService _qrCodeService;

    public HRController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPasswordGeneratorService passwordGenerator,
        IAuditLogService auditLogService,
        IEmailService emailService,
        IQRCodeService qrCodeService)
    {
        _context = context;
        _userManager = userManager;
        _passwordGenerator = passwordGenerator;
        _auditLogService = auditLogService;
        _emailService = emailService;
        _qrCodeService = qrCodeService;
    }

    private async Task<ApplicationUser?> GetHRUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Role != UserRole.HR) return null;
        return user;
    }

    // GET: /HR/Index  →  Views/HR/Index.cshtml
    public async Task<IActionResult> Index()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today  = DateTime.Today;
        var deptId = user.DepartmentId; // HR only sees their own team

        var totalMembers = await _context.Employees
            .CountAsync(e => e.CompanyId == user.CompanyId && e.IsActive
                          && (deptId == null || e.DepartmentId == deptId));

        var todayAttendance = await _context.Attendances
            .Where(a => a.Employee.CompanyId == user.CompanyId && a.Date.Date == today
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .ToListAsync();

        var presentCount = todayAttendance.Count(a => a.TimeIn != null);
        var lateCount    = todayAttendance.Count(a => a.LateMinutes > 0);
        var absentCount  = totalMembers - presentCount;

        var pendingLeaves = await _context.Leaves
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Where(l => l.Employee.CompanyId == user.CompanyId && l.Status == LeaveStatus.Pending
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .OrderBy(l => l.StartDate).Take(5).ToListAsync();

        var lateArrivals = await _context.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Where(a => a.Employee.CompanyId == user.CompanyId && a.Date.Date == today
                     && a.LateMinutes > 0
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .ToListAsync();

        var presentEmployeeIds = todayAttendance.Select(a => a.EmployeeId).ToList();
        var absentEmployees = await _context.Employees
            .Include(e => e.User)
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId)
                     && !presentEmployeeIds.Contains(e.Id))
            .ToListAsync();

        ViewBag.TotalMembers    = totalMembers;
        ViewBag.PresentCount    = presentCount;
        ViewBag.LateCount       = lateCount;
        ViewBag.AbsentCount     = absentCount;
        ViewBag.PendingLeaves   = pendingLeaves;
        ViewBag.LateArrivals    = lateArrivals;
        ViewBag.AbsentEmployees = absentEmployees;
        ViewBag.TeamName        = deptId.HasValue
            ? (await _context.Departments.FindAsync(deptId))?.DepartmentName ?? "My Team"
            : "All Teams";

        return View();
    }

    // GET: /HR/MyTeam  →  Views/HR/MyTeam.cshtml
    public async Task<IActionResult> MyTeam()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var deptId = user.DepartmentId;

        var employees = await _context.Employees
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId))
            .Include(e => e.User).Include(e => e.Department).Include(e => e.Shift)
            .OrderBy(e => e.LastName).ToListAsync();

        var staffUsers = await _context.Users
            .Where(u => u.CompanyId == user.CompanyId && u.IsActive
                     && (u.Role == UserRole.HR || u.Role == UserRole.Accountant)
                     && (deptId == null || u.DepartmentId == deptId))
            .Include(u => u.Department)
            .ToListAsync();

        var department = deptId.HasValue ? await _context.Departments.FindAsync(deptId) : null;
        var shifts     = await _context.Shifts
            .Where(s => s.CompanyId == user.CompanyId && s.IsActive).ToListAsync();

        // Auto-create default shifts if none exist
        if (!shifts.Any())
        {
            var morning = new Shift
            {
                CompanyId = user.CompanyId ?? 0, ShiftName = "Morning Shift (10AM-7PM)",
                StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(19, 0, 0),
                BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0),
                GracePeriodMinutes = 15, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            var night = new Shift
            {
                CompanyId = user.CompanyId ?? 0, ShiftName = "Night Shift (10PM-7AM)",
                StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(7, 0, 0),
                IsNightShift = true, GracePeriodMinutes = 15, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            _context.Shifts.AddRange(morning, night);
            await _context.SaveChangesAsync();
            shifts = new List<Shift> { morning, night };
        }

        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        ViewBag.Employees    = employees;
        ViewBag.StaffUsers   = staffUsers;
        ViewBag.CurrentHRUser = user;
        ViewBag.TeamName     = department?.DepartmentName ?? "My Team";
        ViewBag.TotalMembers = employees.Count + staffUsers.Count;
        ViewBag.ActiveMembers = employees.Count(e => e.IsActive) + staffUsers.Count;
        ViewBag.NewThisMonth = employees.Count(e => e.HireDate >= startOfMonth);
        ViewBag.Shifts       = shifts;

        return View();
    }

    // GET: /HR/CreateEmployee  →  Views/HR/CreateEmployee.cshtml
    [HttpGet]
    public async Task<IActionResult> CreateEmployee()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        await LoadDropdowns(user.CompanyId ?? 0, user.DepartmentId);
        return View();
    }

    // POST: /HR/CreateEmployee
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            await LoadDropdowns(user.CompanyId ?? 0, user.DepartmentId);
            return View(model);
        }

        // Get company default daily rate (salary set by manager, not HR)
        var company = await _context.Companies.FindAsync(user.CompanyId);
        var defaultDailyRate = company?.DefaultDailyRate ?? 500m;
        var workingDays      = company?.WorkingDaysPerMonth > 0 ? company.WorkingDaysPerMonth : 22;
        var basicSalary      = defaultDailyRate * workingDays;

        // Auto-generate employee number
        var employeeCount  = await _context.Employees.CountAsync(e => e.CompanyId == user.CompanyId);
        var employeeNumber = $"EMP-{user.CompanyId:D4}-{(employeeCount + 1):D5}";

        // Random secure temporary password
        var tempPassword = _passwordGenerator.GenerateRandomPassword(12);

        // Generate staff code
        var staffCode = await PayroTech.Utilities.StaffCodeGenerator.GenerateAsync(_context, user.CompanyId ?? 0, UserRole.Employee);
        var kioskPin  = PayroTech.Utilities.StaffCodeGenerator.GetDefaultPin(staffCode);

        // Force employee into HR's own team — HR cannot assign to other teams
        var assignedDeptId = user.DepartmentId ?? model.DepartmentId;

        var employeeUser = new ApplicationUser
        {
            UserName = model.Email, Email = model.Email,
            FirstName = model.FirstName, LastName = model.LastName,
            CompanyId = user.CompanyId, Role = UserRole.Employee,
            IsActive = true, EmailConfirmed = true,
            BirthDate = model.BirthDate,
            Gender = model.Gender ?? "",
            CivilStatus = model.CivilStatus ?? "",
            BloodType = model.BloodType ?? "",
            Address = model.Address ?? "",
            ContactNumber = model.PhoneNumber ?? "",
            EmergencyContactName = model.EmergencyContactName ?? "",
            EmergencyContactRelation = model.EmergencyContactRelation ?? "",
            EmergencyContactNumber = model.EmergencyContactNumber ?? "",
            DepartmentId = assignedDeptId,
            EmployeeNumber = employeeNumber,
            StartDate = DateTime.Today,
            DailyRate = defaultDailyRate,
            ShiftId = model.ShiftId,
            StaffCode = staffCode,
            KioskPin = kioskPin,
            MustChangePassword = true,
            RequiresFaceEnrollment = true
        };

        // Generate QR code
        var qrHash = _qrCodeService.GenerateUniqueCode(Guid.NewGuid().ToString());
        employeeUser.QRCodeHash = qrHash;
        employeeUser.QRCodeGeneratedAt = DateTime.UtcNow;

        var userResult = await _userManager.CreateAsync(employeeUser, tempPassword);
        if (!userResult.Succeeded)
        {
            foreach (var error in userResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            await LoadDropdowns(user.CompanyId ?? 0, user.DepartmentId);
            return View(model);
        }

        var employee = new Employee
        {
            EmployeeNumber = employeeNumber,
            FirstName = model.FirstName, MiddleName = model.MiddleName, LastName = model.LastName,
            Email = model.Email, ContactNumber = model.PhoneNumber, Address = model.Address,
            DateOfBirth = model.BirthDate, Gender = model.Gender ?? "",
            HireDate = DateTime.Today,
            CompanyId = user.CompanyId ?? 0,
            DepartmentId = assignedDeptId, ShiftId = model.ShiftId,
            BasicSalary = basicSalary,
            SalaryType = SalaryType.Monthly,
            DailyRate = defaultDailyRate,
            HourlyRate = defaultDailyRate / 8,
            UserId = employeeUser.Id, IsActive = true
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Send welcome email
        await _emailService.SendStaffWelcomeEmailAsync(
            model.Email,
            $"{model.FirstName} {model.LastName}",
            company?.CompanyName ?? "Your Company",
            "Employee",
            tempPassword);

        await _auditLogService.LogAsync(user.Id, "Created Employee Account", "Employee",
            employee.Id.ToString(), null,
            new { Email = model.Email, EmployeeNumber = employeeNumber, CompanyId = user.CompanyId },
            user.CompanyId);

        TempData["Success"] = $"Employee {model.FirstName} {model.LastName} created (#{employeeNumber}). Welcome email sent to {model.Email}.";
        return RedirectToAction(nameof(MyTeam));
    }

    // GET: /HR/Attendance  →  Views/HR/Attendance.cshtml
    public async Task<IActionResult> Attendance(string? date = null)
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var selectedDate = date != null && DateTime.TryParse(date, out var parsed)
            ? parsed.Date : DateTime.Today;

        var deptId = user.DepartmentId;

        var attendanceRecords = await _context.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.Employee.Department)
            .Where(a => a.Employee.CompanyId == user.CompanyId && a.Date.Date == selectedDate
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .OrderBy(a => a.Employee.LastName).ToListAsync();

        var allEmployees = await _context.Employees
            .Include(e => e.User).Include(e => e.Department).Include(e => e.Shift)
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId)).ToListAsync();

        var onLeaveToday = await _context.Leaves
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Where(l => l.Employee.CompanyId == user.CompanyId &&
                        l.Status == LeaveStatus.Approved &&
                        l.StartDate <= selectedDate && l.EndDate >= selectedDate
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .ToListAsync();

        var shifts = await _context.Shifts
            .Where(s => s.CompanyId == user.CompanyId && s.IsActive).ToListAsync();

        ViewBag.AttendanceRecords = attendanceRecords;
        ViewBag.AllEmployees      = allEmployees;
        ViewBag.PresentCount      = attendanceRecords.Count(a => a.TimeIn != null);
        ViewBag.LateCount         = attendanceRecords.Count(a => a.LateMinutes > 0);
        ViewBag.AbsentCount       = allEmployees.Count - attendanceRecords.Count(a => a.TimeIn != null);
        ViewBag.OnLeaveCount      = onLeaveToday.Count;
        ViewBag.OnLeaveToday      = onLeaveToday;
        ViewBag.SelectedDate      = selectedDate;
        ViewBag.Shifts            = shifts;

        return View();
    }

    // POST: /HR/SaveAttendance — manual add or edit a single attendance record
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAttendance(
        int employeeId, string date,
        string? timeIn, string? timeOut,
        string status, string? remarks)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        if (!DateTime.TryParse(date, out var attendanceDate))
            return Json(new { success = false, message = "Invalid date." });

        // Verify employee belongs to HR's company (and team if scoped)
        var employee = await _context.Employees
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.CompanyId == user.CompanyId
                && (user.DepartmentId == null || e.DepartmentId == user.DepartmentId));

        if (employee == null)
            return Json(new { success = false, message = "Employee not found." });

        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == attendanceDate.Date);

        var isNew = attendance == null;
        attendance ??= new Attendance { EmployeeId = employeeId, Date = attendanceDate.Date };

        // Parse times
        DateTime? parsedTimeIn  = null;
        DateTime? parsedTimeOut = null;

        if (!string.IsNullOrWhiteSpace(timeIn) && TimeSpan.TryParse(timeIn, out var tin))
            parsedTimeIn = attendanceDate.Date.Add(tin);
        if (!string.IsNullOrWhiteSpace(timeOut) && TimeSpan.TryParse(timeOut, out var tout))
            parsedTimeOut = attendanceDate.Date.Add(tout);

        attendance.TimeIn  = parsedTimeIn;
        attendance.TimeOut = parsedTimeOut;
        attendance.Remarks = remarks;
        attendance.IsApproved = true;

        // Parse status
        attendance.Status = status switch
        {
            "Present"  => AttendanceStatus.Present,
            "Late"     => AttendanceStatus.Late,
            "Absent"   => AttendanceStatus.Absent,
            "OnLeave"  => AttendanceStatus.OnLeave,
            "HalfDay"  => AttendanceStatus.HalfDay,
            "Holiday"  => AttendanceStatus.Holiday,
            _          => AttendanceStatus.Present
        };

        // Recalculate late minutes
        attendance.LateMinutes = 0;
        attendance.LateDeductionAmount = 0;
        if (parsedTimeIn != null && employee.Shift != null)
        {
            var shiftStart = attendanceDate.Date.Add(employee.Shift.StartTime);
            var graceEnd   = shiftStart.AddMinutes(employee.Shift.GracePeriodMinutes);
            if (parsedTimeIn > graceEnd)
            {
                attendance.LateMinutes = (int)(parsedTimeIn.Value - shiftStart).TotalMinutes;
                attendance.Status      = AttendanceStatus.Late;
                var minuteRate = (employee.DailyRate ?? 0) / (8m * 60m);
                attendance.LateDeductionAmount = Math.Round(minuteRate * attendance.LateMinutes, 2);
            }
        }

        // Recalculate worked minutes
        attendance.WorkedMinutes = 0;
        if (parsedTimeIn != null && parsedTimeOut != null)
            attendance.WorkedMinutes = (int)(parsedTimeOut.Value - parsedTimeIn.Value).TotalMinutes;

        // Recalculate overtime minutes
        attendance.OvertimeMinutes = 0;
        attendance.OvertimeAmount  = 0;
        if (parsedTimeOut != null && employee.Shift != null)
        {
            var shiftEnd = attendanceDate.Date.Add(employee.Shift.EndTime);
            if (employee.Shift.IsNightShift && shiftEnd < attendanceDate.Date.Add(employee.Shift.StartTime))
                shiftEnd = shiftEnd.AddDays(1);
            if (parsedTimeOut > shiftEnd)
            {
                attendance.OvertimeMinutes = (int)(parsedTimeOut.Value - shiftEnd).TotalMinutes;
                var hourlyRate = employee.HourlyRate ?? ((employee.DailyRate ?? 0) / 8m);
                attendance.OvertimeAmount = Math.Round(hourlyRate * 0.25m * (attendance.OvertimeMinutes / 60m), 2);
            }
        }

        if (isNew)
            _context.Attendances.Add(attendance);

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id,
            isNew ? "Manual Attendance Entry" : "Attendance Correction",
            "Attendance", attendance.Id.ToString(), null,
            new { EmployeeId = employeeId, Date = date, TimeIn = timeIn, TimeOut = timeOut, Status = status },
            user.CompanyId);

        return Json(new
        {
            success    = true,
            message    = isNew ? "Attendance record added." : "Attendance record updated.",
            lateMinutes = attendance.LateMinutes,
            otMinutes  = attendance.OvertimeMinutes,
            workedMinutes = attendance.WorkedMinutes
        });
    }

    // POST: /HR/DeleteAttendance — remove an attendance record
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttendance(int employeeId, string date)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        if (!DateTime.TryParse(date, out var attendanceDate))
            return Json(new { success = false, message = "Invalid date." });

        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                && a.Date.Date == attendanceDate.Date
                && a.Employee.CompanyId == user.CompanyId);

        if (attendance == null)
            return Json(new { success = false, message = "Record not found." });

        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Attendance record deleted." });
    }

    // GET: /HR/Leaves  →  Views/HR/Leaves.cshtml
    public async Task<IActionResult> Leaves()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today        = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var deptId       = user.DepartmentId;

        var pendingLeaves = await _context.Leaves
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Include(l => l.Employee.Department)
            .Where(l => l.Employee.CompanyId == user.CompanyId && l.Status == LeaveStatus.Pending
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .OrderBy(l => l.StartDate).ToListAsync();

        var recentDecisions = await _context.Leaves
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Where(l => l.Employee.CompanyId == user.CompanyId &&
                        (l.Status == LeaveStatus.Approved || l.Status == LeaveStatus.Rejected)
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .OrderByDescending(l => l.UpdatedAt).Take(10).ToListAsync();

        ViewBag.PendingLeaves      = pendingLeaves;
        ViewBag.ApprovedThisMonth  = await _context.Leaves.CountAsync(l =>
            l.Employee.CompanyId == user.CompanyId && l.Status == LeaveStatus.Approved
            && l.CreatedAt >= startOfMonth
            && (deptId == null || l.Employee.DepartmentId == deptId));
        ViewBag.RejectedLeaves     = await _context.Leaves.CountAsync(l =>
            l.Employee.CompanyId == user.CompanyId && l.Status == LeaveStatus.Rejected
            && (deptId == null || l.Employee.DepartmentId == deptId));
        ViewBag.OnLeaveToday       = await _context.Leaves.CountAsync(l =>
            l.Employee.CompanyId == user.CompanyId && l.Status == LeaveStatus.Approved &&
            l.StartDate <= today && l.EndDate >= today
            && (deptId == null || l.Employee.DepartmentId == deptId));
        ViewBag.RecentDecisions    = recentDecisions;

        return View();
    }

    // GET: /HR/Payroll  →  Views/HR/Payroll.cshtml
    public async Task<IActionResult> Payroll()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today      = DateTime.Today;
        var deptId     = user.DepartmentId;
        var companyId  = user.CompanyId ?? 0;
        var periodStart = new DateTime(today.Year, today.Month, 1);
        var periodEnd   = periodStart.AddMonths(1).AddDays(-1);

        // Check if already submitted
        var existing = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.CompanyId == companyId
                                   && p.StartDate == periodStart
                                   && p.Status != PayrollStatus.Draft);
        ViewBag.AlreadySubmitted = existing != null;
        ViewBag.ExistingStatus   = existing?.Status.ToString();
        ViewBag.PeriodLabel      = periodStart.ToString("MMMM yyyy");

        // Get submitted periods for history
        var periods = await _context.PayrollPeriods
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.StartDate).Take(6).ToListAsync();
        ViewBag.Periods = periods;

        // Build preview from attendance (same logic as SubmitPayroll GET)
        var company   = await _context.Companies.FindAsync(companyId);
        var holidays  = await _context.Holidays
            .Where(h => h.CompanyId == companyId && h.Date >= periodStart && h.Date <= periodEnd)
            .ToListAsync();
        var holidayDates = holidays.Select(h => h.Date.Date).ToHashSet();

        var employees = await _context.Employees
            .Include(e => e.User).Include(e => e.Shift)
            .Where(e => e.CompanyId == companyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId))
            .ToListAsync();

        var attendance = await _context.Attendances
            .Where(a => a.Employee.CompanyId == companyId
                     && a.Date >= periodStart && a.Date <= today
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .ToListAsync();

        var approvedLeaves = await _context.Leaves
            .Where(l => l.Employee.CompanyId == companyId
                     && l.Status == LeaveStatus.Approved
                     && l.StartDate <= periodEnd && l.EndDate >= periodStart
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .ToListAsync();

        var workDaysInPeriod = Enumerable.Range(0, (today - periodStart).Days + 1)
            .Select(d => periodStart.AddDays(d))
            .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
            .ToList();
        var totalWorkDays = workDaysInPeriod.Count;

        var payrollItems = employees.Select(emp =>
        {
            var empAtt      = attendance.Where(a => a.EmployeeId == emp.Id).ToList();
            var dailyRate   = emp.DailyRate  ?? company?.DefaultDailyRate ?? 500m;
            var hourlyRate  = emp.HourlyRate ?? (dailyRate / 8m);
            var daysWorked  = (decimal)empAtt.Count(a => a.TimeIn != null);
            var lateMinutes = empAtt.Sum(a => a.LateMinutes);
            var otMinutes   = empAtt.Sum(a => a.OvertimeMinutes);
            var undertimeMinutes = empAtt
                .Where(a => a.TimeIn != null && a.TimeOut != null && a.WorkedMinutes > 0)
                .Sum(a => Math.Max(0, (8 * 60) - a.WorkedMinutes));
            var empLeaveDays = approvedLeaves
                .Where(l => l.EmployeeId == emp.Id && l.LeaveType != LeaveType.Unpaid)
                .Sum(l => l.TotalDays > 0 ? l.TotalDays
                    : (decimal)(Math.Min((l.EndDate - l.StartDate).Days + 1,
                        workDaysInPeriod.Count(d => d >= l.StartDate && d <= l.EndDate))));
            var absentDays = Math.Max(0, totalWorkDays - daysWorked - empLeaveDays);
            var holidayDaysWorked = empAtt.Where(a => a.TimeIn != null && holidayDates.Contains(a.Date.Date)).ToList();
            var regularHolidayDays = holidayDaysWorked.Count(a => holidays.Any(h => h.Date.Date == a.Date.Date && h.HolidayType == HolidayType.Regular));
            var specialHolidayDays = holidayDaysWorked.Count(a => holidays.Any(h => h.Date.Date == a.Date.Date && h.HolidayType == HolidayType.Special));
            var holidayPayRate = company?.HolidayPayRate ?? 200;
            var holidayPay = dailyRate * (holidayPayRate / 100m - 1m) * regularHolidayDays + dailyRate * 0.30m * specialHolidayDays;
            var basicPay        = dailyRate * daysWorked;
            var otPay           = (hourlyRate * 1.25m) * (otMinutes / 60m);
            var gross           = basicPay + otPay + holidayPay;
            var lateDeduct      = (hourlyRate / 60m) * lateMinutes;
            var undertimeDeduct = (hourlyRate / 60m) * undertimeMinutes;
            var absenceDeduct   = dailyRate * absentDays;
            var sss             = Math.Min(gross * 0.045m, 900m);
            var philhealth      = Math.Min(gross * 0.025m, 625m);
            var pagibig         = Math.Min(gross * 0.02m, 200m);
            var taxable         = gross - sss - philhealth - pagibig;
            var tax             = taxable > 33333m ? (taxable - 33333m) * 0.20m : 0m;
            var totalDeduct     = lateDeduct + undertimeDeduct + absenceDeduct + sss + philhealth + pagibig + tax;
            return new
            {
                EmployeeId      = emp.Id,
                FullName        = emp.FullName,
                DaysWorked      = daysWorked,
                OtHours         = Math.Round(otMinutes / 60m, 1),
                LateMinutes     = lateMinutes,
                AbsentDays      = absentDays,
                BasicPay        = Math.Round(basicPay, 2),
                OtPay           = Math.Round(otPay, 2),
                LateDeduct      = Math.Round(lateDeduct, 2),
                UndertimeDeduct = Math.Round(undertimeDeduct, 2),
                AbsenceDeduct   = Math.Round(absenceDeduct, 2),
                TotalDeduct     = Math.Round(totalDeduct, 2),
                NetPay          = Math.Round(gross - totalDeduct, 2),
                GrossPay        = Math.Round(gross, 2)
            };
        }).ToList();

        ViewBag.PayrollItems    = payrollItems;
        ViewBag.TotalBasicPay   = payrollItems.Sum(p => p.BasicPay);
        ViewBag.TotalOtPay      = payrollItems.Sum(p => p.OtPay);
        ViewBag.TotalDeduct     = payrollItems.Sum(p => p.TotalDeduct);
        ViewBag.TotalNetPay     = payrollItems.Sum(p => p.NetPay);
        ViewBag.TotalSss        = payrollItems.Sum(p => (decimal)p.LateDeduct); // placeholder — real SSS in submit
        ViewBag.TotalPhilHealth = 0m;
        ViewBag.TotalPagIbig    = 0m;
        ViewBag.TotalTax        = 0m;

        return View();
    }

    // GET: /HR/BudgetTracker  →  Views/HR/BudgetTracker.cshtml
    public async Task<IActionResult> BudgetTracker()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /HR/OvertimeApproval — list pending overtime records for HR's team
    [HttpGet]
    public async Task<IActionResult> OvertimeApproval()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var deptId = user.DepartmentId;

        var pending = await _context.Overtimes
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Include(o => o.Employee.Department)
            .Where(o => o.Employee.CompanyId == user.CompanyId
                     && o.Status == LeaveStatus.Pending
                     && (deptId == null || o.Employee.DepartmentId == deptId))
            .OrderByDescending(o => o.Date)
            .ToListAsync();

        var recent = await _context.Overtimes
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Where(o => o.Employee.CompanyId == user.CompanyId
                     && (o.Status == LeaveStatus.Approved || o.Status == LeaveStatus.Rejected)
                     && (deptId == null || o.Employee.DepartmentId == deptId))
            .OrderByDescending(o => o.ApprovedAt ?? o.CreatedAt)
            .Take(20)
            .ToListAsync();

        ViewBag.PendingOvertimes = pending;
        ViewBag.RecentOvertimes  = recent;
        return View();
    }

    // POST: /HR/ApproveOvertime
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOvertime(int id)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var ot = await _context.Overtimes
            .Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.Id == id && o.Employee.CompanyId == user.CompanyId);

        if (ot == null) return Json(new { success = false, message = "Overtime record not found." });
        if (ot.Status != LeaveStatus.Pending) return Json(new { success = false, message = "Already processed." });

        ot.Status     = LeaveStatus.Approved;
        ot.ApprovedAt = DateTime.UtcNow;

        // Sync back to Attendance record so payroll picks it up
        var att = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == ot.EmployeeId && a.Date.Date == ot.Date.Date);
        if (att != null)
        {
            att.OvertimeMinutes = ot.TotalMinutes;
            var hourlyRate = ot.Employee.HourlyRate ?? ((ot.Employee.DailyRate ?? 0) / 8m);
            att.OvertimeAmount = Math.Round(hourlyRate * 0.25m * (ot.TotalMinutes / 60m), 2);
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, message = $"Overtime approved: {ot.TotalMinutes / 60.0:F1} hrs on {ot.Date:MMM dd}." });
    }

    // POST: /HR/RejectOvertime
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOvertime(int id, string? reason)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var ot = await _context.Overtimes
            .Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.Id == id && o.Employee.CompanyId == user.CompanyId);

        if (ot == null) return Json(new { success = false, message = "Overtime record not found." });

        ot.Status     = LeaveStatus.Rejected;
        ot.ApprovedAt = DateTime.UtcNow;

        // Zero out attendance overtime if rejected
        var att = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == ot.EmployeeId && a.Date.Date == ot.Date.Date);
        if (att != null)
        {
            att.OvertimeMinutes = 0;
            att.OvertimeAmount  = 0;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Overtime rejected." });
    }

    // GET: /HR/SubmitPayroll  →  Views/HR/SubmitPayroll.cshtml
    public async Task<IActionResult> SubmitPayroll()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today      = DateTime.Today;
        var deptId     = user.DepartmentId;
        var companyId  = user.CompanyId ?? 0;

        // Get current month period dates
        var periodStart = new DateTime(today.Year, today.Month, 1);
        var periodEnd   = periodStart.AddMonths(1).AddDays(-1);
        var payDate     = periodEnd.AddDays(5);

        // Check if already submitted this period
        var existing = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.CompanyId == companyId
                                   && p.StartDate == periodStart
                                   && p.Status != PayrollStatus.Draft);
        ViewBag.AlreadySubmitted = existing != null;
        ViewBag.ExistingStatus   = existing?.Status.ToString();

        // Get employees in HR's team
        var employees = await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Shift)
            .Where(e => e.CompanyId == companyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId))
            .ToListAsync();

        // Get attendance for this period
        var attendance = await _context.Attendances
            .Where(a => a.Employee.CompanyId == companyId
                     && a.Date >= periodStart && a.Date <= today
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .ToListAsync();

        var company   = await _context.Companies.FindAsync(companyId);
        var holidays  = await _context.Holidays
            .Where(h => h.CompanyId == companyId && h.Date >= periodStart && h.Date <= periodEnd)
            .ToListAsync();
        var holidayDates = holidays.Select(h => h.Date.Date).ToHashSet();

        var approvedLeaves = await _context.Leaves
            .Where(l => l.Employee.CompanyId == companyId
                     && l.Status == LeaveStatus.Approved
                     && l.StartDate <= periodEnd && l.EndDate >= periodStart
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .ToListAsync();

        var workDaysInPeriod = Enumerable.Range(0, (today - periodStart).Days + 1)
            .Select(d => periodStart.AddDays(d))
            .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
            .ToList();
        var totalWorkDays = workDaysInPeriod.Count;

        // Build payroll preview per employee
        var payrollItems = employees.Select(emp =>
        {
            var empAtt      = attendance.Where(a => a.EmployeeId == emp.Id).ToList();
            var dailyRate   = emp.DailyRate  ?? company?.DefaultDailyRate ?? 500m;
            var hourlyRate  = emp.HourlyRate ?? (dailyRate / 8m);
            var daysWorked  = (decimal)empAtt.Count(a => a.TimeIn != null);
            var lateMinutes = empAtt.Sum(a => a.LateMinutes);
            var otMinutes   = empAtt.Sum(a => a.OvertimeMinutes);
            var undertimeMinutes = empAtt
                .Where(a => a.TimeIn != null && a.TimeOut != null && a.WorkedMinutes > 0)
                .Sum(a => Math.Max(0, (8 * 60) - a.WorkedMinutes));

            var empLeaveDays = approvedLeaves
                .Where(l => l.EmployeeId == emp.Id && l.LeaveType != LeaveType.Unpaid)
                .Sum(l => l.TotalDays > 0 ? l.TotalDays
                    : (decimal)(Math.Min((l.EndDate - l.StartDate).Days + 1,
                        workDaysInPeriod.Count(d => d >= l.StartDate && d <= l.EndDate))));
            var absentDays = Math.Max(0, totalWorkDays - daysWorked - empLeaveDays);

            var holidayDaysWorked = empAtt.Where(a => a.TimeIn != null && holidayDates.Contains(a.Date.Date)).ToList();
            var regularHolidayDays = holidayDaysWorked.Count(a => holidays.Any(h => h.Date.Date == a.Date.Date && h.HolidayType == HolidayType.Regular));
            var specialHolidayDays = holidayDaysWorked.Count(a => holidays.Any(h => h.Date.Date == a.Date.Date && h.HolidayType == HolidayType.Special));
            var holidayPayRate = company?.HolidayPayRate ?? 200;
            var holidayPay = dailyRate * (holidayPayRate / 100m - 1m) * regularHolidayDays
                           + dailyRate * 0.30m * specialHolidayDays;

            var basicPay        = dailyRate * daysWorked;
            var otPay           = (hourlyRate * 1.25m) * (otMinutes / 60m);
            var gross           = basicPay + otPay + holidayPay;
            var lateDeduct      = (hourlyRate / 60m) * lateMinutes;
            var undertimeDeduct = (hourlyRate / 60m) * undertimeMinutes;
            var absenceDeduct   = dailyRate * absentDays;
            var sss             = Math.Min(gross * 0.045m, 900m);
            var philhealth      = Math.Min(gross * 0.025m, 625m);
            var pagibig         = Math.Min(gross * 0.02m, 200m);
            var taxable         = gross - sss - philhealth - pagibig;
            var tax             = taxable > 33333m ? (taxable - 33333m) * 0.20m : 0m;
            var totalDeduct     = lateDeduct + undertimeDeduct + absenceDeduct + sss + philhealth + pagibig + tax;

            return new
            {
                EmployeeId      = emp.Id,
                FullName        = emp.FullName,
                DaysWorked      = daysWorked,
                OtHours         = Math.Round(otMinutes / 60m, 1),
                LateMinutes     = lateMinutes,
                UndertimeMinutes = undertimeMinutes,
                AbsentDays      = absentDays,
                HolidayPay      = Math.Round(holidayPay, 2),
                BasicPay        = Math.Round(basicPay, 2),
                OtPay           = Math.Round(otPay, 2),
                LateDeduct      = Math.Round(lateDeduct, 2),
                UndertimeDeduct = Math.Round(undertimeDeduct, 2),
                AbsenceDeduct   = Math.Round(absenceDeduct, 2),
                Sss             = Math.Round(sss, 2),
                PhilHealth      = Math.Round(philhealth, 2),
                PagIbig         = Math.Round(pagibig, 2),
                Tax             = Math.Round(tax, 2),
                TotalDeduct     = Math.Round(totalDeduct, 2),
                NetPay          = Math.Round(gross - totalDeduct, 2),
                GrossPay        = Math.Round(gross, 2)
            };
        }).ToList();

        ViewBag.PayrollItems  = payrollItems;
        ViewBag.PeriodStart   = periodStart;
        ViewBag.PeriodEnd     = periodEnd;
        ViewBag.PayDate       = payDate;
        ViewBag.TotalEmployees = employees.Count;
        ViewBag.TotalBasicPay = payrollItems.Sum(p => p.BasicPay);
        ViewBag.TotalOtPay    = payrollItems.Sum(p => p.OtPay);
        ViewBag.TotalDeduct   = payrollItems.Sum(p => p.TotalDeduct);
        ViewBag.TotalNetPay   = payrollItems.Sum(p => p.NetPay);
        ViewBag.TotalSss      = payrollItems.Sum(p => p.Sss);
        ViewBag.TotalPhilHealth = payrollItems.Sum(p => p.PhilHealth);
        ViewBag.TotalPagIbig  = payrollItems.Sum(p => p.PagIbig);
        ViewBag.TotalTax      = payrollItems.Sum(p => p.Tax);

        return View();
    }

    // POST: /HR/SubmitPayroll — creates PayrollPeriod + Payroll records, sets status to Processed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitPayroll(string? notes)
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today     = DateTime.Today;
        var deptId    = user.DepartmentId;
        var companyId = user.CompanyId ?? 0;

        var periodStart = new DateTime(today.Year, today.Month, 1);
        var periodEnd   = periodStart.AddMonths(1).AddDays(-1);

        // Prevent duplicate submission
        var existing = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.CompanyId == companyId
                                   && p.StartDate == periodStart
                                   && p.Status != PayrollStatus.Draft);
        if (existing != null)
        {
            TempData["Warning"] = $"Payroll for {periodStart:MMMM yyyy} was already submitted (Status: {existing.Status}).";
            return RedirectToAction(nameof(Payroll));
        }

        var company   = await _context.Companies.FindAsync(companyId);
        var holidays  = await _context.Holidays
            .Where(h => h.CompanyId == companyId && h.Date >= periodStart && h.Date <= periodEnd)
            .ToListAsync();

        // Create PayrollPeriod
        var period = new PayrollPeriod
        {
            CompanyId  = companyId,
            PeriodName = $"{periodStart:MMMM yyyy} Payroll",
            StartDate  = periodStart,
            EndDate    = periodEnd,
            PayDate    = periodEnd.AddDays(5),
            Status     = PayrollStatus.Processed
        };
        _context.PayrollPeriods.Add(period);
        await _context.SaveChangesAsync();

        // Get employees
        var employees = await _context.Employees
            .Include(e => e.Shift)
            .Where(e => e.CompanyId == companyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId))
            .ToListAsync();

        // Get attendance for the period
        var attendance = await _context.Attendances
            .Where(a => a.Employee.CompanyId == companyId
                     && a.Date >= periodStart && a.Date <= today
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .ToListAsync();

        // Get approved leaves for the period
        var approvedLeaves = await _context.Leaves
            .Where(l => l.Employee.CompanyId == companyId
                     && l.Status == LeaveStatus.Approved
                     && l.StartDate <= periodEnd && l.EndDate >= periodStart
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .ToListAsync();

        // Build set of working days in the period (Mon–Fri, excluding holidays)
        var holidayDates = holidays.Select(h => h.Date.Date).ToHashSet();
        var workDaysInPeriod = Enumerable.Range(0, (today - periodStart).Days + 1)
            .Select(d => periodStart.AddDays(d))
            .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
            .ToList();
        var totalWorkDays = workDaysInPeriod.Count;

        // Create Payroll records for each employee
        foreach (var emp in employees)
        {
            var empAtt      = attendance.Where(a => a.EmployeeId == emp.Id).ToList();
            var dailyRate   = emp.DailyRate  ?? company?.DefaultDailyRate ?? 500m;
            var hourlyRate  = emp.HourlyRate ?? (dailyRate / 8m);

            // Days actually worked (TimeIn recorded)
            var daysWorked  = (decimal)empAtt.Count(a => a.TimeIn != null);

            // Late & overtime from attendance
            var lateMinutes = empAtt.Sum(a => a.LateMinutes);
            var otMinutes   = empAtt.Sum(a => a.OvertimeMinutes);

            // Undertime: employee clocked out early (worked < 8h on days they came in)
            var undertimeMinutes = empAtt
                .Where(a => a.TimeIn != null && a.TimeOut != null && a.WorkedMinutes > 0)
                .Sum(a => Math.Max(0, (8 * 60) - a.WorkedMinutes));

            // Absent days = total work days - days worked - approved leave days
            var empLeaveDays = approvedLeaves
                .Where(l => l.EmployeeId == emp.Id && l.LeaveType != LeaveType.Unpaid)
                .Sum(l => l.TotalDays > 0 ? l.TotalDays
                    : (decimal)(Math.Min((l.EndDate - l.StartDate).Days + 1,
                        workDaysInPeriod.Count(d => d >= l.StartDate && d <= l.EndDate))));
            var absentDays = Math.Max(0, totalWorkDays - daysWorked - empLeaveDays);

            // Holiday pay: days worked on a holiday
            var holidayDaysWorked = empAtt
                .Where(a => a.TimeIn != null && holidayDates.Contains(a.Date.Date))
                .ToList();
            var regularHolidayDays = holidayDaysWorked
                .Count(a => holidays.Any(h => h.Date.Date == a.Date.Date && h.HolidayType == HolidayType.Regular));
            var specialHolidayDays = holidayDaysWorked
                .Count(a => holidays.Any(h => h.Date.Date == a.Date.Date && h.HolidayType == HolidayType.Special));

            // Holiday pay = extra pay on top of regular daily rate
            // Regular holiday: 200% total = 100% extra; Special: 130% total = 30% extra
            var holidayPayRate = company?.HolidayPayRate ?? 200;
            var regularHolidayExtra = dailyRate * (holidayPayRate / 100m - 1m) * regularHolidayDays;
            var specialHolidayExtra = dailyRate * 0.30m * specialHolidayDays;
            var holidayPay = regularHolidayExtra + specialHolidayExtra;

            // Earnings
            var basicPay       = dailyRate * daysWorked;
            var otPay          = (hourlyRate * 1.25m) * (otMinutes / 60m);
            var gross          = basicPay + otPay + holidayPay;

            // Deductions
            var lateDeduct     = (hourlyRate / 60m) * lateMinutes;
            var undertimeDeduct = (hourlyRate / 60m) * undertimeMinutes;
            var absenceDeduct  = dailyRate * absentDays;
            var sss            = Math.Min(gross * 0.045m, 900m);
            var philhealth     = Math.Min(gross * 0.025m, 625m);
            var pagibig        = Math.Min(gross * 0.02m, 200m);
            var taxable        = gross - sss - philhealth - pagibig;
            var tax            = taxable > 33333m ? (taxable - 33333m) * 0.20m : 0m;
            var totalDeduct    = lateDeduct + undertimeDeduct + absenceDeduct + sss + philhealth + pagibig + tax;

            _context.Payrolls.Add(new Payroll
            {
                EmployeeId             = emp.Id,
                PayrollPeriodId        = period.Id,
                BasicPay               = Math.Round(basicPay, 2),
                OvertimePay            = Math.Round(otPay, 2),
                HolidayPay             = Math.Round(holidayPay, 2),
                GrossPay               = Math.Round(gross, 2),
                LateDeduction          = Math.Round(lateDeduct, 2),
                UndertimeDeduction     = Math.Round(undertimeDeduct, 2),
                AbsenceDeduction       = Math.Round(absenceDeduct, 2),
                SSSContribution        = Math.Round(sss, 2),
                PhilHealthContribution = Math.Round(philhealth, 2),
                PagIbigContribution    = Math.Round(pagibig, 2),
                WithholdingTax         = Math.Round(tax, 2),
                TotalDeductions        = Math.Round(totalDeduct, 2),
                NetPay                 = Math.Round(gross - totalDeduct, 2),
                DaysWorked             = daysWorked,
                OvertimeHours          = Math.Round(otMinutes / 60m, 2),
                LateHours              = Math.Round(lateMinutes / 60m, 2),
                UndertimeHours         = Math.Round(undertimeMinutes / 60m, 2),
                AbsentDays             = absentDays,
                Status                 = PayrollStatus.Processed
            });
        }

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Submitted Payroll for Approval", "PayrollPeriod",
            period.Id.ToString(), null,
            new { Period = period.PeriodName, EmployeeCount = employees.Count, Notes = notes },
            user.CompanyId);

        TempData["Success"] = $"✅ Payroll for {period.PeriodName} submitted successfully! {employees.Count} employee records created. Waiting for Manager approval.";
        return RedirectToAction(nameof(Payroll));
    }

    // GET: /HR/IncidentReport  →  Views/HR/IncidentReport.cshtml
    public async Task<IActionResult> IncidentReport()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /HR/AuditLogs  →  Views/HR/AuditLogs.cshtml
    public async Task<IActionResult> AuditLogs(int page = 1, int days = 7, string? action = null, string? search = null)
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var fromDate = DateTime.UtcNow.AddDays(-days);
        var pageSize = 50;

        var query = _context.AuditLogs
            .Include(l => l.User)
            .Where(l => l.CompanyId == user.CompanyId && l.CreatedAt >= fromDate &&
                        (l.Action.Contains("Failed Login") || l.Action.Contains("Password") ||
                         l.Action.Contains("Security") || l.Action.Contains("Unauthorized")))
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(action))  query = query.Where(l => l.Action.Contains(action));
        if (!string.IsNullOrEmpty(search))  query = query.Where(l =>
            l.User != null && (l.User.FirstName.Contains(search) ||
                               l.User.LastName.Contains(search) ||
                               l.User.Email!.Contains(search)));

        var totalCount = await query.CountAsync();
        var logs       = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.Days        = days;
        ViewBag.Action      = action;
        ViewBag.Search      = search;

        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> GetLogDetails(int id)
    {
        var user = await GetHRUser();
        if (user == null) return Unauthorized();

        var log = await _context.AuditLogs.Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id && l.CompanyId == user.CompanyId);

        if (log == null) return NotFound();

        return Json(new
        {
            action    = log.Action,
            createdAt = log.CreatedAt,
            details   = log.NewValues,
            userName  = log.User?.FullName,
            userEmail = log.User?.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var leave = await _context.Leaves.Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id && l.Employee.CompanyId == user.CompanyId);

        if (leave == null)
            return Json(new { success = false, message = "Leave request not found" });

        if (leave.Status != LeaveStatus.Pending)
            return Json(new { success = false, message = "Leave request already processed" });

        leave.Status     = LeaveStatus.Approved;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.UpdatedAt  = DateTime.UtcNow;

        // ── Auto-deduct leave balance ──────────────────────────────────────
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == leave.EmployeeId && b.Year == DateTime.Today.Year);

        if (balance == null)
        {
            // Create balance record if missing (15 days each by default)
            balance = new LeaveBalance
            {
                EmployeeId             = leave.EmployeeId,
                Year                   = DateTime.Today.Year,
                VacationLeaveBalance   = 15,
                SickLeaveBalance       = 15,
                VacationLeaveUsed      = 0,
                SickLeaveUsed          = 0
            };
            _context.LeaveBalances.Add(balance);
        }

        var days = leave.TotalDays > 0 ? leave.TotalDays
                   : (decimal)(leave.EndDate - leave.StartDate).TotalDays + 1;

        if (leave.LeaveType == LeaveType.Vacation || leave.LeaveType == LeaveType.Emergency)
        {
            balance.VacationLeaveBalance = Math.Max(0, balance.VacationLeaveBalance - days);
            balance.VacationLeaveUsed   += days;
        }
        else if (leave.LeaveType == LeaveType.Sick)
        {
            balance.SickLeaveBalance = Math.Max(0, balance.SickLeaveBalance - days);
            balance.SickLeaveUsed   += days;
        }
        // Unpaid/Maternity/Paternity — no balance deduction

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Approved Leave Request", "Leave",
            leave.Id.ToString(), null,
            new { LeaveId = leave.Id, EmployeeId = leave.EmployeeId, LeaveType = leave.LeaveType.ToString(), Days = days },
            user.CompanyId);

        return Json(new { success = true, message = $"Leave approved. {days} day(s) deducted from balance." });
    }

    [HttpPost]
    public async Task<IActionResult> RejectLeave(int id, [FromBody] RejectLeaveRequest request)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var leave = await _context.Leaves.Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id && l.Employee.CompanyId == user.CompanyId);

        if (leave == null)
            return Json(new { success = false, message = "Leave request not found" });

        if (leave.Status != LeaveStatus.Pending)
            return Json(new { success = false, message = "Leave request already processed" });

        leave.Status           = LeaveStatus.Rejected;
        leave.ApproverRemarks  = request.Reason;
        leave.UpdatedAt        = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Rejected Leave Request", "Leave",
            leave.Id.ToString(), null,
            new { LeaveId = leave.Id, EmployeeId = leave.EmployeeId, Reason = request.Reason },
            user.CompanyId);

        return Json(new { success = true, message = "Leave rejected" });
    }

    // GET: /HR/Overtime — view pending overtime requests
    public async Task<IActionResult> Overtime()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var deptId = user.DepartmentId;

        var pending = await _context.Overtimes
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Include(o => o.Employee.Department)
            .Where(o => o.Employee.CompanyId == user.CompanyId
                     && o.Status == LeaveStatus.Pending
                     && (deptId == null || o.Employee.DepartmentId == deptId))
            .OrderByDescending(o => o.Date)
            .ToListAsync();

        var approved = await _context.Overtimes
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Where(o => o.Employee.CompanyId == user.CompanyId
                     && o.Status == LeaveStatus.Approved
                     && (deptId == null || o.Employee.DepartmentId == deptId))
            .OrderByDescending(o => o.Date).Take(20)
            .ToListAsync();

        ViewBag.PendingOT  = pending;
        ViewBag.ApprovedOT = approved;
        return View();
    }

    // POST: /HR/ApproveOvertime
    [HttpPost]
    public async Task<IActionResult> ApproveOvertime(int id)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var ot = await _context.Overtimes.Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.Id == id && o.Employee.CompanyId == user.CompanyId);

        if (ot == null) return Json(new { success = false, message = "Overtime record not found" });
        if (ot.Status != LeaveStatus.Pending) return Json(new { success = false, message = "Already processed" });

        ot.Status      = LeaveStatus.Approved;
        ot.ApprovedAt  = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Sync back to attendance record
        var att = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == ot.EmployeeId && a.Date.Date == ot.Date.Date);
        if (att != null)
        {
            att.OvertimeMinutes = ot.TotalMinutes;
            var hourlyRate = ot.Employee.HourlyRate ?? ((ot.Employee.DailyRate ?? 0) / 8m);
            att.OvertimeAmount = Math.Round(hourlyRate * 0.25m * (ot.TotalMinutes / 60m), 2);
            await _context.SaveChangesAsync();
        }

        await _auditLogService.LogAsync(user.Id, "Approved Overtime", "Overtime",
            ot.Id.ToString(), null,
            new { EmployeeId = ot.EmployeeId, Date = ot.Date, Minutes = ot.TotalMinutes },
            user.CompanyId);

        return Json(new { success = true, message = $"Overtime approved: {ot.TotalMinutes} minutes on {ot.Date:MMM dd}" });
    }

    // POST: /HR/RejectOvertime
    [HttpPost]
    public async Task<IActionResult> RejectOvertime(int id)
    {
        var user = await GetHRUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var ot = await _context.Overtimes.Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.Id == id && o.Employee.CompanyId == user.CompanyId);

        if (ot == null) return Json(new { success = false, message = "Overtime record not found" });

        ot.Status = LeaveStatus.Rejected;
        await _context.SaveChangesAsync();

        // Zero out attendance OT if rejected
        var att = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == ot.EmployeeId && a.Date.Date == ot.Date.Date);
        if (att != null)
        {
            att.OvertimeMinutes = 0;
            att.OvertimeAmount  = 0;
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, message = "Overtime rejected." });
    }

    // POST: /HR/UpdateShift — HR can update shift for employees in their own team only
    // HR/Accountant in same team are visible but NOT editable by HR
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShift(string userId, int shiftId)
    {
        var hr = await GetHRUser();
        if (hr == null) return Json(new { success = false, message = "Unauthorized" });

        // Load the target user
        var target = await _context.Users.FirstOrDefaultAsync(u =>
            u.Id == userId && u.CompanyId == hr.CompanyId);

        if (target == null)
            return Json(new { success = false, message = "User not found" });

        // HR can only edit Employees — not HR/Accountant
        if (target.Role != UserRole.Employee)
            return Json(new { success = false, message = "HR can only change shift for Employees." });

        // Must be in same team
        if (hr.DepartmentId.HasValue && target.DepartmentId != hr.DepartmentId)
            return Json(new { success = false, message = "You can only edit staff in your own team." });

        // Verify shift belongs to same company
        var shift = await _context.Shifts.FirstOrDefaultAsync(s =>
            s.Id == shiftId && s.CompanyId == hr.CompanyId);
        if (shift == null)
            return Json(new { success = false, message = "Invalid shift." });

        // Update ApplicationUser
        target.ShiftId = shiftId;
        await _userManager.UpdateAsync(target);

        // Update Employee record if exists
        var emp = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        if (emp != null)
        {
            emp.ShiftId = shiftId;
            await _context.SaveChangesAsync();
        }

        await _auditLogService.LogAsync(hr.Id, "Updated Employee Shift", "ApplicationUser",
            userId, null, new { ShiftId = shiftId, ShiftName = shift.ShiftName }, hr.CompanyId);

        return Json(new { success = true, shiftName = shift.ShiftName });
    }

    private async Task LoadDropdowns(int companyId, int? hrDepartmentId = null)
    {
        // HR can only assign employees to their own team
        var deptQuery = _context.Departments.Where(d => d.CompanyId == companyId && d.IsActive);
        if (hrDepartmentId.HasValue)
            deptQuery = deptQuery.Where(d => d.Id == hrDepartmentId.Value);

        ViewBag.Departments = await deptQuery.ToListAsync();

        // Load shifts — auto-create defaults if none exist for this company
        var shifts = await _context.Shifts
            .Where(s => s.CompanyId == companyId && s.IsActive).ToListAsync();

        if (!shifts.Any())
        {
            var morning = new Shift
            {
                CompanyId = companyId, ShiftName = "Morning Shift (10AM-7PM)",
                StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(19, 0, 0),
                BreakStart = new TimeSpan(13, 0, 0), BreakEnd = new TimeSpan(14, 0, 0),
                GracePeriodMinutes = 15, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            var night = new Shift
            {
                CompanyId = companyId, ShiftName = "Night Shift (10PM-7AM)",
                StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(7, 0, 0),
                IsNightShift = true, GracePeriodMinutes = 15, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            _context.Shifts.AddRange(morning, night);
            await _context.SaveChangesAsync();
            shifts = new List<Shift> { morning, night };
        }

        ViewBag.Shifts = shifts;
    }
}

public class CreateEmployeeViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = "Male";
    public string? CivilStatus { get; set; }
    public string? BloodType { get; set; }
    public int? DepartmentId { get; set; }
    public int? ShiftId { get; set; }
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? EmergencyContactNumber { get; set; }
}

public class RejectLeaveRequest
{
    public string Reason { get; set; } = string.Empty;
}
