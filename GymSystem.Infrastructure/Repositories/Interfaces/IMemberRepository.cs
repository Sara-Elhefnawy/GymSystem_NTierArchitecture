using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface IMemberRepository : IRepository<Member>
{
    Task<Member?> GetWithMembershipDetailsAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Member>> GetMembersWithActiveMembershipAsync(CancellationToken ct = default);

    Task<bool> IsEmailTakenAsync(string normalizedEmail, int? excludeMemberId = null, CancellationToken ct = default);
    Task<bool> IsPhoneTakenAsync(string phone, int? excludeMemberId = null, CancellationToken ct = default);

    Task<Member?> GetWithHealthRecordAsync(int id, bool trackChanges = false, CancellationToken ct = default);
}
