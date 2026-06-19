using GymSystem.Domain.Attachments;
using GymSystem.Domain.Mappings;
using GymSystem.Domain.QRCode;
using GymSystem.Domain.Services;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Attachments;
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

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MemberProfile>();
            cfg.AddProfile<HealthRecordProfile>();
        }, typeof(MemberProfile).Assembly);

        return services;
    }

}
