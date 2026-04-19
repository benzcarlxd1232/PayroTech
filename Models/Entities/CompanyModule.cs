using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class CompanyModule : BaseEntity
{
    public int CompanyId { get; set; }
    public ModuleType ModuleType { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public virtual Company Company { get; set; } = null!;
}
