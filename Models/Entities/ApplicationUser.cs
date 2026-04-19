using Microsoft.AspNetCore.Identity;
using PayroTech.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PayroTech.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    
    // Enhanced: Middle name for full names, middle initial for forms
    public string MiddleName { get; set; } = string.Empty;
    
    [StringLength(1)]
    [RegularExpression(@"^[A-Za-z]?$", ErrorMessage = "Middle initial must be a single letter")]
    public string MiddleInitial { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    public string FullName => string.IsNullOrEmpty(MiddleName) 
        ? $"{FirstName} {LastName}" 
        : $"{FirstName} {MiddleName} {LastName}";
    public int? CompanyId { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? BirthDate { get; set; }
    
    // Personal Details
    public string Gender { get; set; } = string.Empty;
    public string CivilStatus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    [StringLength(11, MinimumLength = 11, ErrorMessage = "Contact number must be exactly 11 digits")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact number must start with 09 and be exactly 11 digits")]
    public string ContactNumber { get; set; } = string.Empty;
    
    // Employment Details
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public DateTime? StartDate { get; set; }
    
    // Emergency Contact
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactRelation { get; set; } = string.Empty;
    
    [StringLength(11, MinimumLength = 11, ErrorMessage = "Emergency contact number must be exactly 11 digits")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Emergency contact number must start with 09 and be exactly 11 digits")]
    public string EmergencyContactNumber { get; set; } = string.Empty;
    
    // Face Image (stored as base64 or file path)
    public string FaceImagePath { get; set; } = string.Empty;

    // Medical / Additional Info (set by manager during staff creation)
    public string BloodType { get; set; } = string.Empty;

    // First login / Password change requirements
    public bool MustChangePassword { get; set; } = false;  // Force password change on first login
    public bool RequiresFaceEnrollment { get; set; } = false;  // Force face enrollment on first login

    // Enhanced: Face enrollment completion tracking
    public DateTime? FaceEnrollmentCompletedAt { get; set; }
    
    // Enhanced: First login setup completion tracking
    public DateTime? FirstLoginSetupCompletedAt { get; set; }

    // Kiosk-based attendance fields
    public string? QRCodeHash { get; set; }  // Unique QR code identifier for attendance
    public byte[]? FaceEncodingData { get; set; }  // Face recognition encoding data
    public DateTime? QRCodeGeneratedAt { get; set; }
    public bool IsFaceEnrolled { get; set; } = false;

    // Staff Code & Kiosk PIN
    public string StaffCode { get; set; } = string.Empty;  // e.g. HR-000001, ACC-000001, EMP-000001
    public string KioskPin { get; set; } = string.Empty;   // 4-digit PIN (default = last 4 of StaffCode)
    public int? ShiftId { get; set; }                       // Assigned shift for kiosk late/OT detection

    // Navigation
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual Department? Department { get; set; }
    public virtual Employee? Employee { get; set; }
}
