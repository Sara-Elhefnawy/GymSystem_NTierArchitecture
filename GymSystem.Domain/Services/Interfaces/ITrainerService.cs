using GymSystem.Domain.DTOs.Session.Lookups;
using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services.Interfaces;

public interface ITrainerService
{
    Task<Result<IReadOnlyList<IndexTrainerDTO>>> GetAllAsync(CancellationToken ct = default);

    Task<Result> CreateAsync(CreateTrainerDTO model, CancellationToken ct = default);

    Task<Result<bool>> IsEmailTakenAsync(string email, CancellationToken ct = default);
    Task<Result<bool>> IsPhoneTakenAsync(string phone, CancellationToken ct = default);

    Task<Result<DetailsTrainerDTO>> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<Result<EditTrainerDTO>> GetForEditAsync(int id, CancellationToken ct = default);

    Task<Result> UpdateAsync(EditTrainerDTO model, CancellationToken ct = default);

    Task<Result<DeleteTrainerDTO>> GetForDeleteAsync(int id, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<TrainerLookupDTO>> GetTrainerLookupAsync(CancellationToken ct = default);
}
