using GymSystem.Domain.DTOs.Session;
using GymSystem.Infrastructure.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IUnitOfWork uow, ILogger<SessionService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<IndexSessionDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var sessions = await _uow.Sessions.GetAllAsync(ct);

        var dtoList = sessions.Select(session => {
            var status = session.StartDate > DateTime.Now ? "Upcoming" :
                         session.EndDate < DateTime.Now ? "Completed" : "Ongoing";

            var duration = session.EndDate - session.StartDate;

            return new IndexSessionDTO
            {
                Id = session.Id,
                Specialty = session.Category.Name,
                Description = session.Description,
                TrainerName = session.Trainer.Name,
                StartDate = session.StartDate.ToString("MMM dd, yyyy"),
                //TimeRange = $"{session.StartDate:hh:mm tt} - {session.EndDate:hh:mm tt}",
                TimeRange = session.StartDate - session.EndDate,
                Duration = duration.ToString(@"hh\:mm\:ss"),
                Capacity = session.Capacity,
                AvailableSlots = session.Bookings.Count,
                Status = status
            };
        }).ToList();

        return dtoList;
    }
}
