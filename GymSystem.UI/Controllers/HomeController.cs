using GymSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class HomeController : Controller
{
    private readonly GymAppDbContext _context;
    public HomeController(GymAppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
