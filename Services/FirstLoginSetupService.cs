using Microsoft.AspNetCore.Identity;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;

namespace PayroTech.Services;

public interface IFirstLoginSetupService
{
    Task<SetupStepResult> DetermineNextStepAsync(string userId);
    Task<bool> IsSetupCompleteAsync(string userId);
    Task<bool> CompletePasswordChangeStepAsync(string userId);
    Task<bool> CompleteFaceEnrollmentStepAsync(string userId);
}

public class FirstLoginSetupService : IFirstLoginSetupService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public FirstLoginSetupService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<SetupStepResult> DetermineNextStepAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new SetupStepResult { IsComplete = false, ErrorMessage = "User not found" };
        }

        // Password change takes priority for ALL roles (including Manager)
        if (user.MustChangePassword)
        {
            return new SetupStepResult
            {
                RequiresPasswordChange = true,
                RequiresFaceEnrollment = false,
                IsComplete = false,
                NextStep = "ChangePassword",
                StepDescription = "You must change your temporary password before continuing."
            };
        }

        // Face enrollment required for ALL roles except SuperAdmin
        if (user.RequiresFaceEnrollment && user.Role != UserRole.ErpSuperAdmin)
        {
            return new SetupStepResult
            {
                RequiresPasswordChange = false,
                RequiresFaceEnrollment = true,
                IsComplete = false,
                NextStep = "EnrollFace",
                StepDescription = "Please capture your face photo for your ID card."
            };
        }

        // Setup complete
        if (user.FirstLoginSetupCompletedAt == null)
        {
            user.FirstLoginSetupCompletedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return new SetupStepResult
        {
            RequiresPasswordChange = false,
            RequiresFaceEnrollment = false,
            IsComplete = true,
            NextStep = "Dashboard",
            StepDescription = "Setup complete! Welcome to PayroTech."
        };
    }

    public async Task<bool> IsSetupCompleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (user.MustChangePassword) return false;
        if (user.RequiresFaceEnrollment) return false;
        return true;
    }

    public async Task<bool> CompletePasswordChangeStepAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.MustChangePassword = false;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> CompleteFaceEnrollmentStepAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.RequiresFaceEnrollment = false;
        user.FaceEnrollmentCompletedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}

public class SetupStepResult
{
    public bool RequiresPasswordChange { get; set; }
    public bool RequiresFaceEnrollment { get; set; }
    public bool IsComplete { get; set; }
    public string NextStep { get; set; } = string.Empty;
    public string StepDescription { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}