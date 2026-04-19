using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Employee : BaseEntity
{
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    public string? Suffix { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? CivilStatus { get; set; }
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }

    // Employment Details
    public DateTime HireDate { get; set; }
    public DateTime? RegularizationDate { get; set; }
    public DateTime? SeparationDate { get; set; }
    public string? SeparationReason { get; set; }

    // Salary Information
    public decimal BasicSalary { get; set; }
    public SalaryType SalaryType { get; set; } = SalaryType.Monthly;
    public decimal? DailyRate { get; set; }
    public decimal? HourlyRate { get; set; }

    // Government IDs
    public string? SSSNumber { get; set; }
    public string? PhilHealthNumber { get; set; }
    public string? PagIbigNumber { get; set; }
    public string? TINNumber { get; set; }

    // Bank Details
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }

    // Foreign Keys
    public int? DepartmentId { get; set; }
    public int? ShiftId { get; set; }
    public int? SupervisorId { get; set; }
    public string? UserId { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual Department? Department { get; set; }
    public virtual Shift? Shift { get; set; }
    public virtual Employee? Supervisor { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    public virtual ICollection<Overtime> Overtimes { get; set; } = new List<Overtime>();
    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
