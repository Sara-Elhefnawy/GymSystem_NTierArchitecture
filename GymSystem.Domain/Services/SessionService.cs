using GymSystem.Domain.Abstractions.QueryService;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Session.Enums;
using GymSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using GymSystem.Domain.Common;
using Mapster;

namespace GymSystem.Domain.Services;

public class SessionService(
    ISessionQueryService sessionQueryService,
    IUnitOfWork uow,
    ILogger<SessionService> logger) : ISessionService
{
    public async Task<Result<IReadOnlyList<IndexSessionDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var sessions = await sessionQueryService.GetIndexItemsAsync(ct);

            var dtoList = sessions.Adapt<IReadOnlyList<IndexSessionDTO>>();

            // Set the Status for each DTO (calculated)
            foreach (var dto in dtoList)
            {
                dto.Status = GetStatus(dto.StartDate, dto.EndDate, DateTime.Now);
            }

            return Result.Ok(dtoList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all sessions");
            return Result.Fail<IReadOnlyList<IndexSessionDTO>>("Failed to retrieve sessions", "DATABASE_ERROR");
        }
    }

    public async Task<Result> CreateAsync(CreateSessionDTO model, CancellationToken ct = default)
    {
        try
        {
            var start = model.StartDate;
            var end = model.EndDate;

            if (start >= end)
                return Result.Fail("End date and time must be after start date and time", "INVALID_DATE_RANGE");

            if (start <= DateTime.Now)
                return Result.Fail("Start date and time must be in the future", "PAST_START_DATE");

            if (model.Capacity < 1 || model.Capacity > 25)
                return Result.Fail("Session capacity must be between 1 and 25", "INVALID_CAPACITY");

            var category = await uow.Categories.GetByIdAsync(model.CategoryId, ct);
            if (category is null)
                return Result.Fail("Invalid category.", "CATEGORY_NOT_FOUND");

            var trainer = await uow.Trainers.GetByIdAsync(model.TrainerId, ct);
            if (trainer is null)
                return Result.Fail("Invalid trainer.", "TRAINER_NOT_FOUND");

            if (ValidateTrainerSpecialty(trainer, category).IsFailure)
                return Result.Fail($"Trainer specialty '{trainer.Specialty}' does not match category '{category.Name}'. Only trainers with matching specialty can lead this session.", "SPECIALTY_MISMATCH");

            if (await uow.Sessions.HasTrainerConflictAsync(trainer.Id, start, end, null, ct))
                return Result.Fail("Trainer is not available during the selected time slot", "TRAINER_CONFLICT");

            var session = model.Adapt<Session>();

            await uow.Sessions.AddAsync(session, ct);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Session created successfully with ID: {Id}", session.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating session");
            return Result.Fail("An unexpected error occurred", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<DetailsSessionDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings],
                ct: ct);

            if (session is null)
            {
                logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail<DetailsSessionDTO>("Session not found", nameof(id));
            }

            var activeBookings = await uow.Bookings.GetBookingsWithActiveMembershipForSessionAsync(id, ct);

            var dto = session.Adapt<DetailsSessionDTO>();

            dto.AvailableSlots = session.Capacity - activeBookings.Count();

            dto.Status = GetStatus(session.StartDate, session.EndDate, DateTime.Now);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting session details for ID: {Id}", id);
            return Result.Fail<DetailsSessionDTO>("Failed to retrieve session details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditSessionDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!],
                ct: ct);

            if (session is null)
            {
                logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail<EditSessionDTO>("Session not found", "SESSION_NOT_FOUND");
            }

            var dto = session.Adapt<EditSessionDTO>();

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting session for edit, ID: {Id}", id);
            return Result.Fail<EditSessionDTO>("Failed to retrieve session data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditSessionDTO model, CancellationToken ct = default)
    {
        try
        {
            var session = await uow.Sessions.GetByIdTrackingIncludingAsync(
                model.Id,
                trackChanges: false,
                includes: [s => s.Category!, s => s.Bookings],
                ct: ct);

            if (session is null)
                return Result.Fail("Session not found", "SESSION_NOT_FOUND");

            var currentTime = DateTime.Now;

            if (session.StartDate <= currentTime)
            {
                return Result.Fail("Cannot edit sessions that have already started", "SESSION_NOT_EDITABLE");
            }

            if (model.StartDate >= model.EndDate)
                return Result.Fail("End date and time must be after start date and time", "INVALID_DATE_RANGE");

            if (model.StartDate <= currentTime)
                return Result.Fail("Start date and time must be in the future", "PAST_START_DATE");

            var trainer = await uow.Trainers.GetByIdAsync(model.TrainerId, ct);
            if (trainer is null)
                return Result.Fail("Invalid trainer.", "TRAINER_NOT_FOUND");

            if (ValidateTrainerSpecialty(trainer, session.Category).IsFailure)
                return Result.Fail($"Trainer specialty '{trainer.Specialty}' does not match category '{session.Category.Name}'", "SPECIALTY_MISMATCH");

            if (await uow.Sessions.HasTrainerConflictAsync(trainer.Id, model.StartDate, model.EndDate, model.Id, ct))
                return Result.Fail("Trainer is not available during the selected time slot", "TRAINER_CONFLICT");

            TypeAdapter.Adapt(model, session);

            uow.Sessions.Update(session, ct);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Session {Id} updated successfully", session.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating session {Id}", model.Id);
            return Result.Fail("Failed to update session", "UPDATE_ERROR");
        }
    }

    public async Task<Result<DeleteSessionDTO>> GetForDeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings],
                ct);

            if (session is null)
            {
                logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail<DeleteSessionDTO>("Session not found", "SESSION_NOT_FOUND");
            }

            var dto = session.Adapt<DeleteSessionDTO>();

            var status = GetStatus(session.StartDate, session.EndDate, DateTime.Now);
            dto.Status = status.ToString();
            dto.CanDelete = status == SessionStatus.Upcoming;

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting session for delete, ID: {Id}", id);
            return Result.Fail<DeleteSessionDTO>("Failed to retrieve session data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings],
                ct);

            if (session is null)
            {
                logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail("Session not found", "SESSION_NOT_FOUND");
            }

            var currentTime = DateTime.Now;

            var status = GetStatus(session.StartDate, session.EndDate, DateTime.Now);

            if (status != SessionStatus.Upcoming)
                return Result.Fail("Only upcoming sessions can be deleted.", string.Empty);

            foreach (var booking in session.Bookings)
                await uow.Bookings.SoftDeleteAsync(booking, ct);

            await uow.Sessions.SoftDeleteAsync(session, ct);
            await uow.Sessions.SaveChangesAsync(ct);

            logger.LogInformation("SESSION {Id} deleted successfully", id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting session {Id}", id);
            return Result.Fail("Failed to delete session", "DELETE_ERROR");
        }
    }

    private Result ValidateTrainerSpecialty(Trainer trainer, Category category)
    {
        var trainerSpecialty = trainer.Specialty.ToString().Trim() ?? string.Empty;
        var categoryName = category.Name?.Trim() ?? string.Empty;

        // Normalize both strings for comparison (remove spaces and underscores)
        var normalizedTrainerSpecialty = trainerSpecialty.Replace("_", "").Replace(" ", "").ToLowerInvariant();
        var normalizedCategoryName = categoryName.Replace(" ", "").Replace("_", "").ToLowerInvariant();

        // Case-insensitive match
        if (!string.Equals(normalizedTrainerSpecialty, normalizedCategoryName, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Trainer specialty '{TrainerSpecialty}' does not match category '{CategoryName}'",
                trainerSpecialty, categoryName);
            return Result.Fail($"Trainer specialty '{trainerSpecialty}' does not match category '{categoryName}'. Only trainers with matching specialty can lead this session.");
        }

        return Result.Ok();
    }

    private SessionStatus GetStatus(DateTime startDate, DateTime endDate, DateTime now)
    {
        if (now < startDate) return SessionStatus.Upcoming;
        if (now <= endDate) return SessionStatus.Ongoing;
        return SessionStatus.Completed;
    }
}
