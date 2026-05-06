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
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Set a longer timeout for migration/seeding on startup
        context.Database.SetCommandTimeout(180);

        // Apply pending migrations
        await context.Database.MigrateAsync();

        // Seed initial data
        await DatabaseSeeder.SeedAsync(context, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        // Don't crash the app — let it start and show an error page instead
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

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
