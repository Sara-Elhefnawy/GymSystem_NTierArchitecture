using GymSystem.Infrastructure.Data;
using GymSystem.UI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
