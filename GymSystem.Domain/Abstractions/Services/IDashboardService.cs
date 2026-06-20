using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Home;

namespace GymSystem.Domain.Abstractions.Services;

public interface IDashboardService
{
    Task<Result<DashboardHomeDTO>> GetHomeStatisticsAsync(CancellationToken ct = default);
}
