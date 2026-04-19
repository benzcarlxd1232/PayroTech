namespace PayroTech.Models.Entities;

public class Shift : BaseEntity
{
    public int CompanyId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStart { get; set; }
    public TimeSpan? BreakEnd { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public bool IsNightShift { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
