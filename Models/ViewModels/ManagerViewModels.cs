using PayroTech.Models.Entities;

namespace PayroTech.Models.ViewModels;

public class ManagerDashboardViewModel
{
    public string CompanyName { get; set; } = string.Empty;
    public int TotalTeams { get; set; }
    public int TotalEmployees { get; set; }  // Employee role only
    public int TotalStaff { get; set; }      // All roles (HR + Accountant + Employee)
    public decimal TotalBudget { get; set; }
    public int PendingApprovals { get; set; }
    
    // Staff counts
    public int HRCount { get; set; }
    public int AccountantCount { get; set; }
    
    // Warnings
    public bool NeedsHR => HRCount == 0;
    public bool NeedsAccountant => AccountantCount == 0;
    public bool NeedsInitialSetup { get; set; }
    
    // Recent data
    public List<TeamBudgetInfo> TeamBudgets { get; set; } = new();
    public List<PayrollSubmissionInfo> PendingPayrolls { get; set; } = new();
    public List<IncidentInfo> ActiveIncidents { get; set; } = new();
}

public class TeamBudgetInfo
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AllocatedBudget { get; set; }
    public decimal UsedBudget { get; set; }
    public decimal RemainingBudget => AllocatedBudget - UsedBudget;
    public int UtilizationPercent => AllocatedBudget > 0 ? (int)((UsedBudget / AllocatedBudget) * 100) : 0;
    public string StatusClass => UtilizationPercent >= 95 ? "danger" : UtilizationPercent >= 85 ? "warning" : "safe";
}

public class PayrollSubmissionInfo
{
    public int PayrollId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string HRName { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public int EmployeeCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLate { get; set; }
}

public class IncidentInfo
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;  // Late Submission, Budget Request, Absence Report
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal? RequestedAmount { get; set; }
    public string? Explanation { get; set; }
    public string Status { get; set; } = "Pending";
    public string Severity { get; set; } = "Normal";  // High, Normal, Low
}

public class InitialSetupViewModel
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    
    // Work Schedule Settings
    public int WorkDaysPerWeek { get; set; } = 5;
    public int WorkingDaysPerMonth { get; set; } = 22;
    public bool WorkOnHolidays { get; set; } = false;
    public int HolidayPayRate { get; set; } = 200;       // 200 = double pay (200%)
    public decimal OvertimeRatePerHour { get; set; } = 1.25m; // 1.25 = 125% per hour
    
    // Salary Settings by Role
    public decimal HRDailyRate { get; set; } = 800;
    public decimal AccountantDailyRate { get; set; } = 800;
    public decimal DefaultDailyRate { get; set; } = 500;
    
    // HR Account
    public string? HRFirstName { get; set; }
    public string? HRLastName { get; set; }
    public string? HREmail { get; set; }
    public int? HRBirthYear { get; set; }
    
    // Accountant Account
    public string? AccountantFirstName { get; set; }
    public string? AccountantLastName { get; set; }
    public string? AccountantEmail { get; set; }
    public int? AccountantBirthYear { get; set; }
}

public class CompanySettingsViewModel
{
    public int WorkDaysPerWeek { get; set; } = 5;
    public int WorkingDaysPerMonth { get; set; } = 22;
    public bool WorkOnHolidays { get; set; } = false;
    public int HolidayPayRate { get; set; } = 200;
    public decimal OvertimeRatePerHour { get; set; } = 1.25m;
    public decimal HRDailyRate { get; set; } = 800;
    public decimal AccountantDailyRate { get; set; } = 800;
    public decimal DefaultDailyRate { get; set; } = 500;
}
