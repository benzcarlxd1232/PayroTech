using System.ComponentModel.DataAnnotations;

namespace PayroTech.Models.ViewModels;

public class TeamManagementViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int HRCount { get; set; }
    public int AccountantCount { get; set; }
    public int EmployeeCount { get; set; }
    public decimal MonthlyBudget { get; set; }
    public decimal UsedBudget { get; set; }
    public string HRAssigned { get; set; } = string.Empty;
    
    public decimal BudgetPercentage => MonthlyBudget > 0 ? (UsedBudget / MonthlyBudget * 100) : 0;
    public decimal RemainingBudget => MonthlyBudget - UsedBudget;
}

public class CreateTeamRequest
{
    [Required(ErrorMessage = "Team name is required")]
    [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    public string Description { get; set; } = string.Empty;
    
    public string? AssignedHRId { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
    public decimal MonthlyBudget { get; set; }
}