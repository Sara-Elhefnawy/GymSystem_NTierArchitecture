using GymSystem.Domain.Entities;

namespace GymSystem.Domain.Abstractions.Repositories;

public interface IPlanRepository : IRepository<Plan>
{
    Task<bool> HasActiveMembershipsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken ct = default);
}
