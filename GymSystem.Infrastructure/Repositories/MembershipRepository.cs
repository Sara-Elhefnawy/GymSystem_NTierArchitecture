using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public class MembershipRepository(GymAppDbContext dbContext) : Repository<Membership>(dbContext), IMembershipRepository
{
}
