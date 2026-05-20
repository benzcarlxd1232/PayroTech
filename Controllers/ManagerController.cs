using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Models.ViewModels;
using PayroTech.Services;

namespace PayroTech.Controllers;

[Authorize]
public class ManagerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGeneratorService _passwordGenerator;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;
    private readonly IQRCodeService _qrCodeService;

    public ManagerController(
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

    private async Task<ApplicationUser?> GetManagerUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.Role != UserRole.CompanyAdmin && user?.Role != UserRole.Supervisor)
            return null;
        return user;
    }

    // GET: /Manager/Index  →  Views/Manager/Index.cshtml
    public async Task<IActionResult> Index()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        // Force password change if still on temporary password
        if (user.MustChangePassword)
            return RedirectToAction("ChangePassword", "Auth");

        var company = await _context.Companies
            .Include(c => c.Employees).Include(c => c.Departments)
            .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

        if (company == null) return RedirectToAction("Index", "Home");

        if (!company.IsInitialSetupComplete)
            return RedirectToAction("InitialSetup");

        var departments = await _context.Departments
            .Where(d => d.CompanyId == user.CompanyId && d.IsActive).ToListAsync();

        var viewModel = new ManagerDashboardViewModel
        {
            CompanyName      = company.CompanyName,
            TotalTeams       = departments.Count,
            TotalEmployees   = await _context.Employees.CountAsync(e => e.CompanyId == user.CompanyId && e.IsActive),
            TotalStaff       = await _context.Users.CountAsync(u => u.CompanyId == user.CompanyId && u.IsActive
                                   && (u.Role == UserRole.Employee || u.Role == UserRole.HR || u.Role == UserRole.Accountant)),
            HRCount          = await _context.Users.CountAsync(u => u.CompanyId == user.CompanyId && u.Role == UserRole.HR && u.IsActive),
            AccountantCount  = await _context.Users.CountAsync(u => u.CompanyId == user.CompanyId && u.Role == UserRole.Accountant && u.IsActive),
            NeedsInitialSetup = false,
            TeamBudgets      = new List<TeamBudgetInfo>(),
            PendingPayrolls  = new List<PayrollSubmissionInfo>(),
            ActiveIncidents  = new List<IncidentInfo>()
        };

        foreach (var dept in departments)
        {
            viewModel.TeamBudgets.Add(new TeamBudgetInfo
            {
                TeamId         = dept.Id,
                TeamName       = dept.DepartmentName,
                EmployeeCount  = await _context.Employees.CountAsync(e => e.DepartmentId == dept.Id && e.IsActive),
                AllocatedBudget = 0,
                UsedBudget     = 0
            });
        }

        return View(viewModel);
    }

    // GET: /Manager/InitialSetup
    [HttpGet]
    public async Task<IActionResult> InitialSetup()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        if (company == null) return RedirectToAction("Index", "Home");

        // Already done — go to dashboard
        if (company.IsInitialSetupComplete && !user.RequiresFaceEnrollment)
            return RedirectToAction("Index");

        // If work schedule is done but face enrollment still pending, go straight to face enrollment
        if (company.IsInitialSetupComplete && user.RequiresFaceEnrollment)
            return RedirectToAction("EnrollFace", "Auth");

        var viewModel = new InitialSetupViewModel
        {
            CompanyId          = company.Id,
            CompanyName        = company.CompanyName,
            WorkDaysPerWeek    = company.WorkDaysPerWeek,
            WorkingDaysPerMonth = company.WorkingDaysPerMonth,
            WorkOnHolidays     = company.WorkOnHolidays,
            HolidayPayRate     = company.HolidayPayRate,
            OvertimeRatePerHour = company.OvertimeRatePerHour
        };

        return View(viewModel);
    }

    // POST: /Manager/InitialSetup — saves work schedule then goes to face enrollment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitialSetup(InitialSetupViewModel model)
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        if (company == null) return RedirectToAction("Index", "Home");

        // Save work schedule settings
        company.WorkDaysPerWeek     = model.WorkDaysPerWeek;
        company.WorkingDaysPerMonth = model.WorkingDaysPerMonth;
        company.WorkOnHolidays      = model.WorkOnHolidays;
        company.HolidayPayRate      = model.HolidayPayRate;
        company.OvertimeRatePerHour = model.OvertimeRatePerHour;
        company.IsInitialSetupComplete = true;
        await _context.SaveChangesAsync();

        // If manager still needs face enrollment, go there; otherwise go to dashboard
        if (user.RequiresFaceEnrollment)
            return RedirectToAction("EnrollFace", "Auth");

        return RedirectToAction("Index");
    }

    // GET: /Manager/Teams  →  Views/Manager/Teams.cshtml
    public async Task<IActionResult> Teams()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        if (user.MustChangePassword) return RedirectToAction("ChangePassword", "Auth");

        var departments = await _context.Departments
            .Where(d => d.CompanyId == user.CompanyId && d.IsActive)
            .Include(d => d.Employees)
            .ToListAsync();

        ViewBag.Departments = departments;
        ViewBag.HasTeams = departments.Any();
        return View();
    }

    // POST: /Manager/CreateTeam
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeam(string teamName, string? teamCode, string? description)
    {
        var user = await GetManagerUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        if (string.IsNullOrWhiteSpace(teamName))
            return Json(new { success = false, message = "Team name is required" });

        var code = string.IsNullOrWhiteSpace(teamCode)
            ? new string(teamName.ToUpper().Where(char.IsLetter).Take(4).ToArray()).PadRight(3, 'X')
            : teamCode.ToUpper();

        // Ensure unique code within company
        var exists = await _context.Departments
            .AnyAsync(d => d.CompanyId == user.CompanyId && d.DepartmentCode == code);
        if (exists) code = code + DateTime.Now.Second.ToString();

        var dept = new Department
        {
            CompanyId      = user.CompanyId ?? 0,
            DepartmentName = teamName,
            DepartmentCode = code,
            IsActive       = true,
            CreatedAt      = DateTime.UtcNow
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Created Team", "Department",
            dept.Id.ToString(), null, new { Name = teamName }, user.CompanyId);

        return Json(new { success = true, message = $"Team '{teamName}' created successfully", teamId = dept.Id });
    }

    // POST: /Manager/DeleteTeam
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var user = await GetManagerUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var dept = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == user.CompanyId);

        if (dept == null) return Json(new { success = false, message = "Team not found" });

        dept.IsActive = false;
        dept.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Team deleted" });
    }

    // GET: /Manager/HRManagement  →  Views/Manager/HRManagement.cshtml
    public async Task<IActionResult> HRManagement()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var staffUsers = await _context.Users
            .Where(u => u.CompanyId == user.CompanyId && (u.Role == UserRole.HR || u.Role == UserRole.Accountant))
            .OrderBy(u => u.Role).ThenBy(u => u.LastName).ToListAsync();

        ViewBag.StaffUsers = staffUsers;
        return View();
    }

    // GET: /Manager/CreateStaff  →  Views/Manager/CreateStaff.cshtml
    [HttpGet]
    public async Task<IActionResult> CreateStaff()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        if (user.MustChangePassword) return RedirectToAction("ChangePassword", "Auth");

        ViewBag.Branches = await _context.Branches
            .Where(b => b.CompanyId == user.CompanyId && b.IsActive).ToListAsync();
        ViewBag.Departments = await _context.Departments
            .Where(d => d.CompanyId == user.CompanyId && d.IsActive).ToListAsync();
        ViewBag.Shifts = await EnsureShiftsAsync(user.CompanyId ?? 0);

        return View();
    }

    // POST: /Manager/CreateStaff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        ViewBag.Branches = await _context.Branches
            .Where(b => b.CompanyId == user.CompanyId && b.IsActive).ToListAsync();
        ViewBag.Departments = await _context.Departments
            .Where(d => d.CompanyId == user.CompanyId && d.IsActive).ToListAsync();
        ViewBag.Shifts = await EnsureShiftsAsync(user.CompanyId ?? 0);

        if (!ModelState.IsValid) return View(model);

        // Duplicate email check removed — allowed for testing

        // --- Team capacity check ---
        if (model.DepartmentId.HasValue && model.DepartmentId > 0)
        {
            var deptId = model.DepartmentId.Value;
            var targetRole = model.Role switch
            {
                "HR"         => UserRole.HR,
                "Accountant" => UserRole.Accountant,
                _            => UserRole.Employee
            };

            if (targetRole == UserRole.HR)
            {
                var hrCount = await _context.Users.CountAsync(u =>
                    u.DepartmentId == deptId && u.Role == UserRole.HR && u.IsActive);
                if (hrCount >= 3)
                {
                    ModelState.AddModelError("DepartmentId", "This team already has the maximum of 3 HR staff.");
                    return View(model);
                }
            }
            else if (targetRole == UserRole.Accountant)
            {
                var accCount = await _context.Users.CountAsync(u =>
                    u.DepartmentId == deptId && u.Role == UserRole.Accountant && u.IsActive);
                if (accCount >= 3)
                {
                    ModelState.AddModelError("DepartmentId", "This team already has the maximum of 3 Accountants.");
                    return View(model);
                }
            }
            else // Employee
            {
                var empCount = await _context.Employees.CountAsync(e =>
                    e.DepartmentId == deptId && e.IsActive);
                if (empCount >= 20)
                {
                    ModelState.AddModelError("DepartmentId", "This team has reached the maximum of 20 employees.");
                    return View(model);
                }
            }
        }

        // Always auto-generate a random secure temporary password
        var tempPassword = _passwordGenerator.GenerateRandomPassword(12);

        var role = model.Role switch
        {
            "HR"         => UserRole.HR,
            "Accountant" => UserRole.Accountant,
            _            => UserRole.Employee
        };

        // Always auto-generate employee number
        var empPrefix      = role == UserRole.HR ? "HR" : role == UserRole.Accountant ? "ACC" : "EMP";
        var employeeNumber = $"{empPrefix}-{user.CompanyId:D4}-{DateTime.Now:yyyyMMddHHmmss}";

        // Generate staff code (sequential, role-prefixed)
        var staffCode = await PayroTech.Utilities.StaffCodeGenerator.GenerateAsync(_context, user.CompanyId ?? 0, role);
        var kioskPin  = PayroTech.Utilities.StaffCodeGenerator.GetDefaultPin(staffCode);

        var faceImagePath = "";
        if (!string.IsNullOrEmpty(model.FaceImageBase64))
        {
            try
            {
                var base64Data = model.FaceImageBase64.Contains(",")
                    ? model.FaceImageBase64.Split(',')[1] : model.FaceImageBase64;
                var imageBytes = Convert.FromBase64String(base64Data);
                var fileName   = $"{Guid.NewGuid()}.jpg";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "faces");
                Directory.CreateDirectory(uploadPath);
                await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadPath, fileName), imageBytes);
                faceImagePath = $"/uploads/faces/{fileName}";
            }
            catch (Exception ex)
            {
                // Continue without face image — log for debugging
                System.Diagnostics.Debug.WriteLine($"Face image processing failed: {ex.Message}");
            }
        }

        var newUser = new ApplicationUser
        {
            UserName = model.Email, Email = model.Email,
            FirstName = model.FirstName, MiddleName = model.MiddleName, LastName = model.LastName,
            ContactNumber = model.ContactNumber, Gender = model.Gender,
            CivilStatus = model.CivilStatus, Address = model.Address,
            BloodType = model.BloodType,
            CompanyId = user.CompanyId,
            BranchId = model.BranchId > 0 ? model.BranchId : null,
            DepartmentId = model.DepartmentId > 0 ? model.DepartmentId : null,
            ShiftId = model.ShiftId,
            Role = role, IsActive = true, EmailConfirmed = true,
            BirthDate = model.BirthDate, DailyRate = model.DailyRate,
            EmployeeNumber = employeeNumber, StartDate = DateTime.Today,
            StaffCode = staffCode, KioskPin = kioskPin,
            EmergencyContactName = model.EmergencyContactName,
            EmergencyContactRelation = model.EmergencyContactRelation,
            EmergencyContactNumber = model.EmergencyContactNumber,
            FaceImagePath = faceImagePath,
            MustChangePassword = true,
            RequiresFaceEnrollment = true,  // Always require face enrollment for ID card
            IsFaceEnrolled = false
        };

        var result = await _userManager.CreateAsync(newUser, tempPassword);
        if (result.Succeeded)
        {
            var qrCodeHash = _qrCodeService.GenerateUniqueCode(newUser.Id);
            newUser.QRCodeHash = qrCodeHash;
            newUser.QRCodeGeneratedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(newUser);

            // Send welcome email with temporary password
            await _emailService.SendStaffWelcomeEmailAsync(
                model.Email,
                $"{model.FirstName} {model.LastName}",
                (await _context.Companies.FindAsync(user.CompanyId))?.CompanyName ?? "Your Company",
                role.ToString(),
                tempPassword);

            await _auditLogService.LogAsync(user.Id, $"Created {role} Account", "ApplicationUser",
                newUser.Id, null,
                new { Email = model.Email, Role = role.ToString(), CompanyId = user.CompanyId, EmployeeNumber = employeeNumber },
                user.CompanyId);

            // Auto-create leave balance for Employee role
            if (role == UserRole.Employee)
            {
                var empRecord = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == newUser.Id);
                if (empRecord != null)
                {
                    _context.LeaveBalances.Add(new LeaveBalance
                    {
                        EmployeeId           = empRecord.Id,
                        Year                 = DateTime.Today.Year,
                        VacationLeaveBalance = 15,
                        SickLeaveBalance     = 15,
                        VacationLeaveUsed    = 0,
                        SickLeaveUsed        = 0
                    });
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"{role} account created for {model.FirstName} {model.LastName}. Welcome email sent to {model.Email}.";
            TempData["ShowStaffID"] = newUser.Id;
            return RedirectToAction(nameof(AllStaff));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // GET: /Manager/AllStaff  →  Views/Manager/AllStaff.cshtml
    public async Task<IActionResult> AllStaff(
        int page = 1,
        string? role = null,
        int? teamId = null,
        string? search = null)
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        if (user.MustChangePassword) return RedirectToAction("ChangePassword", "Auth");

        const int pageSize = 15;

        // --- HR + Accountant (ApplicationUser) ---
        var staffQuery = _context.Users
            .Where(u => u.CompanyId == user.CompanyId && u.IsActive &&
                        (u.Role == UserRole.HR || u.Role == UserRole.Accountant))
            .Include(u => u.Department)
            .AsQueryable();

        // --- Employees (Employee entity) ---
        var empQuery = _context.Employees
            .Where(e => e.CompanyId == user.CompanyId && e.IsActive)
            .Include(e => e.Department)
            .Include(e => e.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            staffQuery = staffQuery.Where(u =>
                u.FirstName.Contains(search) || u.LastName.Contains(search) ||
                u.Email!.Contains(search) || u.EmployeeNumber.Contains(search));
            empQuery = empQuery.Where(e =>
                e.FirstName.Contains(search) || e.LastName.Contains(search) ||
                e.Email!.Contains(search) || e.EmployeeNumber.Contains(search));
        }

        if (teamId.HasValue && teamId > 0)
        {
            staffQuery = staffQuery.Where(u => u.DepartmentId == teamId);
            empQuery   = empQuery.Where(e => e.DepartmentId == teamId);
        }

        // Build unified list
        var staffRows = await staffQuery.Select(u => new StaffRow
        {
            Id             = u.Id,
            StaffCode      = u.StaffCode,
            FullName       = u.FirstName + " " + u.LastName,
            Email          = u.Email ?? "",
            RoleLabel      = u.Role == UserRole.HR ? "HR" : "Accountant",
            TeamName       = u.Department != null ? u.Department.DepartmentName : "—",
            TeamId         = u.DepartmentId,
            IsActive       = u.IsActive,
            StartDate      = u.StartDate,
            CurrentShiftId = u.ShiftId
        }).ToListAsync();

        var empRows = await empQuery.Select(e => new StaffRow
        {
            Id             = e.UserId ?? e.Id.ToString(),
            StaffCode      = e.User != null ? e.User.StaffCode : e.EmployeeNumber,
            FullName       = e.FirstName + " " + e.LastName,
            Email          = e.Email ?? "",
            RoleLabel      = "Employee",
            TeamName       = e.Department != null ? e.Department.DepartmentName : "—",
            TeamId         = e.DepartmentId,
            IsActive       = e.IsActive,
            StartDate      = e.HireDate,
            CurrentShiftId = e.ShiftId
        }).ToListAsync();

        var allRows = staffRows.Concat(empRows).AsQueryable();

        if (!string.IsNullOrEmpty(role))
            allRows = allRows.Where(r => r.RoleLabel == role);

        var total    = allRows.Count();
        var rows     = allRows
            .OrderBy(r => r.TeamName).ThenBy(r => r.RoleLabel).ThenBy(r => r.FullName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Populate ShiftName from Shifts table
        var allShifts = await _context.Shifts
            .Where(s => s.CompanyId == user.CompanyId && s.IsActive).ToListAsync();
        foreach (var r in rows)
            r.ShiftName = allShifts.FirstOrDefault(s => s.Id == r.CurrentShiftId)?.ShiftName ?? "—";

        var departments = await _context.Departments
            .Where(d => d.CompanyId == user.CompanyId && d.IsActive)
            .ToListAsync();

        ViewBag.Rows        = rows;
        ViewBag.TotalCount  = total;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.PageSize    = pageSize;
        ViewBag.RoleFilter  = role;
        ViewBag.TeamFilter  = teamId;
        ViewBag.Search      = search;
        ViewBag.Departments = departments;
        ViewBag.Shifts      = await EnsureShiftsAsync(user.CompanyId ?? 0);

        return View();
    }

    /// <summary>
    /// Returns shifts for the company, auto-creating the two default shifts if none exist.
    /// </summary>
    private async Task<List<Shift>> EnsureShiftsAsync(int companyId)
    {
        var shifts = await _context.Shifts
            .Where(s => s.CompanyId == companyId && s.IsActive)
            .ToListAsync();

        if (shifts.Any()) return shifts;

        // Auto-create the two standard shifts for this company
        var morning = new Shift
        {
            CompanyId          = companyId,
            ShiftName          = "Morning Shift (10AM-7PM)",
            StartTime          = new TimeSpan(10, 0, 0),
            EndTime            = new TimeSpan(19, 0, 0),
            BreakStart         = new TimeSpan(13, 0, 0),
            BreakEnd           = new TimeSpan(14, 0, 0),
            GracePeriodMinutes = 15,
            IsActive           = true,
            CreatedAt          = DateTime.UtcNow
        };
        var night = new Shift
        {
            CompanyId          = companyId,
            ShiftName          = "Night Shift (10PM-7AM)",
            StartTime          = new TimeSpan(22, 0, 0),
            EndTime            = new TimeSpan(7, 0, 0),
            IsNightShift       = true,
            GracePeriodMinutes = 15,
            IsActive           = true,
            CreatedAt          = DateTime.UtcNow
        };

        _context.Shifts.AddRange(morning, night);
        await _context.SaveChangesAsync();

        return new List<Shift> { morning, night };
    }

    // POST: /Manager/DeactivateStaff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateStaff(string id)
    {
        var user = await GetManagerUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        // Try ApplicationUser first
        var appUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == user.CompanyId);
        if (appUser != null)
        {
            appUser.IsActive = false;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Try Employee
        if (int.TryParse(id, out int empId))
        {
            var emp = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == empId && e.CompanyId == user.CompanyId);
            if (emp != null)
            {
                emp.IsActive = false;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
        }

        return Json(new { success = false, message = "Staff not found" });
    }

    // GET: /Manager/BudgetAllocation  →  Views/Manager/BudgetAllocation.cshtml
    public async Task<IActionResult> BudgetAllocation()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Manager/PayrollApproval  →  Views/Manager/PayrollApproval.cshtml
    public async Task<IActionResult> PayrollApproval()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var companyId = user.CompanyId ?? 0;

        // Pending — submitted by HR, waiting for manager approval
        var pending = await _context.PayrollPeriods
            .Include(p => p.Payrolls).ThenInclude(pr => pr.Employee).ThenInclude(e => e.Department)
            .Where(p => p.CompanyId == companyId && p.Status == PayrollStatus.Processed)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        // Recently approved / rejected
        var history = await _context.PayrollPeriods
            .Include(p => p.Payrolls)
            .Where(p => p.CompanyId == companyId
                     && (p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.Paid))
            .OrderByDescending(p => p.ProcessedAt ?? p.StartDate)
            .Take(10)
            .ToListAsync();

        ViewBag.PendingPeriods  = pending;
        ViewBag.HistoryPeriods  = history;
        ViewBag.PendingCount    = pending.Count;
        ViewBag.PendingTotal    = pending.Sum(p => p.Payrolls.Sum(pr => pr.NetPay));

        return View();
    }

    // POST: /Manager/ApprovePayroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePayroll(int periodId)
    {
        var user = await GetManagerUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var period = await _context.PayrollPeriods
            .Include(p => p.Payrolls)
            .FirstOrDefaultAsync(p => p.Id == periodId && p.CompanyId == user.CompanyId);

        if (period == null)
            return Json(new { success = false, message = "Payroll period not found." });

        if (period.Status != PayrollStatus.Processed)
            return Json(new { success = false, message = $"Cannot approve — current status is {period.Status}." });

        period.Status      = PayrollStatus.Approved;
        period.ProcessedAt = DateTime.UtcNow;

        foreach (var payroll in period.Payrolls)
            payroll.Status = PayrollStatus.Approved;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Approved Payroll", "PayrollPeriod",
            period.Id.ToString(), null,
            new { Period = period.PeriodName, Total = period.Payrolls.Sum(p => p.NetPay) },
            user.CompanyId);

        return Json(new { success = true, message = $"✅ {period.PeriodName} approved! Accountant can now distribute salaries." });
    }

    // POST: /Manager/RejectPayroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectPayroll(int periodId, string? reason)
    {
        var user = await GetManagerUser();
        if (user == null) return Json(new { success = false, message = "Unauthorized" });

        var period = await _context.PayrollPeriods
            .Include(p => p.Payrolls)
            .FirstOrDefaultAsync(p => p.Id == periodId && p.CompanyId == user.CompanyId);

        if (period == null)
            return Json(new { success = false, message = "Payroll period not found." });

        period.Status      = PayrollStatus.Draft;  // Send back to HR
        period.ProcessedAt = DateTime.UtcNow;

        foreach (var payroll in period.Payrolls)
            payroll.Status = PayrollStatus.Draft;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Rejected Payroll", "PayrollPeriod",
            period.Id.ToString(), null,
            new { Period = period.PeriodName, Reason = reason },
            user.CompanyId);

        return Json(new { success = true, message = $"Payroll rejected and sent back to HR. Reason: {reason}" });
    }

    // GET: /Manager/GetPayrollDetails?periodId=X — returns payroll breakdown for modal
    [HttpGet]
    public async Task<IActionResult> GetPayrollDetails(int periodId)
    {
        var user = await GetManagerUser();
        if (user == null) return Unauthorized();

        var payrolls = await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.PayrollPeriodId == periodId && p.Employee.CompanyId == user.CompanyId)
            .OrderBy(p => p.Employee.LastName)
            .ToListAsync();

        var result = payrolls.Select(p => new
        {
            employeeName   = p.Employee?.FullName ?? "—",
            department     = p.Employee?.Department?.DepartmentName ?? "—",
            basicPay       = p.BasicPay,
            overtimePay    = p.OvertimePay,
            totalDeductions = p.TotalDeductions,
            netPay         = p.NetPay
        });

        return Json(new
        {
            payrolls     = result,
            totalNetPay  = payrolls.Sum(p => p.NetPay),
            totalGross   = payrolls.Sum(p => p.GrossPay),
            employeeCount = payrolls.Count
        });
    }

    // GET: /Manager/IncidentReports  →  Views/Manager/IncidentReports.cshtml
    public async Task<IActionResult> IncidentReports()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Manager/GovernmentCompliance  →  Views/Manager/GovernmentCompliance.cshtml
    public async Task<IActionResult> GovernmentCompliance()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Manager/Reports  →  Views/Manager/Reports.cshtml
    public async Task<IActionResult> Reports()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        return View();
    }

    // GET: /Manager/AuditLogs  →  Views/Manager/AuditLogs.cshtml
    public async Task<IActionResult> AuditLogs(int page = 1, int days = 7, string? action = null, string? search = null)
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var fromDate = DateTime.UtcNow.AddDays(-days);
        var pageSize = 20;

        // Query ALL logs for the manager's company (not just their own)
        var query = _context.AuditLogs
            .Include(l => l.User)
            .Where(l => l.CompanyId == user.CompanyId && l.CreatedAt >= fromDate)
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(action)) query = query.Where(l => l.Action.Contains(action));
        if (!string.IsNullOrEmpty(search)) query = query.Where(l =>
            l.Action.Contains(search) || (l.NewValues != null && l.NewValues.Contains(search)));

        var totalCount = await query.CountAsync();
        var logs       = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CurrentPage    = page;
        ViewBag.TotalPages     = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.SelectedDays   = days.ToString();
        ViewBag.SelectedAction = action ?? "";
        ViewBag.SelectedSearch = search ?? "";

        return View(logs);
    }

    // GET: /Manager/CompanySettings  →  Views/Manager/CompanySettings.cshtml
    public async Task<IActionResult> CompanySettings()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");
        if (user.MustChangePassword) return RedirectToAction("ChangePassword", "Auth");

        var company = await _context.Companies.FindAsync(user.CompanyId);
        if (company == null) return RedirectToAction("Index", "Home");

        var vm = new CompanySettingsViewModel
        {
            WorkDaysPerWeek      = company.WorkDaysPerWeek,
            WorkingDaysPerMonth  = company.WorkingDaysPerMonth,
            WorkOnHolidays       = company.WorkOnHolidays,
            HolidayPayRate       = company.HolidayPayRate,
            OvertimeRatePerHour  = company.OvertimeRatePerHour,
            PayrollFrequency     = company.PayrollFrequency,
            HRDailyRate          = company.HRDailyRate ?? 800,
            AccountantDailyRate  = company.AccountantDailyRate ?? 800,
            DefaultDailyRate     = company.DefaultDailyRate ?? 500
        };

        return View(vm);
    }

    // POST: /Manager/CompanySettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompanySettings(CompanySettingsViewModel model)
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid) return View(model);

        var company = await _context.Companies.FindAsync(user.CompanyId);
        if (company == null) return RedirectToAction("Index", "Home");

        company.WorkDaysPerWeek     = model.WorkDaysPerWeek;
        company.WorkingDaysPerMonth = model.WorkingDaysPerMonth;
        company.WorkOnHolidays      = model.WorkOnHolidays;
        company.HolidayPayRate      = model.HolidayPayRate;
        company.OvertimeRatePerHour = model.OvertimeRatePerHour;
        company.PayrollFrequency    = model.PayrollFrequency;
        company.HRDailyRate         = model.HRDailyRate;
        company.AccountantDailyRate = model.AccountantDailyRate;
        company.DefaultDailyRate    = model.DefaultDailyRate;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(user.Id, "Updated Company Settings", "Company",
            company.Id.ToString(), null,
            new { DefaultDailyRate = model.DefaultDailyRate, WorkDaysPerWeek = model.WorkDaysPerWeek },
            user.CompanyId);

        TempData["Success"] = "Company settings saved successfully.";
        return RedirectToAction(nameof(CompanySettings));
    }

    // POST: /Manager/UpdateShift — Manager can update shift for any staff in their company
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShift(string userId, int shiftId)
    {
        var manager = await GetManagerUser();
        if (manager == null) return Json(new { success = false, message = "Unauthorized" });

        var target = await _context.Users.FirstOrDefaultAsync(u =>
            u.Id == userId && u.CompanyId == manager.CompanyId);

        if (target == null)
            return Json(new { success = false, message = "User not found" });

        var shift = await _context.Shifts.FirstOrDefaultAsync(s =>
            s.Id == shiftId && s.CompanyId == manager.CompanyId);
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

        await _auditLogService.LogAsync(manager.Id, "Updated Staff Shift", "ApplicationUser",
            userId, null, new { ShiftId = shiftId, ShiftName = shift.ShiftName }, manager.CompanyId);

        return Json(new { success = true, shiftName = shift.ShiftName });
    }

    // GET: /Manager/IDRequests  →  Views/Manager/IDRequests.cshtml
    public async Task<IActionResult> IDRequests()
    {
        var user = await GetManagerUser();
        if (user == null) return RedirectToAction("Index", "Home");

        var requests = await _context.IDRequests
            .Include(r => r.User)
            .Where(r => r.CompanyId == user.CompanyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(requests);
    }
}

public class CreateStaffViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string MiddleInitial { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string CivilStatus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string Role { get; set; } = "HR";
    public int BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ShiftId { get; set; }
    public decimal DailyRate { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactRelation { get; set; } = string.Empty;
    public string EmergencyContactNumber { get; set; } = string.Empty;
    public string FaceImageBase64 { get; set; } = string.Empty;
}

public class StaffRow
{
    public string Id { get; set; } = string.Empty;
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public string? ShiftName { get; set; }
    public int? CurrentShiftId { get; set; }
}
