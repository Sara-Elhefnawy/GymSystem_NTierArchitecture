using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface IMembershipRepository : IRepository<Membership>
{
    Task<Membership?> GetActiveMembershipByMemberIdAsync(int memberId, CancellationToken ct = default);
    Task<IEnumerable<Membership>> GetActiveMembershipsAsync(CancellationToken ct = default);
    Task<bool> IsMemberAlreadyHasActivePlanAsync(int memberId, CancellationToken ct = default);
    Task<bool> CancelMembershipAsync(int memberId, CancellationToken ct = default);
}
