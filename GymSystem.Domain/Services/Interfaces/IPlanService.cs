using GymSystem.Domain.DTOs.Plan;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services.Interfaces;

public interface IPlanService
{
    Task<Result<IReadOnlyList<IndexPlanDTO>>> GetAllAsync(CancellationToken ct = default);

    Task<Result<DetailsPlanDTO>> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<Result> UpdateAsync(EditPlanDTO dto, CancellationToken ct = default);
    Task<Result<EditPlanDTO>> GetForEditAsync(int id, CancellationToken ct = default);

    Task<Result> ToggleActivationAsync(int id, CancellationToken ct = default);
}
