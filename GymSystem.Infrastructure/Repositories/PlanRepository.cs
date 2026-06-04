using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class PlanRepository(GymAppDbContext dbContext) : Repository<Plan>(dbContext), IPlanRepository
{
}
