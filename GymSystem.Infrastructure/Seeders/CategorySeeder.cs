using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;
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
                new() { Name = "Cardio", RequiredSpecialty = Specialty.Cardio },
                new() { Name = "Boxing", RequiredSpecialty = Specialty.Boxing },
                new() { Name = "CrossFit", RequiredSpecialty = Specialty.CrossFit },
                new() { Name = "Yoga", RequiredSpecialty = Specialty.Yoga },
                new() { Name = "GeneralFitness", RequiredSpecialty = Specialty.GeneralFitness },
                new() { Name = "PersonalTraining", RequiredSpecialty = Specialty.PersonalTraining },
                new() { Name = "Bodybuilding", RequiredSpecialty = Specialty.Bodybuilding },
            };

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();
    }
}
