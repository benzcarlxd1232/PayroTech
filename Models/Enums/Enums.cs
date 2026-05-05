namespace PayroTech.Models.Enums;

public enum ModuleType
{
    Attendance = 1,
    Payroll = 2,
    Leave = 3,
    Overtime = 4,
    GovernmentCompliance = 5,
    Reports = 6
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    OnLeave = 4,
    HalfDay = 5,
    Holiday = 6
}

public enum LeaveType
{
    Vacation = 1,
    Sick = 2,
    Emergency = 3,
    Maternity = 4,
    Paternity = 5,
    Unpaid = 6
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum OvertimeType
{
    Regular = 1,      // 1.25x
    RestDay = 2,      // 1.30x
    Holiday = 3,      // 2.00x
    RestDayHoliday = 4 // 2.60x
}

public enum HolidayType
{
    Regular = 1,      // 200% pay
    Special = 2       // 130% pay
}

public enum PayrollStatus
{
    Draft = 1,
    Processing = 2,
    Processed = 3,
    Approved = 4,
    Paid = 5
}

public enum SalaryType
{
    Monthly = 1,
    Daily = 2,
    Hourly = 3
}

public enum PayrollDeadlineType
{
    FirstHalf = 1,  // 15th of month
    SecondHalf = 2  // 30th/31st of month
}

public enum PayrollFrequency
{
    Monthly    = 1,  // Once a month (1st–last day)
    SemiMonthly = 2  // Twice a month (1st–15th, 16th–last day)
}

public enum PayrollDeadlineStatus
{
    Pending = 1,
    Submitted = 2,
    Approved = 3,
    Overdue = 4
}

public enum NotificationType
{
    PayrollDue = 1,
    PayrollOverdue = 2,
    BudgetApprovalNeeded = 3,
    IncidentReport = 4,
    SystemAlert = 5,
    LoginSecurity = 6,
    IDCardReady = 7   // Notifies staff their ID replacement was approved
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum IncidentType
{
    LostIDCard = 1,
    LateArrival = 2,
    AbsentWithoutNotice = 3,
    EquipmentDamage = 4,
    WorkplaceAccident = 5,
    PayrollDelay = 6,
    BudgetIssue = 7,
    SystemError = 8,
    SecurityBreach = 9,
    Other = 10
}

public enum IncidentSeverity
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}

public enum IncidentStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4
}

public enum IDRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Printed = 3
}
