using PayroTech.Models.Enums;

namespace PayroTech.Models.ViewModels;

public class SuperAdminDashboardViewModel
{
    public int TotalCompanies { get; set; }
    public int ActiveCompanies { get; set; }
    public int TotalUsers { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalStaff { get; set; }          // All users (employees + HR + accountants + admins)
    public int TotalHR { get; set; }
    public int TotalAccountants { get; set; }
    public int TotalCompanyAdmins { get; set; }
    public List<CompanyListItem> RecentCompanies { get; set; } = new();
    public List<CompanyListItem> ExpiringSubscriptions { get; set; } = new();
}

public class CompanyListItem
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsSubscriptionActive { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateCompanyViewModel
{
    public string CompanyCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? TIN { get; set; }
    public string? SSSEmployerNumber { get; set; }
    public string? PhilHealthEmployerNumber { get; set; }
    public string? PagIbigEmployerNumber { get; set; }
    public DateTime SubscriptionStart { get; set; } = DateTime.Today;
    public DateTime SubscriptionEnd { get; set; } = DateTime.Today.AddYears(1);
    public List<ModuleType>? EnabledModules { get; set; }

    // Admin credentials
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
    public string? AdminFirstName { get; set; }
    public string? AdminLastName { get; set; }
}

public class EditCompanyViewModel
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? TIN { get; set; }
    public string? SSSEmployerNumber { get; set; }
    public string? PhilHealthEmployerNumber { get; set; }
    public string? PagIbigEmployerNumber { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public bool IsActive { get; set; }
    public bool IsSubscriptionActive { get; set; }
    public List<ModuleType>? EnabledModules { get; set; }
}

public class ModuleInfo
{
    public ModuleType ModuleType { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CompaniesUsingCount { get; set; }
}
