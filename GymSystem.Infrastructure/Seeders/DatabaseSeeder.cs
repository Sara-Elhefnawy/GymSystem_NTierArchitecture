
using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Identities;
using GymSystem.Infrastructure.Seeders.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GymSystem.Infrastructure.Seeders;

public class DatabaseSeeder(
    GymAppDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
{
    private readonly GymAppDbContext _context = dbContext;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;

    public async Task SeedAllAsync()
    {
        await SeedIdentityAsync();
        await SeedDataAsync();
    }

    public async Task SeedIdentityAsync()
    {
        await IdentitySeeder.SeedAsync(_roleManager, _userManager, _configuration);
    }

    public async Task SeedDataAsync()
    {
        await dbContext.Database.MigrateAsync();

        await PlanSeeder.SeedAsync(dbContext);
        await CategorySeeder.SeedAsync(dbContext);
    }
}
