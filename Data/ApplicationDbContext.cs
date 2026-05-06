using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayroTech.Models.Entities;

namespace PayroTech.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<CompanyModule> CompanyModules { get; set; } = null!;
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Shift> Shifts { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<Leave> Leaves { get; set; } = null!;
    public DbSet<LeaveBalance> LeaveBalances { get; set; } = null!;
    public DbSet<Overtime> Overtimes { get; set; } = null!;
    public DbSet<Holiday> Holidays { get; set; } = null!;
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; } = null!;
    public DbSet<Payroll> Payrolls { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<VendorLog> VendorLogs { get; set; } = null!;
    public DbSet<PayrollDeadline> PayrollDeadlines { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<IncidentReport> IncidentReports { get; set; } = null!;
    public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
    public DbSet<IDRequest> IDRequests { get; set; } = null!;
    public DbSet<TwoFactorCode> TwoFactorCodes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Company configuration
        builder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.CompanyCode).IsUnique();
            entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CompanyCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HRDailyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AccountantDailyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DefaultDailyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OvertimeRatePerHour).HasColumnType("decimal(5,2)");
        });

        // Employee configuration
        builder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeNumber }).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DailyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Shift)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Supervisor)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Department configuration
        builder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.DepartmentCode }).IsUnique();
            entity.Property(e => e.DepartmentName).HasMaxLength(100).IsRequired();

            entity.HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Attendance configuration
        builder.Entity<Attendance>(entity =>
        {
            entity.HasIndex(e => new { e.EmployeeId, e.Date }).IsUnique();
            entity.Property(e => e.LateDeductionAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OvertimeAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(a => a.ApprovedBy)
                .WithMany()
                .HasForeignKey(a => a.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Leave configuration
        builder.Entity<Leave>(entity =>
        {
            entity.Property(e => e.TotalDays).HasColumnType("decimal(18,2)");

            entity.HasOne(l => l.ApprovedBy)
                .WithMany()
                .HasForeignKey(l => l.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // LeaveBalance configuration
        builder.Entity<LeaveBalance>(entity =>
        {
            entity.Property(e => e.VacationLeaveBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SickLeaveBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.VacationLeaveUsed).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SickLeaveUsed).HasColumnType("decimal(18,2)");
        });

        // Overtime configuration
        builder.Entity<Overtime>(entity =>
        {
            entity.Property(e => e.Multiplier).HasColumnType("decimal(18,2)");

            entity.HasOne(o => o.ApprovedBy)
                .WithMany()
                .HasForeignKey(o => o.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Payroll configuration
        builder.Entity<Payroll>(entity =>
        {
            entity.HasIndex(e => new { e.EmployeeId, e.PayrollPeriodId }).IsUnique();

            entity.Property(e => e.BasicPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OvertimePay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.HolidayPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NightDifferentialPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Allowances).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OtherEarnings).HasColumnType("decimal(18,2)");
            entity.Property(e => e.GrossPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LateDeduction).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UndertimeDeduction).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AbsenceDeduction).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SSSContribution).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PhilHealthContribution).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PagIbigContribution).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WithholdingTax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OtherDeductions).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetPay).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DaysWorked).HasColumnType("decimal(18,2)");
            entity.Property(e => e.HoursWorked).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OvertimeHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LateHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UndertimeHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AbsentDays).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.ProcessedBy)
                .WithMany()
                .HasForeignKey(p => p.ProcessedById)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Company)
                .WithMany()
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
                
            entity.HasOne(u => u.Branch)
                .WithMany(b => b.Employees)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.NoAction);
                
            entity.HasOne(u => u.Department)
                .WithMany(d => d.StaffMembers)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);
                
            entity.Property(e => e.DailyRate).HasColumnType("decimal(18,2)");
        });
        
        // Branch configuration
        builder.Entity<Branch>(entity =>
        {
            entity.HasOne(b => b.Company)
                .WithMany()
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PayrollDeadline configuration
        builder.Entity<PayrollDeadline>(entity =>
        {
            entity.Property(pd => pd.SubmittedById).HasMaxLength(450);
            entity.Property(pd => pd.ApprovedById).HasMaxLength(450);

            entity.HasOne(pd => pd.Company)
                .WithMany()
                .HasForeignKey(pd => pd.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(pd => pd.Department)
                .WithMany()
                .HasForeignKey(pd => pd.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(pd => pd.SubmittedBy)
                .WithMany()
                .HasForeignKey(pd => pd.SubmittedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(pd => pd.ApprovedBy)
                .WithMany()
                .HasForeignKey(pd => pd.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Notification configuration
        builder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.Title).HasMaxLength(200).IsRequired();
            entity.Property(n => n.Message).HasMaxLength(1000).IsRequired();
            entity.Property(n => n.UserId).HasMaxLength(450).IsRequired();

            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IncidentReport configuration
        builder.Entity<IncidentReport>(entity =>
        {
            entity.Property(ir => ir.Title).HasMaxLength(200).IsRequired();
            entity.Property(ir => ir.Description).HasMaxLength(2000).IsRequired();
            entity.Property(ir => ir.WhatHappened).HasMaxLength(1000);
            entity.Property(ir => ir.WhyHappened).HasMaxLength(1000);
            entity.Property(ir => ir.ReportedById).HasMaxLength(450).IsRequired();
            entity.Property(ir => ir.AssignedToId).HasMaxLength(450);

            entity.HasOne(ir => ir.Company)
                .WithMany()
                .HasForeignKey(ir => ir.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(ir => ir.ReportedBy)
                .WithMany()
                .HasForeignKey(ir => ir.ReportedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(ir => ir.AssignedTo)
                .WithMany()
                .HasForeignKey(ir => ir.AssignedToId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // LoginAttempt configuration
        builder.Entity<LoginAttempt>(entity =>
        {
            entity.Property(la => la.Email).HasMaxLength(256).IsRequired();
            entity.Property(la => la.IpAddress).HasMaxLength(45).IsRequired();
            entity.Property(la => la.UserAgent).HasMaxLength(500);
            entity.Property(la => la.FailureReason).HasMaxLength(200);

            entity.HasIndex(la => new { la.Email, la.AttemptedAt });
            entity.HasIndex(la => new { la.IpAddress, la.AttemptedAt });
        });

        // IDRequest configuration
        builder.Entity<IDRequest>(entity =>
        {
            entity.Property(ir => ir.Reason).HasMaxLength(500).IsRequired();
            entity.Property(ir => ir.ApprovalNotes).HasMaxLength(500);

            entity.HasOne(ir => ir.User)
                .WithMany()
                .HasForeignKey(ir => ir.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ir => ir.ApprovedBy)
                .WithMany()
                .HasForeignKey(ir => ir.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ir => ir.Company)
                .WithMany()
                .HasForeignKey(ir => ir.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // VendorLog configuration
        builder.Entity<VendorLog>(entity =>
        {
            entity.Property(vl => vl.VendorId).HasMaxLength(450).IsRequired();
            entity.Property(vl => vl.VendorName).HasMaxLength(200).IsRequired();
            entity.Property(vl => vl.Action).HasMaxLength(100).IsRequired();
            entity.Property(vl => vl.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(vl => vl.IpAddress).HasMaxLength(45).IsRequired();
            entity.Property(vl => vl.UserAgent).HasMaxLength(500);
            entity.Property(vl => vl.RequestPath).HasMaxLength(500);

            entity.HasOne(vl => vl.Vendor)
                .WithMany()
                .HasForeignKey(vl => vl.VendorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(vl => new { vl.VendorId, vl.CreatedAt });
            entity.HasIndex(vl => vl.Action);
            entity.HasIndex(vl => vl.CreatedAt);
        });

        // TwoFactorCode configuration
        builder.Entity<TwoFactorCode>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UserId).HasMaxLength(450).IsRequired();
            entity.Property(t => t.Code).HasMaxLength(10).IsRequired();
            entity.HasIndex(t => new { t.UserId, t.IsUsed });

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
