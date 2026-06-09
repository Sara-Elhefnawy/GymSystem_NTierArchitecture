using GymSystem.Domain.DTOs.Trainer;

namespace GymSystem.Domain.Services;

public interface ITrainerService
{
    Task<IReadOnlyList<IndexTrainerDTO>> GetAllAsync(CancellationToken ct = default);

    Task<bool> CreateAsync(CreateTrainerDTO model, CancellationToken ct = default);

    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<bool> IsPhoneTakenAsync(string phone, CancellationToken ct = default);
}
