using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public interface ISessionRepository : IRepository<Session>
{
    Task<bool> HasUpcomingSessionsForTrainerAsync(int trainerId, DateTime utcNow, CancellationToken ct = default);

    Task<IReadOnlyList<Session>> GetAllWithDetailsAsync(CancellationToken ct = default);

    Task<bool> HasTrainerConflictAsync(
        int trainerId, 
        DateTime start, 
        DateTime end,
        int? excludeSessionId = null, 
        CancellationToken ct = default);
}
