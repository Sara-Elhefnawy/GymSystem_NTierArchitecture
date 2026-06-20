using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;

namespace GymSystem.Infrastructure.Repositories;

public class CategoryRepository(GymAppDbContext dbContext) : Repository<Category>(dbContext), ICategoryRepository
{
}
