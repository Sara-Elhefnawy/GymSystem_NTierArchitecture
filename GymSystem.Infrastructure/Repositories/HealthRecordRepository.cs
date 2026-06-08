using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public class HealthRecordRepository(GymAppDbContext dbContext) : Repository<HealthRecord>(dbContext), IHealthRecordRepository
{
}
