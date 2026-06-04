using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(
        this IServiceCollection services)
    {
        return services;
    }

}
