namespace PayroTech.Models.ViewModels;

public class StandardTableViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi bi-table";
    public string CssClass { get; set; } = string.Empty;
    public string EmptyMessage { get; set; } = "No data available";
    
    public List<StandardTableColumn> Columns { get; set; } = new();
    public List<List<string>> Data { get; set; } = new();
    public StandardTablePagination? Pagination { get; set; }
}

public class StandardTableColumn
{
    public string Header { get; set; } = string.Empty;
    public bool IsActionColumn { get; set; } = false;
    public string CssClass { get; set; } = string.Empty;
    public int? MaxWidth { get; set; }
}

public class StandardTablePagination
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; } = 0;
    public string OnPageChangeFunction { get; set; } = "changePage";
}