using GymSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Seeders;

public class DatabaseSeeder(GymAppDbContext dbContext)
{
    public async Task SeedAllAsync()
    {
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.MigrateAsync();

        await PlanSeeder.SeedAsync(dbContext);
        await CategorySeeder.SeedAsync(dbContext);
    }
}
