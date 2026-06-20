using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Membership;

namespace GymSystem.Domain.Abstractions.Services;

public interface IMembershipService
{
    Task<Result<IEnumerable<IndexMembershipDTO>>> GetActiveMembershipsAsync(CancellationToken ct = default);
    Task<Result> CreateMembershipAsync(CreateMembershipDTO model, CancellationToken ct = default);
    Task<Result> CancelMembershipAsync(int memberId, CancellationToken ct = default);
}
