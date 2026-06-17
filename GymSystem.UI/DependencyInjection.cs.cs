using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace GymSystem.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddUIServices(
        this IServiceCollection services)
    {
        services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            // makes all accessed pages authenticated
            // instead of writing [Authorize] that checks if there is User object from CliamPrinciple and checks if it has roles
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        })
            .AddEntityFrameworkStores<GymAppDbContext>()  // stores IUserStore, IRoleStore
            .AddDefaultTokenProviders();                  // Reset password

        services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromHours(10);
            options.SlidingExpiration = true;    // if user is active it doesn't expire the cookie

        });

        return services;
    }
}
