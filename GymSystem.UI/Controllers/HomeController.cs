using GymSystem.Domain.Abstractions.Services;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels;
using GymSystem.UI.ViewModels.Home;
using Mapster;
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

        var viewModels = result.Value.Adapt<DashboardHomeViewModel>();

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
