namespace PayroTech.Models.Entities;

public class Department : BaseEntity
{
    public int CompanyId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int? ManagerId { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual Employee? Manager { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<ApplicationUser> StaffMembers { get; set; } = new List<ApplicationUser>();
}
