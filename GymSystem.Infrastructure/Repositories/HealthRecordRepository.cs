using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;

namespace GymSystem.Infrastructure.Repositories;

public class HealthRecordRepository(GymAppDbContext dbContext) : Repository<HealthRecord>(dbContext), IHealthRecordRepository
{
}
