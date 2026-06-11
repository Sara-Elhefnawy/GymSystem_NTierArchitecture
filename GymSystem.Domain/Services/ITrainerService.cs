using GymSystem.Domain.DTOs.Trainer;

namespace GymSystem.Domain.Services;

public interface ITrainerService
{
    Task<IReadOnlyList<IndexTrainerDTO>> GetAllAsync(CancellationToken ct = default);

    Task<bool> CreateAsync(CreateTrainerDTO model, CancellationToken ct = default);

    Task<DetailsTrainerDTO?> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<EditTrainerDTO?> GetForEditAsync(int id, CancellationToken ct = default);

    Task<bool> UpdateAsync(EditTrainerDTO model, CancellationToken ct = default);

    Task<DeleteTrainerDTO?> GetForDeleteAsync(int id, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<bool> IsPhoneTakenAsync(string phone, CancellationToken ct = default);
}
