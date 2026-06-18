using GymSystem.Domain.DTOs.Memberships;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services.Interfaces;

public interface IMembershipService
{
    Task<Result<IEnumerable<IndexMembershipDTO>>> GetActiveMembershipsAsync(CancellationToken ct = default);
    Task<Result> CreateMembershipAsync(CreateMembershipDTO model, CancellationToken ct = default);
    Task<Result> CancelMembershipAsync(int memberId, CancellationToken ct = default);
    Task<Result<bool>> IsMemberHasActivePlanAsync(int memberId, CancellationToken ct = default);
}
