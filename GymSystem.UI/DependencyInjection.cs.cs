using GymSystem.Infrastructure.Seeders;

namespace GymSystem.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddUIServices(
        this IServiceCollection services)
    {
        services.AddControllersWithViews();

        return services;
    }
}
