using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;

namespace GymSystem.Domain.Abstractions.Services;

public interface IMemberService
{
    Task<Result<IReadOnlyList<IndexMemberDTO>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<IndexMemberDTO>>> GetMembersWithActiveMembershipAsync(CancellationToken ct = default);

    Task<Result> CreateAsync(CreateMemberDTO model, CancellationToken ct = default);

    Task<Result<bool>> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<Result<bool>> IsPhoneTakenAsync(string phone, CancellationToken ct = default);

    Task<Result<DetailsMemberDTO>> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<Result<DetailsHealthRecordDTO>> GetHealthRecordAsync(int id, CancellationToken ct = default);

    Task<Result> UpdateAsync(EditMemberDTO dto, CancellationToken ct = default);
    Task<Result<EditMemberDTO>> GetForEditAsync(int id, CancellationToken ct = default);

    Task<Result<DeleteMemberDTO>> GetForDeleteAsync(int id, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<Result<byte[]>> GetMemberPhotoAsync(int id, CancellationToken ct = default);
}
