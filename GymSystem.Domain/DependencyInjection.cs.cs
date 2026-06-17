using GymSystem.Domain.Attachments;
using GymSystem.Domain.Services;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Attachments;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IAttachmentService, AttachmentService>();

        return services;
    }

}
