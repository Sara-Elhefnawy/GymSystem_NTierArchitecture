using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.DTOs.Home;
using Microsoft.Extensions.Logging;
using GymSystem.Domain.Common;

namespace GymSystem.Domain.Services;

public class DashboardService(IUnitOfWork uow, ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<Result<DashboardHomeDTO>> GetHomeStatisticsAsync(CancellationToken ct = default)
    {
        try
        {

            var now = DateTime.Now;
            var dateOnleNow = DateOnly.FromDateTime(now);

            var dto = new DashboardHomeDTO {
                TotalMembers = await uow.Members.CountAsync(ct: ct),
                ActiveMembers = await uow.Members.CountAsync(m => m.Memberships.Any(ms => ms.StartDate <= dateOnleNow && ms.EndDate >= dateOnleNow), ct: ct),
                TotalTrainers = await uow.Trainers.CountAsync(ct: ct),
                CompletedSessions = await uow.Sessions.CountAsync(s => s.EndDate < now, ct: ct),
                OngoingSessions = await uow.Sessions.CountAsync(s => s.StartDate <= now && s.EndDate >= now, ct: ct),
                UpcomingSessions = await uow.Sessions.CountAsync(s => s.StartDate < now, ct: ct)
            };

            logger.LogInformation("Dashboard statistics loaded successfully");

            return Result.Ok(dto);
        } catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load dashboard statistics");

            return Result.Fail<DashboardHomeDTO>("Unable to load dashboard statistics");
        }
    }
}
