using GymSystem.Domain.QRCode;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

[Route("CheckIn")]
public class CheckInController : Controller
{
    private readonly IBookingService _bookings;
    private readonly IQrService _qrService;

    public CheckInController(IBookingService bookingService, IQrService qrService)
    {
        _bookings = bookingService;
        _qrService = qrService;
    }

    [HttpGet("Scan")]
    public IActionResult Scan()
    {
        return View();
    }

    [HttpGet("GenerateSignature")]
    public IActionResult GenerateSignature(int memberId)
    {
        try
        {
            var signedUrl = _qrService.BuildSignedUrl(memberId);
            var signature = signedUrl.Split("sig=")[1];
            return Json(new { success = true, signature });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Failed to generate signature" });
        }
    }

    [HttpPost("Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm([FromForm] int memberId, [FromForm] string sig, CancellationToken ct = default)
    {
        try
        {
            if (!_qrService.ValidateSignature(memberId, sig))
            {
                return Json(new { success = false, message = "Invalid QR code signature" });
            }

            var result = await _bookings.CheckInViaQRAsync(memberId, ct);

            if (result.IsFailure)
            {
                this.HandleErrorResult(result);  // Use the error handler
                return Json(new { success = false, message = result.Error });
            }

            var checkInResult = result.Value;

            string message;
            if (checkInResult.IsAlreadyAttended)
            {
                message = $"Member {checkInResult.MemberName} already attended {checkInResult.SessionName} today";
            }
            else if (checkInResult.WasAutoBooked)
            {
                message = $"Check-in successful! {checkInResult.MemberName} was auto-booked for {checkInResult.SessionName}";
            }
            else
            {
                message = $"Check-in successful for {checkInResult.MemberName} in {checkInResult.SessionName}!";
            }

            return Json(new
            {
                success = true,
                message = message,
                memberName = checkInResult.MemberName,
                sessionName = checkInResult.SessionName,
                alreadyAttended = checkInResult.IsAlreadyAttended,
                wasAutoBooked = checkInResult.WasAutoBooked
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred during check-in" });
        }
    }
}
