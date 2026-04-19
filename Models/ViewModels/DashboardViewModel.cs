namespace PayroTech.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalCompanies { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int TodayAttendance { get; set; }
    public List<EmployeeListItem> RecentEmployees { get; set; } = new();
}

public class EmployeeListItem
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}
