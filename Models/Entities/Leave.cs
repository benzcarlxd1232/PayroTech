using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Leave : BaseEntity
{
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverRemarks { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedBy { get; set; }
}

public class LeaveBalance : BaseEntity
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public decimal VacationLeaveBalance { get; set; }
    public decimal SickLeaveBalance { get; set; }
    public decimal VacationLeaveUsed { get; set; }
    public decimal SickLeaveUsed { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
}
