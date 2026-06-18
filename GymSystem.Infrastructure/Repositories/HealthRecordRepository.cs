using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Repositories.Interfaces;

namespace GymSystem.Infrastructure.Repositories;

public class HealthRecordRepository(GymAppDbContext dbContext) : Repository<HealthRecord>(dbContext), IHealthRecordRepository
{
}
