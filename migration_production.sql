IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] int NOT NULL IDENTITY,
        [CompanyCode] nvarchar(50) NOT NULL,
        [CompanyName] nvarchar(200) NOT NULL,
        [Address] nvarchar(max) NULL,
        [ContactNumber] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [TIN] nvarchar(max) NULL,
        [SSSEmployerNumber] nvarchar(max) NULL,
        [PhilHealthEmployerNumber] nvarchar(max) NULL,
        [PagIbigEmployerNumber] nvarchar(max) NULL,
        [SubscriptionStart] datetime2 NULL,
        [SubscriptionEnd] datetime2 NULL,
        [IsSubscriptionActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [CompanyId] int NULL,
        [Role] int NOT NULL,
        [IsActive] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [CompanyModules] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [ModuleType] int NOT NULL,
        [IsEnabled] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CompanyModules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyModules_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Holidays] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [HolidayName] nvarchar(max) NOT NULL,
        [Date] datetime2 NOT NULL,
        [HolidayType] int NOT NULL,
        [IsNationwide] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Holidays] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Holidays_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [PayrollPeriods] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [PeriodName] nvarchar(max) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [PayDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [ProcessedById] int NULL,
        [ProcessedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PayrollPeriods] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayrollPeriods_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Shifts] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [ShiftName] nvarchar(max) NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [BreakStart] time NULL,
        [BreakEnd] time NULL,
        [GracePeriodMinutes] int NOT NULL,
        [IsNightShift] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Shifts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Shifts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NULL,
        [UserId] nvarchar(450) NULL,
        [Action] nvarchar(max) NOT NULL,
        [EntityName] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_AuditLogs_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Attendances] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [TimeIn] datetime2 NULL,
        [TimeOut] datetime2 NULL,
        [Status] int NOT NULL,
        [LateMinutes] int NOT NULL,
        [UndertimeMinutes] int NOT NULL,
        [OvertimeMinutes] int NOT NULL,
        [WorkedMinutes] int NOT NULL,
        [NightDifferentialMinutes] int NOT NULL,
        [IsApproved] bit NOT NULL,
        [ApprovedById] int NULL,
        [ApprovedAt] datetime2 NULL,
        [IsLocked] bit NOT NULL,
        [Remarks] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [DepartmentCode] nvarchar(450) NOT NULL,
        [DepartmentName] nvarchar(100) NOT NULL,
        [ManagerId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Departments_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [EmployeeNumber] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [MiddleName] nvarchar(max) NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Suffix] nvarchar(max) NULL,
        [DateOfBirth] datetime2 NULL,
        [Gender] nvarchar(max) NULL,
        [CivilStatus] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [ContactNumber] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [HireDate] datetime2 NOT NULL,
        [RegularizationDate] datetime2 NULL,
        [SeparationDate] datetime2 NULL,
        [SeparationReason] nvarchar(max) NULL,
        [BasicSalary] decimal(18,2) NOT NULL,
        [SalaryType] int NOT NULL,
        [DailyRate] decimal(18,2) NULL,
        [HourlyRate] decimal(18,2) NULL,
        [SSSNumber] nvarchar(max) NULL,
        [PhilHealthNumber] nvarchar(max) NULL,
        [PagIbigNumber] nvarchar(max) NULL,
        [TINNumber] nvarchar(max) NULL,
        [BankName] nvarchar(max) NULL,
        [BankAccountNumber] nvarchar(max) NULL,
        [DepartmentId] int NULL,
        [ShiftId] int NULL,
        [SupervisorId] int NULL,
        [UserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Employees_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
        CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]),
        CONSTRAINT [FK_Employees_Employees_SupervisorId] FOREIGN KEY ([SupervisorId]) REFERENCES [Employees] ([Id]),
        CONSTRAINT [FK_Employees_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shifts] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaveBalances] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [Year] int NOT NULL,
        [VacationLeaveBalance] decimal(18,2) NOT NULL,
        [SickLeaveBalance] decimal(18,2) NOT NULL,
        [VacationLeaveUsed] decimal(18,2) NOT NULL,
        [SickLeaveUsed] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_LeaveBalances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LeaveBalances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Leaves] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [LeaveType] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [TotalDays] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ApprovedById] int NULL,
        [ApprovedAt] datetime2 NULL,
        [ApproverRemarks] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Leaves] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Leaves_Employees_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [Employees] ([Id]),
        CONSTRAINT [FK_Leaves_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Overtimes] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [TotalMinutes] int NOT NULL,
        [OvertimeType] int NOT NULL,
        [Multiplier] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ApprovedById] int NULL,
        [ApprovedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Overtimes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Overtimes_Employees_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [Employees] ([Id]),
        CONSTRAINT [FK_Overtimes_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE TABLE [Payrolls] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [PayrollPeriodId] int NOT NULL,
        [BasicPay] decimal(18,2) NOT NULL,
        [OvertimePay] decimal(18,2) NOT NULL,
        [HolidayPay] decimal(18,2) NOT NULL,
        [NightDifferentialPay] decimal(18,2) NOT NULL,
        [Allowances] decimal(18,2) NOT NULL,
        [OtherEarnings] decimal(18,2) NOT NULL,
        [GrossPay] decimal(18,2) NOT NULL,
        [LateDeduction] decimal(18,2) NOT NULL,
        [UndertimeDeduction] decimal(18,2) NOT NULL,
        [AbsenceDeduction] decimal(18,2) NOT NULL,
        [SSSContribution] decimal(18,2) NOT NULL,
        [PhilHealthContribution] decimal(18,2) NOT NULL,
        [PagIbigContribution] decimal(18,2) NOT NULL,
        [WithholdingTax] decimal(18,2) NOT NULL,
        [OtherDeductions] decimal(18,2) NOT NULL,
        [TotalDeductions] decimal(18,2) NOT NULL,
        [NetPay] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [ProcessedById] int NULL,
        [ProcessedAt] datetime2 NULL,
        [DaysWorked] decimal(18,2) NOT NULL,
        [HoursWorked] decimal(18,2) NOT NULL,
        [OvertimeHours] decimal(18,2) NOT NULL,
        [LateHours] decimal(18,2) NOT NULL,
        [UndertimeHours] decimal(18,2) NOT NULL,
        [AbsentDays] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Payrolls] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payrolls_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Payrolls_Employees_ProcessedById] FOREIGN KEY ([ProcessedById]) REFERENCES [Employees] ([Id]),
        CONSTRAINT [FK_Payrolls_PayrollPeriods_PayrollPeriodId] FOREIGN KEY ([PayrollPeriodId]) REFERENCES [PayrollPeriods] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_CompanyId] ON [AspNetUsers] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Attendances_ApprovedById] ON [Attendances] ([ApprovedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Attendances_EmployeeId_Date] ON [Attendances] ([EmployeeId], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CompanyId] ON [AuditLogs] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Companies_CompanyCode] ON [Companies] ([CompanyCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CompanyModules_CompanyId] ON [CompanyModules] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_CompanyId_DepartmentCode] ON [Departments] ([CompanyId], [DepartmentCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_ManagerId] ON [Departments] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_CompanyId_EmployeeNumber] ON [Employees] ([CompanyId], [EmployeeNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_ShiftId] ON [Employees] ([ShiftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_SupervisorId] ON [Employees] ([SupervisorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Employees_UserId] ON [Employees] ([UserId]) WHERE [UserId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Holidays_CompanyId] ON [Holidays] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveBalances_EmployeeId] ON [LeaveBalances] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Leaves_ApprovedById] ON [Leaves] ([ApprovedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Leaves_EmployeeId] ON [Leaves] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Overtimes_ApprovedById] ON [Overtimes] ([ApprovedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Overtimes_EmployeeId] ON [Overtimes] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayrollPeriods_CompanyId] ON [PayrollPeriods] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payrolls_EmployeeId_PayrollPeriodId] ON [Payrolls] ([EmployeeId], [PayrollPeriodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payrolls_PayrollPeriodId] ON [Payrolls] ([PayrollPeriodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payrolls_ProcessedById] ON [Payrolls] ([ProcessedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Shifts_CompanyId] ON [Shifts] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    ALTER TABLE [Attendances] ADD CONSTRAINT [FK_Attendances_Employees_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [Employees] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    ALTER TABLE [Attendances] ADD CONSTRAINT [FK_Attendances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    ALTER TABLE [Departments] ADD CONSTRAINT [FK_Departments_Employees_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Employees] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325131758_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260325131758_InitialCreate', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403112000_AddKioskAttendanceFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [QRCodeHash] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403112000_AddKioskAttendanceFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [FaceEncodingData] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403112000_AddKioskAttendanceFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [QRCodeGeneratedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403112000_AddKioskAttendanceFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsFaceEnrolled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403112000_AddKioskAttendanceFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403112000_AddKioskAttendanceFields', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403121754_SyncModelChanges'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'QRCodeHash');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [QRCodeHash] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403121754_SyncModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403121754_SyncModelChanges', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403123610_AllModelChanges'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [BirthDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403123610_AllModelChanges'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403123610_AllModelChanges'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [RequiresFaceEnrollment] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403123610_AllModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403123610_AllModelChanges', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260404000000_AddCompanySetupFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [IsInitialSetupComplete] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260404000000_AddCompanySetupFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [DefaultDailyRate] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260404000000_AddCompanySetupFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260404000000_AddCompanySetupFields', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [WorkDaysPerWeek] int NOT NULL DEFAULT 5;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [WorkingDaysPerMonth] int NOT NULL DEFAULT 22;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [WorkOnHolidays] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [HolidayPayRate] int NOT NULL DEFAULT 200;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [HRDailyRate] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    ALTER TABLE [Companies] ADD [AccountantDailyRate] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406000000_AddCompanyWorkScheduleFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406000000_AddCompanyWorkScheduleFields', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Address] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [BranchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CivilStatus] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ContactNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DailyRate] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [EmergencyContactName] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [EmergencyContactNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [EmergencyContactRelation] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [EmployeeNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [FaceImagePath] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Gender] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [MiddleName] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [StartDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] int NOT NULL IDENTITY,
        [BranchName] nvarchar(100) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Province] nvarchar(100) NOT NULL,
        [ContactNumber] nvarchar(20) NOT NULL,
        [IsMainBranch] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CompanyId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Branches_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_BranchId] ON [AspNetUsers] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    CREATE INDEX [IX_Branches_CompanyId] ON [Branches] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410154629_AddUserProfileFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410154629_AddUserProfileFields', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410173045_AddDepartmentIdToApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DepartmentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410173045_AddDepartmentIdToApplicationUser'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_DepartmentId] ON [AspNetUsers] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410173045_AddDepartmentIdToApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410173045_AddDepartmentIdToApplicationUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410173045_AddDepartmentIdToApplicationUser', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413082731_AddDepartmentIdIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260413082731_AddDepartmentIdIndex', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413092039_FixCompanyDecimalPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260413092039_FixCompanyDecimalPrecision', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [FaceEnrollmentCompletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [FirstLoginSetupCompletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [MiddleInitial] nvarchar(1) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE TABLE [IncidentReports] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [ReportedById] nvarchar(450) NOT NULL,
        [AssignedToId] nvarchar(450) NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [Type] int NOT NULL,
        [Severity] int NOT NULL,
        [Status] int NOT NULL,
        [WhenHappened] datetime2 NULL,
        [WhatHappened] nvarchar(1000) NOT NULL,
        [WhyHappened] nvarchar(1000) NOT NULL,
        [ResolvedAt] datetime2 NULL,
        [Resolution] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_IncidentReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IncidentReports_AspNetUsers_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_IncidentReports_AspNetUsers_ReportedById] FOREIGN KEY ([ReportedById]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_IncidentReports_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE TABLE [LoginAttempts] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(256) NOT NULL,
        [IpAddress] nvarchar(45) NOT NULL,
        [UserAgent] nvarchar(500) NOT NULL,
        [IsSuccessful] bit NOT NULL,
        [FailureReason] nvarchar(200) NULL,
        [AttemptedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_LoginAttempts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] int NOT NULL,
        [Priority] int NOT NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [ActionUrl] nvarchar(max) NULL,
        [RelatedEntityType] nvarchar(max) NULL,
        [RelatedEntityId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE TABLE [PayrollDeadlines] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [DepartmentId] int NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [Type] int NOT NULL,
        [Status] int NOT NULL,
        [SubmittedById] nvarchar(450) NULL,
        [SubmittedAt] datetime2 NULL,
        [ApprovedById] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PayrollDeadlines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayrollDeadlines_AspNetUsers_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_PayrollDeadlines_AspNetUsers_SubmittedById] FOREIGN KEY ([SubmittedById]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_PayrollDeadlines_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
        CONSTRAINT [FK_PayrollDeadlines_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_IncidentReports_AssignedToId] ON [IncidentReports] ([AssignedToId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_IncidentReports_CompanyId] ON [IncidentReports] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_IncidentReports_ReportedById] ON [IncidentReports] ([ReportedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_LoginAttempts_Email_AttemptedAt] ON [LoginAttempts] ([Email], [AttemptedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_LoginAttempts_IpAddress_AttemptedAt] ON [LoginAttempts] ([IpAddress], [AttemptedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_PayrollDeadlines_ApprovedById] ON [PayrollDeadlines] ([ApprovedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_PayrollDeadlines_CompanyId] ON [PayrollDeadlines] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_PayrollDeadlines_DepartmentId] ON [PayrollDeadlines] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    CREATE INDEX [IX_PayrollDeadlines_SubmittedById] ON [PayrollDeadlines] ([SubmittedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413124824_AddUIUXEnhancements'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260413124824_AddUIUXEnhancements', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'EmergencyContactNumber');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [EmergencyContactNumber] nvarchar(11) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'ContactNumber');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [ContactNumber] nvarchar(11) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    CREATE TABLE [IDRequests] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] int NOT NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [ApprovalNotes] nvarchar(500) NULL,
        [CompanyId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_IDRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IDRequests_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_IDRequests_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_IDRequests_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    CREATE INDEX [IX_IDRequests_ApprovedByUserId] ON [IDRequests] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    CREATE INDEX [IX_IDRequests_CompanyId] ON [IDRequests] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    CREATE INDEX [IX_IDRequests_UserId] ON [IDRequests] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414101350_UpdateIDRequestStatusEnum'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414101350_UpdateIDRequestStatusEnum', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414105419_AddIDRequestPrintTracking'
)
BEGIN
    ALTER TABLE [IDRequests] ADD [IsArchived] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414105419_AddIDRequestPrintTracking'
)
BEGIN
    ALTER TABLE [IDRequests] ADD [IsPrinted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414105419_AddIDRequestPrintTracking'
)
BEGIN
    ALTER TABLE [IDRequests] ADD [PrintedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414105419_AddIDRequestPrintTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414105419_AddIDRequestPrintTracking', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415124940_AddVendorLogTable'
)
BEGIN
    CREATE TABLE [VendorLogs] (
        [Id] int NOT NULL IDENTITY,
        [VendorId] nvarchar(450) NOT NULL,
        [VendorName] nvarchar(200) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(45) NOT NULL,
        [UserAgent] nvarchar(500) NULL,
        [RequestPath] nvarchar(500) NULL,
        [IsSuccess] bit NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_VendorLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VendorLogs_AspNetUsers_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415124940_AddVendorLogTable'
)
BEGIN
    CREATE INDEX [IX_VendorLogs_Action] ON [VendorLogs] ([Action]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415124940_AddVendorLogTable'
)
BEGIN
    CREATE INDEX [IX_VendorLogs_CreatedAt] ON [VendorLogs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415124940_AddVendorLogTable'
)
BEGIN
    CREATE INDEX [IX_VendorLogs_VendorId_CreatedAt] ON [VendorLogs] ([VendorId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260415124940_AddVendorLogTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260415124940_AddVendorLogTable', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423193316_SyncAllModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260423193316_SyncAllModelChanges', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424074156_AddBloodTypeToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424074156_AddBloodTypeToUser', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424074340_SyncBloodType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424074340_SyncBloodType', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424074439_FinalSync'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [BloodType] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424074439_FinalSync'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424074439_FinalSync', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424154533_AddOvertimeRateToCompany'
)
BEGIN
    ALTER TABLE [Companies] ADD [OvertimeRatePerHour] decimal(5,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424154533_AddOvertimeRateToCompany'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424154533_AddOvertimeRateToCompany', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424193516_AddStaffCodeAndKioskPin'
)
BEGIN
    ALTER TABLE [Attendances] ADD [LateDeductionAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424193516_AddStaffCodeAndKioskPin'
)
BEGIN
    ALTER TABLE [Attendances] ADD [OvertimeAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424193516_AddStaffCodeAndKioskPin'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [KioskPin] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424193516_AddStaffCodeAndKioskPin'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [StaffCode] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424193516_AddStaffCodeAndKioskPin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424193516_AddStaffCodeAndKioskPin', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424203739_AddShiftIdToUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ShiftId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260424203739_AddShiftIdToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260424203739_AddShiftIdToUser', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427185344_AddCreatedAtAndDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260427185344_AddCreatedAtAndDetails', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429103236_AddAuditLogDetailsAndUserAgent'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [Details] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429103236_AddAuditLogDetailsAndUserAgent'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [UserAgent] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429103236_AddAuditLogDetailsAndUserAgent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429103236_AddAuditLogDetailsAndUserAgent', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505180530_AddPayrollFrequency'
)
BEGIN
    ALTER TABLE [Companies] ADD [PayrollFrequency] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505180530_AddPayrollFrequency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260505180530_AddPayrollFrequency', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506182453_Add2FAAndLockout'
)
BEGIN
    CREATE TABLE [TwoFactorCodes] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Code] nvarchar(10) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsUsed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TwoFactorCodes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TwoFactorCodes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506182453_Add2FAAndLockout'
)
BEGIN
    CREATE INDEX [IX_TwoFactorCodes_UserId_IsUsed] ON [TwoFactorCodes] ([UserId], [IsUsed]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506182453_Add2FAAndLockout'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506182453_Add2FAAndLockout', N'9.0.0');
END;

COMMIT;
GO

