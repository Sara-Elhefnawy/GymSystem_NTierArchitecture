using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class MemberRepository(GymAppDbContext dbContext) : Repository<Member>(dbContext), IMemberRepository
{
    private readonly DbSet<Member> _dbSet = dbContext.Set<Member>();

    public async Task<Member?> GetWithBookingsAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Member?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(m => m.Memberships)
            .ThenInclude(ms => ms.Plan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Member?> GetWithHealthRecordAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(m => m.HealthRecord)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
}
