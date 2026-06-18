using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class SessionRepository(GymAppDbContext dbContext) : Repository<Session>(dbContext), ISessionRepository
{
    private readonly DbSet<Session> _dbSet = dbContext.Set<Session>();

    public async Task<IReadOnlyList<Session>> GetAllWithBookingsAsync(CancellationToken ct = default)
        => await _dbSet.Include(s => s.Category)
            .Include(s => s.Trainer)
            .Include(s => s.Bookings)
            .OrderBy(s => s.StartDate)
            .ToListAsync(ct);

    public async Task<Session?> GetByIdWithBookingsAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(s => s.Category)
            .Include(s => s.Trainer)
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> HasUpcomingSessionsForTrainerAsync(int trainerId, DateTime now, CancellationToken ct = default)
        => await _dbSet
            .AnyAsync(s => s.TrainerId == trainerId && s.EndDate >= now, ct);

    public async Task<IEnumerable<Session>> GetSessionsForDateAsync(DateTime date, CancellationToken ct = default)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        return await _dbSet
            .Include(s => s.Category)
            .Include(s => s.Trainer)
            .Include(s => s.Bookings.Where(b => !b.IsDeleted))
            .Where(s =>
                !s.IsDeleted &&
                s.StartDate >= startOfDay &&
                s.StartDate < endOfDay)
            .OrderBy(s => s.StartDate)
            .ToListAsync(ct);
    }

    public async Task<bool> HasTrainerConflictAsync(
            int trainerId, 
            DateTime start, 
            DateTime end,
            int? excludeSessionId = null, 
            CancellationToken ct = default)
        => await _dbSet.AnyAsync(s =>
                s.TrainerId == trainerId
                && s.Id != excludeSessionId
                && s.StartDate < end
                && s.EndDate > start, ct);

    public async Task<Session?> GetActiveSessionAtTimeAsync(DateTime time, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Include(s => s.Bookings.Where(b => !b.IsDeleted))
            .FirstOrDefaultAsync(s =>
                !s.IsDeleted &&
                s.StartDate <= time &&
                s.EndDate >= time,
                ct);
    }
}
