using GymSystem.Domain.DTOs.Home;
using GymSystem.UI.ViewModels.Home;
using Mapster;

namespace GymSystem.UI.Mappings;

public class HomeViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from DashboardHomeStatisticsDTO to DashboardHomeViewModel
        config.NewConfig<DashboardHomeDTO, DashboardHomeViewModel>()
            .Map(dest => dest.TotalMembers, src => src.TotalMembers)
            .Map(dest => dest.ActiveMembers, src => src.ActiveMembers)
            .Map(dest => dest.TotalTrainers, src => src.TotalTrainers)
            .Map(dest => dest.UpcomingSessions, src => src.UpcomingSessions)
            .Map(dest => dest.OngoingSessions, src => src.OngoingSessions)
            .Map(dest => dest.CompletedSessions, src => src.CompletedSessions);
    }
}
