using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Services;
using System.Security.Cryptography;
using System.Text;

namespace PayroTech.Controllers;

/// <summary>
/// Kiosk controller — QR + PIN dual verification, auto time-in/out,
/// late detection with deduction, overtime auto-record.
/// Public endpoint (no auth required) for kiosk devices.
/// </summary>
public class KioskController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KioskController> _logger;
    private readonly IAuditLogService _auditLogService;

    public KioskController(
        ApplicationDbContext context,
        ILogger<KioskController> logger,
        IAuditLogService auditLogService)
    {
        _context = context;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult Index() => View("~/Views/Kiosk/Landing.cshtml");

    [HttpGet]
    public IActionResult Attendance(int? companyId)
    {
        ViewBag.CompanyId = companyId;
        return View("~/Views/Kiosk/Attendance.cshtml");
    }

    /// <summary>
    /// Step 1 — Verify QR code, return employee info for PIN entry.
    /// Uses StaffCode (not EmployeeNumber) for display.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> VerifyQRCode([FromBody] QRVerifyRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.QRCode))
                return Json(new { success = false, message = "Invalid QR code" });

            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.QRCodeHash == request.QRCode);

            if (user == null)
                return Json(new { success = false, message = "QR code not recognised" });

            if (user.Role == UserRole.ErpSuperAdmin)
                return Json(new { success = false, message = "SuperAdmin does not use kiosk attendance" });

            // For HR/Accountant/Manager we use ApplicationUser directly (no Employee record required)
            var staffCode = user.StaffCode;
            if (string.IsNullOrEmpty(staffCode))
                staffCode = user.EmployeeNumber; // fallback for older accounts

            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Staff code not set. Please contact your manager." });

            // Determine the internal employee ID (for attendance record)
            // HR/Accountant/Manager may not have an Employee entity — use userId as key
            int? employeeId = user.Employee?.Id;

            return Json(new
            {
                success      = true,
                userId       = user.Id,
                employeeId   = employeeId,
                employeeName = user.FullName,
                staffCode    = staffCode,
                isManager    = user.Role == UserRole.CompanyAdmin,
                faceImagePath = string.IsNullOrEmpty(user.FaceImagePath) ? null : user.FaceImagePath,
                message      = user.Role == UserRole.CompanyAdmin
                    ? "Manager verified. Processing attendance..."
                    : "QR verified. Enter your 4-digit PIN."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying QR code");
            return Json(new { success = false, message = "An error occurred. Please try again." });
        }
    }

    /// <summary>
    /// Manager QR-only attendance — no PIN required.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessAttendanceQROnly([FromBody] QRVerifyRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.QRCode))
                return Json(new { success = false, message = "Invalid QR code" });

            var user = await _context.Users
                .Include(u => u.Employee).ThenInclude(e => e!.Shift)
                .FirstOrDefaultAsync(u => u.QRCodeHash == request.QRCode);

            if (user == null || user.Role != UserRole.CompanyAdmin)
                return Json(new { success = false, message = "Unauthorized" });

            // Auto-create Employee record if missing
            var employee = user.Employee;
            if (employee == null)
            {
                employee = new Employee
                {
                    CompanyId      = user.CompanyId ?? 0,
                    EmployeeNumber = user.StaffCode,
                    FirstName      = user.FirstName,
                    LastName       = user.LastName,
                    Email          = user.Email,
                    HireDate       = user.StartDate ?? DateTime.Today,
                    DepartmentId   = user.DepartmentId,
                    ShiftId        = user.ShiftId,
                    BasicSalary    = user.DailyRate * 22,
                    DailyRate      = user.DailyRate,
                    HourlyRate     = user.DailyRate / 8,
                    SalaryType     = SalaryType.Monthly,
                    UserId         = user.Id,
                    IsActive       = true,
                    CreatedAt      = DateTime.UtcNow
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                employee = await _context.Employees.Include(e => e.Shift)
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);
            }

            var today = DateTime.Today;
            var now   = DateTime.Now;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee!.Id && a.Date == today);

            string type = (attendance == null || attendance.TimeIn == null) ? "in"
                        : attendance.TimeOut == null ? "out" : "in";

            string message;
            if (type == "in")
            {
                if (attendance == null)
                {
                    attendance = new Attendance { EmployeeId = employee!.Id, Date = today, TimeIn = now, Status = AttendanceStatus.Present };
                    _context.Attendances.Add(attendance);
                }
                else { attendance.TimeIn = now; attendance.Status = AttendanceStatus.Present; }
                message = $"Clocked in at {now:hh:mm tt}";
            }
            else
            {
                attendance!.TimeOut = now;
                attendance.WorkedMinutes = (int)(now - attendance.TimeIn!.Value).TotalMinutes;
                message = $"Clocked out at {now:hh:mm tt}";
            }

            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(user.Id, $"Kiosk {type} (Manager QR)", "Attendance",
                employee!.Id.ToString(), null, new { Type = type, Time = now.ToString("HH:mm:ss") }, employee.CompanyId);

            return Json(new { success = true, type, employeeName = user.FullName,
                staffCode = user.StaffCode, time = now.ToString("hh:mm tt"),
                isLate = false, isOvertime = false, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing manager QR attendance");
            return Json(new { success = false, message = "An error occurred. Please try again." });
        }
    }

    /// <summary>
    /// Step 2 — Verify PIN (last 4 digits of StaffCode or custom KioskPin),
    /// then auto clock-in or clock-out with late/overtime calculation.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessAttendanceWithPIN([FromBody] AttendanceRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.PIN))
                return Json(new { success = false, message = "Invalid request" });

            var user = await _context.Users
                .Include(u => u.Employee)
                    .ThenInclude(e => e!.Shift)
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
                return Json(new { success = false, message = "User not found" });

            // Verify PIN against KioskPin (custom) or default last-4 of StaffCode
            var codeForPin = string.IsNullOrEmpty(user.StaffCode) ? user.EmployeeNumber : user.StaffCode;
            var expectedPin = string.IsNullOrEmpty(user.KioskPin)
                ? PayroTech.Utilities.StaffCodeGenerator.GetDefaultPin(codeForPin ?? "")
                : user.KioskPin;

            if (request.PIN != expectedPin)
            {
                await _auditLogService.LogAsync(user.Id, "Failed Kiosk PIN", "Attendance",
                    user.Id, null, new { Time = DateTime.Now.ToString("HH:mm:ss") }, user.CompanyId);
                return Json(new { success = false, message = "Incorrect PIN. Please try again." });
            }

            // --- Determine employee record ---
            // HR/Accountant/Manager may not have an Employee entity.
            // Auto-create one on first kiosk use so attendance can be recorded.
            var employee = user.Employee;
            if (employee == null)
            {
                employee = new Employee
                {
                    CompanyId      = user.CompanyId ?? 0,
                    EmployeeNumber = user.StaffCode,
                    FirstName      = user.FirstName,
                    LastName       = user.LastName,
                    Email          = user.Email,
                    HireDate       = user.StartDate ?? DateTime.Today,
                    DepartmentId   = user.DepartmentId,
                    ShiftId        = user.ShiftId,
                    BasicSalary    = user.DailyRate * 22,
                    DailyRate      = user.DailyRate,
                    HourlyRate     = user.DailyRate / 8,
                    SalaryType     = SalaryType.Monthly,
                    UserId         = user.Id,
                    IsActive       = true,
                    CreatedAt      = DateTime.UtcNow
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Reload with Shift
                employee = await _context.Employees
                    .Include(e => e.Shift)
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);

                if (employee == null)
                    return Json(new { success = false, message = "Employee record could not be created. Please contact your manager." });
            }

            var today = DateTime.Today;
            var now   = DateTime.Now;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee!.Id && a.Date == today);

            string type;
            if (attendance == null || attendance.TimeIn == null)
                type = "in";
            else if (attendance.TimeOut == null)
                type = "out";
            else
                type = "in"; // second shift

            string message;

            if (type == "in")
            {
                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = employee.Id,
                        Date       = today,
                        TimeIn     = now,
                        Status     = AttendanceStatus.Present
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.TimeIn  = now;
                    attendance.Status  = AttendanceStatus.Present;
                }

                // --- Late detection ---
                var shift = employee!.Shift;
                if (shift == null && user.ShiftId.HasValue)
                    shift = await _context.Shifts.FindAsync(user.ShiftId.Value);

                if (shift != null)
                {
                    var shiftStart  = today.Add(shift.StartTime);
                    var graceEnd    = shiftStart.AddMinutes(shift.GracePeriodMinutes);

                    if (now > graceEnd)
                    {
                        var lateMinutes = (int)(now - shiftStart).TotalMinutes;
                        attendance.LateMinutes = lateMinutes;
                        attendance.Status      = AttendanceStatus.Late;

                        // Auto-calculate late deduction: (DailyRate / WorkingMinutes) * lateMinutes
                        var company        = await _context.Companies.FindAsync(employee.CompanyId);
                        var workingMinutes = (company?.WorkingDaysPerMonth ?? 22) * 8 * 60 / (company?.WorkingDaysPerMonth ?? 22); // minutes per day
                        var minuteRate     = (employee.DailyRate ?? 0) / (8 * 60);
                        attendance.LateDeductionAmount = Math.Round(minuteRate * lateMinutes, 2);
                    }
                }

                message = attendance.LateMinutes > 0
                    ? $"Clocked in LATE at {now:hh:mm tt} ({attendance.LateMinutes} min late)"
                    : $"Clocked in at {now:hh:mm tt}";
            }
            else // out
            {
                attendance!.TimeOut      = now;
                attendance.WorkedMinutes = (int)(now - attendance.TimeIn!.Value).TotalMinutes;

                // --- Overtime detection ---
                var shift = employee!.Shift;
                if (shift == null && user.ShiftId.HasValue)
                    shift = await _context.Shifts.FindAsync(user.ShiftId.Value);

                if (shift != null)
                {
                    var shiftEnd       = today.Add(shift.EndTime);
                    // Handle night shift crossing midnight
                    if (shift.IsNightShift && shiftEnd < today.Add(shift.StartTime))
                        shiftEnd = shiftEnd.AddDays(1);

                    if (now > shiftEnd)
                    {
                        var overtimeMinutes = (int)(now - shiftEnd).TotalMinutes;
                        attendance.OvertimeMinutes = overtimeMinutes;

                        // Auto-create Overtime record
                        var hourlyRate   = employee.HourlyRate ?? ((employee.DailyRate ?? 0) / 8);
                        // Overtime extra pay = 25% of hourly rate per OT hour (not full rate)
                        var otExtraRate  = hourlyRate * 0.25m;
                        var otAmount     = Math.Round(otExtraRate * (overtimeMinutes / 60m), 2);
                        attendance.OvertimeAmount = otAmount;

                        var existingOt = await _context.Overtimes
                            .FirstOrDefaultAsync(o => o.EmployeeId == employee.Id && o.Date == today);

                        if (existingOt == null)
                        {
                            _context.Overtimes.Add(new Overtime
                            {
                                EmployeeId   = employee.Id,
                                Date         = today,
                                StartTime    = shift.EndTime,
                                EndTime      = now.TimeOfDay,
                                TotalMinutes = overtimeMinutes,
                                OvertimeType = OvertimeType.Regular,
                                Multiplier   = 1.25m,   // standard OT multiplier stored for record
                                Reason       = "Auto-detected from kiosk clock-out",
                                Status       = LeaveStatus.Pending
                            });
                        }
                    }

                    // --- Night differential (10PM–6AM = +10% of hourly rate) ---
                    if (attendance.TimeIn != null)
                    {
                        var ndStart = today.Add(new TimeSpan(22, 0, 0)); // 10PM
                        var ndEnd   = today.AddDays(1).Add(new TimeSpan(6, 0, 0)); // 6AM next day

                        var workedFrom = attendance.TimeIn.Value;
                        var workedTo   = now;

                        // Overlap between worked hours and night differential window
                        var overlapStart = workedFrom > ndStart ? workedFrom : ndStart;
                        var overlapEnd   = workedTo < ndEnd ? workedTo : ndEnd;

                        if (overlapEnd > overlapStart)
                        {
                            var ndMinutes = (int)(overlapEnd - overlapStart).TotalMinutes;
                            attendance.NightDifferentialMinutes = ndMinutes;
                        }
                    }
                }

                message = attendance.OvertimeMinutes > 0
                    ? $"Clocked out at {now:hh:mm tt} (+{attendance.OvertimeMinutes} min OT)"
                    : $"Clocked out at {now:hh:mm tt}";
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(user.Id, $"Kiosk {type}", "Attendance",
                employee.Id.ToString(), null,
                new { Type = type, Time = now.ToString("HH:mm:ss"), Method = "QR+PIN" },
                employee.CompanyId);

            return Json(new
            {
                success      = true,
                type         = type,
                employeeName = user.FullName,
                staffCode    = user.StaffCode,
                time         = now.ToString("hh:mm tt"),
                isLate       = attendance.LateMinutes > 0,
                isOvertime   = attendance.OvertimeMinutes > 0,
                message      = message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing attendance with PIN");
            return Json(new { success = false, message = "An error occurred. Please try again." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendanceStatus(string qrCode)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.QRCodeHash == qrCode);

        if (user?.Employee == null)
            return Json(new { success = false, message = "User not found" });

        var today      = DateTime.Today;
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == user.Employee.Id && a.Date == today);

        return Json(new
        {
            success      = true,
            employeeName = user.FullName,
            staffCode    = user.StaffCode,
            date         = today.ToString("MMMM dd, yyyy"),
            timeIn       = attendance?.TimeIn?.ToString("hh:mm tt"),
            timeOut      = attendance?.TimeOut?.ToString("hh:mm tt"),
            status       = attendance == null ? "Not Yet Clocked In"
                         : attendance.TimeOut == null ? "Clocked In" : "Clocked Out"
        });
    }

    public static string GenerateQRCodeHash(string userId)
    {
        using var sha256 = SHA256.Create();
        var input     = $"{userId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}

// ── Request models ──────────────────────────────────────────────────────────

public class QRVerifyRequest
{
    public string QRCode { get; set; } = string.Empty;
}

public class AttendanceRequest
{
    public string UserId { get; set; } = string.Empty;   // ApplicationUser.Id
    public int? EmployeeId { get; set; }                  // Employee.Id (may be null for staff)
    public string PIN { get; set; } = string.Empty;
}

public class QRScanRequest
{
    public string QRCode { get; set; } = string.Empty;
    public string Type { get; set; } = "in";
    public int? CompanyId { get; set; }
}

public class FaceRecognitionRequest
{
    public byte[]? FaceData { get; set; }
    public string Type { get; set; } = "in";
    public int? CompanyId { get; set; }
}
