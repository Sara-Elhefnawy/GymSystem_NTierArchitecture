using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;

namespace GymSystem.Domain.Services;

public interface IMemberService
{
    Task<IEnumerable<IndexMemberDTO>> GetAllAsync(CancellationToken ct = default);
    Task<bool> CreateAsync(CreateMemberDTO model, CancellationToken ct = default);

    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<bool> IsPhoneTakenAsync(string phone, CancellationToken ct = default);

    Task<DetailsMemberDTO?> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<DetailsHealthRecordDTO?> GetHealthRecordAsync( int id, CancellationToken ct = default);

    Task<bool> UpdateAsync(EditMemberDTO dto, CancellationToken ct = default);
    Task<EditMemberDTO?> GetForEditAsync(int id, CancellationToken ct = default);

    Task<DeleteMemberDTO?> GetForDeleteAsync(int id, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
