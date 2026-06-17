using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.DTOs.Session.Enums;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.Infrastructure.QueryService;
using GymSystem.Infrastructure.UnitOfWorks;
using GymSystem.Shared.Common;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SessionService> _logger;
    private readonly ISessionQueryService _sessionQueryService;

    public SessionService(ISessionQueryService sessionQueryService, IUnitOfWork uow, ILogger<SessionService> logger)
    {
        _sessionQueryService = sessionQueryService;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<IndexSessionDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var sessions = await _sessionQueryService.GetIndexItemsAsync(ct);
            var dtoList = sessions.Select(session => {
                return new IndexSessionDTO
                {
                    Id = session.Id,
                    CategoryName = session.CategoryName,
                    Description = session.Description,
                    TrainerName = session.TrainerName,
                    StartDate = session.StartDate,
                    EndDate = session.EndDate,
                    MaxCapacity = session.MaxCapacity,
                    AvailableSlots = session.AvailableSlots,
                    Status = GetStatus(session.StartDate, session.EndDate, DateTime.Now)
                };
            }).ToList();

            return Result.Ok<IReadOnlyList<IndexSessionDTO>>(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all sessions");
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

            var category = await _uow.Categories.GetByIdAsync(model.CategoryId, ct);
            if (category is null)
                return Result.Fail("Invalid category.", "CATEGORY_NOT_FOUND");

            var trainer = await _uow.Trainers.GetByIdAsync(model.TrainerId, ct);
            if (trainer is null)
                return Result.Fail("Invalid trainer.", "TRAINER_NOT_FOUND");

            if (!IsTrainerSpecialtyMatchingCategory(trainer.Specialty, category.Name))
                return Result.Fail($"Trainer specialty '{trainer.Specialty}' does not match category '{category.Name}'. Only trainers with matching specialty can lead this session.", "SPECIALTY_MISMATCH");

            if (await _uow.Sessions.HasTrainerConflictAsync(trainer.Id, start, end, null, ct))
                return Result.Fail("Trainer is not available during the selected time slot", "TRAINER_CONFLICT");

            var session = new Session
            {
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Capacity = model.Capacity,
                CategoryId = model.CategoryId,
                Description = model.Description,
                TrainerId = model.TrainerId
            };

            await _uow.Sessions.AddAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Session created successfully with ID: {Id}", session.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return Result.Fail("An unexpected error occurred", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<DetailsSessionDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await _uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings],
                ct: ct);

            if (session is null)
            {
                _logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail<DetailsSessionDTO>("Session not found", nameof(id));
            }

            var dto = new DetailsSessionDTO
            {
                Id = session.Id,
                CategoryName = session.Category.Name,
                Description = session.Description,
                TrainerName = session.Trainer.Name,
                StartDate = session.StartDate,
                EndDate = session.EndDate,
                AvailableSlots = session.Bookings.Count,
                MaxCapacity = session.Capacity,
                Status = GetStatus(session.StartDate, session.EndDate, DateTime.Now)
            };

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session details for ID: {Id}", id);
            return Result.Fail<DetailsSessionDTO>("Failed to retrieve session details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditSessionDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await _uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!],
                ct: ct);

            if (session is null)
            {
                _logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail<EditSessionDTO>("Session not found", "SESSION_NOT_FOUND");
            }

            var dto = new EditSessionDTO
            {
                Id = session.Id,
                TrainerId = session.TrainerId,
                Description = session.Description,
                StartDate = session.StartDate,
                EndDate = session.EndDate
            };

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session for edit, ID: {Id}", id);
            return Result.Fail<EditSessionDTO>("Failed to retrieve session data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditSessionDTO model, CancellationToken ct = default)
    {
        try
        {
            var session = await _uow.Sessions.GetByIdTrackingIncludingAsync(
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

            var trainer = await _uow.Trainers.GetByIdAsync(model.TrainerId, ct);
            if (trainer is null)
                return Result.Fail("Invalid trainer.", "TRAINER_NOT_FOUND");

            if (!IsTrainerSpecialtyMatchingCategory(trainer.Specialty, session.Category.Name))
                return Result.Fail($"Trainer specialty '{trainer.Specialty}' does not match category '{session.Category.Name}'", "SPECIALTY_MISMATCH");

            if (await _uow.Sessions.HasTrainerConflictAsync(trainer.Id, model.StartDate, model.EndDate, model.Id, ct))
                return Result.Fail("Trainer is not available during the selected time slot", "TRAINER_CONFLICT");

            session.TrainerId = model.TrainerId;
            session.Description = model.Description.Trim();
            session.StartDate = model.StartDate;
            session.EndDate = model.EndDate;
            session.UpdatedAt = DateTime.Now;

            _uow.Sessions.Update(session, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {Id}", model.Id);
            return Result.Fail("Failed to update session", "UPDATE_ERROR");
        }
    }

    public async Task<Result<DeleteSessionDTO>> GetForDeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await _uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings], 
                ct);

            if (session is null)
            {
                _logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail<DeleteSessionDTO>("Session not found", "SESSION_NOT_FOUND");
            }

            var status = GetStatus(session.StartDate, session.EndDate, DateTime.Now);

            var dto = new DeleteSessionDTO
            {
                Id = session.Id,
                Specialty = session.Category.Name,
                TrainerName = session.Trainer.Name,
                Description = session.Description,
                StartDate = session.StartDate,
                EndDate = session.EndDate,
                BookedCount = session.Bookings.Count,
                MaxCapacity = session.Capacity,
                Status = status.ToString(),
                CanDelete = status == SessionStatus.Upcoming
            };

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session for delete, ID: {Id}", id);
            return Result.Fail<DeleteSessionDTO>("Failed to retrieve session data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var session = await _uow.Sessions.GetByIdTrackingIncludingAsync(
                id,
                trackChanges: false,
                includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings],
                ct);

            if (session is null)
            {
                _logger.LogWarning("Session not found with ID: {Id}", id);
                return Result.Fail("Session not found", "SESSION_NOT_FOUND");
            }

            var currentTime = DateTime.Now;

            var status = GetStatus(session.StartDate, session.EndDate, DateTime.Now);

            if (status != SessionStatus.Upcoming)
                return Result.Fail("Only upcoming sessions can be deleted.", string.Empty);

            foreach (var booking in session.Bookings)
                await _uow.Bookings.SoftDeleteAsync(booking, ct);

            await _uow.Sessions.SoftDeleteAsync(session, ct);
            await _uow.Sessions.SaveChangesAsync(ct);

            _logger.LogInformation("SESSION {Id} deleted successfully", id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {Id}", id);
            return Result.Fail("Failed to delete session", "DELETE_ERROR");
        }
    }

    private bool IsTrainerSpecialtyMatchingCategory(Specialty trainerSpecialty, string categoryName)
    {
        return categoryName switch
        {
            "Yoga" => trainerSpecialty == Specialty.Yoga,
            "Cardio" => trainerSpecialty == Specialty.Cardio,
            "CrossFit" => trainerSpecialty == Specialty.CrossFit,
            "Boxing" => trainerSpecialty == Specialty.Boxing,
            "Strength" => trainerSpecialty == Specialty.Bodybuilding || trainerSpecialty == Specialty.PersonalTraining,
            _ => false
        };
    }

    private SessionStatus GetStatus(DateTime startDate, DateTime endDate, DateTime now)
    {
        if (now < startDate) return SessionStatus.Upcoming;
        if (now <= endDate) return SessionStatus.Ongoing;
        return SessionStatus.Completed;
    }
}
