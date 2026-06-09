using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public class CategoryRepository(GymAppDbContext dbContext) : Repository<Category>(dbContext), ICategoryRepository
{
}
