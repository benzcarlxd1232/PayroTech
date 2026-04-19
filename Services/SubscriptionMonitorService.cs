using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;

namespace PayroTech.Services;

public interface ISubscriptionMonitorService
{
    Task CheckAndSendSubscriptionWarningsAsync();
    Task<List<Company>> GetExpiringSoonCompaniesAsync(int days = 14);
    Task<List<Company>> GetExpiredCompaniesAsync();
    Task<Dictionary<string, int>> GetSubscriptionStatisticsAsync();
}

public class SubscriptionMonitorService : ISubscriptionMonitorService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SubscriptionMonitorService> _logger;

    public SubscriptionMonitorService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<SubscriptionMonitorService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task CheckAndSendSubscriptionWarningsAsync()
    {
        try
        {
            _logger.LogInformation("Starting subscription warning check...");

            var today = DateTime.Today;
            var companies = await _context.Companies
                .Include(c => c.Employees)
                    .ThenInclude(e => e.User)
                .Where(c => c.IsActive && c.SubscriptionEnd.HasValue)
                .ToListAsync();

            int warningsSent = 0;
            int expiredNotifications = 0;

            foreach (var company in companies)
            {
                if (!company.SubscriptionEnd.HasValue) continue;

                var daysUntilExpiry = (company.SubscriptionEnd.Value - today).Days;
                
                // Find company admin (manager) through ApplicationUser
                var companyAdmin = company.Employees
                    .Where(e => e.User != null && e.User.Role == Models.Enums.UserRole.CompanyAdmin)
                    .Select(e => e.User)
                    .FirstOrDefault();

                if (companyAdmin == null || string.IsNullOrEmpty(companyAdmin.Email))
                {
                    _logger.LogWarning("Company {CompanyName} has no company admin with email", company.CompanyName);
                    continue;
                }

                // Send warnings at 7 days, 3 days, and 1 day before expiry
                if (daysUntilExpiry == 7 || daysUntilExpiry == 3 || daysUntilExpiry == 1)
                {
                    var sent = await _emailService.SendSubscriptionWarningEmailAsync(
                        companyAdmin.Email,
                        companyAdmin.FullName,
                        company.CompanyName,
                        company.SubscriptionEnd.Value,
                        daysUntilExpiry
                    );

                    if (sent)
                    {
                        warningsSent++;
                        _logger.LogInformation("Warning sent to {Manager} for {Company} ({Days} days remaining)",
                            companyAdmin.Email, company.CompanyName, daysUntilExpiry);
                    }
                }
                // Send expiry notification if subscription expired today
                else if (daysUntilExpiry == 0)
                {
                    var sent = await _emailService.SendSubscriptionExpiredEmailAsync(
                        companyAdmin.Email,
                        companyAdmin.FullName,
                        company.CompanyName,
                        company.SubscriptionEnd.Value
                    );

                    if (sent)
                    {
                        expiredNotifications++;
                        
                        // Deactivate subscription
                        company.IsSubscriptionActive = false;
                        _logger.LogInformation("Subscription expired for {Company}, deactivated",
                            company.CompanyName);
                    }
                }
                // Deactivate if already expired
                else if (daysUntilExpiry < 0 && company.IsSubscriptionActive)
                {
                    company.IsSubscriptionActive = false;
                    _logger.LogInformation("Subscription deactivated for {Company} (expired {Days} days ago)",
                        company.CompanyName, Math.Abs(daysUntilExpiry));
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription check complete. Warnings sent: {Warnings}, Expired notifications: {Expired}",
                warningsSent, expiredNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during subscription warning check");
        }
    }

    public async Task<List<Company>> GetExpiringSoonCompaniesAsync(int days = 14)
    {
        var cutoffDate = DateTime.Today.AddDays(days);
        
        return await _context.Companies
            .Where(c => c.IsActive && 
                       c.SubscriptionEnd.HasValue && 
                       c.SubscriptionEnd.Value <= cutoffDate &&
                       c.SubscriptionEnd.Value >= DateTime.Today)
            .OrderBy(c => c.SubscriptionEnd)
            .ToListAsync();
    }

    public async Task<List<Company>> GetExpiredCompaniesAsync()
    {
        return await _context.Companies
            .Where(c => c.SubscriptionEnd.HasValue && 
                       c.SubscriptionEnd.Value < DateTime.Today)
            .OrderByDescending(c => c.SubscriptionEnd)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetSubscriptionStatisticsAsync()
    {
        var today = DateTime.Today;
        
        var stats = new Dictionary<string, int>
        {
            ["TotalCompanies"] = await _context.Companies.CountAsync(),
            ["ActiveSubscriptions"] = await _context.Companies
                .CountAsync(c => c.IsActive && c.IsSubscriptionActive),
            ["ExpiredSubscriptions"] = await _context.Companies
                .CountAsync(c => c.SubscriptionEnd.HasValue && c.SubscriptionEnd.Value < today),
            ["ExpiringIn7Days"] = await _context.Companies
                .CountAsync(c => c.IsActive && 
                               c.SubscriptionEnd.HasValue && 
                               c.SubscriptionEnd.Value <= today.AddDays(7) &&
                               c.SubscriptionEnd.Value >= today),
            ["ExpiringIn30Days"] = await _context.Companies
                .CountAsync(c => c.IsActive && 
                               c.SubscriptionEnd.HasValue && 
                               c.SubscriptionEnd.Value <= today.AddDays(30) &&
                               c.SubscriptionEnd.Value >= today)
        };

        return stats;
    }
}
