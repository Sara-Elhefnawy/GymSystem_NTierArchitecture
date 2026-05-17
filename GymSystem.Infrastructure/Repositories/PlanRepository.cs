using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly GymAppDbContext _dbContext;
    public PlanRepository(GymAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Plan plan)
        => _dbContext.Plans.Add(plan);

    public void Delete(Plan plan)
        => _dbContext.Plans.Remove(plan);

    public async Task<IEnumerable<Plan>> GetAllAsync()
        => await _dbContext.Plans.ToListAsync();

    public async Task<Plan?> GetByIdAsync(int id)
        => await _dbContext.Plans.FirstOrDefaultAsync(x => x.Id == id);

    public void Update(Plan plan)
        => _dbContext.Update(plan);

    public async Task<int> SaveChangesAsync()
        => await _dbContext.SaveChangesAsync();
}
