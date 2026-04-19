using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Entities;

namespace PayroTech.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Notification/Count — returns unread count for bell badge
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { count = 0 });

        var count = await _context.Notifications
            .CountAsync(n => n.UserId == user.Id && !n.IsRead);

        return Json(new { count });
    }

    // GET: /Notification/List — returns recent notifications as JSON
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new List<object>());

        var notifications = await _context.Notifications
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.IsRead,
                n.ActionUrl,
                n.Type,
                CreatedAt = n.CreatedAt.ToString("MMM dd, hh:mm tt")
            })
            .ToListAsync();

        return Json(notifications);
    }

    // POST: /Notification/MarkRead/{id}
    [HttpPost]
    public async Task<IActionResult> MarkRead(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var n = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

        if (n != null)
        {
            n.IsRead = true;
            n.ReadAt  = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, actionUrl = n?.ActionUrl });
    }

    // POST: /Notification/MarkAllRead
    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var unread = await _context.Notifications
            .Where(n => n.UserId == user.Id && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}
