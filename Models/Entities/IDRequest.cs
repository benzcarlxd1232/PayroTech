namespace PayroTech.Models.Entities;

public class IDRequest : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public string Reason { get; set; } = string.Empty;
    public Models.Enums.IDRequestStatus Status { get; set; } = Models.Enums.IDRequestStatus.Pending;
    
    public string? ApprovedByUserId { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    
    public bool IsPrinted { get; set; } = false;
    public DateTime? PrintedAt { get; set; }
    public bool IsArchived { get; set; } = false;
    
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
