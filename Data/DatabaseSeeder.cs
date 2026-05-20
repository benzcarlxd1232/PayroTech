using Microsoft.AspNetCore.Identity;
using PayroTech.Controllers;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using System.Security.Cryptography;

namespace PayroTech.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Each step is wrapped in try-catch so failures on constrained hosting
        // (e.g., MonsterASP free plan timeouts) don't abort the entire seeder.

        // Step 1: Seed ERP Super Admin (CRITICAL — must succeed for login)
        try
        {
            await SeedSuperAdminAsync(userManager);
            Console.WriteLine("SEED: SuperAdmin accounts created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR [SuperAdmin]: {ex.Message}");
        }

        // Step 2: Seed Sample Company
        try
        {
            await SeedSampleCompanyAsync(context, userManager);
            Console.WriteLine("SEED: Sample company (PayroTech Solutions) created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR [SampleCompany]: {ex.Message}");
        }

        // Step 3: Seed Demo Company with full payroll data
        try
        {
            await SeedDemoCompanyAsync(context, userManager);
            Console.WriteLine("SEED: Demo company (Demo Corp) created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR [DemoCompany]: {ex.Message}");
        }

        // Step 4: Seed Presentation Demo Company
        try
        {
            await SeedPresentationCompanyAsync(context, userManager);
            Console.WriteLine("SEED: Presentation company (Sunrise Bakery) created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR [PresentationCompany]: {ex.Message}");
        }

        // Step 5: Update demo passwords
        try
        {
            await UpdateDemoPasswordsAsync(userManager);
            Console.WriteLine("SEED: Demo passwords updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR [UpdatePasswords]: {ex.Message}");
        }

        // Step 6: Ensure face enrollment flags
        try
        {
            await EnsureFaceEnrollmentFlagsAsync(userManager);
            Console.WriteLine("SEED: Face enrollment flags set successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR [FaceEnrollment]: {ex.Message}");
        }
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
            ["superadmin@payrotech.com"]      = "SuperAdmin@123",
            ["superadmin2@payrotech.com"]     = "SuperAdmin2@123",
            ["superadmin3@payrotech.com"]     = "SuperAdmin3@123",
            ["admin@payrotech.com"]           = "Admin@12345678",
            ["hr@payrotech.com"]              = "Hr@1234567890",
            ["employee@payrotech.com"]        = "Employee@12345",
            ["accountant@payrotech.com"]      = "Accountant@1234",
            // Demo company accounts
            ["manager@democorp.ph"]           = "Manager@12345",
            ["hr1@democorp.ph"]               = "HrDemo@12345",
            ["accountant1@democorp.ph"]       = "AccDemo@12345",
            ["emp1@democorp.ph"]              = "EmpDemo@12345",
            ["emp2@democorp.ph"]              = "EmpDemo@12345",
            ["emp3@democorp.ph"]              = "EmpDemo@12345",
            ["emp4@democorp.ph"]              = "EmpDemo@12345",
            ["emp5@democorp.ph"]              = "EmpDemo@12345",
            // Presentation company (Sunrise Bakery)
            ["sunrise.manager@demo.ph"]       = "SunriseManager@123",
            ["sunrise.hr@demo.ph"]            = "SunriseHR@123",
            ["sunrise.acc@demo.ph"]           = "SunriseAcc@123",
            ["sunrise.emp1@demo.ph"]          = "SunriseEmp@123",
            ["sunrise.emp2@demo.ph"]          = "SunriseEmp@123",
            ["sunrise.emp3@demo.ph"]          = "SunriseEmp@123",
        };

        foreach (var (email, newPassword) in demoPasswords)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) continue;

            var valid = await userManager.CheckPasswordAsync(user, newPassword);
            if (valid) continue;

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        // Primary SuperAdmin
        await CreateSuperAdminIfNotExists(userManager,
            "superadmin@payrotech.com", "Super", "Admin", "SuperAdmin@123");

        // Backup SuperAdmin accounts
        await CreateSuperAdminIfNotExists(userManager,
            "superadmin2@payrotech.com", "Super", "Admin2", "SuperAdmin2@123");

        await CreateSuperAdminIfNotExists(userManager,
            "superadmin3@payrotech.com", "Super", "Admin3", "SuperAdmin3@123");
    }

    private static async Task CreateSuperAdminIfNotExists(
        UserManager<ApplicationUser> userManager,
        string email, string firstName, string lastName, string password)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;

        var admin = new ApplicationUser
        {
            UserName               = email,
            Email                  = email,
            FirstName              = firstName,
            LastName               = lastName,
            Role                   = UserRole.ErpSuperAdmin,
            IsActive               = true,
            EmailConfirmed         = true,
            MustChangePassword     = false,
            RequiresFaceEnrollment = false
        };
        await userManager.CreateAsync(admin, password);
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

    // ─────────────────────────────────────────────────────────────────────────
    // DEMO COMPANY — "Demo Corp Philippines" with full payroll flow data
    // Accounts: manager@democorp.ph / hr1@democorp.ph / accountant1@democorp.ph
    //           emp1-5@democorp.ph
    // All passwords set in UpdateDemoPasswordsAsync above
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedDemoCompanyAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Skip if demo company already exists
        if (context.Companies.Any(c => c.CompanyCode == "DEMOCORP"))
            return;

        // ── Company ──────────────────────────────────────────────────────────
        var company = new Company
        {
            CompanyCode             = "DEMOCORP",
            CompanyName             = "Demo Corp Philippines",
            Address                 = "456 Ayala Avenue, Makati City, Metro Manila",
            ContactNumber           = "+63 2 8123 4567",
            Email                   = "info@democorp.ph",
            TIN                     = "987-654-321-000",
            SSSEmployerNumber       = "98-7654321-0",
            PhilHealthEmployerNumber = "98765432101",
            PagIbigEmployerNumber   = "987654321012",
            SubscriptionStart       = DateTime.UtcNow.AddMonths(-3),
            SubscriptionEnd         = DateTime.UtcNow.AddMonths(9),
            IsSubscriptionActive    = true,
            IsInitialSetupComplete  = true,
            WorkDaysPerWeek         = 5,
            WorkingDaysPerMonth     = 22,
            WorkOnHolidays          = true,
            HolidayPayRate          = 200,
            OvertimeRatePerHour     = 1.25m,
            DefaultDailyRate        = 700,
            HRDailyRate             = 900,
            AccountantDailyRate     = 950
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        // Enable all modules
        foreach (ModuleType mt in Enum.GetValues(typeof(ModuleType)))
            context.CompanyModules.Add(new CompanyModule { CompanyId = company.Id, ModuleType = mt, IsEnabled = true });
        await context.SaveChangesAsync();

        // ── Shifts ───────────────────────────────────────────────────────────
        var dayShift = new Shift
        {
            CompanyId = company.Id, ShiftName = "Day Shift (8AM-5PM)",
            StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(17, 0, 0),
            BreakStart = new TimeSpan(12, 0, 0), BreakEnd = new TimeSpan(13, 0, 0),
            GracePeriodMinutes = 15, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        context.Shifts.Add(dayShift);
        await context.SaveChangesAsync();

        // ── Departments ──────────────────────────────────────────────────────
        var salesDept = new Department { CompanyId = company.Id, DepartmentCode = "SALES", DepartmentName = "Sales Team", IsActive = true };
        var techDept  = new Department { CompanyId = company.Id, DepartmentCode = "TECH",  DepartmentName = "Tech Team",  IsActive = true };
        var opsDept   = new Department { CompanyId = company.Id, DepartmentCode = "OPS",   DepartmentName = "Operations", IsActive = true };
        context.Departments.AddRange(salesDept, techDept, opsDept);
        await context.SaveChangesAsync();

        // ── Users ────────────────────────────────────────────────────────────
        var makeUser = (string email, string first, string last, UserRole role, int? deptId) => new ApplicationUser
        {
            UserName = email, Email = email, FirstName = first, LastName = last,
            CompanyId = company.Id, DepartmentId = deptId, Role = role,
            IsActive = true, EmailConfirmed = true,
            MustChangePassword = false, RequiresFaceEnrollment = false, IsFaceEnrolled = true,
            QRCodeHash = KioskController.GenerateQRCodeHash(email), QRCodeGeneratedAt = DateTime.UtcNow,
            StaffCode = $"{(role == UserRole.HR ? "HR" : role == UserRole.Accountant ? "ACC" : role == UserRole.CompanyAdmin ? "MGR" : "EMP")}-DC-{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000,9999)}",
            DailyRate = role == UserRole.HR ? 900 : role == UserRole.Accountant ? 950 : 700
        };

        var managerUser    = makeUser("manager@democorp.ph",    "Carlo",  "Reyes",   UserRole.CompanyAdmin, null);
        var hrUser         = makeUser("hr1@democorp.ph",        "Liza",   "Santos",  UserRole.HR,           salesDept.Id);
        var accountantUser = makeUser("accountant1@democorp.ph","Marco",  "Dela Cruz",UserRole.Accountant,  techDept.Id);

        await userManager.CreateAsync(managerUser,    "Manager@12345");
        await userManager.CreateAsync(hrUser,         "HrDemo@12345");
        await userManager.CreateAsync(accountantUser, "AccDemo@12345");

        // ── Employees ────────────────────────────────────────────────────────
        var today = DateTime.Today;
        var empData = new[]
        {
            ("emp1@democorp.ph", "Jose",    "Bautista",  salesDept.Id, 800m,  "EMP-DC-0001"),
            ("emp2@democorp.ph", "Maria",   "Cruz",      salesDept.Id, 750m,  "EMP-DC-0002"),
            ("emp3@democorp.ph", "Pedro",   "Villanueva",techDept.Id,  900m,  "EMP-DC-0003"),
            ("emp4@democorp.ph", "Ana",     "Mendoza",   techDept.Id,  850m,  "EMP-DC-0004"),
            ("emp5@democorp.ph", "Roberto", "Garcia",    opsDept.Id,   700m,  "EMP-DC-0005"),
        };

        var empUsers = new List<ApplicationUser>();
        foreach (var (email, first, last, deptId, rate, code) in empData)
        {
            var eu = makeUser(email, first, last, UserRole.Employee, deptId);
            eu.StaffCode  = code;
            eu.DailyRate  = rate;
            eu.KioskPin   = code[^4..];
            await userManager.CreateAsync(eu, "EmpDemo@12345");
            empUsers.Add(eu);
        }

        // Employee entities (linked to ApplicationUser)
        var employees = new List<Employee>();
        var empDetails = new[]
        {
            (empUsers[0], salesDept.Id, 800m,  "EMP-DC-0001", "12-1111111-1", "11111111111", "111111111111", "111-111-111"),
            (empUsers[1], salesDept.Id, 750m,  "EMP-DC-0002", "12-2222222-2", "22222222222", "222222222222", "222-222-222"),
            (empUsers[2], techDept.Id,  900m,  "EMP-DC-0003", "12-3333333-3", "33333333333", "333333333333", "333-333-333"),
            (empUsers[3], techDept.Id,  850m,  "EMP-DC-0004", "12-4444444-4", "44444444444", "444444444444", "444-444-444"),
            (empUsers[4], opsDept.Id,   700m,  "EMP-DC-0005", "12-5555555-5", "55555555555", "555555555555", "555-555-555"),
        };

        int empNum = 1;
        foreach (var (eu, deptId, rate, code, sss, ph, pagibig, tin) in empDetails)
        {
            var emp = new Employee
            {
                CompanyId      = company.Id,
                UserId         = eu.Id,
                EmployeeNumber = code,
                FirstName      = eu.FirstName,
                LastName       = eu.LastName,
                Email          = eu.Email,
                DateOfBirth    = new DateTime(1990 + empNum, empNum * 2, 10),
                Gender         = empNum % 2 == 0 ? "Female" : "Male",
                CivilStatus    = "Single",
                Address        = $"{empNum * 100} Demo St, Makati City",
                ContactNumber  = $"+63 9{empNum:D2}0 000 000{empNum}",
                HireDate       = today.AddMonths(-6),
                BasicSalary    = rate * 22,
                SalaryType     = SalaryType.Daily,
                DailyRate      = rate,
                HourlyRate     = rate / 8,
                SSSNumber      = sss,
                PhilHealthNumber = ph,
                PagIbigNumber  = pagibig,
                TINNumber      = tin,
                DepartmentId   = deptId,
                ShiftId        = dayShift.Id,
                IsActive       = true
            };
            employees.Add(emp);
            empNum++;
        }
        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();

        // Leave balances
        foreach (var emp in employees)
            context.LeaveBalances.Add(new LeaveBalance { EmployeeId = emp.Id, Year = today.Year, VacationLeaveBalance = 15, SickLeaveBalance = 15 });
        await context.SaveChangesAsync();

        // ── Attendance (last 22 working days) ────────────────────────────────
        var attendanceRecords = new List<Attendance>();
        var workDays = Enumerable.Range(1, 30)
            .Select(d => today.AddDays(-d))
            .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
            .Take(22).ToList();


        foreach (var emp in employees)
        {
            var dailyRate  = emp.DailyRate  ?? 700m;
            var hourlyRate = emp.HourlyRate ?? (dailyRate / 8m);

            foreach (var day in workDays)
            {
                // 90% attendance rate
                if (RandomNumberGenerator.GetInt32(100) < 10) continue;

                var lateMinutes = RandomNumberGenerator.GetInt32(100) < 15 ? RandomNumberGenerator.GetInt32(5, 45) : 0;
                var timeIn  = dayShift.StartTime.Add(TimeSpan.FromMinutes(lateMinutes));
                var timeOut = dayShift.EndTime.Add(TimeSpan.FromMinutes(RandomNumberGenerator.GetInt32(0, 40) - 10));
                var otMinutes = RandomNumberGenerator.GetInt32(100) < 20 ? RandomNumberGenerator.GetInt32(60, 180) : 0; // 1–3 hrs OT

                var lateDeduct = (hourlyRate / 60m) * lateMinutes;
                var otAmount   = (hourlyRate * 1.25m) * (otMinutes / 60m);

                attendanceRecords.Add(new Attendance
                {
                    EmployeeId          = emp.Id,
                    Date                = day,
                    TimeIn              = day.Add(timeIn),
                    TimeOut             = day.Add(timeOut),
                    LateMinutes         = lateMinutes,
                    OvertimeMinutes     = otMinutes,
                    WorkedMinutes       = (int)(dayShift.EndTime - dayShift.StartTime).TotalMinutes - 60 + otMinutes,
                    LateDeductionAmount = lateDeduct,
                    OvertimeAmount      = otAmount,
                    Status              = lateMinutes > 0 ? AttendanceStatus.Late : AttendanceStatus.Present,
                    IsApproved          = true
                });
            }
        }
        context.Attendances.AddRange(attendanceRecords);
        await context.SaveChangesAsync();

        // ── Payroll Periods ──────────────────────────────────────────────────
        // Period 1: Last month (Approved — ready for accountant to distribute)
        var lastMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var lastMonthEnd   = lastMonthStart.AddMonths(1).AddDays(-1);

        var period1 = new PayrollPeriod
        {
            CompanyId  = company.Id,
            PeriodName = $"{lastMonthStart:MMMM yyyy} Payroll",
            StartDate  = lastMonthStart,
            EndDate    = lastMonthEnd,
            PayDate    = lastMonthEnd.AddDays(5),
            Status     = PayrollStatus.Approved
        };

        // Period 2: Two months ago (Paid — historical)
        var twoMonthsStart = lastMonthStart.AddMonths(-1);
        var twoMonthsEnd   = twoMonthsStart.AddMonths(1).AddDays(-1);

        var period2 = new PayrollPeriod
        {
            CompanyId  = company.Id,
            PeriodName = $"{twoMonthsStart:MMMM yyyy} Payroll",
            StartDate  = twoMonthsStart,
            EndDate    = twoMonthsEnd,
            PayDate    = twoMonthsEnd.AddDays(5),
            Status     = PayrollStatus.Paid
        };

        context.PayrollPeriods.AddRange(period1, period2);
        await context.SaveChangesAsync();

        // ── Payroll Records ──────────────────────────────────────────────────
        static (decimal sss, decimal philhealth, decimal pagibig, decimal tax) ComputeDeductions(decimal gross)
        {
            var sss        = Math.Min(gross * 0.045m, 900m);
            var philhealth = Math.Min(gross * 0.025m, 625m);
            var pagibig    = Math.Min(gross * 0.02m,  200m);
            var taxable    = gross - sss - philhealth - pagibig;
            var tax        = taxable > 33333m ? (taxable - 33333m) * 0.20m : 0m;
            return (sss, philhealth, pagibig, tax);
        }

        var payrollsToAdd = new List<Payroll>();

        foreach (var emp in employees)
        {
            var dailyRateP  = emp.DailyRate  ?? 700m;
            var hourlyRateP = emp.HourlyRate ?? (dailyRateP / 8m);

            // Period 1 — Approved (pending distribution by accountant)
            var daysWorked1 = (decimal)workDays.Count(d => d >= lastMonthStart && d <= lastMonthEnd) * 0.9m;
            var otMinutes1  = attendanceRecords
                .Where(a => a.EmployeeId == emp.Id && a.Date >= lastMonthStart && a.Date <= lastMonthEnd)
                .Sum(a => a.OvertimeMinutes);
            var lateMin1    = attendanceRecords
                .Where(a => a.EmployeeId == emp.Id && a.Date >= lastMonthStart && a.Date <= lastMonthEnd)
                .Sum(a => a.LateMinutes);

            var otHours1    = otMinutes1 / 60m;
            var basicPay1   = dailyRateP * daysWorked1;
            var otPay1      = hourlyRateP * otHours1 * 1.25m;
            var lateDeduct1 = (hourlyRateP / 60m) * lateMin1;
            var gross1      = basicPay1 + otPay1;
            var (sss1, ph1, pi1, tax1) = ComputeDeductions(gross1);
            var totalDeduct1 = lateDeduct1 + sss1 + ph1 + pi1 + tax1;

            payrollsToAdd.Add(new Payroll
            {
                EmployeeId             = emp.Id,
                PayrollPeriodId        = period1.Id,
                BasicPay               = basicPay1,
                OvertimePay            = otPay1,
                GrossPay               = gross1,
                LateDeduction          = lateDeduct1,
                SSSContribution        = sss1,
                PhilHealthContribution = ph1,
                PagIbigContribution    = pi1,
                WithholdingTax         = tax1,
                TotalDeductions        = totalDeduct1,
                NetPay                 = gross1 - totalDeduct1,
                DaysWorked             = daysWorked1,
                OvertimeHours          = otHours1,
                LateHours              = lateMin1 / 60m,
                Status                 = PayrollStatus.Approved
            });

            // Period 2 — Paid (historical)
            var daysWorked2 = 20m;
            var basicPay2   = dailyRateP * daysWorked2;
            var gross2      = basicPay2;
            var (sss2, ph2, pi2, tax2) = ComputeDeductions(gross2);
            var totalDeduct2 = sss2 + ph2 + pi2 + tax2;

            payrollsToAdd.Add(new Payroll
            {
                EmployeeId             = emp.Id,
                PayrollPeriodId        = period2.Id,
                BasicPay               = basicPay2,
                OvertimePay            = 0,
                GrossPay               = gross2,
                SSSContribution        = sss2,
                PhilHealthContribution = ph2,
                PagIbigContribution    = pi2,
                WithholdingTax         = tax2,
                TotalDeductions        = totalDeduct2,
                NetPay                 = gross2 - totalDeduct2,
                DaysWorked             = daysWorked2,
                Status                 = PayrollStatus.Paid,
                ProcessedAt          = twoMonthsEnd.AddDays(5)
            });
        }

        context.Payrolls.AddRange(payrollsToAdd);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRESENTATION COMPANY — "Sunrise Bakery Co." — clean demo for professor
    // Shows full flow: HR submits attendance → Manager approves budget → Accountant distributes
    //
    // Accounts:
    //   Manager:    sunrise.manager@demo.ph  / SunriseManager@123
    //   HR:         sunrise.hr@demo.ph       / SunriseHR@123
    //   Accountant: sunrise.acc@demo.ph      / SunriseAcc@123
    //   Employees:  sunrise.emp1-3@demo.ph   / SunriseEmp@123
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedPresentationCompanyAsync(
        ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.Companies.Any(c => c.CompanyCode == "SUNRISE"))
            return;

        // ── Company ──────────────────────────────────────────────────────────
        var company = new Company
        {
            CompanyCode              = "SUNRISE",
            CompanyName              = "Sunrise Bakery Co.",
            Address                  = "88 Rizal Street, Cebu City",
            ContactNumber            = "+63 32 888 1234",
            Email                    = "info@sunrisebakery.ph",
            TIN                      = "111-222-333-000",
            SSSEmployerNumber        = "11-2223334-0",
            PhilHealthEmployerNumber = "11222333401",
            PagIbigEmployerNumber    = "112223334012",
            SubscriptionStart        = DateTime.UtcNow.AddMonths(-1),
            SubscriptionEnd          = DateTime.UtcNow.AddMonths(11),
            IsSubscriptionActive     = true,
            IsInitialSetupComplete   = true,
            IsActive                 = true,
            WorkDaysPerWeek          = 6,   // Mon–Sat (bakery)
            WorkingDaysPerMonth      = 26,
            WorkOnHolidays           = true,
            HolidayPayRate           = 200,
            OvertimeRatePerHour      = 1.25m,
            DefaultDailyRate         = 650m,
            HRDailyRate              = 850m,
            AccountantDailyRate      = 900m,
            PayrollFrequency         = PayrollFrequency.Monthly
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        foreach (ModuleType mt in Enum.GetValues(typeof(ModuleType)))
            context.CompanyModules.Add(new CompanyModule { CompanyId = company.Id, ModuleType = mt, IsEnabled = true });
        await context.SaveChangesAsync();

        // ── Shift ─────────────────────────────────────────────────────────────
        var shift = new Shift
        {
            CompanyId = company.Id, ShiftName = "Morning Shift (6AM-2PM)",
            StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(14, 0, 0),
            BreakStart = new TimeSpan(10, 0, 0), BreakEnd = new TimeSpan(10, 30, 0),
            GracePeriodMinutes = 10, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        context.Shifts.Add(shift);
        await context.SaveChangesAsync();

        // ── Departments ───────────────────────────────────────────────────────
        var prodDept  = new Department { CompanyId = company.Id, DepartmentCode = "PROD",  DepartmentName = "Production",  IsActive = true };
        var salesDept = new Department { CompanyId = company.Id, DepartmentCode = "SALES", DepartmentName = "Sales",       IsActive = true };
        context.Departments.AddRange(prodDept, salesDept);
        await context.SaveChangesAsync();

        // ── Users ─────────────────────────────────────────────────────────────
        var makeUser = (string email, string first, string last, UserRole role, int? deptId, decimal rate) =>
            new ApplicationUser
            {
                UserName = email, Email = email, FirstName = first, LastName = last,
                CompanyId = company.Id, DepartmentId = deptId, Role = role,
                IsActive = true, EmailConfirmed = true,
                MustChangePassword = false, RequiresFaceEnrollment = false, IsFaceEnrolled = true,
                QRCodeHash = KioskController.GenerateQRCodeHash(email), QRCodeGeneratedAt = DateTime.UtcNow,
                StaffCode = $"{(role == UserRole.HR ? "HR" : role == UserRole.Accountant ? "ACC" : role == UserRole.CompanyAdmin ? "MGR" : "EMP")}-SR-{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000,9999)}",
                DailyRate = rate
            };

        var managerUser    = makeUser("sunrise.manager@demo.ph", "Maria",  "Santos",  UserRole.CompanyAdmin, null,          800m);
        var hrUser         = makeUser("sunrise.hr@demo.ph",      "Jose",   "Reyes",   UserRole.HR,           prodDept.Id,   850m);
        var accountantUser = makeUser("sunrise.acc@demo.ph",     "Ana",    "Cruz",    UserRole.Accountant,   salesDept.Id,  900m);

        await userManager.CreateAsync(managerUser,    "SunriseManager@123");
        await userManager.CreateAsync(hrUser,         "SunriseHR@123");
        await userManager.CreateAsync(accountantUser, "SunriseAcc@123");

        // ── Employees ─────────────────────────────────────────────────────────
        var today = DateTime.Today;
        var empData = new[]
        {
            ("sunrise.emp1@demo.ph", "Pedro",   "Villanueva", prodDept.Id,  650m, "EMP-SR-0001"),
            ("sunrise.emp2@demo.ph", "Liza",    "Bautista",   prodDept.Id,  650m, "EMP-SR-0002"),
            ("sunrise.emp3@demo.ph", "Roberto", "Mendoza",    salesDept.Id, 700m, "EMP-SR-0003"),
        };

        var empUsers = new List<ApplicationUser>();
        foreach (var (email, first, last, deptId, rate, code) in empData)
        {
            var eu = makeUser(email, first, last, UserRole.Employee, deptId, rate);
            eu.StaffCode = code;
            eu.KioskPin  = code[^4..];
            await userManager.CreateAsync(eu, "SunriseEmp@123");
            empUsers.Add(eu);
        }

        var employees = new List<Employee>();
        var empDetails = new[]
        {
            (empUsers[0], prodDept.Id,  650m, "EMP-SR-0001", "11-1111111-1", "11111111111", "111111111111", "111-111-111"),
            (empUsers[1], prodDept.Id,  650m, "EMP-SR-0002", "11-2222222-2", "22222222222", "222222222222", "222-222-222"),
            (empUsers[2], salesDept.Id, 700m, "EMP-SR-0003", "11-3333333-3", "33333333333", "333333333333", "333-333-333"),
        };

        int n = 1;
        foreach (var (eu, deptId, rate, code, sss, ph, pagibig, tin) in empDetails)
        {
            employees.Add(new Employee
            {
                CompanyId = company.Id, UserId = eu.Id, EmployeeNumber = code,
                FirstName = eu.FirstName, LastName = eu.LastName, Email = eu.Email,
                DateOfBirth = new DateTime(1992 + n, n * 3, 10),
                Gender = n % 2 == 0 ? "Female" : "Male", CivilStatus = "Single",
                Address = $"{n * 10} Bakery Lane, Cebu City",
                ContactNumber = $"+63 9{n:D2}0 111 111{n}",
                HireDate = today.AddMonths(-8),
                BasicSalary = rate * 26,
                SalaryType = SalaryType.Daily,
                DailyRate = rate, HourlyRate = rate / 8m,
                SSSNumber = sss, PhilHealthNumber = ph, PagIbigNumber = pagibig, TINNumber = tin,
                DepartmentId = deptId, ShiftId = shift.Id, IsActive = true
            });
            n++;
        }
        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();

        // Leave balances
        foreach (var emp in employees)
            context.LeaveBalances.Add(new LeaveBalance { EmployeeId = emp.Id, Year = today.Year, VacationLeaveBalance = 15, SickLeaveBalance = 15 });
        await context.SaveChangesAsync();

        // ── Attendance (last 22 working days — realistic data) ────────────────
        var workDays = Enumerable.Range(1, 35)
            .Select(d => today.AddDays(-d))
            .Where(d => d.DayOfWeek != DayOfWeek.Sunday)  // Mon–Sat for bakery
            .Take(22).ToList();


        var attendanceRecords = new List<Attendance>();
        foreach (var emp in employees)
        {
            var dailyRate  = emp.DailyRate  ?? 650m;
            var hourlyRate = emp.HourlyRate ?? (dailyRate / 8m);

            foreach (var day in workDays)
            {
                if (RandomNumberGenerator.GetInt32(100) < 8) continue; // 92% attendance

                var lateMinutes = RandomNumberGenerator.GetInt32(100) < 12 ? RandomNumberGenerator.GetInt32(5, 30) : 0;
                var timeIn  = shift.StartTime.Add(TimeSpan.FromMinutes(lateMinutes));
                var timeOut = shift.EndTime.Add(TimeSpan.FromMinutes(RandomNumberGenerator.GetInt32(0, 50) - 5));
                var otMinutes = RandomNumberGenerator.GetInt32(100) < 25 ? RandomNumberGenerator.GetInt32(30, 120) : 0;

                attendanceRecords.Add(new Attendance
                {
                    EmployeeId          = emp.Id,
                    Date                = day,
                    TimeIn              = day.Add(timeIn),
                    TimeOut             = day.Add(timeOut),
                    LateMinutes         = lateMinutes,
                    OvertimeMinutes     = otMinutes,
                    WorkedMinutes       = (int)(shift.EndTime - shift.StartTime).TotalMinutes - 30 + otMinutes,
                    LateDeductionAmount = Math.Round((hourlyRate / 60m) * lateMinutes, 2),
                    OvertimeAmount      = Math.Round((hourlyRate * 0.25m) * (otMinutes / 60m), 2),
                    Status              = lateMinutes > 0 ? AttendanceStatus.Late : AttendanceStatus.Present,
                    IsApproved          = true
                });
            }
        }
        context.Attendances.AddRange(attendanceRecords);
        await context.SaveChangesAsync();

        // ── Payroll Period — Approved (ready for accountant to distribute) ────
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);

        var period = new PayrollPeriod
        {
            CompanyId  = company.Id,
            PeriodName = $"{monthStart:MMMM yyyy} Payroll",
            StartDate  = monthStart,
            EndDate    = monthEnd,
            PayDate    = monthEnd.AddDays(5),
            Status     = PayrollStatus.Approved  // Manager already approved — accountant can distribute
        };
        context.PayrollPeriods.Add(period);
        await context.SaveChangesAsync();

        // Compute payroll for each employee
        static (decimal sss, decimal ph, decimal pi, decimal tax) Deduct(decimal gross)
        {
            var s = Math.Min(gross * 0.045m, 900m);
            var p = Math.Min(gross * 0.025m, 625m);
            var i = Math.Min(gross * 0.02m,  200m);
            var t = (gross - s - p - i) > 33333m ? ((gross - s - p - i) - 33333m) * 0.20m : 0m;
            return (s, p, i, t);
        }

        var payrolls = new List<Payroll>();
        foreach (var emp in employees)
        {
            var empAtt      = attendanceRecords.Where(a => a.EmployeeId == emp.Id && a.Date >= monthStart).ToList();
            var dailyRateP  = emp.DailyRate  ?? 650m;
            var hourlyRateP = emp.HourlyRate ?? (dailyRateP / 8m);
            var daysWorked  = (decimal)empAtt.Count(a => a.TimeIn != null);
            var lateMin     = empAtt.Sum(a => a.LateMinutes);
            var otMin       = empAtt.Sum(a => a.OvertimeMinutes);
            var absentDays  = Math.Max(0, 22m - daysWorked);

            var basicPay    = dailyRateP * daysWorked;
            var otPay       = (hourlyRateP * 1.25m) * (otMin / 60m);
            var gross       = basicPay + otPay;
            var lateDeduct  = (hourlyRateP / 60m) * lateMin;
            var absDeduct   = dailyRateP * absentDays;
            var (sss, ph, pi, tax) = Deduct(gross);
            var totalDeduct = lateDeduct + absDeduct + sss + ph + pi + tax;

            payrolls.Add(new Payroll
            {
                EmployeeId             = emp.Id,
                PayrollPeriodId        = period.Id,
                BasicPay               = Math.Round(basicPay, 2),
                OvertimePay            = Math.Round(otPay, 2),
                GrossPay               = Math.Round(gross, 2),
                LateDeduction          = Math.Round(lateDeduct, 2),
                AbsenceDeduction       = Math.Round(absDeduct, 2),
                SSSContribution        = Math.Round(sss, 2),
                PhilHealthContribution = Math.Round(ph, 2),
                PagIbigContribution    = Math.Round(pi, 2),
                WithholdingTax         = Math.Round(tax, 2),
                TotalDeductions        = Math.Round(totalDeduct, 2),
                NetPay                 = Math.Round(gross - totalDeduct, 2),
                DaysWorked             = daysWorked,
                OvertimeHours          = Math.Round(otMin / 60m, 2),
                LateHours              = Math.Round(lateMin / 60m, 2),
                AbsentDays             = absentDays,
                Status                 = PayrollStatus.Approved  // Ready for accountant
            });
        }
        context.Payrolls.AddRange(payrolls);
        await context.SaveChangesAsync();
    }
}
