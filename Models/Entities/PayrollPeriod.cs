using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class PayrollPeriod : BaseEntity
{
    public int CompanyId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PayDate { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public int? ProcessedById { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
