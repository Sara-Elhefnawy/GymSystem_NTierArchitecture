using GymSystem.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class CheckInController : Controller
{
    private readonly CheckInService _checkInService;

    public CheckInController(CheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    // Show check-in page
    public IActionResult Index()
    {
        return View();
    }

    // Process check-in
    [HttpPost]
    public async Task<IActionResult> CheckIn(int memberId)
    {
        var (success, message) = await _checkInService.RecordCheckInAsync(memberId);

        if (success)
        {
            TempData["Success"] = message;
        }
        else
        {
            TempData["Error"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // View today's check-ins
    public async Task<IActionResult> TodayCheckIns()
    {
        var checkIns = await _checkInService.GetTodayCheckInsAsync();
        var count = await _checkInService.GetTodayCheckInCountAsync();

        ViewBag.TodayCount = count;
        return View(checkIns);
    }
}
