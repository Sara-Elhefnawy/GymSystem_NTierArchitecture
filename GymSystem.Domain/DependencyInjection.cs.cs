using GymSystem.Domain.Abstractions.Attachments;
using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Abstractions.QrService;
using GymSystem.Domain.Attachments;
using GymSystem.Domain.QRCode;
using Mapster;
using MapsterMapper;
using GymSystem.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IMembershipService, MembershipService>();

        services.AddScoped<IAttachmentService, AttachmentService>();

        services.AddScoped<IQrService, QrService>();
        services.Configure<QrCodeSettings>(configuration.GetSection("QrCodeSettings"));

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(DependencyInjection).Assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
