using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Plan;

namespace GymSystem.Domain.Abstractions.Services;

public interface IPlanService
{
    Task<Result<IReadOnlyList<IndexPlanDTO>>> GetActivePlansAsync(CancellationToken ct = default);

    Task<Result<DetailsPlanDTO>> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<Result> UpdateAsync(EditPlanDTO dto, CancellationToken ct = default);
    Task<Result<EditPlanDTO>> GetForEditAsync(int id, CancellationToken ct = default);

    Task<Result> ToggleActivationAsync(int id, CancellationToken ct = default);
}
