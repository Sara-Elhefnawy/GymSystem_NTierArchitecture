using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Interceptor;
using GymSystem.Infrastructure.QueryService;
using GymSystem.Infrastructure.Repositories;
using GymSystem.Infrastructure.Repositories.Interfaces;
using GymSystem.Infrastructure.Seeders;
using GymSystem.Infrastructure.Services;
using GymSystem.Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        services.AddDbContext<GymAppDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            var softDeleteInterceptor = serviceProvider.GetRequiredService<SoftDeleteInterceptor>();

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found."),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(GymAppDbContext).Assembly.FullName));
            options.AddInterceptors(auditInterceptor, softDeleteInterceptor);
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISessionQueryService, SessionQueryService>();

        services.AddScoped<IAnonymizationService, AnonymizationService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
