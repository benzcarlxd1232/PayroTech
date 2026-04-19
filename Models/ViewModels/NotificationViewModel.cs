namespace PayroTech.Models.ViewModels;

public class NotificationViewModel
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // "NewCompany", "FailedLogin", "General"
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-bell";
    public string? Link { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class NotificationSummary
{
    public int UnreadCount { get; set; }
    public List<NotificationViewModel> RecentNotifications { get; set; } = new();
}
