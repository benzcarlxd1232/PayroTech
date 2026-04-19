using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class PayrollDeadline : BaseEntity
{
    public int CompanyId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime DueDate { get; set; }
    public PayrollDeadlineType Type { get; set; } // FirstHalf, SecondHalf
    public PayrollDeadlineStatus Status { get; set; } = PayrollDeadlineStatus.Pending;
    public string? SubmittedById { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual ApplicationUser? SubmittedBy { get; set; }
    public virtual ApplicationUser? ApprovedBy { get; set; }
}

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
}

public class IncidentReport : BaseEntity
{
    public int CompanyId { get; set; }
    public string ReportedById { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentType Type { get; set; }
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Normal;
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    
    // Incident Report Questions
    public DateTime? WhenHappened { get; set; }
    public string WhatHappened { get; set; } = string.Empty;
    public string WhyHappened { get; set; } = string.Empty;
    
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual ApplicationUser ReportedBy { get; set; } = null!;
    public virtual ApplicationUser? AssignedTo { get; set; }
}

public class LoginAttempt : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}