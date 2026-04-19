using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models;
using PayroTech.Models.Entities;
using PayroTech.Models.Enums;
using PayroTech.Models.ViewModels;

namespace PayroTech.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // If user is not authenticated, redirect to landing page
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Index", "Landing");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Landing");

        // Always enforce setup steps first — regardless of role
        if (user.MustChangePassword)
            return RedirectToAction("ChangePassword", "Auth");

        if (user.RequiresFaceEnrollment)
            return RedirectToAction("EnrollFace", "Auth");

        // If face enrolled but no face image path — also needs enrollment
        // (handles existing accounts that were created before this feature)
        if (!user.IsFaceEnrolled
            && user.Role != UserRole.ErpSuperAdmin
            && user.RequiresFaceEnrollment)
        {
            return RedirectToAction("EnrollFace", "Auth");
        }

        // Route to role dashboard
        return user.Role switch
        {
            UserRole.ErpSuperAdmin                              => RedirectToAction("Index", "SuperAdmin"),
            UserRole.CompanyAdmin or UserRole.Supervisor        => RedirectToAction("Index", "Manager"),
            UserRole.HR                                         => RedirectToAction("Index", "HR"),
            UserRole.Employee                                   => RedirectToAction("Index", "MyPortal"),
            UserRole.Accountant                                 => RedirectToAction("Index", "Accountant"),
            _                                                   => RedirectToAction("Index", "Landing")
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

