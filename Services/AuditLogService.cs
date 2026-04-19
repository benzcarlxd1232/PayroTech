using PayroTech.Data;
using PayroTech.Models.Entities;
using System.Text.Json;

namespace PayroTech.Services;

public interface IAuditLogService
{
    Task LogAsync(string userId, string action, string entityName, string? entityId = null, 
                  object? oldValues = null, object? newValues = null, int? companyId = null);
    Task LogLoginAsync(string userId, string ipAddress);
    Task LogLogoutAsync(string userId);
    Task<List<AuditLog>> GetLogsForUserAsync(string userId, int days = 30);
    Task<List<AuditLog>> GetLogsForCompanyAsync(int companyId, int days = 30);
}

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string userId, string action, string entityName, string? entityId = null,
                               object? oldValues = null, object? newValues = null, int? companyId = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            CompanyId = companyId,
            IpAddress = GetClientIpAddress(),
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogLoginAsync(string userId, string ipAddress)
    {
        await LogAsync(userId, "Login", "Authentication", null, null, 
            new { LoginTime = DateTime.UtcNow, IpAddress = ipAddress });
    }

    public async Task LogLogoutAsync(string userId)
    {
        await LogAsync(userId, "Logout", "Authentication", null, null,
            new { LogoutTime = DateTime.UtcNow });
    }

    public async Task<List<AuditLog>> GetLogsForUserAsync(string userId, int days = 30)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);
        return await Task.FromResult(_context.AuditLogs
            .Where(l => l.UserId == userId && l.CreatedAt >= fromDate)
            .OrderByDescending(l => l.CreatedAt)
            .ToList());
    }

    public async Task<List<AuditLog>> GetLogsForCompanyAsync(int companyId, int days = 30)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);
        return await Task.FromResult(_context.AuditLogs
            .Where(l => l.CompanyId == companyId && l.CreatedAt >= fromDate)
            .OrderByDescending(l => l.CreatedAt)
            .ToList());
    }

    private string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
