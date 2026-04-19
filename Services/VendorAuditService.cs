using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using System.Text.Json;

namespace PayroTech.Services;

public interface IVendorAuditService
{
    Task LogVendorActionAsync(string vendorId, string vendorName, string action, string entityType, 
        string? entityId = null, string? description = null, object? oldValues = null, object? newValues = null);
    Task LogVendorLoginAsync(string vendorId, string vendorName, string ipAddress, string? userAgent = null);
    Task LogVendorLogoutAsync(string vendorId, string vendorName, string ipAddress);
    Task<List<VendorLog>> GetVendorLogsAsync(int days = 30, string? action = null, string? search = null);
    Task<List<VendorLog>> GetVendorLoginHistoryAsync(int days = 30);
}

public class VendorAuditService : IVendorAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VendorAuditService> _logger;

    public VendorAuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<VendorAuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogVendorActionAsync(string vendorId, string vendorName, string action, string entityType, 
        string? entityId = null, string? description = null, object? oldValues = null, object? newValues = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            var requestPath = httpContext?.Request.Path.ToString();

            var log = new VendorLog
            {
                VendorId = vendorId,
                VendorName = vendorName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RequestPath = requestPath,
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<VendorLog>().Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vendor action logged: {Action} by {VendorName} from {IpAddress}", 
                action, vendorName, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log vendor action: {Action}", action);
        }
    }

    public async Task LogVendorLoginAsync(string vendorId, string vendorName, string ipAddress, string? userAgent = null)
    {
        try
        {
            var log = new VendorLog
            {
                VendorId = vendorId,
                VendorName = vendorName,
                Action = "LOGIN",
                EntityType = "Authentication",
                Description = $"Vendor logged in from {ipAddress}",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RequestPath = "/Auth/Login",
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<VendorLog>().Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vendor login logged: {VendorName} from {IpAddress}", vendorName, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log vendor login");
        }
    }

    public async Task LogVendorLogoutAsync(string vendorId, string vendorName, string ipAddress)
    {
        try
        {
            var log = new VendorLog
            {
                VendorId = vendorId,
                VendorName = vendorName,
                Action = "LOGOUT",
                EntityType = "Authentication",
                Description = $"Vendor logged out from {ipAddress}",
                IpAddress = ipAddress,
                RequestPath = "/Auth/Logout",
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<VendorLog>().Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log vendor logout");
        }
    }

    public async Task<List<VendorLog>> GetVendorLogsAsync(int days = 30, string? action = null, string? search = null)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        var query = _context.Set<VendorLog>()
            .Where(log => log.CreatedAt >= cutoffDate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(log => log.Action == action);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(log => 
                log.VendorName.Contains(search) ||
                log.Action.Contains(search) ||
                log.EntityType.Contains(search) ||
                (log.Description != null && log.Description.Contains(search)));
        }

        return await query
            .OrderByDescending(log => log.CreatedAt)
            .Take(1000)
            .ToListAsync();
    }

    public async Task<List<VendorLog>> GetVendorLoginHistoryAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await _context.Set<VendorLog>()
            .Where(log => log.CreatedAt >= cutoffDate && 
                         (log.Action == "LOGIN" || log.Action == "LOGOUT"))
            .OrderByDescending(log => log.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        // Check for forwarded IP (when behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP address
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
