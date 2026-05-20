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

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IQRCodeService _qrCodeService;
    private readonly ApplicationDbContext _context;
    private readonly IFirstLoginSetupService _firstLoginSetupService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ILoginSecurityService _loginSecurity;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService,
        IQRCodeService qrCodeService,
        ApplicationDbContext context,
        IFirstLoginSetupService firstLoginSetupService,
        ITwoFactorService twoFactorService,
        ILoginSecurityService loginSecurity,
        IEmailService emailService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _signInManager        = signInManager;
        _userManager          = userManager;
        _auditLogService      = auditLogService;
        _qrCodeService        = qrCodeService;
        _context              = context;
        _firstLoginSetupService = firstLoginSetupService;
        _twoFactorService     = twoFactorService;
        _loginSecurity        = loginSecurity;
        _emailService         = emailService;
        _configuration        = configuration;
        _httpClientFactory    = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"]       = returnUrl;
        ViewBag.RecaptchaSiteKey    = _configuration["ReCaptcha:SiteKey"] ?? "";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"]    = returnUrl;
        ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"] ?? "";

        if (!ModelState.IsValid)
            return View(model);

        // ── Verify reCAPTCHA FIRST — block bots before any DB query ──────────
        var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
        if (string.IsNullOrWhiteSpace(recaptchaToken))
        {
            ModelState.AddModelError(string.Empty, "Please complete the reCAPTCHA verification.");
            return View(model);
        }

        var recaptchaValid = await VerifyRecaptchaAsync(recaptchaToken);
        if (!recaptchaValid)
        {
            ModelState.AddModelError(string.Empty, "reCAPTCHA verification failed. Please try again.");
            return View(model);
        }

        var ip        = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        // ── Check lockout / cooldown BEFORE password check ──────────────────
        var secState = await _loginSecurity.GetStateAsync(model.Email);
        if (secState.IsLocked)
        {
            ModelState.AddModelError(string.Empty, secState.Message);
            return View(model);
        }
        if (secState.IsCoolingDown)
        {
            ModelState.AddModelError(string.Empty, secState.Message);
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            await _loginSecurity.RecordFailedAttemptAsync(model.Email, ip, userAgent);
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account has been deactivated. Contact your administrator.");
            return View(model);
        }

        // Check if account is locked in ApplicationUser
        if (await _userManager.IsLockedOutAsync(user))
        {
            ModelState.AddModelError(string.Empty, "Your account is locked. A password reset link has been sent.");
            return View(model);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            var afterFail = await _loginSecurity.RecordFailedAttemptAsync(model.Email, ip, userAgent);

            // Log hazard attempt for CompanyAdmin and SuperAdmin only
            await LogHazardAttemptAsync(user, ip, afterFail.FailedCount);

            // If now locked → lock account + send reset email
            if (afterFail.IsLocked)
            {
                user.IsActive = false; // soft-lock
                await _userManager.UpdateAsync(user);
                await SendAccountLockedEmailAsync(user);
                ModelState.AddModelError(string.Empty, "Your account has been locked due to too many failed attempts. A password reset link has been sent.");
                return View(model);
            }

            if (afterFail.IsCoolingDown)
            {
                ModelState.AddModelError(string.Empty, afterFail.Message);
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // ── Password correct — generate 2FA OTP ─────────────────────────────
        await _loginSecurity.RecordSuccessAsync(model.Email);

        // Store pending user in session for 2FA step
        HttpContext.Session.SetString("2fa_userId",    user.Id);
        HttpContext.Session.SetString("2fa_returnUrl", returnUrl ?? "");
        HttpContext.Session.SetString("2fa_remember",  model.RememberMe ? "1" : "0");

        await _twoFactorService.GenerateAndSendOtpAsync(user.Id, user.Email!, user.FullName);

        return RedirectToAction("VerifyOtp");
    }

    // ── 2FA Verification ─────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult VerifyOtp()
    {
        var userId = HttpContext.Session.GetString("2fa_userId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        var smtpEmail = _configuration["EmailSettings:SmtpUsername"] ?? "your email";
        ViewBag.SmtpEmail = smtpEmail;
        return View("~/Views/Auth/VerifyOtp.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(string code)
    {
        var userId    = HttpContext.Session.GetString("2fa_userId");
        var returnUrl = HttpContext.Session.GetString("2fa_returnUrl") ?? "";
        var remember  = HttpContext.Session.GetString("2fa_remember") == "1";

        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        var smtpEmail = _configuration["EmailSettings:SmtpUsername"] ?? "your email";
        ViewBag.SmtpEmail = smtpEmail;

        // Track OTP verification attempts (max 5)
        var attempts = HttpContext.Session.GetInt32("2fa_otpAttempts") ?? 0;

        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "Please enter the verification code.");
            return View("~/Views/Auth/VerifyOtp.cshtml");
        }

        attempts++;
        HttpContext.Session.SetInt32("2fa_otpAttempts", attempts);

        if (attempts >= 5)
        {
            // Exceeded max attempts — invalidate OTP and force re-login
            await _twoFactorService.InvalidateOtpAsync(userId);
            HttpContext.Session.Remove("2fa_userId");
            HttpContext.Session.Remove("2fa_returnUrl");
            HttpContext.Session.Remove("2fa_remember");
            HttpContext.Session.Remove("2fa_otpAttempts");

            TempData["Error"] = "Too many incorrect OTP attempts. Please log in again.";
            return RedirectToAction("Login");
        }

        var valid = await _twoFactorService.ValidateOtpAsync(userId, code.Trim());
        if (!valid)
        {
            var remaining = 5 - attempts;
            ModelState.AddModelError(string.Empty, $"Invalid or expired code. {remaining} attempt{(remaining != 1 ? "s" : "")} remaining.");
            return View("~/Views/Auth/VerifyOtp.cshtml");
        }

        // OTP valid — sign in
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToAction("Login");

        // Clear session
        HttpContext.Session.Remove("2fa_userId");
        HttpContext.Session.Remove("2fa_returnUrl");
        HttpContext.Session.Remove("2fa_remember");
        HttpContext.Session.Remove("2fa_otpAttempts");

        await _signInManager.SignInAsync(user, remember);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var loginIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        await _auditLogService.LogLoginAsync(user.Id, loginIp, user.CompanyId);

        // Also log to VendorLog for SuperAdmin visibility
        _context.VendorLogs.Add(new VendorLog
        {
            VendorId    = user.Id,
            VendorName  = user.FullName,
            Action      = "Login",
            EntityType  = "Authentication",
            Description = $"{user.FullName} ({user.Email}) logged in from {loginIp}",
            IpAddress   = loginIp,
            UserAgent   = Request.Headers["User-Agent"].ToString(),
            RequestPath = "/Auth/VerifyOtp",
            IsSuccess   = true,
            CreatedAt   = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // First-login setup check
        var setupResult = await _firstLoginSetupService.DetermineNextStepAsync(user.Id);
        if (!setupResult.IsComplete)
        {
            if (setupResult.RequiresPasswordChange) return RedirectToAction("ChangePassword");
            if (setupResult.RequiresFaceEnrollment) return RedirectToAction("EnrollFace");
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> ResendOtp()
    {
        var userId = HttpContext.Session.GetString("2fa_userId");
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Json(new { success = false });

        await _twoFactorService.GenerateAndSendOtpAsync(user.Id, user.Email!, user.FullName);
        return Json(new { success = true });
    }

    // ── Password Reset (for locked accounts) ─────────────────────────────────
    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        ViewBag.Token = token;
        ViewBag.Email = email;
        return View("~/Views/Auth/ResetPassword.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string token, string email, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            ViewBag.Token = token;
            ViewBag.Email = email;
            return View("~/Views/Auth/ResetPassword.cshtml");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            TempData["Success"] = "If the account exists, the password has been reset.";
            return RedirectToAction("Login");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            // Re-activate account
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            // Clear failed attempts by recording a success
            await _loginSecurity.RecordSuccessAsync(email);

            TempData["Success"] = "Password reset successfully. You can now sign in.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        ViewBag.Token = token;
        ViewBag.Email = email;
        return View("~/Views/Auth/ResetPassword.cshtml");
    }

    // ── Forgot Password (OTP-based) ──────────────────────────────────────────
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View("~/Views/Auth/ForgotPassword.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Please enter your email address.");
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        // Always show same success message to prevent user enumeration
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            TempData["ForgotPasswordSuccess"] = "If an account with that email exists, a verification code has been sent.";
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        // Generate & send OTP using existing TwoFactorService
        await _twoFactorService.GenerateAndSendOtpAsync(user.Id, user.Email!, user.FullName);

        // Log: Password reset requested
        await LogForgotPasswordEventAsync(user, "Password Reset Requested",
            $"Password reset OTP sent for account {user.Email} ({user.FullName})");

        // Store state in session for OTP verification step
        HttpContext.Session.SetString("fp_userId", user.Id);
        HttpContext.Session.SetString("fp_email", user.Email!);
        HttpContext.Session.SetInt32("fp_otpAttempts", 0);

        TempData["ForgotPasswordSuccess"] = "If an account with that email exists, a verification code has been sent.";
        return RedirectToAction("VerifyForgotPasswordOtp");
    }

    // ── Forgot Password: OTP Verification ────────────────────────────────────
    [HttpGet]
    public IActionResult VerifyForgotPasswordOtp()
    {
        var userId = HttpContext.Session.GetString("fp_userId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("ForgotPassword");

        var smtpEmail = _configuration["EmailSettings:SmtpUsername"] ?? "your email";
        ViewBag.SmtpEmail = smtpEmail;
        ViewBag.MaxAttempts = 5;
        ViewBag.AttemptsUsed = HttpContext.Session.GetInt32("fp_otpAttempts") ?? 0;
        return View("~/Views/Auth/VerifyForgotPasswordOtp.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> VerifyForgotPasswordOtp(string code)
    {
        var userId = HttpContext.Session.GetString("fp_userId");
        var email  = HttpContext.Session.GetString("fp_email");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            return RedirectToAction("ForgotPassword");

        var smtpEmail = _configuration["EmailSettings:SmtpUsername"] ?? "your email";
        ViewBag.SmtpEmail = smtpEmail;
        ViewBag.MaxAttempts = 5;

        // Track OTP verification attempts (max 5)
        var attempts = HttpContext.Session.GetInt32("fp_otpAttempts") ?? 0;

        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "Please enter the verification code.");
            ViewBag.AttemptsUsed = attempts;
            return View("~/Views/Auth/VerifyForgotPasswordOtp.cshtml");
        }

        attempts++;
        HttpContext.Session.SetInt32("fp_otpAttempts", attempts);

        if (attempts >= 5)
        {
            // Exceeded max attempts — invalidate OTP and force restart
            await _twoFactorService.InvalidateOtpAsync(userId);

            // Log: Max OTP attempts exceeded
            var lockedUser = await _userManager.FindByIdAsync(userId);
            if (lockedUser != null)
                await LogForgotPasswordEventAsync(lockedUser, "Password Reset OTP Max Attempts",
                    $"Max 5 OTP attempts exceeded for {email}. Password reset process forced to restart.");

            HttpContext.Session.Remove("fp_userId");
            HttpContext.Session.Remove("fp_email");
            HttpContext.Session.Remove("fp_otpAttempts");

            TempData["ForgotPasswordError"] = "Too many incorrect attempts. Please restart the password reset process.";
            return RedirectToAction("ForgotPassword");
        }

        var valid = await _twoFactorService.ValidateOtpAsync(userId, code.Trim());
        if (!valid)
        {
            // Log: Failed OTP attempt
            var failUser = await _userManager.FindByIdAsync(userId);
            if (failUser != null)
                await LogForgotPasswordEventAsync(failUser, "Password Reset OTP Failed",
                    $"Failed OTP verification attempt #{attempts} for {email}. {5 - attempts} attempts remaining.");

            var remaining = 5 - attempts;
            ModelState.AddModelError(string.Empty, $"Invalid or expired code. {remaining} attempt{(remaining != 1 ? "s" : "")} remaining.");
            ViewBag.AttemptsUsed = attempts;
            return View("~/Views/Auth/VerifyForgotPasswordOtp.cshtml");
        }

        // OTP verified — generate password reset token and store in session
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToAction("ForgotPassword");

        // Log: OTP verified successfully
        await LogForgotPasswordEventAsync(user, "Password Reset OTP Verified",
            $"OTP verified successfully for {email}. User may now set a new password.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        HttpContext.Session.SetString("fp_resetToken", token);
        HttpContext.Session.Remove("fp_otpAttempts");

        return RedirectToAction("ResetForgotPassword");
    }

    // ── Forgot Password: Resend OTP ──────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> ResendForgotPasswordOtp()
    {
        var userId = HttpContext.Session.GetString("fp_userId");
        var email  = HttpContext.Session.GetString("fp_email");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            return Json(new { success = false });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Json(new { success = false });

        // Reset attempt counter on resend
        HttpContext.Session.SetInt32("fp_otpAttempts", 0);
        await _twoFactorService.GenerateAndSendOtpAsync(user.Id, user.Email!, user.FullName);

        // Log: OTP resent
        await LogForgotPasswordEventAsync(user, "Password Reset OTP Resent",
            $"New OTP code sent for password reset of {user.Email}. Attempt counter reset.");

        return Json(new { success = true });
    }

    // ── Forgot Password: Set New Password (after OTP verified) ───────────────
    [HttpGet]
    public IActionResult ResetForgotPassword()
    {
        var userId = HttpContext.Session.GetString("fp_userId");
        var token  = HttpContext.Session.GetString("fp_resetToken");
        var email  = HttpContext.Session.GetString("fp_email");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return RedirectToAction("ForgotPassword");

        ViewBag.Email = email;
        return View("~/Views/Auth/ResetForgotPassword.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> ResetForgotPassword(string newPassword, string confirmPassword)
    {
        var userId = HttpContext.Session.GetString("fp_userId");
        var token  = HttpContext.Session.GetString("fp_resetToken");
        var email  = HttpContext.Session.GetString("fp_email");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return RedirectToAction("ForgotPassword");

        ViewBag.Email = email;

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            ModelState.AddModelError(string.Empty, "Both password fields are required.");
            return View("~/Views/Auth/ResetForgotPassword.cshtml");
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View("~/Views/Auth/ResetForgotPassword.cshtml");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Success"] = "If the account exists, the password has been reset.";
            return RedirectToAction("Login");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            // Re-activate account if it was locked
            if (!user.IsActive)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
            }

            // Clear failed attempts
            await _loginSecurity.RecordSuccessAsync(email);

            // Log: Password reset completed
            await LogForgotPasswordEventAsync(user, "Password Reset Completed",
                $"Password was successfully reset for {email} ({user.FullName}) via forgot password flow.");

            // Clear all forgot-password session data
            HttpContext.Session.Remove("fp_userId");
            HttpContext.Session.Remove("fp_email");
            HttpContext.Session.Remove("fp_resetToken");
            HttpContext.Session.Remove("fp_otpAttempts");

            TempData["Success"] = "Password reset successfully. You can now sign in with your new password.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View("~/Views/Auth/ResetForgotPassword.cshtml");
    }


    // ── Forgot Password Logging Helper ────────────────────────────────────────
    private async Task LogForgotPasswordEventAsync(ApplicationUser user, string action, string description)
    {
        var ip        = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();
        if (userAgent.Length > 500) userAgent = userAgent[..500];

        // AuditLog — visible in Manager (company) view
        if (user.CompanyId.HasValue)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId     = user.Id,
                Action     = action,
                EntityType = "Security",
                EntityId   = user.Id,
                NewValues  = description,
                IpAddress  = ip,
                UserAgent  = userAgent,
                CompanyId  = user.CompanyId,
                CreatedAt  = DateTime.UtcNow
            });
        }

        // VendorLog — visible in SuperAdmin view
        _context.VendorLogs.Add(new VendorLog
        {
            VendorId    = user.Id,
            VendorName  = user.FullName,
            Action      = action,
            EntityType  = "Security",
            EntityId    = user.Id,
            Description = description,
            IpAddress   = ip,
            UserAgent   = userAgent,
            RequestPath = Request.Path.ToString(),
            IsSuccess   = action == "Password Reset Completed" || action == "Password Reset OTP Verified",
            CreatedAt   = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<bool> VerifyRecaptchaAsync(string token)
    {
        try
        {
            var secretKey = _configuration["ReCaptcha:SecretKey"];
            if (string.IsNullOrEmpty(secretKey) || secretKey == "YOUR_RECAPTCHA_SECRET_KEY")
                return true; // skip if not configured

            using var http = _httpClientFactory.CreateClient();
            var response = await http.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"]   = secretKey,
                    ["response"] = token
                }));

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"reCAPTCHA response: {json}");

            // Parse properly using System.Text.Json
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("success", out var successProp))
            {
                return successProp.GetBoolean();
            }

            System.Diagnostics.Debug.WriteLine($"reCAPTCHA response missing 'success' property: {json}");
            return false;
        }
        catch (Exception ex)
        {
            // Fail open — don't block login if reCAPTCHA service is down
            System.Diagnostics.Debug.WriteLine($"reCAPTCHA verification error: {ex.Message}");
            return true;
        }
    }

    private async Task LogHazardAttemptAsync(ApplicationUser user, string ip, int failedCount)
    {
        // Only log for CompanyAdmin and SuperAdmin
        if (user.Role != UserRole.CompanyAdmin && user.Role != UserRole.ErpSuperAdmin)
            return;

        var description = $"Failed login attempt #{failedCount} from IP {ip}";

        if (user.Role == UserRole.ErpSuperAdmin)
        {
            // SuperAdmin → vendor log only
            _context.VendorLogs.Add(new VendorLog
            {
                VendorId    = user.Id,
                VendorName  = user.FullName,
                Action      = "Failed Login",
                EntityType  = "Security",
                Description = description,
                IpAddress   = ip,
                IsSuccess   = false,
                CreatedAt   = DateTime.UtcNow
            });
        }
        else if (user.Role == UserRole.CompanyAdmin)
        {
            // CompanyAdmin → both AuditLog (manager sees) AND VendorLog (superadmin sees)
            _context.AuditLogs.Add(new AuditLog
            {
                UserId      = user.Id,
                Action      = "Failed Login",
                EntityType  = "Security",
                EntityId    = user.Id,
                NewValues   = description,
                IpAddress   = ip,
                CompanyId   = user.CompanyId,
                CreatedAt   = DateTime.UtcNow
            });
            _context.VendorLogs.Add(new VendorLog
            {
                VendorId    = user.Id,
                VendorName  = user.FullName,
                Action      = "Failed Login",
                EntityType  = "Security",
                Description = description,
                IpAddress   = ip,
                IsSuccess   = false,
                CreatedAt   = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SendAccountLockedEmailAsync(ApplicationUser user)
    {
        var token     = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action("ResetPassword", "Auth",
            new { token, email = user.Email }, Request.Scheme) ?? "#";

        var smtpEmail = _configuration["EmailSettings:SmtpUsername"] ?? "benz1232carl@gmail.com";

        var html = $@"
<!DOCTYPE html><html><head><style>
body{{font-family:'Segoe UI',Arial,sans-serif;background:#f1f5f9;padding:20px;}}
.box{{max-width:520px;margin:0 auto;background:#fff;border-radius:12px;padding:36px;box-shadow:0 4px 12px rgba(0,0,0,.1);}}
.logo{{color:#2563eb;font-size:22px;font-weight:700;text-align:center;margin-bottom:8px;}}
.alert{{background:#fee2e2;border:1px solid #fecaca;border-radius:8px;padding:16px;margin:20px 0;color:#dc2626;}}
.btn{{display:inline-block;background:#dc2626;color:#fff;padding:12px 28px;text-decoration:none;border-radius:8px;font-weight:600;}}
</style></head><body>
<div class='box'>
  <div class='logo'>💼 PayroTech</div>
  <div class='alert'>
    <strong>🔒 Account Locked</strong><br>
    The account <strong>{user.Email}</strong> ({user.FullName}) has been locked due to too many failed login attempts.
  </div>
  <p>To unlock and reset the password, click the button below:</p>
  <center><a href='{resetLink}' class='btn'>🔐 Reset Password & Unlock Account</a></center>
  <p style='margin-top:20px;font-size:.82rem;color:#6b7280;'>This link expires in 24 hours. If this was not you, contact your system administrator immediately.</p>
</div></body></html>";

        await _emailService.SendEmailAsync(smtpEmail, "PayroTech Security", $"Account Locked: {user.Email}", html);
    }

    // ── Existing actions (unchanged) ──────────────────────────────────────────
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ChangePassword()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        return View("~/Views/Auth/ChangePassword.cshtml", new EnhancedChangePasswordViewModel
        {
            IsFirstTimeChange = user.MustChangePassword,
            UserName          = user.FullName
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(EnhancedChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Auth/ChangePassword.cshtml", model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        if (user.MustChangePassword)
        {
            var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                await _firstLoginSetupService.CompletePasswordChangeStepAsync(user.Id);
                await _auditLogService.LogAsync(user.Id, "Password Changed", "User", user.Id, null,
                    new { FirstTimeChange = true }, user.CompanyId);

                TempData["SuccessMessage"] = "Password changed successfully!";

                var setupResult = await _firstLoginSetupService.DetermineNextStepAsync(user.Id);
                if (!setupResult.IsComplete && setupResult.RequiresFaceEnrollment)
                    return RedirectToAction("EnrollFace");

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View("~/Views/Auth/ChangePassword.cshtml", model);
        }

        ModelState.AddModelError(string.Empty, "Password change not required.");
        return View("~/Views/Auth/ChangePassword.cshtml", model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EnrollFace()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        if (user.Role == UserRole.ErpSuperAdmin) return RedirectToAction("Index", "Home");

        return View("~/Views/Auth/EnrollFace.cshtml", new FaceEnrollmentViewModel
        {
            UserId          = user.Id,
            UserName        = user.FullName,
            QRCode          = user.QRCodeHash ?? "",
            IsRequired      = user.RequiresFaceEnrollment,
            InstructionText = "Please look straight at the camera. This photo will be used on your ID card."
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EnrollFace([FromBody] FaceEnrollmentRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "User not found" });
        if (request == null) return Json(new { success = false, message = "Invalid request data" });

        try
        {
            if (!string.IsNullOrEmpty(request.FaceData))
            {
                var base64Data = request.FaceData.Contains(",")
                    ? request.FaceData.Split(',')[1] : request.FaceData;
                var imageBytes = Convert.FromBase64String(base64Data);
                user.FaceEncodingData = imageBytes;
                user.IsFaceEnrolled   = true;

                try
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "faces");
                    Directory.CreateDirectory(uploadPath);
                    var fileName = $"{user.Id}_{DateTime.UtcNow.Ticks}.jpg";
                    await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadPath, fileName), imageBytes);
                    user.FaceImagePath = $"/uploads/faces/{fileName}";
                }
                catch (Exception imgEx)
                {
                    // Face image file save failed — face data still in DB
                    System.Diagnostics.Debug.WriteLine($"Face image save error: {imgEx.Message}");
                }
            }

            if (string.IsNullOrEmpty(user.QRCodeHash))
            {
                user.QRCodeHash          = KioskController.GenerateQRCodeHash(user.Id);
                user.QRCodeGeneratedAt   = DateTime.UtcNow;
            }

            await _firstLoginSetupService.CompleteFaceEnrollmentStepAsync(user.Id);
            await _userManager.UpdateAsync(user);
            await _auditLogService.LogAsync(user.Id, "Face Enrolled", "User", user.Id, null,
                new { EnrolledAt = DateTime.UtcNow }, user.CompanyId);

            string redirectUrl = (user.Role == UserRole.CompanyAdmin || user.Role == UserRole.Supervisor)
                ? Url.Action("Index", "Manager") ?? "/"
                : Url.Action("Generate", "IDCard") ?? "/";

            return Json(new { success = true, message = "Face enrolled successfully!", redirectUrl });
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
        if (user == null) return RedirectToAction("Login");

        if (string.IsNullOrEmpty(user.QRCodeHash))
        {
            user.QRCodeHash        = KioskController.GenerateQRCodeHash(user.Id);
            user.QRCodeGeneratedAt = DateTime.UtcNow;
        }

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
            var logoutIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            await _auditLogService.LogLogoutAsync(user.Id, user.CompanyId);

            // Also log to VendorLog for SuperAdmin visibility
            _context.VendorLogs.Add(new VendorLog
            {
                VendorId    = user.Id,
                VendorName  = user.FullName,
                Action      = "Logout",
                EntityType  = "Authentication",
                Description = $"{user.FullName} ({user.Email}) logged out from {logoutIp}",
                IpAddress   = logoutIp,
                UserAgent   = Request.Headers["User-Agent"].ToString(),
                RequestPath = "/Auth/Logout",
                IsSuccess   = true,
                CreatedAt   = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        await _signInManager.SignOutAsync();
        Response.Cookies.Delete(".AspNetCore.Identity.Application");
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"]        = "no-cache";
        Response.Headers["Expires"]       = "0";

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

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

        ViewBag.QRCodeDataUrl = !string.IsNullOrEmpty(fullUser.QRCodeHash)
            ? _qrCodeService.GenerateQRCodeDataUrl(fullUser.QRCodeHash) : "";

        return View("~/Views/IDCard/StaffIDCard.cshtml", fullUser);
    }
}
