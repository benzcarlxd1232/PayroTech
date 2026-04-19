using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Holiday : BaseEntity
{
    public int CompanyId { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public HolidayType HolidayType { get; set; }
    public bool IsNationwide { get; set; } = true;

    // Navigation
    public virtual Company Company { get; set; } = null!;
}
