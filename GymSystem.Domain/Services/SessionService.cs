using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.DTOs.Session.Enums;
using GymSystem.Domain.DTOs.Session.Lookups;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.QueryService;
using GymSystem.Infrastructure.UnitOfWorks;
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
                    Status = GetStatus(session.StartDate, session.EndDate, DateTime.UtcNow)
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

    public async Task<Result<CreateSessionDTO>> GetCreateFormAsync(CancellationToken ct = default)
        => await PrepareCreateFormAsync(new CreateSessionDTO { Capacity = 25 }, ct);

    public async Task<Result<CreateSessionDTO>> PrepareCreateFormAsync(
        CreateSessionDTO model, CancellationToken ct = default)
    {
        var categories = await _uow.Categories.GetAllAsync(ct);
        var trainers = await _uow.Trainers.GetAllAsync(ct);

        model.Categories = categories.Select(c => new CategoryLookupDTO
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        model.Trainers = trainers.Select(t => new TrainerLookupDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList();

        return Result.Ok(model);
    }

    public async Task<Result> CreateAsync(CreateSessionDTO model, CancellationToken ct = default)
    {
        var start = model.StartDate;
        var end = model.EndDate;

        if (start >= end)
            return Result.Fail("End date and time must be after start date and time");

        if (start <= DateTime.UtcNow)
            return Result.Fail("Start date and time must be in the future");

        if (model.Capacity < 1 || model.Capacity > 25)
            return Result.Fail("Session capacity must be bewenn 1 and 25");

        if (!await _uow.Categories.ExistsAsync(c => c.Id == model.CategoryId, ct))
            return Result.Fail("Invalid category.", nameof(model.CategoryId));

        if (!await _uow.Trainers.ExistsAsync(t => t.Id == model.TrainerId, ct))
            return Result.Fail("Invalid trainer.", nameof(model.TrainerId));

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

        return Result.Ok();
    }

    public async Task<Result<DetailsSessionDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var session = await _uow.Sessions.GetByIdTrackingIncludingAsync(
            id,
            trackChanges: false,
            includes: [s => s.Trainer!, s => s.Category!, s => s.Bookings],
            ct: ct);

        if (session is null)
            return Result.Fail<DetailsSessionDTO>("Session not found", nameof(id));
        
        return Result.Ok(new DetailsSessionDTO
        {
            CategoryName = session.Category.Name,
            Description = session.Description,
            TrainerName = session.Trainer.Name,
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            AvailableSlots = session.Bookings.Count,
            MaxCapacity = session.Capacity,
            Status = GetStatus(session.StartDate, session.EndDate, DateTime.UtcNow)
        });
    }

    private SessionStatus GetStatus(DateTime startDate, DateTime endDate, DateTime utcNow)
    {
        SessionStatus status;
        if (utcNow < startDate)
            status = SessionStatus.Upcoming;
        else if (utcNow <= endDate)
            status = SessionStatus.Ongoing;
        else
            status = SessionStatus.Completed;
        return status;
    }
}
