using GymSystem.Domain.Services;

namespace GymSystem.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddUIServices(
        this IServiceCollection services)
    {
        services.AddControllersWithViews();

        services.AddScoped<IMemberService, MemberService>();

        return services;
    }
}
