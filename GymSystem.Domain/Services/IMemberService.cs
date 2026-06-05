using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Results;

namespace GymSystem.Domain.Services;

public interface IMemberService
{
    Task<IEnumerable<MemberIndexDTO>> GetAllAsync(CancellationToken ct = default);
    Task<bool> CreateAsync(CreateMemberDTO model, CancellationToken ct = default);
}
