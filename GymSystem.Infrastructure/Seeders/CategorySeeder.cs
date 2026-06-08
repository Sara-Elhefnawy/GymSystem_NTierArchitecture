using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Seeders;

public static class CategorySeeder
{
    public static async Task SeedAsync(GymAppDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
            return;

        var categories = new List<Category>
            {
                new() { Name = "Cardio" },
                new() { Name = "Strength" },
                new() { Name = "Yoga" }
            };

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();
    }
}
