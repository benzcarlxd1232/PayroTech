using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Overtime : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int TotalMinutes { get; set; }
    public OvertimeType OvertimeType { get; set; }
    public decimal Multiplier { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedBy { get; set; }
}
