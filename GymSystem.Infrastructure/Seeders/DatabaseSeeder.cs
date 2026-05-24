using GymSystem.Infrastructure.Data;

namespace GymSystem.Infrastructure.Seeders;

public class DatabaseSeeder(GymAppDbContext dbContext)
{
    public async Task SeedAllAsync()
    {
        await dbContext.Database.EnsureCreatedAsync();

        await PlanSeeder.SeedAsync(dbContext);
        await CategorySeeder.SeedAsync(dbContext);
    }
}
