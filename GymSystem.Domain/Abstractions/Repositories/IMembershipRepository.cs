using GymSystem.Domain.Entities;

namespace GymSystem.Domain.Abstractions.Repositories;

public interface IMembershipRepository : IRepository<Membership>
{
    Task<Membership?> GetByIdWithIncludesAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Membership>> GetActiveMembershipsAsync(CancellationToken ct = default);
    Task<bool> IsMemberAlreadyHasActivePlanAsync(int memberId, CancellationToken ct = default);
    Task<bool> CancelMembershipByIdAsync(int membershipId, CancellationToken ct = default);
    Task<bool> CancelMembershipAsync(int memberId, CancellationToken ct = default);
}
