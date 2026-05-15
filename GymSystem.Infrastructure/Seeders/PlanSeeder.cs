using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Seeders;

public static class PlanSeeder
{
    public static async Task SeedAsync(GymAppDbContext dbContext)
    {
        if (await dbContext.Plans.AnyAsync())
            return;

        var plans = new List<Plan>
            {
                new() {
                    Name = "Basic Plan",
                    Description = "Access to gym equipment during staffed hours.",
                    DurationDays = 30,
                    Price = 300,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new() {
                    Name = "Standard Plan",
                    Description = "Includes gym equipment and 2 group classes per week.",
                    DurationDays = 60,
                    Price = 500,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new() {
                    Name = "Premium Plan",
                    Description = "Unlimited access to equipment, classes, and sauna.",
                    DurationDays = 90,
                    Price = 900,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new() {
                    Name = "Annual Plan",
                    Description = "Full year access with personal trainer sessions.",
                    DurationDays = 365,
                    Price = 3000,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

        await dbContext.Plans.AddRangeAsync(plans);
        await dbContext.SaveChangesAsync();
    }
}
