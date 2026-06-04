using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Interceptor;
using GymSystem.Infrastructure.Seeders;
using GymSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddTransient<AuditInterceptor>();
        services.AddTransient<SoftDeleteInterceptor>();

        services.AddDbContext<GymAppDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            var softDeleteInterceptor = serviceProvider.GetRequiredService<SoftDeleteInterceptor>();

            options
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(GymAppDbContext).Assembly.FullName)
                );
            options.AddInterceptors(auditInterceptor, softDeleteInterceptor);
        });

        // Register services
        services.AddScoped<IAnonymizationService, AnonymizationService>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
