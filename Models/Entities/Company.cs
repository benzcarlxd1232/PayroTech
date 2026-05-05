using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Company : BaseEntity
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? TIN { get; set; }
    public string? SSSEmployerNumber { get; set; }
    public string? PhilHealthEmployerNumber { get; set; }
    public string? PagIbigEmployerNumber { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public bool IsSubscriptionActive { get; set; } = true;
    
    // Initial Setup Tracking
    public bool IsInitialSetupComplete { get; set; } = false;
    
    // Work Schedule Settings
    public int WorkDaysPerWeek { get; set; } = 5;  // 5 = Mon-Fri, 6 = Mon-Sat
    public int WorkingDaysPerMonth { get; set; } = 22;
    public bool WorkOnHolidays { get; set; } = false;
    public int HolidayPayRate { get; set; } = 200;  // Percentage (200 = double pay)
    public decimal OvertimeRatePerHour { get; set; } = 1.25m;  // Multiplier (1.25 = 125%)
    public PayrollFrequency PayrollFrequency { get; set; } = PayrollFrequency.Monthly;

    // Default Salary Rates by Role
    public decimal? HRDailyRate { get; set; }
    public decimal? AccountantDailyRate { get; set; }
    public decimal? DefaultDailyRate { get; set; }  // Employee default

    // Navigation
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<CompanyModule> CompanyModules { get; set; } = new List<CompanyModule>();
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    public virtual ICollection<PayrollPeriod> PayrollPeriods { get; set; } = new List<PayrollPeriod>();
}
