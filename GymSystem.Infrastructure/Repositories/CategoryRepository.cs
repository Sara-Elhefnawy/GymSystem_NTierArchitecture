using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Repositories.Interfaces;

namespace GymSystem.Infrastructure.Repositories;

public class CategoryRepository(GymAppDbContext dbContext) : Repository<Category>(dbContext), ICategoryRepository
{
}
