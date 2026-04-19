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
    public async Task<IActionResult> Attendance()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var today  = DateTime.Today;
        var deptId = user.DepartmentId;

        var attendanceRecords = await _context.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.Employee.Department)
            .Where(a => a.Employee.CompanyId == user.CompanyId && a.Date.Date == today
                     && (deptId == null || a.Employee.DepartmentId == deptId))
            .OrderBy(a => a.Employee.LastName).ToListAsync();

        var allEmployees = await _context.Employees
            .Include(e => e.User).Include(e => e.Department)
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive
                     && (deptId == null || e.DepartmentId == deptId)).ToListAsync();

        var onLeaveToday = await _context.Leaves
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Where(l => l.Employee.CompanyId == user.CompanyId &&
                        l.Status == LeaveStatus.Approved &&
                        l.StartDate <= today && l.EndDate >= today
                     && (deptId == null || l.Employee.DepartmentId == deptId))
            .ToListAsync();

        ViewBag.AttendanceRecords = attendanceRecords;
        ViewBag.AllEmployees      = allEmployees;
        ViewBag.PresentCount      = attendanceRecords.Count(a => a.TimeIn != null);
        ViewBag.LateCount         = attendanceRecords.Count(a => a.LateMinutes > 0);
        ViewBag.AbsentCount       = allEmployees.Count - attendanceRecords.Count(a => a.TimeIn != null);
        ViewBag.OnLeaveCount      = onLeaveToday.Count;
        ViewBag.OnLeaveToday      = onLeaveToday;
        ViewBag.SelectedDate      = today;

        return View();
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
        return View();
    }

    // GET: /HR/BudgetTracker  →  Views/HR/BudgetTracker.cshtml
    public async Task<IActionResult> BudgetTracker()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /HR/SubmitPayroll  →  Views/HR/SubmitPayroll.cshtml
    public async Task<IActionResult> SubmitPayroll()
    {
        var user = await GetHRUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
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

        leave.Status      = LeaveStatus.Approved;
        leave.ApprovedAt  = DateTime.UtcNow;
        leave.UpdatedAt   = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Approved Leave Request", "Leave",
            leave.Id.ToString(), null,
            new { LeaveId = leave.Id, EmployeeId = leave.EmployeeId, LeaveType = leave.LeaveType.ToString() },
            user.CompanyId);

        return Json(new { success = true, message = "Leave approved" });
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
