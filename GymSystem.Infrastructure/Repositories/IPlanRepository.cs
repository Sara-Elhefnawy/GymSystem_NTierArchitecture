using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<Plan?> GetByIdAsync(int id);
    void Add(Plan plan);
    void Update(Plan plan);
    void Delete(Plan plan);
    Task<int> SaveChangesAsync();
}
