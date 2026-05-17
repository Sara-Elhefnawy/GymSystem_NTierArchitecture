using GymSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class HomeController : Controller
{
    private readonly GymAppDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(GymAppDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        // ← ADD simple log
        _logger.LogInformation("Home page was viewed");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Privacy page was viewed");
        return View();
    }

    public IActionResult TriggerError()
    {
        _logger.LogWarning("User triggered a test error");
        throw new InvalidOperationException("Test exception from HomeController!");
    }
}