using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;

namespace PayroTech.Services;

public interface ILoginSecurityService
{
    /// <summary>Records a failed login attempt. Returns the lockout state.</summary>
    Task<LoginSecurityResult> RecordFailedAttemptAsync(string email, string ipAddress, string? userAgent);

    /// <summary>Records a successful login — resets counters.</summary>
    Task RecordSuccessAsync(string email);

    /// <summary>Returns current lockout state for an email.</summary>
    Task<LoginSecurityResult> GetStateAsync(string email);
}

public class LoginSecurityResult
{
    public bool IsLocked        { get; set; }
    public bool IsCoolingDown   { get; set; }
    public DateTime? CooldownUntil { get; set; }
    public int FailedCount      { get; set; }
    public string Message       { get; set; } = "";
}

public class LoginSecurityService : ILoginSecurityService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginSecurityService> _logger;

    // Tier 1: 10 fails → 5-min cooldown
    // Tier 2: 8 more fails → 10-min cooldown
    // Tier 3: 8 more fails → account locked
    private const int Tier1Threshold = 10;
    private const int Tier2Threshold = 18; // 10 + 8
    private const int LockThreshold  = 26; // 10 + 8 + 8

    public LoginSecurityService(
        ApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<LoginSecurityService> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginSecurityResult> RecordFailedAttemptAsync(
        string email, string ipAddress, string? userAgent)
    {
        _context.LoginAttempts.Add(new LoginAttempt
        {
            Email        = email,
            IpAddress    = ipAddress,
            UserAgent    = userAgent ?? "",
            IsSuccessful = false,
            AttemptedAt  = DateTime.UtcNow,
            FailureReason = "Invalid credentials"
        });
        await _context.SaveChangesAsync();

        return await GetStateAsync(email);
    }

    public async Task RecordSuccessAsync(string email)
    {
        _context.LoginAttempts.Add(new LoginAttempt
        {
            Email        = email,
            IpAddress    = "—",
            IsSuccessful = true,
            AttemptedAt  = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<LoginSecurityResult> GetStateAsync(string email)
    {
        // Count failed attempts since last success (or ever)
        var lastSuccess = await _context.LoginAttempts
            .Where(a => a.Email == email && a.IsSuccessful)
            .OrderByDescending(a => a.AttemptedAt)
            .Select(a => a.AttemptedAt)
            .FirstOrDefaultAsync();

        var query = _context.LoginAttempts
            .Where(a => a.Email == email && !a.IsSuccessful);

        if (lastSuccess != default)
            query = query.Where(a => a.AttemptedAt > lastSuccess);

        var failedCount = await query.CountAsync();
        var result = new LoginSecurityResult { FailedCount = failedCount };

        if (failedCount >= LockThreshold)
        {
            result.IsLocked = true;
            result.Message  = "Your account has been locked due to too many failed login attempts. A password reset link has been sent.";
            return result;
        }

        if (failedCount >= Tier2Threshold)
        {
            // 10-min cooldown from the 18th failed attempt
            var tier2Start = await query
                .OrderByDescending(a => a.AttemptedAt)
                .Skip(failedCount - Tier2Threshold)
                .Select(a => a.AttemptedAt)
                .FirstOrDefaultAsync();

            var cooldownEnd = tier2Start.AddMinutes(10);
            if (DateTime.UtcNow < cooldownEnd)
            {
                result.IsCoolingDown = true;
                result.CooldownUntil = cooldownEnd;
                result.Message = $"Too many failed attempts. Please wait until {cooldownEnd.ToLocalTime():hh:mm tt} before trying again.";
                return result;
            }
        }
        else if (failedCount >= Tier1Threshold)
        {
            // 5-min cooldown from the 10th failed attempt
            var tier1Start = await query
                .OrderByDescending(a => a.AttemptedAt)
                .Skip(failedCount - Tier1Threshold)
                .Select(a => a.AttemptedAt)
                .FirstOrDefaultAsync();

            var cooldownEnd = tier1Start.AddMinutes(5);
            if (DateTime.UtcNow < cooldownEnd)
            {
                result.IsCoolingDown = true;
                result.CooldownUntil = cooldownEnd;
                result.Message = $"Too many failed attempts. Please wait until {cooldownEnd.ToLocalTime():hh:mm tt} before trying again.";
                return result;
            }
        }

        return result;
    }
}
