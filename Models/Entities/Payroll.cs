using PayroTech.Models.Enums;

namespace PayroTech.Models.Entities;

public class Payroll : BaseEntity
{
    public int EmployeeId { get; set; }
    public int PayrollPeriodId { get; set; }

    // Earnings
    public decimal BasicPay { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal HolidayPay { get; set; }
    public decimal NightDifferentialPay { get; set; }
    public decimal Allowances { get; set; }
    public decimal OtherEarnings { get; set; }
    public decimal GrossPay { get; set; }

    // Deductions
    public decimal LateDeduction { get; set; }
    public decimal UndertimeDeduction { get; set; }
    public decimal AbsenceDeduction { get; set; }
    public decimal SSSContribution { get; set; }
    public decimal PhilHealthContribution { get; set; }
    public decimal PagIbigContribution { get; set; }
    public decimal WithholdingTax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net Pay
    public decimal NetPay { get; set; }

    // Status
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public int? ProcessedById { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Days/Hours worked
    public decimal DaysWorked { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal LateHours { get; set; }
    public decimal UndertimeHours { get; set; }
    public decimal AbsentDays { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;
    public virtual Employee? ProcessedBy { get; set; }
}
