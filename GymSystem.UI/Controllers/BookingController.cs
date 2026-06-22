using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Booking;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Booking;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymSystem.UI.Controllers;

[Route("Booking")]
public class BookingController(
    IBookingService bookingService,
    IMemberService memberService,
    ISessionService sessionService) : Controller
{
    private readonly IBookingService _bookings = bookingService;
    private readonly IMemberService _members = memberService;
    private readonly ISessionService _sessions = sessionService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var result = await _bookings.GetAvailableSessionsAsync(ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View(new List<IndexBookingViewModel>());
        }

        var viewModel = result.Value.Adapt<IReadOnlyList<IndexBookingViewModel>>();

        return View(viewModel);
    }

    [HttpGet("GetMembersForUpcomingSession/{id}")]
    public async Task<IActionResult> GetMembersForUpcomingSession(int id, CancellationToken ct = default)
    {
        var result = await _bookings.GetMembersForSessionAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View("BookingDetails", new List<SessionInBookingViewModel>());
        }

        var viewModel = result.Value.Adapt<IReadOnlyList<SessionInBookingViewModel>>();

        ViewData["SessionId"] = id;
        return View("BookingDetails", viewModel);
    }

    [HttpGet("GetMembersForOngoingSessions/{id}")]
    public async Task<IActionResult> GetMembersForOngoingSessions(int id, CancellationToken ct = default)
    {
        var result = await _bookings.GetMembersForSessionAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View("SessionAttendance", new List<SessionInBookingViewModel>());
        }

        var viewModel = result.Value.Adapt<IReadOnlyList<SessionInBookingViewModel>>();

        ViewData["SessionId"] = id;
        return View("SessionAttendance", viewModel);
    }

    [HttpGet("Create/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create(int id, CancellationToken ct = default)
    {
        var membersResult = await _members.GetMembersWithActiveMembershipAsync(ct);

        var sessionResult = await _sessions.GetDetailsAsync(id, ct);

        ViewBag.Members = membersResult.IsSuccess
            ? new SelectList(membersResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());

        ViewBag.SessionId = id;
        ViewBag.SessionInfo = sessionResult.IsSuccess ? sessionResult.Value : null;

        var model = new CreateBookingViewModel { SessionId = id };

        return View(model);
    }

    [HttpPost("Create/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create(int id, [FromForm] CreateBookingViewModel model, CancellationToken ct)
    {
        model.SessionId = id;

        var dto = model.Adapt<CreateBookingDTO>();

        var result = await _bookings.CreateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Booking created successfully.";
            return RedirectToAction(nameof(GetMembersForUpcomingSession), new { id = model.SessionId });
        }

        ErrorHandler.HandleError(result, ModelState);

        if (result.ErrorKey == "NO_ACTIVE_MEMBERSHIP")
        {
            TempData["Info"] = "Would you like to purchase a membership?";
        }

        // Repopulate dropdowns
        var membersResult = await _members.GetAllAsync(ct);
        ViewBag.Members = membersResult.IsSuccess
            ? new SelectList(membersResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());

        return View(model);
    }

    [HttpPost("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel([FromForm] int memberId, [FromForm] int sessionId, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(memberId, sessionId, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Booking cancelled successfully.";
        }
        else
        {
            this.HandleErrorResult(result);
        }

        return RedirectToAction(nameof(GetMembersForUpcomingSession), new { id = sessionId });
    }

    [HttpPost("Attended")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attended([FromForm] int memberId, [FromForm] int sessionId, CancellationToken ct)
    {
        var result = await _bookings.MarkAttendanceAsync(memberId, sessionId, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Attendance recorded successfully.";
        }
        else
        {
            this.HandleErrorResult(result);
        }

        return RedirectToAction(nameof(GetMembersForOngoingSessions), new { id = sessionId });
    }

    [HttpGet("Photo/{id:int}")]
    public async Task<IActionResult> GetPhoto(int id, CancellationToken ct = default)
    {
        try
        {
            var memberResult = await _members.GetDetailsAsync(id, ct);
            if (memberResult.IsFailure || string.IsNullOrEmpty(memberResult.Value.Photo))
            {
                return File("~/images/User.png", "image/png");
            }

            var photoResult = await _members.GetMemberPhotoAsync(id, ct);
            if (photoResult.IsFailure)
            {
                return File("~/images/User.png", "image/png");
            }

            var contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" }
            };

            var extension = Path.GetExtension(memberResult.Value.Photo).ToLowerInvariant();
            var contentType = contentTypes.TryGetValue(extension, out var value) ? value : "image/jpeg";

            return File(photoResult.Value, contentType);
        }
        catch (Exception)
        {
            return File("~/images/User.png", "image/png");
        }
    }
}
