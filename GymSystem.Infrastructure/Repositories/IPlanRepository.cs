using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public interface IPlanRepository : IRepository<Plan>
{
    Task<bool> HasActiveMembershipsAsync(int id, CancellationToken ct = default);
}
