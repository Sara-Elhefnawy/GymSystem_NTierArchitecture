using GymSystem.Domain.Services;
using GymSystem.Infrastructure.Data;
using GymSystem.UI.ViewModels;
using GymSystem.UI.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GymSystem.UI.Controllers;

public class HomeController(GymAppDbContext context, IDashboardService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await dashboard.GetHomeStatisticsAsync(ct);

        if (result.IsFailure)
        {
            TempData["Error"] = "Unable to load statistics. Please try again.";
            return View(new List<DashboardHomeViewModel>());
        }

        var viewModels = new DashboardHomeViewModel
        {
            ActiveMembers = result.Value.ActiveMembers,
            CompletedSessions = result.Value.CompletedSessions,
            OngoingSessions = result.Value.OngoingSessions,
            UpcomingSessions = result.Value.UpcomingSessions,
            TotalMembers = result.Value.TotalMembers,
            TotalTrainers = result.Value.TotalTrainers
        };

        return View(viewModels);
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
