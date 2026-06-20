using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class MemberRepository(GymAppDbContext dbContext) : Repository<Member>(dbContext), IMemberRepository
{
    private readonly DbSet<Member> _dbSet = dbContext.Set<Member>();

    public async Task<Member?> GetWithMembershipDetailsAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(m => m.Memberships)
            .ThenInclude(ms => ms.Plan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Member>> GetMembersWithActiveMembershipAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        return await _dbSet.Where(m => m.Memberships.Any(mem => mem.EndDate >= today))
            .Include(m => m.Memberships)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> IsEmailTakenAsync(string normalizedEmail, int? excludeMemberId = null, CancellationToken ct = default)
        => await _dbSet.AnyAsync(m => m.Email == normalizedEmail && (!excludeMemberId.HasValue || m.Id != excludeMemberId.Value), ct);

    public async Task<bool> IsPhoneTakenAsync(string phone, int? excludeMemberId = null, CancellationToken ct = default)
        => await _dbSet.AnyAsync(m => m.Phone == phone && (!excludeMemberId.HasValue || m.Id != excludeMemberId.Value), ct);

    public Task<Member?> GetWithHealthRecordAsync(int id, bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _dbSet.Include(m => m.HealthRecord)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return query.FirstOrDefaultAsync(m => m.Id == id, ct);
    }
}
