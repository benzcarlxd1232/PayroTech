using System.ComponentModel.DataAnnotations;

namespace PayroTech.Models.ViewModels;

public class FirstLoginSetupViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public string StepDescription { get; set; } = string.Empty;
    public bool RequiresPasswordChange { get; set; }
    public bool RequiresFaceEnrollment { get; set; }
    public int TotalSteps { get; set; }
    public int CurrentStepNumber { get; set; }
}

public class FaceEnrollmentViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public string InstructionText { get; set; } = "Please look straight at the camera and ensure your face is clearly visible.";
}

public class EnhancedChangePasswordViewModel
{
    public string? CurrentPassword { get; set; }
    
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 12)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{12,}$",
        ErrorMessage = "Password must have at least 12 characters with uppercase, lowercase, number, and special character.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm your new password")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public bool IsFirstTimeChange { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class FaceEnrollmentRequest
{
    public string? FaceData { get; set; }  // Changed to string to accept base64 data
}