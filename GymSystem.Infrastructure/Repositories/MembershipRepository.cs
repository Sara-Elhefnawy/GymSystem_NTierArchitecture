using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class MembershipRepository(GymAppDbContext dbContext) : Repository<Membership>(dbContext), IMembershipRepository
{
    public async Task<Membership?> GetActiveMembershipByMemberIdAsync(int memberId, CancellationToken ct = default)
        => await dbContext.Memberships
                .Include(m => m.Member)
                .Include(m => m.Plan)
                .FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateOnly.FromDateTime(DateTime.Now), ct);

    public async Task<IEnumerable<Membership>> GetActiveMembershipsAsync(CancellationToken ct = default)
        => await dbContext.Memberships
                .Include(m => m.Member)
                .Include(m => m.Plan)
                .Where(m => m.EndDate > DateOnly.FromDateTime(DateTime.Now))
                .OrderBy(m => m.Member.Name)
                .ToListAsync(ct);


    public async Task<bool> IsMemberAlreadyHasActivePlanAsync(int memberId, CancellationToken ct = default)
        => await dbContext.Memberships
                .AnyAsync(m => m.MemberId == memberId && m.EndDate > DateOnly.FromDateTime(DateTime.Now), ct);


    public async Task<bool> CancelMembershipAsync(int memberId, CancellationToken ct = default)
    {
        var membership = await dbContext.Memberships
            .FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate > DateOnly.FromDateTime(DateTime.Now), ct);

        if (membership is null)
            return false;

        dbContext.Memberships.Remove(membership);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
