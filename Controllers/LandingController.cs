using Microsoft.AspNetCore.Mvc;

namespace PayroTech.Controllers;

public class LandingController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
