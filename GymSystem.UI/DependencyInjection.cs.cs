using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Identities;
using Microsoft.AspNetCore.Identity;

namespace GymSystem.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddUIServices(
        this IServiceCollection services)
    {
        services.AddControllersWithViews();

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<GymAppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
