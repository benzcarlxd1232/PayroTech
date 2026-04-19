using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Controllers;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Models.ViewModels;
using PayroTech.Services;

namespace PayroTech.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IQRCodeService _qrCodeService;
    private readonly ApplicationDbContext _context;
    private readonly IFirstLoginSetupService _firstLoginSetupService;

    public AuthController(
        SignInManager<ApplicationUser> signInManager, 
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService,
        IQRCodeService qrCodeService,
        ApplicationDbContext context,
        IFirstLoginSetupService firstLoginSetupService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _auditLogService = auditLogService;
        _qrCodeService = qrCodeService;
        _context = context;
        _firstLoginSetupService = firstLoginSetupService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Log the login
            await _auditLogService.LogLoginAsync(user.Id, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // Check setup requirements using FirstLoginSetupService
            var setupResult = await _firstLoginSetupService.DetermineNextStepAsync(user.Id);
            
            if (!setupResult.IsComplete)
            {
                if (setupResult.RequiresPasswordChange)
                {
                    return RedirectToAction("ChangePassword");
                }
                
                if (setupResult.RequiresFaceEnrollment)
                {
                    return RedirectToAction("EnrollFace");
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ChangePassword()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var viewModel = new EnhancedChangePasswordViewModel
        {
            IsFirstTimeChange = user.MustChangePassword,
            UserName = user.FullName
        };

        return View("~/Views/Auth/ChangePassword.cshtml", viewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(EnhancedChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Auth/ChangePassword.cshtml", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // For first-time password change, we don't need the current password verification
        if (user.MustChangePassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                // Use FirstLoginSetupService to complete this step
                await _firstLoginSetupService.CompletePasswordChangeStepAsync(user.Id);

                await _auditLogService.LogAsync(user.Id, "Password Changed", "User", user.Id, null, 
                    new { FirstTimeChange = true }, user.CompanyId);

                TempData["SuccessMessage"] = "Password changed successfully!";

                // Check if there are more setup steps required
                var setupResult = await _firstLoginSetupService.DetermineNextStepAsync(user.Id);
                
                if (!setupResult.IsComplete)
                {
                    if (setupResult.RequiresFaceEnrollment)
                    {
                        return RedirectToAction("EnrollFace");
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("~/Views/Auth/ChangePassword.cshtml", model);
        }
        else
        {
            // This shouldn't happen in first-login setup, but handle it anyway
            ModelState.AddModelError(string.Empty, "Password change not required.");
            return View("~/Views/Auth/ChangePassword.cshtml", model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EnrollFace()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        // SuperAdmin doesn't need face enrollment
        if (user.Role == UserRole.ErpSuperAdmin)
            return RedirectToAction("Index", "Home");

        var viewModel = new FaceEnrollmentViewModel
        {
            UserId = user.Id,
            UserName = user.FullName,
            QRCode = user.QRCodeHash ?? "",
            IsRequired = user.RequiresFaceEnrollment,
            InstructionText = "Please look straight at the camera. This photo will be used on your ID card."
        };

        return View("~/Views/Auth/EnrollFace.cshtml", viewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EnrollFace([FromBody] FaceEnrollmentRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        // Check if request is null
        if (request == null)
        {
            return Json(new { success = false, message = "Invalid request data" });
        }

        try
        {
            // Store the face encoding data and save the image file for ID card
            if (!string.IsNullOrEmpty(request.FaceData))
            {
                var base64Data = request.FaceData;
                if (base64Data.Contains(","))
                    base64Data = base64Data.Split(',')[1];

                var imageBytes = Convert.FromBase64String(base64Data);
                user.FaceEncodingData = imageBytes;
                user.IsFaceEnrolled = true;

                // Save face image to disk for ID card display
                try
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "faces");
                    Directory.CreateDirectory(uploadPath);
                    var fileName = $"{user.Id}_{DateTime.UtcNow.Ticks}.jpg";
                    await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadPath, fileName), imageBytes);
                    user.FaceImagePath = $"/uploads/faces/{fileName}";
                }
                catch { /* continue — ID card will show initials if no photo */ }
            }

            // Generate QR code if not already generated
            if (string.IsNullOrEmpty(user.QRCodeHash))
            {
                user.QRCodeHash = KioskController.GenerateQRCodeHash(user.Id);
                user.QRCodeGeneratedAt = DateTime.UtcNow;
            }

            // Use FirstLoginSetupService to complete this step
            await _firstLoginSetupService.CompleteFaceEnrollmentStepAsync(user.Id);
            await _userManager.UpdateAsync(user);

            await _auditLogService.LogAsync(user.Id, "Face Enrolled", "User", user.Id, null, 
                new { EnrolledAt = DateTime.UtcNow }, user.CompanyId);

            // Determine redirect based on role:
            // Manager/Supervisor → dashboard (ID card accessible from profile anytime)
            // HR / Accountant / Employee → ID card generation page
            string redirectUrl;
            if (user.Role == UserRole.CompanyAdmin || user.Role == UserRole.Supervisor)
                redirectUrl = Url.Action("Index", "Manager") ?? "/";
            else
                redirectUrl = Url.Action("Generate", "IDCard") ?? "/";

            return Json(new { success = true, message = "Face enrolled successfully!", redirectUrl = redirectUrl });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error enrolling face: " + ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SkipFaceEnrollment()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Generate QR code even if face enrollment is skipped
        if (string.IsNullOrEmpty(user.QRCodeHash))
        {
            user.QRCodeHash = KioskController.GenerateQRCodeHash(user.Id);
            user.QRCodeGeneratedAt = DateTime.UtcNow;
        }

        // Use FirstLoginSetupService to complete this step
        await _firstLoginSetupService.CompleteFaceEnrollmentStepAsync(user.Id);
        await _userManager.UpdateAsync(user);

        TempData["InfoMessage"] = "Face enrollment skipped. You can enroll later from your profile.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            await _auditLogService.LogLogoutAsync(user.Id);
        }

        await _signInManager.SignOutAsync();
        
        // Clear the authentication cookie
        Response.Cookies.Delete(".AspNetCore.Identity.Application");
        
        // Set headers to prevent browser caching of authenticated pages
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyIDCard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var fullUser = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (fullUser == null) return RedirectToAction("Login");

        var qrCodeDataUrl = !string.IsNullOrEmpty(fullUser.QRCodeHash)
            ? _qrCodeService.GenerateQRCodeDataUrl(fullUser.QRCodeHash)
            : "";

        ViewBag.QRCodeDataUrl = qrCodeDataUrl;
        return View("~/Views/IDCard/StaffIDCard.cshtml", fullUser);
    }
}
