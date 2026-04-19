using Microsoft.AspNetCore.Identity;
using PayroTech.Controllers;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;

namespace PayroTech.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Seed ERP Super Admin
        await SeedSuperAdminAsync(userManager);

        // Seed Sample Company
        await SeedSampleCompanyAsync(context, userManager);

        // Update demo passwords to meet new 12-char requirement
        await UpdateDemoPasswordsAsync(userManager);

        // Ensure manager without face photo is flagged for enrollment
        await EnsureFaceEnrollmentFlagsAsync(userManager);
    }

    private static async Task EnsureFaceEnrollmentFlagsAsync(UserManager<ApplicationUser> userManager)
    {
        // Any non-SuperAdmin user with no face photo should be flagged for enrollment
        var usersNeedingEnrollment = userManager.Users
            .Where(u => u.Role != UserRole.ErpSuperAdmin
                     && !u.IsFaceEnrolled
                     && (u.FaceImagePath == null || u.FaceImagePath == "")
                     && !u.RequiresFaceEnrollment)
            .ToList();

        foreach (var user in usersNeedingEnrollment)
        {
            user.RequiresFaceEnrollment = true;
            await userManager.UpdateAsync(user);
        }

        // Ensure all non-SuperAdmin users have a StaffCode and KioskPin
        var usersNeedingStaffCode = userManager.Users
            .Where(u => u.Role != UserRole.ErpSuperAdmin
                     && (u.StaffCode == null || u.StaffCode == ""))
            .ToList();

        int seq = 1;
        foreach (var user in usersNeedingStaffCode)
        {
            var prefix = user.Role switch
            {
                UserRole.HR           => "HR",
                UserRole.Accountant   => "ACC",
                UserRole.CompanyAdmin => "MGR",
                _                    => "EMP"
            };
            user.StaffCode = $"{prefix}-{seq:D6}";
            user.KioskPin  = user.StaffCode[^4..]; // last 4 digits
            await userManager.UpdateAsync(user);
            seq++;
        }
    }

    private static async Task UpdateDemoPasswordsAsync(UserManager<ApplicationUser> userManager)
    {
        // Map of email → new password for demo accounts
        var demoPasswords = new Dictionary<string, string>
        {
            ["superadmin@payrotech.com"] = "SuperAdmin@123",
            ["admin@payrotech.com"]      = "Admin@12345678",
            ["hr@payrotech.com"]         = "Hr@1234567890",
            ["employee@payrotech.com"]   = "Employee@12345",
            ["accountant@payrotech.com"] = "Accountant@1234"
        };

        foreach (var (email, newPassword) in demoPasswords)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) continue;

            // Check if password already meets requirements (try to validate)
            var valid = await userManager.CheckPasswordAsync(user, newPassword);
            if (valid) continue; // Already updated

            // Reset to new password
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var superAdminEmail = "superadmin@payrotech.com";

        if (await userManager.FindByEmailAsync(superAdminEmail) != null)
            return;

        var superAdmin = new ApplicationUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            FirstName = "Super",
            LastName = "Admin",
            Role = UserRole.ErpSuperAdmin,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,  // Demo account - no password change required
            RequiresFaceEnrollment = false  // SuperAdmin doesn't use kiosk
        };

        await userManager.CreateAsync(superAdmin, "SuperAdmin@123");
    }

    private static async Task SeedSampleCompanyAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.Companies.Any())
            return;

        // Create sample company: PayroTech Solutions
        var company = new Company
        {
            CompanyCode = "PAYROTECH",
            CompanyName = "PayroTech Solutions Inc.",
            Address = "123 Tech Avenue, Makati City, Philippines",
            ContactNumber = "+63 2 8888 7777",
            Email = "info@payrotech.com",
            TIN = "123-456-789-000",
            SSSEmployerNumber = "12-3456789-0",
            PhilHealthEmployerNumber = "12345678901",
            PagIbigEmployerNumber = "123456789012",
            SubscriptionStart = DateTime.UtcNow,
            SubscriptionEnd = DateTime.UtcNow.AddYears(1),
            IsSubscriptionActive = true,
            IsInitialSetupComplete = true,  // Test company is already set up
            DefaultDailyRate = 600  // Default ₱600/day for test company
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        // Enable all modules
        foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
        {
            context.CompanyModules.Add(new CompanyModule
            {
                CompanyId = company.Id,
                ModuleType = moduleType,
                IsEnabled = true
            });
        }
        await context.SaveChangesAsync();

        // Create shifts — Morning: 10AM-7PM, Night: 10PM-7AM
        var dayShift = new Shift
        {
            CompanyId = company.Id,
            ShiftName = "Morning Shift (10AM-7PM)",
            StartTime = new TimeSpan(10, 0, 0),
            EndTime   = new TimeSpan(19, 0, 0),
            BreakStart = new TimeSpan(13, 0, 0),
            BreakEnd   = new TimeSpan(14, 0, 0),
            GracePeriodMinutes = 15
        };

        var nightShift = new Shift
        {
            CompanyId = company.Id,
            ShiftName = "Night Shift (10PM-7AM)",
            StartTime = new TimeSpan(22, 0, 0),
            EndTime   = new TimeSpan(7, 0, 0),
            IsNightShift = true,
            GracePeriodMinutes = 15
        };

        context.Shifts.AddRange(dayShift, nightShift);
        await context.SaveChangesAsync();

        // Create departments
        var hrDept = new Department { CompanyId = company.Id, DepartmentCode = "HR", DepartmentName = "Human Resources" };
        var itDept = new Department { CompanyId = company.Id, DepartmentCode = "IT", DepartmentName = "Information Technology" };
        var finDept = new Department { CompanyId = company.Id, DepartmentCode = "FIN", DepartmentName = "Finance" };
        var opsDept = new Department { CompanyId = company.Id, DepartmentCode = "OPS", DepartmentName = "Operations" };

        context.Departments.AddRange(hrDept, itDept, finDept, opsDept);
        await context.SaveChangesAsync();

        // Create 2026 Philippine holidays
        var holidays = new List<Holiday>
        {
            new() { CompanyId = company.Id, HolidayName = "New Year's Day", Date = new DateTime(2026, 1, 1), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Maundy Thursday", Date = new DateTime(2026, 4, 2), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Good Friday", Date = new DateTime(2026, 4, 3), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Day of Valor", Date = new DateTime(2026, 4, 9), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Labor Day", Date = new DateTime(2026, 5, 1), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Independence Day", Date = new DateTime(2026, 6, 12), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "National Heroes Day", Date = new DateTime(2026, 8, 31), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Bonifacio Day", Date = new DateTime(2026, 11, 30), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Christmas Day", Date = new DateTime(2026, 12, 25), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Rizal Day", Date = new DateTime(2026, 12, 30), HolidayType = HolidayType.Regular },
            new() { CompanyId = company.Id, HolidayName = "Chinese New Year", Date = new DateTime(2026, 2, 17), HolidayType = HolidayType.Special },
            new() { CompanyId = company.Id, HolidayName = "EDSA Revolution", Date = new DateTime(2026, 2, 25), HolidayType = HolidayType.Special },
            new() { CompanyId = company.Id, HolidayName = "Black Saturday", Date = new DateTime(2026, 4, 4), HolidayType = HolidayType.Special },
            new() { CompanyId = company.Id, HolidayName = "Ninoy Aquino Day", Date = new DateTime(2026, 8, 21), HolidayType = HolidayType.Special },
            new() { CompanyId = company.Id, HolidayName = "All Saints Day", Date = new DateTime(2026, 11, 1), HolidayType = HolidayType.Special },
            new() { CompanyId = company.Id, HolidayName = "Christmas Eve", Date = new DateTime(2026, 12, 24), HolidayType = HolidayType.Special },
            new() { CompanyId = company.Id, HolidayName = "New Year's Eve", Date = new DateTime(2026, 12, 31), HolidayType = HolidayType.Special },
        };

        context.Holidays.AddRange(holidays);
        await context.SaveChangesAsync();

        // Create Company Admin user
        var adminEmail = "admin@payrotech.com";
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Company",
            LastName = "Admin",
            CompanyId = company.Id,
            Role = UserRole.CompanyAdmin,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,  // Demo account - no password change required
            RequiresFaceEnrollment = false,
            QRCodeHash = KioskController.GenerateQRCodeHash(adminEmail),
            QRCodeGeneratedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(adminUser, "Admin@12345678");

        // Create HR user
        var hrEmail = "hr@payrotech.com";
        var hrUser = new ApplicationUser
        {
            UserName = hrEmail,
            Email = hrEmail,
            FirstName = "HR",
            LastName = "Officer",
            CompanyId = company.Id,
            Role = UserRole.HR,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,  // Demo account - no password change required
            RequiresFaceEnrollment = false,
            IsFaceEnrolled = true,  // Pretend face is already enrolled for demo
            QRCodeHash = KioskController.GenerateQRCodeHash(hrEmail),
            QRCodeGeneratedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(hrUser, "Hr@1234567890");

        // Create Employee user (for testing employee portal)
        var empEmail = "employee@payrotech.com";
        var empUser = new ApplicationUser
        {
            UserName = empEmail,
            Email = empEmail,
            FirstName = "Juan",
            LastName = "Dela Cruz",
            CompanyId = company.Id,
            Role = UserRole.Employee,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,  // Demo account - no password change required
            RequiresFaceEnrollment = false,
            IsFaceEnrolled = true,  // Pretend face is already enrolled for demo
            QRCodeHash = KioskController.GenerateQRCodeHash(empEmail),
            QRCodeGeneratedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(empUser, "Employee@12345");

        // Create Accountant user (for testing accountant portal)
        var accountantEmail = "accountant@payrotech.com";
        var accountantUser = new ApplicationUser
        {
            UserName = accountantEmail,
            Email = accountantEmail,
            FirstName = "Ana",
            LastName = "Reyes",
            CompanyId = company.Id,
            Role = UserRole.Accountant,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,  // Demo account - no password change required
            RequiresFaceEnrollment = false,
            IsFaceEnrolled = true,  // Pretend face is already enrolled for demo
            QRCodeHash = KioskController.GenerateQRCodeHash(accountantEmail),
            QRCodeGeneratedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(accountantUser, "Accountant@1234");

        // Create sample employees
        var employees = new List<Employee>
        {
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP-001",
                FirstName = "Juan",
                LastName = "Dela Cruz",
                Email = "juan.delacruz@payrotech.com",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Male",
                CivilStatus = "Married",
                Address = "123 Main St, Manila",
                ContactNumber = "+63 912 345 6789",
                HireDate = new DateTime(2020, 1, 15),
                RegularizationDate = new DateTime(2020, 7, 15),
                BasicSalary = 50000,
                SalaryType = SalaryType.Monthly,
                DailyRate = 2272.73m,
                HourlyRate = 284.09m,
                SSSNumber = "12-3456789-0",
                PhilHealthNumber = "12345678901",
                PagIbigNumber = "123456789012",
                TINNumber = "123-456-789",
                DepartmentId = itDept.Id,
                ShiftId = dayShift.Id
            },
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP-002",
                FirstName = "Maria",
                LastName = "Santos",
                Email = "maria.santos@payrotech.com",
                DateOfBirth = new DateTime(1990, 8, 20),
                Gender = "Female",
                CivilStatus = "Single",
                Address = "456 Oak St, Makati",
                ContactNumber = "+63 923 456 7890",
                HireDate = new DateTime(2021, 3, 1),
                RegularizationDate = new DateTime(2021, 9, 1),
                BasicSalary = 35000,
                SalaryType = SalaryType.Monthly,
                DailyRate = 1590.91m,
                HourlyRate = 198.86m,
                SSSNumber = "23-4567890-1",
                PhilHealthNumber = "23456789012",
                PagIbigNumber = "234567890123",
                TINNumber = "234-567-890",
                DepartmentId = hrDept.Id,
                ShiftId = dayShift.Id,
                UserId = hrUser.Id
            },
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP-003",
                FirstName = "Pedro",
                LastName = "Reyes",
                Email = "pedro.reyes@payrotech.com",
                DateOfBirth = new DateTime(1988, 12, 10),
                Gender = "Male",
                CivilStatus = "Married",
                Address = "789 Pine St, Quezon City",
                ContactNumber = "+63 934 567 8901",
                HireDate = new DateTime(2019, 6, 15),
                RegularizationDate = new DateTime(2019, 12, 15),
                BasicSalary = 40000,
                SalaryType = SalaryType.Monthly,
                DailyRate = 1818.18m,
                HourlyRate = 227.27m,
                SSSNumber = "34-5678901-2",
                PhilHealthNumber = "34567890123",
                PagIbigNumber = "345678901234",
                TINNumber = "345-678-901",
                DepartmentId = finDept.Id,
                ShiftId = dayShift.Id
            },
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP-004",
                FirstName = "Ana",
                LastName = "Garcia",
                Email = "ana.garcia@payrotech.com",
                DateOfBirth = new DateTime(1995, 3, 25),
                Gender = "Female",
                CivilStatus = "Single",
                Address = "321 Elm St, Pasig",
                ContactNumber = "+63 945 678 9012",
                HireDate = new DateTime(2022, 9, 1),
                RegularizationDate = new DateTime(2023, 3, 1),
                BasicSalary = 28000,
                SalaryType = SalaryType.Monthly,
                DailyRate = 1272.73m,
                HourlyRate = 159.09m,
                SSSNumber = "45-6789012-3",
                PhilHealthNumber = "45678901234",
                PagIbigNumber = "456789012345",
                TINNumber = "456-789-012",
                DepartmentId = opsDept.Id,
                ShiftId = dayShift.Id
            },
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP-005",
                FirstName = "Jose",
                LastName = "Rizal",
                Email = "jose.rizal@payrotech.com",
                DateOfBirth = new DateTime(1992, 6, 19),
                Gender = "Male",
                CivilStatus = "Single",
                Address = "555 Bonifacio St, Taguig",
                ContactNumber = "+63 956 789 0123",
                HireDate = new DateTime(2023, 1, 15),
                BasicSalary = 22000,
                SalaryType = SalaryType.Monthly,
                DailyRate = 1000m,
                HourlyRate = 125m,
                SSSNumber = "56-7890123-4",
                PhilHealthNumber = "56789012345",
                PagIbigNumber = "567890123456",
                TINNumber = "567-890-123",
                DepartmentId = itDept.Id,
                ShiftId = dayShift.Id
            }
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();

        // Set supervisors
        var emp1 = employees[0];
        var emp2 = employees[1];
        employees[2].SupervisorId = emp1.Id;
        employees[3].SupervisorId = emp2.Id;
        employees[4].SupervisorId = emp1.Id;

        // Set department managers
        itDept.ManagerId = emp1.Id;
        hrDept.ManagerId = emp2.Id;

        await context.SaveChangesAsync();

        // Create leave balances
        foreach (var employee in employees)
        {
            context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                Year = DateTime.Now.Year,
                VacationLeaveBalance = 15,
                SickLeaveBalance = 15
            });
        }

        await context.SaveChangesAsync();
    }
}
