namespace PayroTech.Models.Entities;

/// <summary>
/// Tracks all vendor (SuperAdmin) actions and changes in the system.
/// This provides a complete audit trail of vendor activities.
/// </summary>
public class VendorLog : BaseEntity
{
    public string VendorId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    // Navigation
    public virtual ApplicationUser? Vendor { get; set; }
}
