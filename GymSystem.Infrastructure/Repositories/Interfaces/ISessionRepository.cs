using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface ISessionRepository : IRepository<Session>
{
    Task<bool> HasUpcomingSessionsForTrainerAsync(int trainerId, DateTime utcNow, CancellationToken ct = default);

    Task<IReadOnlyList<Session>> GetAllWithBookingsAsync(CancellationToken ct = default);
    Task<Session?> GetByIdWithBookingsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Session>> GetSessionsForDateAsync(DateTime date, CancellationToken ct = default);
    Task<Session?> GetActiveSessionAtTimeAsync(DateTime time, CancellationToken ct = default);
    Task<IEnumerable<Session>> GetActiveSessionsAtTimeAsync(DateTime time, CancellationToken ct = default);
    Task<bool> HasTrainerConflictAsync(
        int trainerId,
        DateTime start,
        DateTime end,
        int? excludeSessionId = null,
        CancellationToken ct = default);
}