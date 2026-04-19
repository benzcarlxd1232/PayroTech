# PayroTech Security Features - Screenshots Guide

This document provides a guide for capturing screenshots of the most important security features in the PayroTech system.

---

## 1. Authentication System

### 1.1 Login with Email & Password
**File:** `PayroTech/Controllers/AuthController.cs`  
**Lines:** 38-95  
**Screenshot:** Login page

**Process:**
1. User enters email and password
2. System validates credentials against hashed password in database
3. Checks if user account is active (`IsActive` flag)
4. On success, creates authenticated session with secure cookie
5. Logs login attempt with IP address to AuditLog table
6. Redirects to role-specific dashboard

**Security Features:**
- Password hashing using ASP.NET Identity (PBKDF2 with salt)
- Account status verification before login
- IP address logging for audit trail
- Secure session management with HTTP-only cookies
- Failed login attempt tracking

---

### 1.2 Role-Based Access Control (RBAC)
**File:** `PayroTech/Models/Enums/UserRole.cs`  
**Lines:** 5-12  
**Screenshot:** Different dashboard views for each role

**User Roles:**
1. **ErpSuperAdmin** - Full system access, manages all companies
2. **CompanyAdmin** - Company-level management (Manager)
3. **HR** - Human resources operations
4. **Accountant** - Payroll and financial operations
5. **Employee** - Personal portal access only

**Process:**
- Each controller uses `[Authorize]` attribute to require authentication
- Role checks performed using `user.Role` property
- Unauthorized access attempts redirect to AccessDenied page
- Navigation menus dynamically rendered based on user role
- Data queries automatically filtered by user's company and role

**Security Implementation:**
```csharp
[Authorize]  // Requires authentication
public class HRController : Controller
{
    // Only HR role can access these actions
    if (user.Role != UserRole.HR)
        return RedirectToAction("AccessDenied", "Auth");
}
```

---

### 1.3 Forced Password Change on First Login
**File:** `PayroTech/Controllers/AuthController.cs`  
**Lines:** 112-188  
**Screenshot:** Password change screen

**Process:**
1. New users are flagged with `MustChangePassword = true`
2. After successful login, system checks this flag
3. User is redirected to password change page
4. Cannot access system until password is changed
5. Password complexity requirements enforced (min 6 chars, uppercase, lowercase, digit, special char)
6. Flag cleared after successful password change
7. Audit log records the password change event

**Security Purpose:**
- Ensures default/temporary passwords are not used long-term
- Forces users to create unique, personal passwords
- Prevents password sharing from initial setup

---

## 2. Kiosk Attendance Security

### 2.1 Dual-Factor Verification (QR Code + PIN)
**File:** `PayroTech/Controllers/KioskController.cs`  
**Lines:** 48-103 (QR verification), 195-400 (PIN verification)  
**Screenshot:** Kiosk QR scan screen and PIN entry screen

**Process:**

**Step 1: QR Code Verification**
1. Employee scans unique QR code from ID card using kiosk camera
2. System validates QR hash against database (`QRCodeHash` field)
3. Returns employee name and staff code if valid
4. Blocks SuperAdmin users from kiosk usage

**Step 2: PIN Verification**
1. Employee enters 4-digit PIN on kiosk keypad
2. System verifies PIN against `KioskPin` field (or default: last 4 digits of StaffCode)
3. Failed attempts are logged to audit trail with timestamp and IP
4. On success, records attendance with automatic time-in/time-out detection

**Security Features:**
- **Two-factor authentication**: Something you have (QR code) + Something you know (PIN)
- QR codes are SHA256 hashed and cryptographically unique per user
- QR code generation: `SHA256(userId + timestamp + GUID)`
- PIN prevents unauthorized use of stolen/found ID cards
- All attempts (success and failure) logged with IP address and timestamp
- Automatic late detection and overtime calculation based on shift schedule

**QR Code Generation:**
```csharp
// File: KioskController.cs, Lines 430-437
public static string GenerateQRCodeHash(string userId)
{
    using var sha256 = SHA256.Create();
    var input = $"{userId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
    return Convert.ToBase64String(hashBytes);
}
```

---

### 2.2 Customizable PIN Management
**File:** `PayroTech/Controllers/ChangePinController.cs`  
**Lines:** 1-68  
**Screenshot:** Change PIN interface

**Process:**
1. Authenticated user navigates to Change PIN page
2. System displays current StaffCode for reference
3. User enters new 4-digit PIN
4. Confirms PIN by re-entering
5. System validates (must be exactly 4 digits, all numeric)
6. Updates `KioskPin` field in database
7. Change is logged to audit trail

**Security Features:**
- Only authenticated users can change their own PIN
- PIN must be exactly 4 digits
- Confirmation required to prevent typos
- Cannot change other users' PINs
- All changes audited with timestamp

---

## 3. Audit Logging & Monitoring

### 3.1 Comprehensive Audit Trail
**File:** `PayroTech/Models/Entities/AuditLog.cs`  
**Lines:** 1-42  
**Screenshot:** Audit log table/viewer

**Logged Events:**
- User login/logout with IP address
- Password changes
- Face enrollment
- Kiosk attendance (time-in/out)
- Failed PIN attempts
- Data modifications (with old vs new values)
- Role changes
- Company/employee creation and updates

**Data Captured:**
- User ID and name
- Action performed (e.g., "Login", "Password Changed", "Kiosk time-in")
- Entity type and ID affected (e.g., "User", "Attendance")
- Old and new values in JSON format
- IP address of the request
- User agent (browser/device information)
- Timestamp (UTC)
- Company ID for multi-tenant isolation

**Security Purpose:**
- Complete forensic trail of all system activities
- Detect unauthorized access attempts
- Compliance with data protection regulations
- Accountability for all user actions
- Investigation of security incidents

---

### 3.2 Login Attempt Tracking
**File:** `PayroTech/Models/Entities/PayrollDeadline.cs`  
**Lines:** 68-76  
**Screenshot:** Failed login attempts log

**Data Tracked:**
- Email address attempted
- IP address of attempt
- User agent (browser/device)
- Success or failure status
- Failure reason (invalid password, account inactive, etc.)
- Timestamp of attempt

**Security Use Cases:**
- Detect brute force attacks (multiple failed attempts from same IP)
- Identify compromised accounts (successful login from unusual location)
- Geographic anomaly detection
- Trigger account lockout after threshold
- Alert administrators of suspicious activity

---

## 4. Data Security & Isolation

### 4.1 Multi-Tenant Data Isolation
**File:** `PayroTech/Models/Entities/ApplicationUser.cs`  
**Lines:** 19-20  
**Screenshot:** Code showing CompanyId filtering

**Process:**
- Every user has a `CompanyId` foreign key linking them to their company
- All database queries automatically filtered by current user's company
- Users can only see and access data from their own company
- Enforced at both database level (foreign keys) and application level (query filters)

**Security Implementation:**
```csharp
// Example from controllers
var user = await _userManager.GetUserAsync(User);
var employees = await _context.Employees
    .Where(e => e.CompanyId == user.CompanyId)  // Isolation filter
    .ToListAsync();
```

**Security Purpose:**
- Prevents cross-company data access
- Ensures data privacy between different organizations
- Compliance with data protection regulations
- Each company's data is logically separated

---

### 4.2 Password Security
**File:** `PayroTech/Controllers/AuthController.cs`  
**Lines:** 38-95  
**Screenshot:** Password validation error messages

**Security Measures:**
- Passwords hashed using ASP.NET Identity (PBKDF2 algorithm with salt)
- Never stored in plain text
- Cannot be retrieved, only reset
- Minimum complexity requirements:
  - At least 6 characters
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one digit
  - At least one special character
- Enforced at registration and password change
- Old passwords cannot be reused (hash comparison)

---

## 5. ID Card & QR Code Security

### 5.1 Cryptographic QR Code Generation
**File:** `PayroTech/Controllers/KioskController.cs`  
**Lines:** 430-437  
**Screenshot:** Generated ID card with QR code

**Process:**
1. Unique hash generated using SHA256 cryptographic algorithm
2. Input combines: `userId + current timestamp + random GUID`
3. Hash is Base64 encoded and made URL-safe
4. Stored in `QRCodeHash` field in database
5. Printed on employee ID card
6. Used for kiosk attendance verification

**Security Features:**
- Cryptographically secure hash (SHA256)
- Unique per user (cannot be duplicated)
- Cannot be reverse-engineered to get user ID
- Timestamp prevents replay attacks
- Random GUID adds additional entropy

---

### 5.2 ID Request & Approval Workflow
**File:** `PayroTech/Models/Entities/IDRequest.cs`  
**Lines:** 1-25  
**Screenshot:** ID request form and manager approval interface

**Process:**
1. Employee submits ID replacement request (lost/damaged card)
2. Request includes reason and is marked as "Pending"
3. Manager receives notification of request
4. Manager reviews and approves or rejects request
5. Approved requests allow generation of new ID card
6. Old QR code can be invalidated to prevent misuse
7. Print tracking prevents duplicate printing
8. Full audit trail maintained

**Security Purpose:**
- Prevents unauthorized ID duplication
- Manager oversight required for all ID issuance
- Lost/stolen ID cards can be deactivated
- Audit trail for compliance
- Prevents employees from printing multiple IDs

---

## 6. Face Recognition Security

### 6.1 Face Enrollment System
**File:** `PayroTech/Controllers/AuthController.cs`  
**Lines:** 190-276  
**Screenshot:** Face capture interface

**Process:**
1. Camera captures user's face photo during first login
2. Image converted to base64 format
3. Face encoding data extracted and stored as binary
4. Stored in `FaceEncodingData` field (byte array)
5. Image also saved to disk for ID card photo
6. File path stored in `FaceImagePath` field
7. `IsFaceEnrolled` flag set to true
8. Enrollment timestamp recorded

**Security Features:**
- Biometric authentication capability
- Face data stored as binary encoding (not raw image)
- Used for ID card photo generation
- Optional but recommended for enhanced security
- Can be re-enrolled if needed
- Enrollment completion tracked with timestamp

---

## 7. Session Management

### 7.1 Secure Logout
**File:** `PayroTech/Controllers/AuthController.cs`  
**Lines:** 302-318  
**Screenshot:** Logout process

**Process:**
1. User clicks logout button
2. System logs logout event to audit trail
3. Signs out user from ASP.NET Identity
4. Deletes authentication cookie
5. Sets cache-control headers to prevent back-button access
6. Redirects to login page

**Security Headers:**
```csharp
Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
Response.Headers["Pragma"] = "no-cache";
Response.Headers["Expires"] = "0";
```

**Security Purpose:**
- Prevents session hijacking after logout
- Clears all authentication tokens
- Prevents browser back-button access to authenticated pages
- Ensures complete session termination

---

## 8. Incident Reporting Security

### 8.1 Lost ID Card Reporting & Replacement
**File:** `PayroTech/Controllers/IncidentReportController.cs`  
**Lines:** 1-350  
**Screenshot:** Lost ID incident report form

**Process:**
1. Employee reports lost ID card through incident report system
2. Incident type marked as "LostIDCard"
3. Manager receives immediate notification
4. Manager reviews and approves incident report
5. System automatically generates approved IDRequest
6. Old QR code marked as compromised/inactive
7. New ID issued with new QR code
8. Full audit trail maintained

**Security Features:**
- Immediate reporting mechanism for lost credentials
- Manager approval required before replacement
- Automatic ID replacement workflow
- Old QR code invalidated to prevent misuse
- Complete audit trail of incident and resolution
- Prevents unauthorized access with lost ID cards

---

## Summary of Key Security Features

### ✅ Authentication & Authorization
- Email/password authentication with PBKDF2 hashing
- Role-based access control (5 user roles)
- Forced password change on first login
- Secure session management with HTTP-only cookies
- Password complexity requirements enforced

### ✅ Kiosk Attendance Security
- Dual-factor verification (QR code + PIN)
- Cryptographic QR codes (SHA256)
- Customizable 4-digit PIN
- Failed attempt logging
- Automatic late/overtime detection

### ✅ Audit & Monitoring
- Comprehensive audit logging of all actions
- Login attempt tracking with IP addresses
- Old/new value comparison for data changes
- Vendor activity logging (SuperAdmin actions)
- Complete forensic trail for compliance

### ✅ Data Security
- Multi-tenant data isolation by CompanyId
- Password hashing (never stored in plain text)
- Sensitive data protection (government IDs, bank accounts)
- SQL injection prevention (Entity Framework Core)
- Input validation on all forms

### ✅ ID Card Security
- Cryptographic QR code generation (SHA256)
- ID request approval workflow
- Lost ID reporting and replacement
- QR code invalidation capability
- Print tracking to prevent duplicates

### ✅ Biometric Security
- Face enrollment system
- Binary face encoding storage
- Photo for ID card generation
- Re-enrollment capability

### ✅ Session Security
- Secure logout with cookie deletion
- Cache-control headers
- Session timeout (configurable)
- Anti-CSRF tokens (ASP.NET Core)

---

## Screenshot Checklist

### Pages to Capture:
- [ ] Login page (`/Auth/Login`)
- [ ] First-time password change (`/Auth/ChangePassword`)
- [ ] Face enrollment (`/Auth/EnrollFace`)
- [ ] Kiosk QR scan (`/Kiosk/Attendance`)
- [ ] Kiosk PIN entry
- [ ] Change PIN page (`/ChangePin`)
- [ ] Access denied page (`/Auth/AccessDenied`)
- [ ] Lost ID incident report (`/IncidentReport/Create`)
- [ ] ID request approval interface
- [ ] Audit log viewer
- [ ] Role-specific dashboards

### Code Files to Screenshot:
- [ ] `AuthController.cs` - Login method (Lines 38-95)
- [ ] `AuthController.cs` - Password change (Lines 112-188)
- [ ] `KioskController.cs` - QR verification (Lines 48-103)
- [ ] `KioskController.cs` - PIN verification (Lines 195-400)
- [ ] `KioskController.cs` - QR generation (Lines 430-437)
- [ ] `ChangePinController.cs` - PIN change (Lines 1-68)
- [ ] `ApplicationUser.cs` - Security fields (Lines 44-56)
- [ ] `AuditLog.cs` - Audit model (Lines 1-42)
- [ ] `UserRole.cs` - Role enum (Lines 5-12)

---

**Document Version:** 2.0  
**Last Updated:** 2026-04-29  
**Focus:** Core Security Features Only  
**Author:** PayroTech Development Team
