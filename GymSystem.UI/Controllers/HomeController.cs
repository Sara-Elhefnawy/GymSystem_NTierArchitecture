using GymSystem.Domain.Services.Interfaces;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels;
using GymSystem.UI.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GymSystem.UI.Controllers;

[AllowAnonymous]
public class HomeController(IDashboardService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await dashboard.GetHomeStatisticsAsync(ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);  // Use the error handler
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
