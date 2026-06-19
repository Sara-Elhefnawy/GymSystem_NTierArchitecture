using GymSystem.Infrastructure.Entities;
namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface IPlanRepository : IRepository<Plan>
{
    Task<bool> HasActiveMembershipsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken ct = default);
}
