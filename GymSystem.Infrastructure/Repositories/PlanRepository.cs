using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class PlanRepository(GymAppDbContext dbContext) : Repository<Plan>(dbContext), IPlanRepository
{
    public async Task<bool> HasActiveMembershipsAsync(int planId, CancellationToken ct = default)
    {
        return await dbContext.Set<Membership>()
            .AnyAsync(m => m.PlanId == planId && m.EndDate > DateOnly.FromDateTime(DateTime.Now), ct);
    }

    public async Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken ct = default)
        => await dbContext.Plans
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
}
