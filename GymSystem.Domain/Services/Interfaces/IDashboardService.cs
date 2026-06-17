using GymSystem.Domain.DTOs.Home;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services;

public interface IDashboardService
{
    Task<Result<DashboardHomeDTO>> GetHomeStatisticsAsync(CancellationToken ct = default);
}
