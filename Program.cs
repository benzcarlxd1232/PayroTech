using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;
using PayroTech.Services;

var builder = WebApplication.CreateBuilder(args);

// Load local secrets (gitignored — never committed)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString(
    builder.Environment.IsProduction() ? "ProductionConnection" : "DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(120);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        })
       .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;
    options.User.RequireUniqueEmail = false; // Allow reuse for testing
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

// Add HttpClient for external APIs (PayMongo)
builder.Services.AddHttpClient("PayMongo");

// Add Custom Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
builder.Services.AddScoped<IFirstLoginSetupService, FirstLoginSetupService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<ILoginSecurityService, LoginSecurityService>();

// Vendor-specific services
builder.Services.AddScoped<IVendorAuditService, VendorAuditService>();
builder.Services.AddScoped<ISubscriptionMonitorService, SubscriptionMonitorService>();

// PDF Report service
builder.Services.AddScoped<IPdfReportService, PdfReportService>();

// QuestPDF community license (free for non-commercial / school projects)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Background services
builder.Services.AddSingleton<BackgroundEmailService>();
builder.Services.AddSingleton<IBackgroundEmailService>(sp => sp.GetRequiredService<BackgroundEmailService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<BackgroundEmailService>());
builder.Services.AddHostedService<PayroTech.Services.BackgroundServices.SubscriptionCheckService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Higher timeout for migrations on constrained hosting (free plans)
        context.Database.SetCommandTimeout(300);

        // Apply pending migrations
        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception migEx)
        {
            logger.LogError(migEx, "Migration failed: {Message}", migEx.Message);
            Console.WriteLine($"MIGRATION ERROR: {migEx.Message}");
            // Continue — app can still run with existing schema
        }

        // Seed if database is empty OR if only SuperAdmin exists (partial seed recovery)
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var needsSeed = !context.Users.Any();
            var needsReseed = !needsSeed && !context.Companies.Any() && context.Users.Any();

            if (needsSeed)
            {
                Console.WriteLine("SEED: First run — seeding all data...");
                await DatabaseSeeder.SeedAsync(context, userManager);
                logger.LogInformation("Database seeding completed.");
            }
            else if (needsReseed)
            {
                Console.WriteLine("SEED: Partial seed detected (users exist but no companies) — re-seeding...");
                await DatabaseSeeder.SeedAsync(context, userManager);
                logger.LogInformation("Database re-seeding completed.");
            }
        }
        catch (Exception seedEx)
        {
            logger.LogError(seedEx, "Seeding failed: {Message}", seedEx.Message);
            Console.WriteLine($"SEED ERROR: {seedEx.Message}");
            // Continue — seeding failure should not crash the app
        }
    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "Critical startup error: {Message}", ex.Message);
        Console.WriteLine($"CRITICAL STARTUP ERROR: {ex.Message}");
    }
}

// TEMPORARY: Show detailed errors to diagnose production crash
// TODO: Revert this after fixing the issue
app.UseDeveloperExceptionPage();
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Home/Error");
    // Note: HSTS disabled — hosting plan does not support SSL
    // app.UseHsts();
}

// Note: HTTPS redirect disabled — hosting plan does not support SSL
// app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Add no-cache headers for authenticated pages to prevent back button access after logout
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        // Skip static files
        if (!path.StartsWith("/lib/") && 
            !path.StartsWith("/css/") && 
            !path.StartsWith("/js/") && 
            !path.StartsWith("/images/") &&
            !path.EndsWith(".css") &&
            !path.EndsWith(".js") &&
            !path.EndsWith(".png") &&
            !path.EndsWith(".jpg") &&
            !path.EndsWith(".gif") &&
            !path.EndsWith(".ico") &&
            !path.EndsWith(".woff") &&
            !path.EndsWith(".woff2"))
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
        }
    }
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
