using GymSystem.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GymSystem.UI.Helpers;

public static class ErrorHandler
{
    public static void HandleError<T>(this Controller controller, Result<T> result, ModelStateDictionary modelState)
    {
        HandleError(result, modelState);
    }

    public static void HandleError(Result result, ModelStateDictionary modelState)
    {
        if (result.IsSuccess)
            return;

        // Handle specific error cases
        switch (result.ErrorKey)
        {
            case "SESSION_NOT_FOUND":
                modelState.AddModelError("SessionId", "The selected session no longer exists");
                SetTempData(controller: null, "Warning", "Session not found. Please try again.");
                break;

            case "MEMBER_NOT_FOUND":
                modelState.AddModelError("MemberId", "The selected member no longer exists");
                SetTempData(controller: null, "Warning", "Member not found. Please try again.");
                break;

            case "NO_ACTIVE_MEMBERSHIP":
                modelState.AddModelError("MemberId", "Member does not have an active membership");
                SetTempData(controller: null, "Warning", "This member does not have an active membership. Please purchase a membership first.");
                break;

            case "ALREADY_BOOKED":
                modelState.AddModelError("MemberId", "Member is already booked for this session");
                SetTempData(controller: null, "Warning", "This member is already booked for this session.");
                break;

            case "SESSION_FULL":
                modelState.AddModelError(string.Empty, "Session is at full capacity");
                SetTempData(controller: null, "Error", "This session is fully booked. Please try another session.");
                break;

            case "BOOKING_NOT_FOUND":
                modelState.AddModelError(string.Empty, "Booking not found");
                SetTempData(controller: null, "Warning", "Booking not found or already cancelled.");
                break;

            case "ALREADY_ACTIVE":
                modelState.AddModelError("MemberId", "Member already has an active membership");
                SetTempData(controller: null, "Warning", "This member already has an active membership.");
                break;

            case "PLAN_NOT_FOUND":
                modelState.AddModelError("PlanId", "The selected plan no longer exists");
                SetTempData(controller: null, "Warning", "Plan not found. Please try again.");
                break;

            case "MEMBERSHIP_NOT_FOUND":
                modelState.AddModelError(string.Empty, "No active membership found");
                SetTempData(controller: null, "Warning", "No active membership found to cancel.");
                break;

            case "DATABASE_ERROR":
                modelState.AddModelError(string.Empty, result.Error);
                SetTempData(controller: null, "Error", "A database error occurred. Please try again later.");
                break;

            default:
                modelState.AddModelError(string.Empty, result.Error);
                SetTempData(controller: null, "Error", result.Error);
                break;
        }
    }

    private static void SetTempData(Controller? controller, string type, string message)
    {
        if (controller != null)
        {
            controller.TempData[type] = message;
        }
    }

    // Extension method for controllers
    public static void HandleErrorResult(this Controller controller, Result result)
    {
        if (result.IsSuccess)
            return;

        switch (result.ErrorKey)
        {
            case "SESSION_NOT_FOUND":
                controller.TempData["Warning"] = "Session not found. Please try again.";
                break;

            case "MEMBER_NOT_FOUND":
                controller.TempData["Warning"] = "Member not found. Please try again.";
                break;

            case "NO_ACTIVE_MEMBERSHIP":
                controller.TempData["Warning"] = "This member does not have an active membership. Please purchase a membership first.";
                break;

            case "ALREADY_BOOKED":
                controller.TempData["Warning"] = "This member is already booked for this session.";
                break;

            case "SESSION_FULL":
                controller.TempData["Error"] = "This session is fully booked. Please try another session.";
                break;

            case "BOOKING_NOT_FOUND":
                controller.TempData["Warning"] = "Booking not found or already cancelled.";
                break;

            case "ALREADY_ACTIVE":
                controller.TempData["Warning"] = "This member already has an active membership.";
                break;

            case "PLAN_NOT_FOUND":
                controller.TempData["Warning"] = "Plan not found. Please try again.";
                break;

            case "MEMBERSHIP_NOT_FOUND":
                controller.TempData["Warning"] = "No active membership found to cancel.";
                break;

            case "DATABASE_ERROR":
                controller.TempData["Error"] = "A database error occurred. Please try again later.";
                break;

            default:
                controller.TempData["Error"] = result.Error;
                break;
        }
    }
}
