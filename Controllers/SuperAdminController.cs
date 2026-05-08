using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Models.ViewModels;
using PayroTech.Services;

namespace PayroTech.Controllers;

[Authorize]
public class SuperAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGeneratorService _passwordGenerator;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLogService;
    private readonly IPaymentService _paymentService;
    private readonly IVendorAuditService _vendorAuditService;
    private readonly ISubscriptionMonitorService _subscriptionMonitor;

    public SuperAdminController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        IPasswordGeneratorService passwordGenerator,
        IEmailService emailService,
        IAuditLogService auditLogService,
        IPaymentService paymentService,
        IVendorAuditService vendorAuditService,
        ISubscriptionMonitorService subscriptionMonitor)
    {
        _context = context;
        _userManager = userManager;
        _passwordGenerator = passwordGenerator;
        _emailService = emailService;
        _auditLogService = auditLogService;
        _paymentService = paymentService;
        _vendorAuditService = vendorAuditService;
        _subscriptionMonitor = subscriptionMonitor;
    }

    private async Task<bool> IsSuperAdmin()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Role == UserRole.ErpSuperAdmin;
    }

    // Super Admin Dashboard
    public async Task<IActionResult> Index()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var dashboard = new SuperAdminDashboardViewModel
        {
            TotalCompanies    = await _context.Companies.CountAsync(),
            ActiveCompanies   = await _context.Companies.CountAsync(c => c.IsActive && c.IsSubscriptionActive),
            TotalUsers        = await _context.Users.CountAsync(u => u.Role != UserRole.ErpSuperAdmin && u.IsActive),
            TotalEmployees    = await _context.Employees.CountAsync(e => e.IsActive),
            TotalStaff        = await _context.Users.CountAsync(u => u.Role != UserRole.ErpSuperAdmin && u.IsActive),
            TotalHR           = await _context.Users.CountAsync(u => u.Role == UserRole.HR && u.IsActive),
            TotalAccountants  = await _context.Users.CountAsync(u => u.Role == UserRole.Accountant && u.IsActive),
            TotalCompanyAdmins= await _context.Users.CountAsync(u => u.Role == UserRole.CompanyAdmin && u.IsActive),
            RecentCompanies = await _context.Companies
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new CompanyListItem
                {
                    Id = c.Id,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    IsActive = c.IsActive,
                    IsSubscriptionActive = c.IsSubscriptionActive,
                    SubscriptionEnd = c.SubscriptionEnd,
                    EmployeeCount = c.Employees.Count(e => e.IsActive)
                })
                .ToListAsync(),
            ExpiringSubscriptions = await _context.Companies
                .Where(c => c.IsSubscriptionActive && c.SubscriptionEnd <= DateTime.UtcNow.AddDays(30))
                .OrderBy(c => c.SubscriptionEnd)
                .Take(5)
                .Select(c => new CompanyListItem
                {
                    Id = c.Id,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    IsActive = c.IsActive,
                    IsSubscriptionActive = c.IsSubscriptionActive,
                    SubscriptionEnd = c.SubscriptionEnd,
                    EmployeeCount = c.Employees.Count(e => e.IsActive)
                })
                .ToListAsync()
        };

        return View(dashboard);
    }

    // Company Management
    public async Task<IActionResult> Companies()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var companies = await _context.Companies
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CompanyListItem
            {
                Id = c.Id,
                CompanyCode = c.CompanyCode,
                CompanyName = c.CompanyName,
                IsActive = c.IsActive,
                IsSubscriptionActive = c.IsSubscriptionActive,
                SubscriptionStart = c.SubscriptionStart,
                SubscriptionEnd = c.SubscriptionEnd,
                EmployeeCount = c.Employees.Count(e => e.IsActive)
            })
            .ToListAsync();

        return View(companies);
    }

    public async Task<IActionResult> CreateCompany()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        // Populate ViewBag with dropdown data
        ViewBag.IndustryTypes = new List<string>
        {
            "Technology & IT",
            "Healthcare & Medical", 
            "Education & Training",
            "Retail & E-commerce",
            "Manufacturing",
            "Construction & Real Estate",
            "Finance & Banking",
            "Food & Beverage",
            "Transportation & Logistics",
            "Hospitality & Tourism",
            "Professional Services",
            "Media & Entertainment",
            "Agriculture",
            "Energy & Utilities",
            "Telecommunications",
            "Government & Public Sector",
            "Non-Profit & NGO",
            "Other"
        };

        ViewBag.PaymentMethods = new List<object>
        {
            new { id = 1, name = "GCash", online = true },
            new { id = 2, name = "PayMaya", online = true },
            new { id = 3, name = "Bank Transfer", online = false },
            new { id = 4, name = "Credit/Debit Card", online = true },
            new { id = 5, name = "Over the Counter", online = false }
        };

        // Pre-load provinces for immediate display (no loading state)
        ViewBag.Provinces = new[]
        {
            new { code = "NCR", name = "Metro Manila" },
            new { code = "ABR", name = "Abra" },
            new { code = "AGN", name = "Agusan del Norte" },
            new { code = "AGS", name = "Agusan del Sur" },
            new { code = "AKL", name = "Aklan" },
            new { code = "ALB", name = "Albay" },
            new { code = "ANT", name = "Antique" },
            new { code = "APA", name = "Apayao" },
            new { code = "AUR", name = "Aurora" },
            new { code = "BAS", name = "Basilan" },
            new { code = "BAN", name = "Bataan" },
            new { code = "BTN", name = "Batanes" },
            new { code = "BTG", name = "Batangas" },
            new { code = "BEN", name = "Benguet" },
            new { code = "BIL", name = "Biliran" },
            new { code = "BOH", name = "Bohol" },
            new { code = "BUK", name = "Bukidnon" },
            new { code = "BUL", name = "Bulacan" },
            new { code = "CAG", name = "Cagayan" },
            new { code = "CAN", name = "Camarines Norte" },
            new { code = "CAS", name = "Camarines Sur" },
            new { code = "CAM", name = "Camiguin" },
            new { code = "CAP", name = "Capiz" },
            new { code = "CAT", name = "Catanduanes" },
            new { code = "CAV", name = "Cavite" },
            new { code = "CEB", name = "Cebu" },
            new { code = "COM", name = "Compostela Valley" },
            new { code = "DAO", name = "Davao del Norte" },
            new { code = "DAS", name = "Davao del Sur" },
            new { code = "DAC", name = "Davao de Oro" },
            new { code = "DVO", name = "Davao Occidental" },
            new { code = "EAS", name = "Eastern Samar" },
            new { code = "GUI", name = "Guimaras" },
            new { code = "IFU", name = "Ifugao" },
            new { code = "ILN", name = "Ilocos Norte" },
            new { code = "ILS", name = "Ilocos Sur" },
            new { code = "ILO", name = "Iloilo" },
            new { code = "ISA", name = "Isabela" },
            new { code = "KAL", name = "Kalinga" },
            new { code = "LAG", name = "Laguna" },
            new { code = "LAN", name = "Lanao del Norte" },
            new { code = "LAS", name = "Lanao del Sur" },
            new { code = "LUN", name = "La Union" },
            new { code = "LEY", name = "Leyte" },
            new { code = "MAG", name = "Maguindanao" },
            new { code = "MAD", name = "Marinduque" },
            new { code = "MAS", name = "Masbate" },
            new { code = "MDC", name = "Mindoro Occidental" },
            new { code = "MDR", name = "Mindoro Oriental" },
            new { code = "MSC", name = "Misamis Occidental" },
            new { code = "MSR", name = "Misamis Oriental" },
            new { code = "MOU", name = "Mountain Province" },
            new { code = "NEC", name = "Negros Occidental" },
            new { code = "NER", name = "Negros Oriental" },
            new { code = "NSA", name = "Northern Samar" },
            new { code = "NUE", name = "Nueva Ecija" },
            new { code = "NUV", name = "Nueva Vizcaya" },
            new { code = "PAM", name = "Pampanga" },
            new { code = "PAN", name = "Pangasinan" },
            new { code = "PAL", name = "Palawan" },
            new { code = "QUE", name = "Quezon" },
            new { code = "QUI", name = "Quirino" },
            new { code = "RIZ", name = "Rizal" },
            new { code = "ROM", name = "Romblon" },
            new { code = "SAR", name = "Samar" },
            new { code = "SAG", name = "Sarangani" },
            new { code = "SIG", name = "Siquijor" },
            new { code = "SOR", name = "Sorsogon" },
            new { code = "SCO", name = "South Cotabato" },
            new { code = "SLE", name = "Southern Leyte" },
            new { code = "SUK", name = "Sultan Kudarat" },
            new { code = "SLU", name = "Sulu" },
            new { code = "SUN", name = "Surigao del Norte" },
            new { code = "SUR", name = "Surigao del Sur" },
            new { code = "TAR", name = "Tarlac" },
            new { code = "TAW", name = "Tawi-Tawi" },
            new { code = "ZMB", name = "Zambales" },
            new { code = "ZAN", name = "Zamboanga del Norte" },
            new { code = "ZAS", name = "Zamboanga del Sur" },
            new { code = "ZSI", name = "Zamboanga Sibugay" }
        };

        return View(new CreateCompanyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(CreateCompanyViewModel model)
    {
        if (!await IsSuperAdmin())
            return Json(new { success = false, message = "Unauthorized access." });

        // Manual validation since ModelState may fail on optional fields
        if (string.IsNullOrWhiteSpace(model.CompanyName))
            return Json(new { success = false, message = "Company Name is required." });

        if (string.IsNullOrWhiteSpace(model.AdminEmail))
            return Json(new { success = false, message = "Admin Email is required." });

        try
        {
            // Auto-generate company code if not provided
            var companyCode = string.IsNullOrEmpty(model.CompanyCode)
                ? await GenerateCompanyCode(model.CompanyName)
                : model.CompanyCode.ToUpper();

            // Check if company code exists
            if (await _context.Companies.AnyAsync(c => c.CompanyCode == companyCode))
            {
                companyCode = await GenerateCompanyCode(model.CompanyName);
            }

            var company = new Company
            {
                CompanyCode = companyCode,
                CompanyName = model.CompanyName,
                Address = model.Address,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                TIN = model.TIN,
                SSSEmployerNumber = model.SSSEmployerNumber,
                PhilHealthEmployerNumber = model.PhilHealthEmployerNumber,
                PagIbigEmployerNumber = model.PagIbigEmployerNumber,
                SubscriptionStart = model.SubscriptionStart == default ? DateTime.UtcNow : model.SubscriptionStart,
                SubscriptionEnd = model.SubscriptionEnd == default ? DateTime.UtcNow.AddYears(1) : model.SubscriptionEnd,
                IsSubscriptionActive = true,
                IsActive = true
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Enable selected modules
            if (model.EnabledModules != null && model.EnabledModules.Any())
            {
                foreach (var moduleType in model.EnabledModules)
                {
                    _context.CompanyModules.Add(new CompanyModule
                    {
                        CompanyId = company.Id,
                        ModuleType = moduleType,
                        IsEnabled = true
                    });
                }
                await _context.SaveChangesAsync();
            }

            string tempPassword = string.Empty;
            string emailStatus = string.Empty;

            // Create company admin (Manager) with auto-generated temporary password
            var existingUser = await _userManager.FindByEmailAsync(model.AdminEmail);
            if (existingUser != null)
            {
                return Json(new
                {
                    success = true,
                    warning = true,
                    message = $"Company '{company.CompanyName}' created successfully, but the admin email '{model.AdminEmail}' already exists in the system.",
                    redirectUrl = Url.Action(nameof(Companies))
                });
            }

            // Generate random temporary password for manager
            tempPassword = _passwordGenerator.GenerateRandomPassword(12);

            // Generate QR code for the manager
            var qrCodeHash = KioskController.GenerateQRCodeHash(Guid.NewGuid().ToString());

            var adminUser = new ApplicationUser
            {
                UserName = model.AdminEmail,
                Email = model.AdminEmail,
                FirstName = model.AdminFirstName ?? "Admin",
                LastName = model.AdminLastName ?? company.CompanyName,
                CompanyId = company.Id,
                Role = UserRole.CompanyAdmin,
                IsActive = true,
                EmailConfirmed = true,
                MustChangePassword = true,
                RequiresFaceEnrollment = true,
                QRCodeHash = qrCodeHash,
                QRCodeGeneratedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, tempPassword);
            if (result.Succeeded)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _auditLogService.LogAsync(
                    currentUser?.Id ?? "system",
                    "Created Company Manager Account",
                    "ApplicationUser",
                    adminUser.Id,
                    null,
                    new { Email = model.AdminEmail, CompanyId = company.Id },
                    null);

                // Send welcome email
                var emailSent = await _emailService.SendWelcomeEmailAsync(
                    model.AdminEmail,
                    adminUser.FullName,
                    company.CompanyName,
                    tempPassword);

                emailStatus = emailSent
                    ? "Welcome email sent successfully!"
                    : "Email could not be sent — please share credentials manually.";
            }
            else
            {
                emailStatus = "Admin user creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return Json(new
            {
                success = true,
                message = $"Company '{company.CompanyName}' created successfully! Admin: {model.AdminEmail} | Temp Password: {tempPassword} | {emailStatus}",
                redirectUrl = Url.Action(nameof(Companies))
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    private async Task<string> GenerateCompanyCode(string companyName)
    {
        // Generate code from company name (first 3 letters + random numbers)
        var prefix = new string(companyName.ToUpper()
            .Where(c => char.IsLetter(c))
            .Take(3)
            .ToArray());
        
        if (prefix.Length < 3)
            prefix = prefix.PadRight(3, 'X');

        var random = new Random();
        string code;
        int attempts = 0;
        
        do
        {
            code = $"{prefix}{random.Next(1000, 9999)}";
            attempts++;
        } while (await _context.Companies.AnyAsync(c => c.CompanyCode == code) && attempts < 10);

        return code;
    }

    public async Task<IActionResult> EditCompany(int id)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var company = await _context.Companies
            .Include(c => c.CompanyModules)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound();

        var model = new EditCompanyViewModel
        {
            Id = company.Id,
            CompanyCode = company.CompanyCode,
            CompanyName = company.CompanyName,
            Address = company.Address,
            ContactNumber = company.ContactNumber,
            Email = company.Email,
            TIN = company.TIN,
            SSSEmployerNumber = company.SSSEmployerNumber,
            PhilHealthEmployerNumber = company.PhilHealthEmployerNumber,
            PagIbigEmployerNumber = company.PagIbigEmployerNumber,
            SubscriptionStart = company.SubscriptionStart,
            SubscriptionEnd = company.SubscriptionEnd,
            IsActive = company.IsActive,
            IsSubscriptionActive = company.IsSubscriptionActive,
            EnabledModules = company.CompanyModules.Where(m => m.IsEnabled).Select(m => m.ModuleType).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCompany(EditCompanyViewModel model)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(model);

        var company = await _context.Companies
            .Include(c => c.CompanyModules)
            .FirstOrDefaultAsync(c => c.Id == model.Id);

        if (company == null)
            return NotFound();

        company.CompanyName = model.CompanyName;
        company.Address = model.Address;
        company.ContactNumber = model.ContactNumber;
        company.Email = model.Email;
        company.TIN = model.TIN;
        company.SSSEmployerNumber = model.SSSEmployerNumber;
        company.PhilHealthEmployerNumber = model.PhilHealthEmployerNumber;
        company.PagIbigEmployerNumber = model.PagIbigEmployerNumber;
        company.SubscriptionStart = model.SubscriptionStart;
        company.SubscriptionEnd = model.SubscriptionEnd;
        company.IsActive = model.IsActive;
        company.IsSubscriptionActive = model.IsSubscriptionActive;

        // Update modules
        _context.CompanyModules.RemoveRange(company.CompanyModules);

        if (model.EnabledModules != null && model.EnabledModules.Any())
        {
            foreach (var moduleType in model.EnabledModules)
            {
                _context.CompanyModules.Add(new CompanyModule
                {
                    CompanyId = company.Id,
                    ModuleType = moduleType,
                    IsEnabled = true
                });
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Company '{company.CompanyName}' updated successfully.";
        return RedirectToAction(nameof(Companies));
    }

    public async Task<IActionResult> CompanyDetails(int id)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var company = await _context.Companies
            .Include(c => c.CompanyModules)
            .Include(c => c.Employees.Where(e => e.IsActive))
            .Include(c => c.Departments.Where(d => d.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound();

        var admins = await _context.Users
            .Where(u => u.CompanyId == id && u.Role == UserRole.CompanyAdmin)
            .ToListAsync();

        ViewBag.CompanyAdmins = admins;

        return View(company);
    }

    // Module Management
    public async Task<IActionResult> Modules()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var modules = Enum.GetValues(typeof(ModuleType))
            .Cast<ModuleType>()
            .Select(mt => new ModuleInfo
            {
                ModuleType = mt,
                Name = mt.ToString(),
                Description = GetModuleDescription(mt),
                CompaniesUsingCount = _context.CompanyModules.Count(cm => cm.ModuleType == mt && cm.IsEnabled)
            })
            .ToList();

        return View(modules);
    }

    private string GetModuleDescription(ModuleType moduleType)
    {
        return moduleType switch
        {
            ModuleType.Attendance => "Track employee attendance and time records",
            ModuleType.Payroll => "Process payroll and manage compensation",
            ModuleType.Leave => "Manage leave requests and balances",
            ModuleType.Overtime => "Track and approve overtime hours",
            ModuleType.GovernmentCompliance => "Manage SSS, PhilHealth, and Pag-IBIG compliance",
            ModuleType.Reports => "Generate and view system reports",
            _ => "Module description not available"
        };
    }

    // Reports & Analytics
    public async Task<IActionResult> RevenueReports()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var now = DateTime.UtcNow;
        var startOfYear = new DateTime(now.Year, 1, 1);

        // Monthly revenue (based on active subscriptions per month)
        var companies = await _context.Companies
            .Where(c => c.SubscriptionStart.HasValue)
            .ToListAsync();

        // Build monthly data for current year
        var monthlyData = Enumerable.Range(1, 12).Select(m =>
        {
            var activeInMonth = companies.Count(c =>
                c.SubscriptionStart!.Value <= new DateTime(now.Year, m, DateTime.DaysInMonth(now.Year, m)) &&
                (!c.SubscriptionEnd.HasValue || c.SubscriptionEnd.Value >= new DateTime(now.Year, m, 1)));
            return new { Month = new DateTime(now.Year, m, 1).ToString("MMM"), Count = activeInMonth, Revenue = activeInMonth * 2500m };
        }).ToList();

        var totalRevenue = companies.Count(c => c.IsSubscriptionActive) * 2500m;
        var totalCompanies = await _context.Companies.CountAsync();
        var activeCompanies = await _context.Companies.CountAsync(c => c.IsActive && c.IsSubscriptionActive);
        var expiringSoon = await _context.Companies.CountAsync(c =>
            c.IsSubscriptionActive && c.SubscriptionEnd.HasValue &&
            c.SubscriptionEnd.Value <= now.AddDays(30));

        ViewBag.MonthlyData = monthlyData;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalCompanies = totalCompanies;
        ViewBag.ActiveCompanies = activeCompanies;
        ViewBag.ExpiringSoon = expiringSoon;
        ViewData["Title"] = "Revenue Reports";
        return View("RevenueReports");
    }

    public async Task<IActionResult> ClientAnalytics()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var companies = await _context.Companies
            .Include(c => c.Employees)
            .Include(c => c.CompanyModules)
            .OrderByDescending(c => c.CreatedAt)
            .Take(20)
            .Select(c => new
            {
                c.CompanyName,
                c.IsActive,
                c.IsSubscriptionActive,
                c.SubscriptionStart,
                c.SubscriptionEnd,
                EmployeeCount = c.Employees.Count(e => e.IsActive),
                ModuleCount = c.CompanyModules.Count(m => m.IsEnabled)
            })
            .ToListAsync();

        ViewBag.Companies = companies;
        ViewBag.TotalCompanies = await _context.Companies.CountAsync();
        ViewBag.ActiveCompanies = await _context.Companies.CountAsync(c => c.IsActive && c.IsSubscriptionActive);
        ViewBag.TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive);
        ViewData["Title"] = "Client Analytics";
        return View("ClientAnalytics");
    }

    public async Task<IActionResult> ModuleUsage()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var moduleStats = await _context.CompanyModules
            .Where(m => m.IsEnabled)
            .GroupBy(m => m.ModuleType)
            .Select(g => new { ModuleType = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewBag.ModuleStats = moduleStats;
        ViewBag.TotalCompanies = await _context.Companies.CountAsync();
        ViewData["Title"] = "Module Usage";
        return View("ModuleUsage");
    }

    public async Task<IActionResult> BillingHistory()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var companies = await _context.Companies
            .OrderByDescending(c => c.SubscriptionStart)
            .Select(c => new
            {
                c.Id,
                c.CompanyName,
                c.CompanyCode,
                c.SubscriptionStart,
                c.SubscriptionEnd,
                c.IsSubscriptionActive,
                c.IsActive
            })
            .ToListAsync();

        ViewBag.Companies = companies;
        ViewData["Title"] = "Billing History";
        return View("BillingHistory");
    }

    public async Task<IActionResult> ActivityLogs(int? days = 30, string? action = null, string? search = null)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        // Get vendor logs instead of audit logs
        var logs = await _vendorAuditService.GetVendorLogsAsync(days ?? 30, action, search);

        ViewBag.Days = days;
        ViewBag.Action = action;
        ViewBag.Search = search;
        ViewData["Title"] = "System Logs - Vendor Activity";
        return View("SystemLogs", logs);
    }

    // System Settings — redirect to Companies for now
    public async Task<IActionResult> Settings()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        ViewData["Title"] = "System Settings";
        return View();
    }

    // Security
    public async Task<IActionResult> SystemLogs(int? days = 7, string? action = null, string? search = null)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var startDate = DateTime.UtcNow.AddDays(-(days ?? 7));
        
        var query = _context.VendorLogs
            .Include(v => v.Vendor)
            .Where(v => v.CreatedAt >= startDate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(v => v.Action.Contains(action));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(v => v.Action.Contains(search) || 
                                    (v.Description != null && v.Description.Contains(search)));
        }

        var logs = await query
            .OrderByDescending(v => v.CreatedAt)
            .Take(500)
            .ToListAsync();

        ViewBag.Days = days;
        ViewBag.Action = action;
        ViewBag.Search = search;
        ViewData["Title"] = "System Logs";
        return View("SystemLogs", logs);
    }

    [HttpGet]
    public async Task<IActionResult> GetLogDetails(int id)
    {
        if (!await IsSuperAdmin())
            return Unauthorized();

        var log = await _context.VendorLogs
            .Include(v => v.Vendor)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (log == null)
            return NotFound();

        return Json(new
        {
            action = log.Action,
            createdAt = log.CreatedAt,
            details = log.Description,
            user = log.Vendor?.FullName ?? log.VendorName,
            company = log.EntityType == "Company" ? log.Description : "System"
        });
    }

    public async Task<IActionResult> AuditTrail()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        // Redirect to SystemLogs which has the real implementation
        return RedirectToAction(nameof(SystemLogs));
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        if (!await IsSuperAdmin())
            return Unauthorized();

        var notifications = new List<NotificationViewModel>();
        
        // Get new companies in last 7 days
        var newCompanies = await _context.Companies
            .Where(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var company in newCompanies)
        {
            notifications.Add(new NotificationViewModel
            {
                Type = "NewCompany",
                Title = "New Company Subscription",
                Message = $"{company.CompanyName} has subscribed to PayroTech",
                Icon = "bi-building-add",
                Link = "/SuperAdmin/Companies",
                CreatedAt = company.CreatedAt,
                IsRead = false
            });
        }

        // Get failed login attempts (more than 5 attempts)
        var failedLogins = await _context.AuditLogs
            .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-1) && 
                       (a.Action.Contains("Failed Login") || a.Action.Contains("Login Failed")))
            .GroupBy(a => a.UserId)
            .Where(g => g.Count() >= 5)
            .Select(g => new { UserId = g.Key, Count = g.Count(), LastAttempt = g.Max(a => a.CreatedAt) })
            .ToListAsync();

        foreach (var attempt in failedLogins)
        {
            var user = await _context.Users.FindAsync(attempt.UserId);
            notifications.Add(new NotificationViewModel
            {
                Type = "FailedLogin",
                Title = "Security Alert: Multiple Failed Logins",
                Message = $"{attempt.Count} failed login attempts detected for {user?.Email ?? "unknown user"}",
                Icon = "bi-shield-exclamation",
                Link = "/SuperAdmin/SystemLogs?filter=failed-login",
                CreatedAt = attempt.LastAttempt,
                IsRead = false
            });
        }

        var result = new NotificationSummary
        {
            UnreadCount = notifications.Count,
            RecentNotifications = notifications.OrderByDescending(n => n.CreatedAt).Take(10).ToList()
        };

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        if (!await IsSuperAdmin())
            return Unauthorized();

        // In a real implementation, you'd mark the notification as read in the database
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentTransactions()
    {
        if (!await IsSuperAdmin())
            return Unauthorized();

        // Get vendor logs as transactions
        var transactions = await _context.VendorLogs
            .Include(v => v.Vendor)
            .OrderByDescending(v => v.CreatedAt)
            .Take(10)
            .Select(v => new
            {
                id = v.Id,
                createdAt = v.CreatedAt,
                companyName = v.EntityType == "Company" ? (v.Description ?? "Company") : "System",
                transactionType = v.Action,
                amount = 0m,
                status = v.IsSuccess ? "Completed" : "Failed"
            })
            .ToListAsync();

        return Json(transactions);
    }

    public async Task<IActionResult> TransactionLogs(int page = 1, int days = 30, string? type = null)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var startDate = DateTime.UtcNow.AddDays(-days);
        var pageSize = 20;

        var query = _context.VendorLogs
            .Include(v => v.Vendor)
            .Where(v => v.CreatedAt >= startDate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(v => v.Action.Contains(type));
        }

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.Days = days;
        ViewBag.Type = type;
        ViewData["Title"] = "Transaction Logs";
        return View(logs);
    }

    public async Task<IActionResult> TransactionReceipt(int id)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var transaction = await _context.VendorLogs
            .Include(v => v.Vendor)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (transaction == null)
            return NotFound();

        ViewData["Title"] = "Transaction Receipt";
        return View(transaction);
    }

    public async Task<IActionResult> Reports()
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        ViewData["Title"] = "Reports & Analytics";
        return View();
    }

    // PayMongo Payment Link
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePaymentLink([FromBody] PaymentLinkRequest request)
    {
        if (!await IsSuperAdmin())
            return Unauthorized();

        try
        {
            var description = $"PayroTech {request.PlanName} Plan - {request.CompanyName}";
            var remarks = $"{request.BillingCycle} billing, {request.KioskCount} kiosk(s)";

            var result = await _paymentService.CreatePaymentLinkAsync(description, request.Amount, remarks);

            if (result != null && !string.IsNullOrEmpty(result.CheckoutUrl))
            {
                return Json(new
                {
                    success = true,
                    checkoutUrl = result.CheckoutUrl,
                    referenceNumber = result.ReferenceNumber,
                    paymentId = result.Id
                });
            }

            return Json(new { success = false, message = "Could not create payment link. Please use Cash payment." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Payment service error: " + ex.Message });
        }
    }

    // Test email endpoint
    [HttpGet]
    public async Task<IActionResult> TestEmail(string? email = null)
    {
        if (!await IsSuperAdmin())
            return RedirectToAction("Index", "Home");

        var testEmail = email ?? "test@example.com";
        
        try
        {
            var result = await _emailService.SendEmailAsync(
                testEmail,
                "Test User",
                "PayroTech Email Test",
                @"<h1>Email Test Successful!</h1>
                  <p>If you're reading this, your SMTP configuration is working correctly.</p>
                  <p><strong>Sent from:</strong> PayroTech System</p>
                  <p><strong>Time:</strong> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>");

            if (result)
            {
                TempData["Success"] = $"✅ Test email sent successfully to {testEmail}! Check the inbox (and spam folder).";
            }
            else
            {
                TempData["Error"] = $"❌ Failed to send test email to {testEmail}. Check application logs for details.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"❌ Error sending test email: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}

public class PaymentLinkRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int KioskCount { get; set; }
    public string BillingCycle { get; set; } = "monthly";
}
