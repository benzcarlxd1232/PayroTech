using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PayroTech.Models.Entities;

namespace PayroTech.Controllers;

/// <summary>
/// Allows any authenticated staff to change their kiosk PIN.
/// Accessible from each role's sidebar.
/// </summary>
[Authorize]
public class ChangePinController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePinController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: /ChangePin
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        ViewBag.StaffCode   = user.StaffCode;
        ViewBag.CurrentPin  = string.IsNullOrEmpty(user.KioskPin)
            ? PayroTech.Utilities.StaffCodeGenerator.GetDefaultPin(user.StaffCode)
            : user.KioskPin;

        return View();
    }

    // POST: /ChangePin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string newPin, string confirmPin)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        ViewBag.StaffCode = user.StaffCode;

        if (string.IsNullOrEmpty(newPin) || newPin.Length != 4 || !newPin.All(char.IsDigit))
        {
            ModelState.AddModelError("", "PIN must be exactly 4 digits.");
            return View();
        }

        if (newPin != confirmPin)
        {
            ModelState.AddModelError("", "PINs do not match.");
            return View();
        }

        user.KioskPin = newPin;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Kiosk PIN updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
