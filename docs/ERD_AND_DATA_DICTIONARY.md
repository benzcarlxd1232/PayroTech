# PayroTech - Entity Relationship Diagram (ERD) & Data Dictionary

## Table of Contents
1. [Entity Relationship Diagram](#entity-relationship-diagram)
2. [Data Dictionary](#data-dictionary)
3. [Relationships Summary](#relationships-summary)

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PAYROTECH DATABASE SCHEMA                          │
└─────────────────────────────────────────────────────────────────────────────┘

                                    ┌──────────────┐
                                    │   Company    │
                                    ├──────────────┤
                                    │ Id (PK)      │
                                    │ CompanyCode  │
                                    │ CompanyName  │
                                    │ Address      │
                                    │ ContactNumber│
                                    │ Email        │
                                    │ TIN          │
                                    │ IsActive     │
                                    └──────┬───────┘
                                           │
                    ┌──────────────────────┼──────────────────────┐
                    │                      │                      │
                    ▼                      ▼                      ▼
            ┌──────────────┐      ┌──────────────┐      ┌──────────────┐
            │   Branch     │      │  Department  │      │    Shift     │
            ├──────────────┤      ├──────────────┤      ├──────────────┤
            │ Id (PK)      │      │ Id (PK)      │      │ Id (PK)      │
            │ CompanyId(FK)│      │ CompanyId(FK)│      │ CompanyId(FK)│
            │ BranchName   │      │ DeptCode     │      │ ShiftName    │
            │ Address      │      │ DeptName     │      │ StartTime    │
            │ City         │      │ ManagerId(FK)│      │ EndTime      │
            │ IsMainBranch │      │ IsActive     │      │ GracePeriod  │
            └──────┬───────┘      └──────┬───────┘      └──────┬───────┘
                   │                     │                      │
                   └─────────────────────┼──────────────────────┘
                                         │
                                         ▼
                              ┌──────────────────┐
                              │ ApplicationUser  │
                              ├──────────────────┤
                              │ Id (PK)          │
                              │ UserName         │
                              │ Email            │
                              │ PasswordHash     │
                              │ FirstName        │
                              │ LastName         │
                              │ MiddleName       │
                              │ CompanyId (FK)   │
                              │ BranchId (FK)    │
                              │ DepartmentId(FK) │
                              │ Role (Enum)      │
                              │ IsActive         │
                              │ StaffCode        │
                              │ KioskPin         │
                              │ QRCodeHash       │
                              │ FaceEncodingData │
                              │ FaceImagePath    │
                              │ IsFaceEnrolled   │
                              │ MustChangePassword│
                              └────────┬─────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
                    ▼                  ▼                  ▼
            ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
            │   Employee   │   │  AuditLog    │   │  IDRequest   │
            ├──────────────┤   ├──────────────┤   ├──────────────┤
            │ Id (PK)      │   │ Id (PK)      │   │ Id (PK)      │
            │ CompanyId(FK)│   │ UserId (FK)  │   │ UserId (FK)  │
            │ UserId (FK)  │   │ CompanyId(FK)│   │ CompanyId(FK)│
            │ EmpNumber    │   │ Action       │   │ Reason       │
            │ FirstName    │   │ EntityName   │   │ Status       │
            │ LastName     │   │ EntityId     │   │ ApprovedBy   │
            │ DeptId (FK)  │   │ OldValues    │   │ ApprovedAt   │
            │ ShiftId (FK) │   │ NewValues    │   │ IsPrinted    │
            │ BasicSalary  │   │ IpAddress    │   │ PrintedAt    │
            │ DailyRate    │   │ Timestamp    │   │ IsArchived   │
            │ HourlyRate   │   └──────────────┘   └──────────────┘
            │ SSSNumber    │
            │ TINNumber    │
            │ IsActive     │
            └──────┬───────┘
                   │
    ┌──────────────┼──────────────┬──────────────┬──────────────┐
    │              │              │              │              │
    ▼              ▼              ▼              ▼              ▼
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
│Attendance│  │  Leave   │  │ Overtime │  │ Payroll  │  │ Holiday  │
├──────────┤  ├──────────┤  ├──────────┤  ├──────────┤  ├──────────┤
│Id (PK)   │  │Id (PK)   │  │Id (PK)   │  │Id (PK)   │  │Id (PK)   │
│EmpId(FK) │  │EmpId(FK) │  │EmpId(FK) │  │EmpId(FK) │  │CompId(FK)│
│Date      │  │LeaveType │  │Date      │  │PeriodId  │  │Name      │
│TimeIn    │  │StartDate │  │StartTime │  │BasicPay  │  │Date      │
│TimeOut   │  │EndDate   │  │EndTime   │  │OTPay     │  │Type      │
│Status    │  │TotalDays │  │TotalMins │  │GrossPay  │  │IsNation  │
│LateMins  │  │Reason    │  │OTType    │  │Deductions│  │IsActive  │
│OTMins    │  │Status    │  │Multiplier│  │NetPay    │  └──────────┘
│WorkedMins│  │ApprovedBy│  │Status    │  │Status    │
│IsApproved│  │ApprovedAt│  │ApprovedBy│  │Processed │
└──────────┘  └──────────┘  └──────────┘  └──────────┘

                    ┌──────────────────┐
                    │  Notification    │
                    ├──────────────────┤
                    │ Id (PK)          │
                    │ UserId (FK)      │
                    │ Title            │
                    │ Message          │
                    │ Type (Enum)      │
                    │ Priority (Enum)  │
                    │ IsRead           │
                    │ ReadAt           │
                    │ ActionUrl        │
                    │ CreatedAt        │
                    └──────────────────┘

                    ┌──────────────────┐
                    │ IncidentReport   │
                    ├──────────────────┤
                    │ Id (PK)          │
                    │ CompanyId (FK)   │
                    │ ReportedById(FK) │
                    │ AssignedToId(FK) │
                    │ Title            │
                    │ Description      │
                    │ Type (Enum)      │
                    │ Severity (Enum)  │
                    │ Status (Enum)    │
                    │ WhenHappened     │
                    │ WhatHappened     │
                    │ WhyHappened      │
                    │ Resolution       │
                    └──────────────────┘

            ┌──────────────────┐
            │   VendorLog      │◄──────────────┐
            ├──────────────────┤               │
            │ Id (PK)          │               │
            │ VendorId (FK)    │───────────────┘
            │ VendorName       │        (Links to ApplicationUser)
            │ Action           │
            │ EntityType       │
            │ EntityId         │
            │ Description      │
            │ OldValues        │
            │ NewValues        │
            │ IpAddress        │
            │ IsSuccess        │
            │ CreatedAt        │
            └──────────────────┘
```

---

## Data Dictionary

### 1. ApplicationUser (AspNetUsers)
**Description:** Core user authentication and profile table. Extends ASP.NET Identity.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | string | No | Primary key, unique user identifier | PK |
| UserName | string(256) | No | Login username | Unique, Indexed |
| Email | string(256) | No | User email address | Unique, Indexed |
| PasswordHash | string | No | Hashed password (PBKDF2) | - |
| FirstName | string | No | User's first name | - |
| MiddleName | string | Yes | User's middle name | - |
| MiddleInitial | string(1) | Yes | Middle initial (single letter) | Regex: [A-Za-z] |
| LastName | string | No | User's last name | - |
| CompanyId | int | Yes | Foreign key to Company | FK |
| BranchId | int | Yes | Foreign key to Branch | FK |
| DepartmentId | int | Yes | Foreign key to Department | FK |
| Role | enum | No | User role (1-6) | See UserRole enum |
| IsActive | bool | No | Account active status | Default: true |
| LastLoginAt | datetime | Yes | Last successful login timestamp | - |
| CreatedAt | datetime | No | Account creation timestamp | Default: UTC now |
| BirthDate | datetime | Yes | Date of birth | - |
| Gender | string | Yes | Gender | - |
| CivilStatus | string | Yes | Marital status | - |
| Address | string | Yes | Home address | - |
| ContactNumber | string(11) | Yes | Phone number | Regex: ^09\d{9}$ |
| EmployeeNumber | string | Yes | Legacy employee number | - |
| DailyRate | decimal | No | Daily salary rate | Default: 0 |
| StartDate | datetime | Yes | Employment start date | - |
| EmergencyContactName | string | Yes | Emergency contact person | - |
| EmergencyContactRelation | string | Yes | Relationship to emergency contact | - |
| EmergencyContactNumber | string(11) | Yes | Emergency contact phone | Regex: ^09\d{9}$ |
| FaceImagePath | string | Yes | Path to face photo file | - |
| BloodType | string | Yes | Blood type (A+, O-, etc.) | - |
| MustChangePassword | bool | No | Force password change flag | Default: false |
| RequiresFaceEnrollment | bool | No | Face enrollment required flag | Default: false |
| FaceEnrollmentCompletedAt | datetime | Yes | Face enrollment completion timestamp | - |
| FirstLoginSetupCompletedAt | datetime | Yes | First login setup completion timestamp | - |
| QRCodeHash | string | Yes | Unique QR code for kiosk attendance | Unique |
| FaceEncodingData | byte[] | Yes | Binary face recognition data | - |
| QRCodeGeneratedAt | datetime | Yes | QR code generation timestamp | - |
| IsFaceEnrolled | bool | No | Face enrollment status | Default: false |
| StaffCode | string | Yes | Staff code (HR-001, ACC-001, etc.) | - |
| KioskPin | string | Yes | 4-digit kiosk PIN | - |
| ShiftId | int | Yes | Assigned shift for attendance | FK |

**Indexes:**
- PK: Id
- Unique: UserName, Email, QRCodeHash
- FK: CompanyId, BranchId, DepartmentId, ShiftId

---

### 2. Company
**Description:** Multi-tenant company/organization master table.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyCode | string | No | Unique company code | Unique |
| CompanyName | string | No | Company name | - |
| Address | string | Yes | Company address | - |
| ContactNumber | string | Yes | Company phone | - |
| Email | string | Yes | Company email | - |
| TIN | string | Yes | Tax Identification Number | - |
| SSSEmployerNumber | string | Yes | SSS employer ID | - |
| PhilHealthEmployerNumber | string | Yes | PhilHealth employer ID | - |
| PagIbigEmployerNumber | string | Yes | Pag-IBIG employer ID | - |
| SubscriptionStart | datetime | Yes | Subscription start date | - |
| SubscriptionEnd | datetime | Yes | Subscription end date | - |
| IsSubscriptionActive | bool | No | Subscription active status | Default: true |
| IsInitialSetupComplete | bool | No | Initial setup completion flag | Default: false |
| WorkDaysPerWeek | int | No | Working days per week (5 or 6) | Default: 5 |
| WorkingDaysPerMonth | int | No | Working days per month | Default: 22 |
| WorkOnHolidays | bool | No | Work on holidays flag | Default: false |
| HolidayPayRate | int | No | Holiday pay percentage | Default: 200 |
| OvertimeRatePerHour | decimal | No | Overtime multiplier | Default: 1.25 |
| HRDailyRate | decimal | Yes | Default HR daily rate | - |
| AccountantDailyRate | decimal | Yes | Default Accountant daily rate | - |
| DefaultDailyRate | decimal | Yes | Default Employee daily rate | - |
| IsActive | bool | No | Company active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- Unique: CompanyCode

---

### 3. Branch
**Description:** Company branch/location table.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| BranchName | string(100) | No | Branch name | - |
| Address | string(500) | Yes | Branch address | - |
| City | string(100) | Yes | City | - |
| Province | string(100) | Yes | Province | - |
| ContactNumber | string(20) | Yes | Branch phone | - |
| IsMainBranch | bool | No | Main branch flag | Default: false |
| IsActive | bool | No | Branch active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId

---

### 4. Department
**Description:** Company department/division table.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| DepartmentCode | string | No | Department code | - |
| DepartmentName | string | No | Department name | - |
| ManagerId | int | Yes | Foreign key to Employee (manager) | FK |
| IsActive | bool | No | Department active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId, ManagerId

---

### 5. Employee
**Description:** Employee master data table (separate from user authentication).

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| UserId | string | Yes | Foreign key to ApplicationUser | FK |
| EmployeeNumber | string | No | Employee number | - |
| FirstName | string | No | First name | - |
| MiddleName | string | Yes | Middle name | - |
| LastName | string | No | Last name | - |
| Suffix | string | Yes | Name suffix (Jr., Sr., etc.) | - |
| DateOfBirth | datetime | Yes | Birth date | - |
| Gender | string | Yes | Gender | - |
| CivilStatus | string | Yes | Marital status | - |
| Address | string | Yes | Home address | - |
| ContactNumber | string | Yes | Phone number | - |
| Email | string | Yes | Email address | - |
| HireDate | datetime | No | Date hired | - |
| RegularizationDate | datetime | Yes | Date regularized | - |
| SeparationDate | datetime | Yes | Date separated | - |
| SeparationReason | string | Yes | Reason for separation | - |
| BasicSalary | decimal | No | Monthly basic salary | - |
| SalaryType | enum | No | Salary type (1=Monthly, 2=Daily, 3=Hourly) | Default: 1 |
| DailyRate | decimal | Yes | Daily rate | - |
| HourlyRate | decimal | Yes | Hourly rate | - |
| SSSNumber | string | Yes | SSS number | - |
| PhilHealthNumber | string | Yes | PhilHealth number | - |
| PagIbigNumber | string | Yes | Pag-IBIG number | - |
| TINNumber | string | Yes | TIN number | - |
| BankName | string | Yes | Bank name | - |
| BankAccountNumber | string | Yes | Bank account number | - |
| DepartmentId | int | Yes | Foreign key to Department | FK |
| ShiftId | int | Yes | Foreign key to Shift | FK |
| SupervisorId | int | Yes | Foreign key to Employee (supervisor) | FK |
| IsActive | bool | No | Employee active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId, UserId, DepartmentId, ShiftId, SupervisorId

---


### 6. Shift
**Description:** Work shift schedules table.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| ShiftName | string | No | Shift name (Morning, Night, etc.) | - |
| StartTime | TimeSpan | No | Shift start time | - |
| EndTime | TimeSpan | No | Shift end time | - |
| BreakStart | TimeSpan | Yes | Break start time | - |
| BreakEnd | TimeSpan | Yes | Break end time | - |
| GracePeriodMinutes | int | No | Late grace period in minutes | Default: 15 |
| IsNightShift | bool | No | Night shift flag (crosses midnight) | Default: false |
| IsActive | bool | No | Shift active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId

---

### 7. Attendance
**Description:** Daily employee attendance records with time-in/out tracking.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| EmployeeId | int | No | Foreign key to Employee | FK |
| Date | datetime | No | Attendance date | - |
| TimeIn | datetime | Yes | Clock-in timestamp | - |
| TimeOut | datetime | Yes | Clock-out timestamp | - |
| Status | enum | No | Attendance status (1-6) | See AttendanceStatus enum |
| LateMinutes | int | No | Minutes late | Default: 0 |
| UndertimeMinutes | int | No | Minutes undertime | Default: 0 |
| OvertimeMinutes | int | No | Minutes overtime | Default: 0 |
| WorkedMinutes | int | No | Total minutes worked | Default: 0 |
| NightDifferentialMinutes | int | No | Night differential minutes | Default: 0 |
| LateDeductionAmount | decimal | No | Auto-calculated late deduction | Default: 0 |
| OvertimeAmount | decimal | No | Auto-calculated overtime pay | Default: 0 |
| IsApproved | bool | No | Approval status | Default: false |
| ApprovedById | int | Yes | Foreign key to Employee (approver) | FK |
| ApprovedAt | datetime | Yes | Approval timestamp | - |
| IsLocked | bool | No | Locked for payroll processing | Default: false |
| Remarks | string | Yes | Additional remarks | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: EmployeeId, ApprovedById
- Composite: (EmployeeId, Date) for quick lookups

---

### 8. Leave
**Description:** Employee leave requests and approvals.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| EmployeeId | int | No | Foreign key to Employee | FK |
| LeaveType | enum | No | Leave type (1-6) | See LeaveType enum |
| StartDate | datetime | No | Leave start date | - |
| EndDate | datetime | No | Leave end date | - |
| TotalDays | decimal | No | Total leave days | - |
| Reason | string | Yes | Leave reason | - |
| Status | enum | No | Leave status (1-4) | See LeaveStatus enum |
| ApprovedById | int | Yes | Foreign key to Employee (approver) | FK |
| ApprovedAt | datetime | Yes | Approval timestamp | - |
| ApproverRemarks | string | Yes | Approver's remarks | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: EmployeeId, ApprovedById

---

### 9. Overtime
**Description:** Employee overtime records and approvals.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| EmployeeId | int | No | Foreign key to Employee | FK |
| Date | datetime | No | Overtime date | - |
| StartTime | TimeSpan | No | Overtime start time | - |
| EndTime | TimeSpan | No | Overtime end time | - |
| TotalMinutes | int | No | Total overtime minutes | - |
| OvertimeType | enum | No | OT type (1-4) | See OvertimeType enum |
| Multiplier | decimal | No | Pay multiplier (1.25, 1.30, 2.00, etc.) | - |
| Reason | string | Yes | Overtime reason | - |
| Status | enum | No | Approval status (1-4) | See LeaveStatus enum |
| ApprovedById | int | Yes | Foreign key to Employee (approver) | FK |
| ApprovedAt | datetime | Yes | Approval timestamp | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: EmployeeId, ApprovedById

---

### 10. Payroll
**Description:** Employee payroll computation records.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| EmployeeId | int | No | Foreign key to Employee | FK |
| PayrollPeriodId | int | No | Foreign key to PayrollPeriod | FK |
| BasicPay | decimal | No | Basic salary for period | Default: 0 |
| OvertimePay | decimal | No | Overtime pay | Default: 0 |
| HolidayPay | decimal | No | Holiday pay | Default: 0 |
| NightDifferentialPay | decimal | No | Night differential pay | Default: 0 |
| Allowances | decimal | No | Allowances | Default: 0 |
| OtherEarnings | decimal | No | Other earnings | Default: 0 |
| GrossPay | decimal | No | Total gross pay | Default: 0 |
| LateDeduction | decimal | No | Late deductions | Default: 0 |
| UndertimeDeduction | decimal | No | Undertime deductions | Default: 0 |
| AbsenceDeduction | decimal | No | Absence deductions | Default: 0 |
| SSSContribution | decimal | No | SSS contribution | Default: 0 |
| PhilHealthContribution | decimal | No | PhilHealth contribution | Default: 0 |
| PagIbigContribution | decimal | No | Pag-IBIG contribution | Default: 0 |
| WithholdingTax | decimal | No | Withholding tax | Default: 0 |
| OtherDeductions | decimal | No | Other deductions | Default: 0 |
| TotalDeductions | decimal | No | Total deductions | Default: 0 |
| NetPay | decimal | No | Net pay (take-home) | Default: 0 |
| Status | enum | No | Payroll status (1-5) | See PayrollStatus enum |
| ProcessedById | int | Yes | Foreign key to Employee (processor) | FK |
| ProcessedAt | datetime | Yes | Processing timestamp | - |
| DaysWorked | decimal | No | Days worked in period | Default: 0 |
| HoursWorked | decimal | No | Hours worked in period | Default: 0 |
| OvertimeHours | decimal | No | Overtime hours | Default: 0 |
| LateHours | decimal | No | Late hours | Default: 0 |
| UndertimeHours | decimal | No | Undertime hours | Default: 0 |
| AbsentDays | decimal | No | Absent days | Default: 0 |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: EmployeeId, PayrollPeriodId, ProcessedById

---

### 11. PayrollPeriod
**Description:** Payroll period definitions (cutoff dates).

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| PeriodName | string | No | Period name (e.g., "Jan 1-15, 2026") | - |
| StartDate | datetime | No | Period start date | - |
| EndDate | datetime | No | Period end date | - |
| PayDate | datetime | No | Payment date | - |
| Status | enum | No | Payroll status (1-5) | See PayrollStatus enum |
| ProcessedById | int | Yes | Foreign key to Employee (processor) | FK |
| ProcessedAt | datetime | Yes | Processing timestamp | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId, ProcessedById

---

### 12. Holiday
**Description:** Company holiday calendar.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| HolidayName | string | No | Holiday name | - |
| Date | datetime | No | Holiday date | - |
| HolidayType | enum | No | Holiday type (1=Regular, 2=Special) | - |
| IsNationwide | bool | No | Nationwide holiday flag | Default: true |
| IsActive | bool | No | Holiday active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId

---

### 13. AuditLog
**Description:** System-wide audit trail for all user actions.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | Yes | Foreign key to Company | FK |
| UserId | string | Yes | Foreign key to ApplicationUser | FK |
| Action | string | No | Action performed | - |
| EntityName | string | No | Entity type affected | - |
| EntityId | string | Yes | Entity ID affected | - |
| OldValues | string | Yes | Old values (JSON) | - |
| NewValues | string | Yes | New values (JSON) | - |
| IpAddress | string | Yes | IP address of user | - |
| UserAgent | string | Yes | Browser/device user agent | - |
| Details | string | Yes | Additional details | - |
| CreatedAt | datetime | No | Timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId, UserId
- Index: CreatedAt (for time-based queries)

---

### 14. VendorLog
**Description:** SuperAdmin (vendor) activity audit trail.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| VendorId | string | No | Foreign key to ApplicationUser | FK |
| VendorName | string | No | Vendor name | - |
| Action | string | No | Action performed | - |
| EntityType | string | No | Entity type affected | - |
| EntityId | string | Yes | Entity ID affected | - |
| Description | string | Yes | Action description | - |
| OldValues | string | Yes | Old values (JSON) | - |
| NewValues | string | Yes | New values (JSON) | - |
| IpAddress | string | No | IP address | - |
| UserAgent | string | Yes | Browser/device user agent | - |
| RequestPath | string | Yes | HTTP request path | - |
| IsSuccess | bool | No | Success flag | Default: true |
| ErrorMessage | string | Yes | Error message if failed | - |
| CreatedAt | datetime | No | Timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: VendorId
- Index: CreatedAt

---

### 15. IDRequest
**Description:** Employee ID card replacement requests.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| UserId | string | No | Foreign key to ApplicationUser | FK |
| CompanyId | int | No | Foreign key to Company | FK |
| Reason | string | No | Reason for request | - |
| Status | enum | No | Request status (0-3) | See IDRequestStatus enum |
| ApprovedByUserId | string | Yes | Foreign key to ApplicationUser (approver) | FK |
| ApprovedAt | datetime | Yes | Approval timestamp | - |
| ApprovalNotes | string | Yes | Approver's notes | - |
| IsPrinted | bool | No | Printed flag | Default: false |
| PrintedAt | datetime | Yes | Print timestamp | - |
| IsArchived | bool | No | Archived flag | Default: false |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: UserId, CompanyId, ApprovedByUserId

---

### 16. Notification
**Description:** User notifications and alerts.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| UserId | string | No | Foreign key to ApplicationUser | FK |
| Title | string | No | Notification title | - |
| Message | string | No | Notification message | - |
| Type | enum | No | Notification type (1-7) | See NotificationType enum |
| Priority | enum | No | Priority level (1-4) | See NotificationPriority enum |
| IsRead | bool | No | Read status | Default: false |
| ReadAt | datetime | Yes | Read timestamp | - |
| ActionUrl | string | Yes | Action URL (link) | - |
| RelatedEntityType | string | Yes | Related entity type | - |
| RelatedEntityId | int | Yes | Related entity ID | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: UserId
- Index: (UserId, IsRead) for unread notifications

---

### 17. IncidentReport
**Description:** Workplace incident reports and tracking.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| ReportedById | string | No | Foreign key to ApplicationUser (reporter) | FK |
| AssignedToId | string | Yes | Foreign key to ApplicationUser (assignee) | FK |
| Title | string | No | Incident title | - |
| Description | string | No | Incident description | - |
| Type | enum | No | Incident type (1-10) | See IncidentType enum |
| Severity | enum | No | Severity level (1-4) | See IncidentSeverity enum |
| Status | enum | No | Incident status (1-4) | See IncidentStatus enum |
| WhenHappened | datetime | Yes | When incident occurred | - |
| WhatHappened | string | No | What happened (details) | - |
| WhyHappened | string | No | Why it happened (root cause) | - |
| ResolvedAt | datetime | Yes | Resolution timestamp | - |
| Resolution | string | Yes | Resolution details | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId, ReportedById, AssignedToId

---

### 18. LoginAttempt
**Description:** Login attempt tracking for security monitoring.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| Email | string | No | Email attempted | - |
| IpAddress | string | No | IP address | - |
| UserAgent | string | No | Browser/device user agent | - |
| IsSuccessful | bool | No | Success flag | - |
| FailureReason | string | Yes | Failure reason | - |
| AttemptedAt | datetime | No | Attempt timestamp | Default: UTC now |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- Index: (Email, AttemptedAt) for rate limiting

---

### 19. PayrollDeadline
**Description:** Payroll submission deadlines by department.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| DepartmentId | int | No | Foreign key to Department | FK |
| DueDate | datetime | No | Deadline date | - |
| Type | enum | No | Deadline type (1=FirstHalf, 2=SecondHalf) | - |
| Status | enum | No | Deadline status (1-4) | See PayrollDeadlineStatus enum |
| SubmittedById | string | Yes | Foreign key to ApplicationUser (submitter) | FK |
| SubmittedAt | datetime | Yes | Submission timestamp | - |
| ApprovedById | string | Yes | Foreign key to ApplicationUser (approver) | FK |
| ApprovedAt | datetime | Yes | Approval timestamp | - |
| Notes | string | Yes | Additional notes | - |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId, DepartmentId, SubmittedById, ApprovedById

---

### 20. CompanyModule
**Description:** Company-specific module enablement.

| Column Name | Data Type | Nullable | Description | Constraints |
|------------|-----------|----------|-------------|-------------|
| Id | int | No | Primary key | PK, Identity |
| CompanyId | int | No | Foreign key to Company | FK |
| ModuleType | enum | No | Module type (1-6) | See ModuleType enum |
| IsEnabled | bool | No | Module enabled flag | Default: true |
| IsActive | bool | No | Record active status | Default: true |
| CreatedAt | datetime | No | Creation timestamp | Default: UTC now |

**Indexes:**
- PK: Id
- FK: CompanyId
- Unique: (CompanyId, ModuleType)

---

## Enumerations

### UserRole
| Value | Name | Description |
|-------|------|-------------|
| 1 | ErpSuperAdmin | System vendor/super administrator |
| 2 | CompanyAdmin | Company manager/administrator |
| 3 | HR | Human resources officer |
| 4 | Supervisor | Department supervisor |
| 5 | Employee | Regular employee |
| 6 | Accountant | Accountant/payroll officer |

### AttendanceStatus
| Value | Name | Description |
|-------|------|-------------|
| 1 | Present | Present on time |
| 2 | Absent | Absent |
| 3 | Late | Late arrival |
| 4 | OnLeave | On approved leave |
| 5 | HalfDay | Half-day attendance |
| 6 | Holiday | Holiday |

### LeaveType
| Value | Name | Description |
|-------|------|-------------|
| 1 | Vacation | Vacation leave |
| 2 | Sick | Sick leave |
| 3 | Emergency | Emergency leave |
| 4 | Maternity | Maternity leave |
| 5 | Paternity | Paternity leave |
| 6 | Unpaid | Unpaid leave |

### LeaveStatus
| Value | Name | Description |
|-------|------|-------------|
| 1 | Pending | Pending approval |
| 2 | Approved | Approved |
| 3 | Rejected | Rejected |
| 4 | Cancelled | Cancelled |

### OvertimeType
| Value | Name | Description | Multiplier |
|-------|------|-------------|------------|
| 1 | Regular | Regular overtime | 1.25x |
| 2 | RestDay | Rest day overtime | 1.30x |
| 3 | Holiday | Holiday overtime | 2.00x |
| 4 | RestDayHoliday | Rest day + holiday OT | 2.60x |

### HolidayType
| Value | Name | Description | Pay Rate |
|-------|------|-------------|----------|
| 1 | Regular | Regular holiday | 200% |
| 2 | Special | Special non-working day | 130% |

### PayrollStatus
| Value | Name | Description |
|-------|------|-------------|
| 1 | Draft | Draft/not started |
| 2 | Processing | Being processed |
| 3 | Processed | Computation complete |
| 4 | Approved | Approved for payment |
| 5 | Paid | Payment released |

### SalaryType
| Value | Name | Description |
|-------|------|-------------|
| 1 | Monthly | Monthly salary |
| 2 | Daily | Daily rate |
| 3 | Hourly | Hourly rate |

### NotificationType
| Value | Name | Description |
|-------|------|-------------|
| 1 | PayrollDue | Payroll deadline approaching |
| 2 | PayrollOverdue | Payroll deadline passed |
| 3 | BudgetApprovalNeeded | Budget approval required |
| 4 | IncidentReport | Incident report notification |
| 5 | SystemAlert | System alert |
| 6 | LoginSecurity | Security-related login alert |
| 7 | IDCardReady | ID card ready for pickup |

### NotificationPriority
| Value | Name | Description |
|-------|------|-------------|
| 1 | Low | Low priority |
| 2 | Normal | Normal priority |
| 3 | High | High priority |
| 4 | Urgent | Urgent |

### IncidentType
| Value | Name | Description |
|-------|------|-------------|
| 1 | LostIDCard | Lost ID card |
| 2 | LateArrival | Late arrival |
| 3 | AbsentWithoutNotice | AWOL |
| 4 | EquipmentDamage | Equipment damage |
| 5 | WorkplaceAccident | Workplace accident |
| 6 | PayrollDelay | Payroll delay |
| 7 | BudgetIssue | Budget issue |
| 8 | SystemError | System error |
| 9 | SecurityBreach | Security breach |
| 10 | Other | Other incident |

### IncidentSeverity
| Value | Name | Description |
|-------|------|-------------|
| 1 | Low | Low severity |
| 2 | Normal | Normal severity |
| 3 | High | High severity |
| 4 | Critical | Critical severity |

### IncidentStatus
| Value | Name | Description |
|-------|------|-------------|
| 1 | Open | Open/new |
| 2 | InProgress | In progress |
| 3 | Resolved | Resolved |
| 4 | Closed | Closed |

### IDRequestStatus
| Value | Name | Description |
|-------|------|-------------|
| 0 | Pending | Pending approval |
| 1 | Approved | Approved |
| 2 | Rejected | Rejected |
| 3 | Printed | Printed |

### ModuleType
| Value | Name | Description |
|-------|------|-------------|
| 1 | Attendance | Attendance module |
| 2 | Payroll | Payroll module |
| 3 | Leave | Leave management |
| 4 | Overtime | Overtime management |
| 5 | GovernmentCompliance | Gov't compliance |
| 6 | Reports | Reports module |

---

## Relationships Summary

### One-to-Many Relationships

1. **Company → Branch** (1:N)
   - One company has many branches
   - FK: Branch.CompanyId → Company.Id

2. **Company → Department** (1:N)
   - One company has many departments
   - FK: Department.CompanyId → Company.Id

3. **Company → Shift** (1:N)
   - One company has many shifts
   - FK: Shift.CompanyId → Company.Id

4. **Company → ApplicationUser** (1:N)
   - One company has many users
   - FK: ApplicationUser.CompanyId → Company.Id

5. **Company → Employee** (1:N)
   - One company has many employees
   - FK: Employee.CompanyId → Company.Id

6. **Company → Holiday** (1:N)
   - One company has many holidays
   - FK: Holiday.CompanyId → Company.Id

7. **Company → PayrollPeriod** (1:N)
   - One company has many payroll periods
   - FK: PayrollPeriod.CompanyId → Company.Id

8. **Branch → ApplicationUser** (1:N)
   - One branch has many users
   - FK: ApplicationUser.BranchId → Branch.Id

9. **Department → ApplicationUser** (1:N)
   - One department has many users
   - FK: ApplicationUser.DepartmentId → Department.Id

10. **Department → Employee** (1:N)
    - One department has many employees
    - FK: Employee.DepartmentId → Department.Id

11. **Shift → ApplicationUser** (1:N)
    - One shift has many users
    - FK: ApplicationUser.ShiftId → Shift.Id

12. **Shift → Employee** (1:N)
    - One shift has many employees
    - FK: Employee.ShiftId → Shift.Id

13. **ApplicationUser → Notification** (1:N)
    - One user has many notifications
    - FK: Notification.UserId → ApplicationUser.Id

14. **ApplicationUser → AuditLog** (1:N)
    - One user has many audit logs
    - FK: AuditLog.UserId → ApplicationUser.Id

15. **ApplicationUser → IDRequest** (1:N)
    - One user has many ID requests
    - FK: IDRequest.UserId → ApplicationUser.Id

16. **Employee → Attendance** (1:N)
    - One employee has many attendance records
    - FK: Attendance.EmployeeId → Employee.Id

17. **Employee → Leave** (1:N)
    - One employee has many leave records
    - FK: Leave.EmployeeId → Employee.Id

18. **Employee → Overtime** (1:N)
    - One employee has many overtime records
    - FK: Overtime.EmployeeId → Employee.Id

19. **Employee → Payroll** (1:N)
    - One employee has many payroll records
    - FK: Payroll.EmployeeId → Employee.Id

20. **PayrollPeriod → Payroll** (1:N)
    - One payroll period has many payroll records
    - FK: Payroll.PayrollPeriodId → PayrollPeriod.Id

21. **ApplicationUser → VendorLog** (1:N)
    - One user (SuperAdmin/Vendor) has many vendor logs
    - FK: VendorLog.VendorId → ApplicationUser.Id

### One-to-One Relationships

1. **ApplicationUser ↔ Employee** (1:1)
   - One user can have one employee record
   - FK: Employee.UserId → ApplicationUser.Id

### Self-Referencing Relationships

1. **Employee → Employee** (Supervisor)
   - One employee can supervise many employees
   - FK: Employee.SupervisorId → Employee.Id

2. **Department → Employee** (Manager)
   - One department has one manager (employee)
   - FK: Department.ManagerId → Employee.Id

---

## Security & Audit Features

### Multi-Tenant Isolation
- All company-specific tables have `CompanyId` foreign key
- Queries filtered by current user's company
- Prevents cross-company data access

### Audit Trail
- **AuditLog** table tracks all user actions
- **VendorLog** table tracks SuperAdmin actions
- **LoginAttempt** table tracks login attempts
- Captures: User, Action, Entity, Old/New Values, IP, Timestamp

### Authentication & Authorization
- Password hashing via ASP.NET Identity (PBKDF2)
- Role-based access control (6 roles)
- Session management with secure cookies
- Forced password change on first login

### Kiosk Security
- Dual-factor verification (QR + PIN)
- Unique SHA256 QR codes per user
- PIN verification (4-digit)
- Failed attempt logging

### Biometric Security
- Face enrollment system
- Binary face encoding storage
- Photo for ID card generation

---

## Indexes & Performance

### Primary Keys
- All tables have auto-increment integer primary key (Id)
- ApplicationUser uses string GUID primary key (ASP.NET Identity)

### Foreign Key Indexes
- All foreign keys automatically indexed
- Improves join performance

### Composite Indexes
- (EmployeeId, Date) on Attendance for quick daily lookups
- (UserId, IsRead) on Notification for unread count
- (Email, AttemptedAt) on LoginAttempt for rate limiting
- (CompanyId, ModuleType) unique on CompanyModule

### Unique Constraints
- ApplicationUser: UserName, Email, QRCodeHash
- Company: CompanyCode
- CompanyModule: (CompanyId, ModuleType)

---

**Document Version:** 1.0  
**Last Updated:** 2026-04-29  
**Total Tables:** 20  
**Total Relationships:** 30+  
**Author:** PayroTech Development Team
