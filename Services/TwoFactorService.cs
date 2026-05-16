using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using System.Security.Cryptography;

namespace PayroTech.Services;

public interface ITwoFactorService
{
    Task<string> GenerateAndSendOtpAsync(string userId, string userEmail, string userName);
    Task<bool> ValidateOtpAsync(string userId, string code);
    Task InvalidateOtpAsync(string userId);
}

public class TwoFactorService : ITwoFactorService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwoFactorService> _logger;

    // OTP valid for 10 minutes
    private const int OtpValidMinutes = 10;

    public TwoFactorService(
        ApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<TwoFactorService> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateAndSendOtpAsync(string userId, string userEmail, string userName)
    {
        // Generate 6-digit OTP using cryptographically secure random
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Invalidate any existing OTP for this user
        var existing = await _context.TwoFactorCodes
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();
        foreach (var e in existing) e.IsUsed = true;

        // Save new OTP
        _context.TwoFactorCodes.Add(new TwoFactorCode
        {
            UserId    = userId,
            Code      = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpValidMinutes),
            IsUsed    = false,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Always send to the SMTP account (demo mode)
        var smtpEmail = _configuration["EmailSettings:SmtpUsername"] ?? "benz1232carl@gmail.com";
        var smtpName  = "PayroTech System";

        var html = $@"
<!DOCTYPE html><html><head><style>
body{{font-family:'Segoe UI',Arial,sans-serif;background:#f1f5f9;padding:20px;}}
.box{{max-width:480px;margin:0 auto;background:#fff;border-radius:12px;padding:36px;box-shadow:0 4px 12px rgba(0,0,0,.1);}}
.logo{{color:#2563eb;font-size:22px;font-weight:700;text-align:center;margin-bottom:8px;}}
.code{{font-size:42px;font-weight:800;letter-spacing:10px;color:#0891b2;text-align:center;
       background:#f0f9ff;border:2px dashed #0891b2;border-radius:10px;padding:20px;margin:24px 0;}}
.note{{font-size:.82rem;color:#6b7280;text-align:center;}}
</style></head><body>
<div class='box'>
  <div class='logo'>💼 PayroTech</div>
  <p style='text-align:center;color:#64748b;margin-bottom:4px;'>Two-Factor Authentication</p>
  <hr style='border:none;border-top:1px solid #e2e8f0;margin:16px 0;'>
  <p>A sign-in attempt was made for account: <strong>{userEmail}</strong> ({userName})</p>
  <p>Your one-time verification code is:</p>
  <div class='code'>{code}</div>
  <p class='note'>⏱ This code expires in <strong>{OtpValidMinutes} minutes</strong>.</p>
  <p class='note'>If you did not attempt to sign in, your account may be at risk. Contact your administrator immediately.</p>
</div></body></html>";

        await _emailService.SendEmailAsync(smtpEmail, smtpName, $"PayroTech 2FA Code: {code}", html);
        _logger.LogInformation("2FA OTP sent for user {UserId} to SMTP account", userId);

        return code;
    }

    public async Task<bool> ValidateOtpAsync(string userId, string code)
    {
        var otp = await _context.TwoFactorCodes
            .Where(t => t.UserId == userId && t.Code == code && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (otp == null) return false;

        otp.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task InvalidateOtpAsync(string userId)
    {
        var codes = await _context.TwoFactorCodes
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();
        foreach (var c in codes) c.IsUsed = true;
        await _context.SaveChangesAsync();
    }
}
