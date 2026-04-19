using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Attendance : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    // Computed Values
    public int LateMinutes { get; set; }
    public int UndertimeMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public int NightDifferentialMinutes { get; set; }
    public decimal LateDeductionAmount { get; set; }   // Auto-calculated from daily rate
    public decimal OvertimeAmount { get; set; }         // Auto-calculated from overtime rate

    // Approval
    public bool IsApproved { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool IsLocked { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedBy { get; set; }
}
