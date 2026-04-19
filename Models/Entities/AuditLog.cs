using System.ComponentModel.DataAnnotations.Schema;

namespace PayroTech.Models.Entities;

public class AuditLog : BaseEntity
{
    public int? CompanyId { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Details { get; set; }

    // Backwards-compatibility properties for older views/migrations
    // Map Timestamp to the BaseEntity CreatedAt field without changing the database schema.
    [NotMapped]
    public DateTime Timestamp
    {
        get => CreatedAt;
        set => CreatedAt = value;
    }

    // Map EntityType to EntityName so existing views expecting EntityType keep working.
    [NotMapped]
    public string EntityType
    {
        get => EntityName;
        set => EntityName = value;
    }

    // Navigation
    public virtual Company? Company { get; set; }
    public virtual ApplicationUser? User { get; set; }
}
