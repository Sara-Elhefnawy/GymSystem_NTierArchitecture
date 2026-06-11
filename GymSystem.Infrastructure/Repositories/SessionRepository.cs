using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class SessionRepository(GymAppDbContext dbContext) : Repository<Session>(dbContext), ISessionRepository
{
    private readonly GymAppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<Session>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await _dbContext.Sessions
            .Include(s => s.Category)
            .Include(s => s.Trainer)
            .Include(s => s.Bookings)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<bool> HasUpcomingSessionsForTrainerAsync(int trainerId, DateTime utcNow, CancellationToken ct = default)
        => await _dbContext.Sessions
            .AnyAsync(s => s.TrainerId == trainerId && s.EndDate >= utcNow, ct);

}
