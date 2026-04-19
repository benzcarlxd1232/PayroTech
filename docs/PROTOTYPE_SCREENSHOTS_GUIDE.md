# PayroTech Prototype Screenshots Guide

This document provides a comprehensive guide for capturing screenshots of all user interface screens organized by user role.

---

## Table of Contents
1. [Public/Guest Screens](#publicguest-screens)
2. [SuperAdmin (Vendor) Screens](#superadmin-vendor-screens)
3. [Manager (CompanyAdmin/Client) Screens](#manager-companyadminclient-screens)
4. [HR Screens](#hr-screens)
5. [Accountant Screens](#accountant-screens)
6. [Employee Screens](#employee-screens)
7. [Shared/Common Screens](#sharedcommon-screens)

---

## Public/Guest Screens

### 1. Landing Page
**Label:** Landing Page  
**File:** `Views/Landing/Index.cshtml`  
**URL:** `/`

**Description:**  
The public-facing landing page that introduces PayroTech to potential clients.

**Functionality:**
- Displays company branding and tagline
- Shows system features and benefits
- Provides login button to access the system

**User Interaction:**
- Click "Login" button to navigate to login page
- View system features and pricing information

---

### 2. Login Page
**Label:** Login Page  
**File:** `Views/Auth/Login.cshtml`  
**URL:** `/Auth/Login`

**Description:**  
Secure login page where all users authenticate to access the system.

**Functionality:**
- Email and password input fields
- "Remember Me" checkbox for persistent login
- Form validation for required fields
- Error messages for invalid credentials

**User Interaction:**
- Enter email address and password
- Check "Remember Me" (optional)
- Click "Login" button
- System validates credentials and redirects to role-specific dashboard

---

## SuperAdmin (Vendor) Screens

### 3. SuperAdmin Dashboard
**Label:** SuperAdmin Dashboard  
**File:** `Views/SuperAdmin/Index.cshtml`  
**URL:** `/SuperAdmin`

**Description:**  
Main dashboard for system vendors showing overview of all client companies and system statistics.

**Functionality:**
- Total companies count
- Active subscriptions count
- Total users across all companies
- Recent company registrations
- System-wide statistics

**User Interaction:**
- View high-level metrics
- Navigate to company management
- Access system reports
- Monitor subscription status

---

### 4. Company Management (List)
**Label:** Company Management  
**File:** `Views/SuperAdmin/Companies.cshtml`  
**URL:** `/SuperAdmin/Companies`

**Description:**  
Comprehensive list of all client companies subscribed to the system.

**Functionality:**
- Searchable and filterable company list
- Company code, name, and status display
- Subscription status indicators
- Quick actions (View, Edit, Manage)
- Create new company button

**User Interaction:**
- Search companies by name or code
- Filter by subscription status
- Click "View" to see company details
- Click "Edit" to modify company information
- Click "Create Company" to add new client

---

### 5. Create Company
**Label:** Create New Company  
**File:** `Views/SuperAdmin/CreateCompany.cshtml`  
**URL:** `/SuperAdmin/CreateCompany`

**Description:**  
Form to register a new client company and create their admin account.

**Functionality:**
- Company information fields (name, code, address, contact)
- Government registration numbers (TIN, SSS, PhilHealth, Pag-IBIG)
- Subscription dates (start and end)
- Work schedule settings
- Default salary rates
- Admin account creation

**User Interaction:**
- Fill in company details
- Set subscription period
- Configure work schedule (5 or 6 days/week)
- Set default salary rates for roles
- Create company admin credentials
- Submit to create company

---

### 6. Company Details
**Label:** Company Details  
**File:** `Views/SuperAdmin/CompanyDetails.cshtml`  
**URL:** `/SuperAdmin/CompanyDetails/{id}`

**Description:**  
Detailed view of a specific client company with all information and statistics.

**Functionality:**
- Company profile information
- Subscription details and status
- Employee count and statistics
- Department count
- Branch information
- Government compliance numbers

**User Interaction:**
- View complete company information
- Check subscription status
- Click "Edit Company" to modify details
- Monitor company usage statistics

---

### 7. Edit Company
**Label:** Edit Company  
**File:** `Views/SuperAdmin/EditCompany.cshtml`  
**URL:** `/SuperAdmin/EditCompany/{id}`

**Description:**  
Form to update existing client company information and settings.

**Functionality:**
- Editable company information fields
- Subscription management
- Work schedule updates
- Salary rate adjustments
- Active/inactive status toggle

**User Interaction:**
- Modify company details
- Update subscription dates
- Change work schedule settings
- Adjust default salary rates
- Save changes

---

### 8. System Logs
**Label:** System Activity Logs  
**File:** `Views/SuperAdmin/SystemLogs.cshtml`  
**URL:** `/SuperAdmin/SystemLogs`

**Description:**  
Comprehensive audit trail of all SuperAdmin (vendor) actions across the system.

**Functionality:**
- Chronological list of vendor actions
- Action type, entity affected, timestamp
- Old and new values for changes
- IP address and user agent tracking
- Search and filter capabilities

**User Interaction:**
- View all vendor activities
- Search by action type or date
- Filter by entity type
- Review changes made to companies

---

## Manager (CompanyAdmin/Client) Screens

### 9. Manager Dashboard
**Label:** Manager Dashboard  
**File:** `Views/Manager/Index.cshtml`  
**URL:** `/Manager`

**Description:**  
Main dashboard for company managers showing company-wide overview and key metrics.

**Functionality:**
- Total employees count
- Today's attendance summary
- Pending approvals (leaves, overtime, incidents)
- Department statistics
- Recent activities
- Quick action buttons

**User Interaction:**
- View company metrics
- Navigate to staff management
- Access pending approvals
- Review recent activities
- Quick access to common tasks

---

### 10. Initial Setup Wizard
**Label:** Company Initial Setup  
**File:** `Views/Manager/InitialSetup.cshtml`  
**URL:** `/Manager/InitialSetup`

**Description:**  
Step-by-step wizard for new managers to configure their company settings for the first time.

**Functionality:**
- Branch creation
- Department setup
- Shift schedule configuration
- Holiday calendar setup
- Progress indicator

**User Interaction:**
- Create company branches
- Add departments
- Define work shifts
- Set up holiday calendar
- Complete setup to unlock full system

---

### 11. All Staff Management
**Label:** All Staff  
**File:** `Views/Manager/AllStaff.cshtml`  
**URL:** `/Manager/AllStaff`

**Description:**  
Comprehensive list of all company staff members (HR, Accountant, Employees).

**Functionality:**
- Searchable staff directory
- Filter by role, department, branch
- Staff details (name, role, department, status)
- Quick actions (View, Edit, Deactivate)
- Create new staff button

**User Interaction:**
- Search staff by name or employee number
- Filter by role or department
- View staff details
- Edit staff information
- Create new staff members

---

### 12. Create Staff
**Label:** Create New Staff  
**File:** `Views/Manager/CreateStaff.cshtml`  
**URL:** `/Manager/CreateStaff`

**Description:**  
Form to add new staff members (HR, Accountant, or Employee) to the company.

**Functionality:**
- Personal information fields
- Role selection (HR, Accountant, Employee)
- Department and branch assignment
- Shift assignment
- Salary information
- Emergency contact details
- Account credentials generation

**User Interaction:**
- Fill in staff personal details
- Select role and department
- Assign to branch and shift
- Set salary rate
- Add emergency contact
- Submit to create staff account

---

### 13. Payroll Approval
**Label:** Payroll Approval  
**File:** `Views/Manager/PayrollApproval.cshtml`  
**URL:** `/Manager/PayrollApproval`

**Description:**  
Interface for managers to review and approve payroll submissions from HR/Accountant.

**Functionality:**
- Pending payroll list
- Payroll period details
- Employee count and total amount
- Approve/reject actions
- Comments/notes field

**User Interaction:**
- Review payroll details
- Check employee list and amounts
- Add approval notes
- Approve or reject payroll
- View payroll history

---

### 14. Budget Allocation
**Label:** Budget Allocation  
**File:** `Views/Manager/BudgetAllocation.cshtml`  
**URL:** `/Manager/BudgetAllocation`

**Description:**  
Tool for managers to allocate and track departmental budgets.

**Functionality:**
- Department budget overview
- Allocated vs spent tracking
- Budget adjustment controls
- Visual budget charts
- Budget history

**User Interaction:**
- View department budgets
- Allocate budget to departments
- Monitor spending
- Adjust budgets as needed

---

### 15. Incident Reports (Manager View)
**Label:** Incident Reports  
**File:** `Views/Manager/IncidentReports.cshtml`  
**URL:** `/Manager/IncidentReports`

**Description:**  
Manager interface to review and resolve employee incident reports.

**Functionality:**
- List of all incident reports
- Filter by status, type, severity
- Incident details preview
- Approve/resolve actions
- Lost ID card auto-approval

**User Interaction:**
- View all incident reports
- Filter by status or type
- Click to view incident details
- Approve or resolve incidents
- Add resolution notes

---

### 16. ID Card Requests
**Label:** ID Card Requests  
**File:** `Views/Manager/IDRequests.cshtml`  
**URL:** `/Manager/IDRequests`

**Description:**  
Manager interface to approve or reject employee ID card replacement requests.

**Functionality:**
- Pending ID requests list
- Request reason display
- Employee information
- Approve/reject actions
- Print tracking

**User Interaction:**
- View pending ID requests
- Review request reasons
- Approve or reject requests
- Track printed ID cards

---

### 17. Company Settings
**Label:** Company Settings  
**File:** `Views/Manager/CompanySettings.cshtml`  
**URL:** `/Manager/CompanySettings`

**Description:**  
Configuration page for company-wide settings and preferences.

**Functionality:**
- Company profile editing
- Work schedule settings
- Salary rate defaults
- Holiday calendar management
- Module enablement

**User Interaction:**
- Update company information
- Modify work schedule
- Adjust salary rates
- Manage holidays
- Enable/disable modules

---

### 18. Reports (Manager)
**Label:** Manager Reports  
**File:** `Views/Manager/Reports.cshtml`  
**URL:** `/Manager/Reports`

**Description:**  
Comprehensive reporting dashboard for managers with various report types.

**Functionality:**
- Attendance reports
- Payroll reports
- Leave reports
- Department performance
- Export to PDF/Excel

**User Interaction:**
- Select report type
- Set date range
- Apply filters
- Generate report
- Export or print report

---

### 19. Audit Logs (Manager)
**Label:** Activity Logs  
**File:** `Views/Manager/AuditLogs.cshtml`  
**URL:** `/Manager/AuditLogs`

**Description:**  
Audit trail of all activities within the company for compliance and monitoring.

**Functionality:**
- Chronological activity log
- User actions tracking
- Filter by user, action, date
- Search functionality
- Export capabilities

**User Interaction:**
- View all company activities
- Search by user or action
- Filter by date range
- Export audit logs

---

## HR Screens

### 20. HR Dashboard
**Label:** HR Dashboard  
**File:** `Views/HR/Index.cshtml`  
**URL:** `/HR`

**Description:**  
Main dashboard for HR officers showing employee management overview.

**Functionality:**
- Total employees count
- Today's attendance
- Pending leave requests
- Upcoming holidays
- Recent activities

**User Interaction:**
- View HR metrics
- Navigate to employee management
- Access pending approvals
- Quick access to common HR tasks

---

### 21. Create Employee
**Label:** Create Employee  
**File:** `Views/HR/CreateEmployee.cshtml`  
**URL:** `/HR/CreateEmployee`

**Description:**  
Form for HR to add new employees to the system.

**Functionality:**
- Personal information fields
- Employment details
- Department and shift assignment
- Salary information
- Government IDs
- Bank details
- Emergency contact

**User Interaction:**
- Fill in employee details
- Assign department and shift
- Set salary rate
- Enter government IDs
- Add bank information
- Submit to create employee

---

### 22. My Team
**Label:** My Team  
**File:** `Views/HR/MyTeam.cshtml`  
**URL:** `/HR/MyTeam`

**Description:**  
List of all employees managed by the HR officer.

**Functionality:**
- Employee directory
- Search and filter
- Employee status
- Quick actions
- Export employee list

**User Interaction:**
- View all employees
- Search by name or number
- Filter by department
- Access employee details
- Export to Excel

---

### 23. Attendance Management (HR)
**Label:** Attendance Management  
**File:** `Views/HR/Attendance.cshtml`  
**URL:** `/HR/Attendance`

**Description:**  
Interface for HR to view and manage employee attendance records.

**Functionality:**
- Daily attendance overview
- Late arrivals tracking
- Absent employees
- Overtime records
- Manual attendance entry
- Attendance reports

**User Interaction:**
- View daily attendance
- Filter by date or department
- Add manual attendance
- Approve overtime
- Generate attendance reports

---

### 24. Leave Management (HR)
**Label:** Leave Management  
**File:** `Views/HR/Leaves.cshtml`  
**URL:** `/HR/Leaves`

**Description:**  
Interface for HR to manage employee leave requests.

**Functionality:**
- Pending leave requests
- Leave history
- Leave balance tracking
- Approve/reject actions
- Leave calendar view

**User Interaction:**
- View pending leave requests
- Review leave details
- Approve or reject leaves
- Check employee leave balance
- View leave calendar

---

### 25. Payroll Submission (HR)
**Label:** Submit Payroll  
**File:** `Views/HR/SubmitPayroll.cshtml`  
**URL:** `/HR/SubmitPayroll`

**Description:**  
Form for HR to submit payroll data to manager for approval.

**Functionality:**
- Payroll period selection
- Employee list with attendance
- Salary calculations preview
- Deductions summary
- Submit for approval button

**User Interaction:**
- Select payroll period
- Review employee attendance
- Verify salary calculations
- Add notes
- Submit to manager

---

### 26. Budget Tracker (HR)
**Label:** Budget Tracker  
**File:** `Views/HR/BudgetTracker.cshtml`  
**URL:** `/HR/BudgetTracker`

**Description:**  
Tool for HR to monitor department budget allocation and spending.

**Functionality:**
- Department budget overview
- Spending vs allocation
- Budget alerts
- Expense tracking
- Budget reports

**User Interaction:**
- View department budgets
- Monitor spending
- Track expenses
- Generate budget reports

---

## Accountant Screens

### 27. Accountant Dashboard
**Label:** Accountant Dashboard  
**File:** `Views/Accountant/Index.cshtml`  
**URL:** `/Accountant`

**Description:**  
Main dashboard for accountants showing payroll and financial overview.

**Functionality:**
- Pending payrolls count
- Total payroll amount
- Processed payrolls
- Payment status
- Recent transactions

**User Interaction:**
- View payroll metrics
- Navigate to payroll processing
- Access payment distribution
- Review financial reports

---

### 28. Process Payroll
**Label:** Process Payroll  
**File:** `Views/Accountant/ProcessPayroll.cshtml`  
**URL:** `/Accountant/ProcessPayroll`

**Description:**  
Interface for accountants to compute and process employee payroll.

**Functionality:**
- Payroll period selection
- Employee payroll list
- Automatic calculations (gross, deductions, net)
- Government contributions
- Tax computations
- Process payroll button

**User Interaction:**
- Select payroll period
- Review employee payroll
- Verify calculations
- Adjust if needed
- Process payroll

---

### 29. Distribute Salaries
**Label:** Distribute Salaries  
**File:** `Views/Accountant/DistributeSalaries.cshtml`  
**URL:** `/Accountant/DistributeSalaries`

**Description:**  
Interface for accountants to mark payrolls as paid and distribute salaries.

**Functionality:**
- Approved payroll list
- Employee payment details
- Bank account information
- Mark as paid button
- Payment confirmation

**User Interaction:**
- View approved payrolls
- Review payment details
- Verify bank accounts
- Mark as paid
- Generate payment receipts

---

### 30. Payroll History
**Label:** Payroll History  
**File:** `Views/Accountant/PayrollHistory.cshtml`  
**URL:** `/Accountant/PayrollHistory`

**Description:**  
Historical record of all processed payrolls.

**Functionality:**
- Payroll history list
- Filter by period or status
- Payroll details view
- Export to Excel
- Reprint payslips

**User Interaction:**
- View payroll history
- Filter by date range
- View payroll details
- Export reports
- Reprint payslips

---

## Employee Screens

### 31. Employee Dashboard
**Label:** My Portal  
**File:** `Views/Employee/Index.cshtml`  
**URL:** `/Employee`

**Description:**  
Personal dashboard for employees showing their own information and activities.

**Functionality:**
- Personal attendance summary
- Leave balance
- Recent payslips
- Upcoming holidays
- Personal notifications

**User Interaction:**
- View personal metrics
- Navigate to attendance
- Access payslips
- Request leave
- View notifications

---

### 32. My Attendance
**Label:** My Attendance  
**File:** `Views/Employee/Attendance.cshtml`  
**URL:** `/Employee/Attendance`

**Description:**  
Employee's personal attendance history and records.

**Functionality:**
- Monthly attendance calendar
- Time-in/time-out records
- Late arrivals
- Overtime hours
- Attendance summary

**User Interaction:**
- View attendance history
- Filter by month
- Check late records
- View overtime hours

---

### 33. Request Leave
**Label:** Request Leave  
**File:** `Views/Employee/RequestLeave.cshtml`  
**URL:** `/Employee/RequestLeave`

**Description:**  
Form for employees to submit leave requests.

**Functionality:**
- Leave type selection
- Date range picker
- Leave balance display
- Reason text field
- Submit button

**User Interaction:**
- Select leave type
- Choose start and end dates
- Check leave balance
- Enter reason
- Submit request

---

### 34. My Payslips
**Label:** My Payslips  
**File:** `Views/Employee/Payslips.cshtml`  
**URL:** `/Employee/Payslips`

**Description:**  
Employee's personal payslip history.

**Functionality:**
- Payslip list by period
- Net pay display
- Download/print payslip
- Salary breakdown link

**User Interaction:**
- View payslip history
- Select period
- Download payslip PDF
- View salary breakdown

---

### 35. Salary Breakdown
**Label:** Salary Breakdown  
**File:** `Views/Employee/SalaryBreakdown.cshtml`  
**URL:** `/Employee/SalaryBreakdown`

**Description:**  
Detailed breakdown of employee's salary computation.

**Functionality:**
- Earnings breakdown
- Deductions breakdown
- Government contributions
- Tax computation
- Net pay calculation

**User Interaction:**
- View detailed salary breakdown
- Understand deductions
- Check government contributions
- Verify net pay

---

## Shared/Common Screens

### 36. Change Password (First Login)
**Label:** Change Password  
**File:** `Views/Auth/ChangePassword.cshtml`  
**URL:** `/Auth/ChangePassword`

**Description:**  
Forced password change screen for new users on first login.

**Functionality:**
- New password field
- Confirm password field
- Password strength indicator
- Password requirements display
- Submit button

**User Interaction:**
- Enter new password
- Confirm password
- Meet password requirements
- Submit to change password

---

### 37. Face Enrollment
**Label:** Face Enrollment  
**File:** `Views/Auth/EnrollFace.cshtml`  
**URL:** `/Auth/EnrollFace`

**Description:**  
Face capture interface for biometric enrollment and ID card photo.

**Functionality:**
- Camera preview
- Face detection overlay
- Capture button
- Retake option
- Skip button (optional)

**User Interaction:**
- Allow camera access
- Position face in frame
- Click capture button
- Retake if needed
- Submit or skip

---

### 38. Change PIN
**Label:** Change Kiosk PIN  
**File:** `Views/ChangePin/Index.cshtml`  
**URL:** `/ChangePin`

**Description:**  
Interface for users to change their 4-digit kiosk attendance PIN.

**Functionality:**
- Current PIN display
- New PIN input (4 digits)
- Confirm PIN input
- Staff code display
- Update button

**User Interaction:**
- View current PIN
- Enter new 4-digit PIN
- Confirm new PIN
- Submit to update

---

### 39. My ID Card
**Label:** My ID Card  
**File:** `Views/IDCard/StaffIDCard.cshtml`  
**URL:** `/IDCard/MyIDCard`

**Description:**  
Digital ID card display with QR code for kiosk attendance.

**Functionality:**
- Employee photo
- Name and role
- Staff code
- QR code for scanning
- Company information
- Print button

**User Interaction:**
- View digital ID card
- Print ID card
- Use QR code for kiosk

---

### 40. Request ID Replacement
**Label:** Request ID Replacement  
**File:** `Views/IDCard/Request.cshtml`  
**URL:** `/IDCard/Request`

**Description:**  
Form to request ID card replacement (lost or damaged).

**Functionality:**
- Reason selection
- Description text field
- Current ID status
- Submit button

**User Interaction:**
- Select reason (Lost/Damaged)
- Describe situation
- Submit request
- Wait for manager approval

---

### 41. Create Incident Report
**Label:** Submit Incident Report  
**File:** `Views/IncidentReport/Create.cshtml`  
**URL:** `/IncidentReport/Create`

**Description:**  
Form for employees to report workplace incidents.

**Functionality:**
- Incident type selection
- Title field
- When happened (date/time)
- What happened (description)
- Why happened (cause)
- Severity selection
- Submit button

**User Interaction:**
- Select incident type
- Enter incident title
- Specify when it happened
- Describe what happened
- Explain why it happened
- Select severity
- Submit report

---

### 42. Kiosk Attendance
**Label:** Kiosk Attendance System  
**File:** `Views/Kiosk/Attendance.cshtml`  
**URL:** `/Kiosk/Attendance`

**Description:**  
Kiosk interface for employee attendance (QR + PIN verification).

**Functionality:**
- QR code scanner
- PIN entry keypad
- Employee verification display
- Time-in/time-out confirmation
- Success/error messages

**User Interaction:**
- Scan QR code from ID card
- Enter 4-digit PIN
- View verification result
- See time-in/time-out confirmation

---

## Screenshot Checklist Summary

### By Role:

**SuperAdmin (8 screens):**
- [ ] Dashboard
- [ ] Company List
- [ ] Create Company
- [ ] Company Details
- [ ] Edit Company
- [ ] System Logs
- [ ] Subscription Plans
- [ ] Reports

**Manager (11 screens):**
- [ ] Dashboard
- [ ] Initial Setup
- [ ] All Staff
- [ ] Create Staff
- [ ] Payroll Approval
- [ ] Budget Allocation
- [ ] Incident Reports
- [ ] ID Requests
- [ ] Company Settings
- [ ] Reports
- [ ] Audit Logs

**HR (7 screens):**
- [ ] Dashboard
- [ ] Create Employee
- [ ] My Team
- [ ] Attendance Management
- [ ] Leave Management
- [ ] Submit Payroll
- [ ] Budget Tracker

**Accountant (4 screens):**
- [ ] Dashboard
- [ ] Process Payroll
- [ ] Distribute Salaries
- [ ] Payroll History

**Employee (5 screens):**
- [ ] Dashboard
- [ ] My Attendance
- [ ] Request Leave
- [ ] My Payslips
- [ ] Salary Breakdown

**Shared/Common (7 screens):**
- [ ] Landing Page
- [ ] Login
- [ ] Change Password
- [ ] Face Enrollment
- [ ] Change PIN
- [ ] My ID Card
- [ ] Request ID Replacement
- [ ] Create Incident Report
- [ ] Kiosk Attendance

**Total: 42 Screens**

---

**Document Version:** 1.0  
**Last Updated:** 2026-04-30  
**Total Screens:** 42  
**Author:** PayroTech Development Team
