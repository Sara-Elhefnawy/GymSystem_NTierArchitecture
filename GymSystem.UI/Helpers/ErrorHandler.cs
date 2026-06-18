using GymSystem.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GymSystem.UI.Helpers;

public static class ErrorHandler
{
    private static readonly Dictionary<string, ErrorMessageDefinition> ErrorMessages = new()
    {
        ["SESSION_NOT_FOUND"] = new("SessionId", "The selected session no longer exists", "Warning", "Session not found. Please try again."),
        ["MEMBER_NOT_FOUND"] = new("MemberId", "The selected member no longer exists", "Warning", "Member not found. Please try again."),
        ["NO_ACTIVE_MEMBERSHIP"] = new("MemberId", "Member does not have an active membership", "Warning", "This member does not have an active membership. Please purchase a membership first."),
        ["ALREADY_BOOKED"] = new("MemberId", "Member is already booked for this session", "Warning", "This member is already booked for this session."),
        ["SESSION_FULL"] = new(string.Empty, "Session is at full capacity", "Error", "This session is fully booked. Please try another session."),
        ["BOOKING_NOT_FOUND"] = new(string.Empty, "Booking not found", "Warning", "Booking not found or already cancelled."),
        ["ALREADY_ACTIVE"] = new("MemberId", "Member already has an active membership", "Warning", "This member already has an active membership."),
        ["PLAN_NOT_FOUND"] = new("PlanId", "The selected plan no longer exists", "Warning", "Plan not found. Please try again."),
        ["MEMBERSHIP_NOT_FOUND"] = new(string.Empty, "No active membership found", "Warning", "No active membership found to cancel."),
        ["EMAIL_TAKEN"] = new("Email", "This email is already registered", "Warning", "This email is already registered to another user. Please use a different email."),
        ["PHONE_TAKEN"] = new("Phone", "This phone number is already registered", "Warning", "This phone number is already registered to another user. Please use a different number."),
        ["INVALID_AGE"] = new("DateOfBirth", "Age must be between 12 and 120 years", "Warning", "Age must be between 12 and 120 years."),
        ["INVALID_NAME"] = new("Name", "Name can only contain letters, spaces, hyphens, and apostrophes", "Warning", "Name contains invalid characters. Use only letters, spaces, hyphens, and apostrophes."),
        ["INVALID_GENDER"] = new("Gender", "Please select a valid gender", "Warning", "Please select a valid gender option."),
        ["INVALID_BLOOD_TYPE"] = new("HealthRecord.BloodType", "Please select a valid blood type", "Warning", "Please select a valid blood type from the list."),
        ["INVALID_DATE_RANGE"] = new(string.Empty, "End date must be after start date", "Warning", "End date must be after start date."),
        ["PAST_START_DATE"] = new("StartDate", "Start date must be in the future", "Warning", "Start date must be in the future."),
        ["INVALID_CAPACITY"] = new("Capacity", "Capacity must be between 1 and 25", "Warning", "Capacity must be between 1 and 25."),
        ["SPECIALTY_MISMATCH"] = new("TrainerId", "Trainer specialty does not match the session category", "Warning", "Trainer specialty does not match the session category."),
        ["TRAINER_CONFLICT"] = new("TrainerId", "Trainer is already assigned to another session at this time", "Warning", "Trainer is already assigned to another session at this time."),
        ["SESSION_NOT_EDITABLE"] = new(string.Empty, "This session has already started and cannot be edited", "Warning", "This session has already started and cannot be edited."),
        ["CATEGORY_NOT_FOUND"] = new("CategoryId", "Selected category does not exist", "Warning", "Selected category does not exist."),
        ["TRAINER_NOT_FOUND"] = new("TrainerId", "Selected trainer does not exist", "Warning", "Selected trainer does not exist."),
        ["UPDATE_ERROR"] = new(string.Empty, "Failed to update. Please try again.", "Error", "Failed to update. Please try again."),
        ["INTERNAL_ERROR"] = new(string.Empty, "A system error occurred. Please try again later.", "Error", "A system error occurred. Please try again later."),
        ["DATABASE_ERROR"] = new(string.Empty, "A database error occurred. Please try again later.", "Error", "A database error occurred. Please try again later.")
    };

    // Handle error with ModelState (for form submissions)
    public static void HandleError<T>(this Controller controller, Result<T> result, ModelStateDictionary modelState)
    {
        HandleError(result, modelState);
    }

    public static void HandleError(Result result, ModelStateDictionary modelState)
    {
        if (result.IsSuccess)
            return;

        if (ErrorMessages.TryGetValue(result.ErrorKey ?? string.Empty, out var definition))
        {
            modelState.AddModelError(definition.ModelField, definition.ModelErrorMessage);
        }
        else
        {
            // Fallback for unknown error keys
            modelState.AddModelError(string.Empty, result.Error ?? "An error occurred");
        }
    }

    // Handle error with TempData only (for non-form operations)
    public static void HandleErrorResult(this Controller controller, Result result)
    {
        if (result.IsSuccess)
            return;

        var (type, message) = GetTempDataMessage(result);
        controller.TempData[type] = message;
    }

    // Handle error with both ModelState and TempData (for form submissions)
    public static void HandleErrorResult(this Controller controller, Result result, ModelStateDictionary modelState)
    {
        if (result.IsSuccess)
            return;

        // Add to ModelState
        HandleError(result, modelState);

        // Add to TempData
        var (type, message) = GetTempDataMessage(result);
        controller.TempData[type] = message;
    }

    // Get TempData message from the definition
    private static (string Type, string Message) GetTempDataMessage(Result result)
    {
        if (ErrorMessages.TryGetValue(result.ErrorKey ?? string.Empty, out var definition))
        {
            return (definition.TempDataType, definition.TempDataMessage);
        }

        // Fallback for unknown error keys
        return ("Error", result.Error ?? "An error occurred");
    }

    // Helper record to store error message definitions
    private record ErrorMessageDefinition(
        string ModelField,
        string ModelErrorMessage,
        string TempDataType,
        string TempDataMessage
    );
}
