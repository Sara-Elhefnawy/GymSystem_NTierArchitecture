using GymSystem.Domain.DTOs.Membership;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services.Interfaces;

public interface IMembershipService
{
    Task<Result<IEnumerable<IndexMembershipDTO>>> GetActiveMembershipsAsync(CancellationToken ct = default);
    Task<Result> CreateMembershipAsync(CreateMembershipDTO model, CancellationToken ct = default);
    Task<Result> CancelMembershipAsync(int memberId, CancellationToken ct = default);
}
