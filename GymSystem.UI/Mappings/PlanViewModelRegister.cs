using GymSystem.Domain.DTOs.Plan;
using GymSystem.UI.ViewModels.Plan;
using Mapster;

namespace GymSystem.UI.Mappings;

public class PlanViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from IndexPlanDTO to IndexPlanViewModel
        config.NewConfig<IndexPlanDTO, IndexPlanViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DurationDays, src => src.DurationDays)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.IsActive, src => src.IsActive);

        // Map from DetailsPlanDTO to DetailsPlanViewModel
        config.NewConfig<DetailsPlanDTO, DetailsPlanViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DurationDays, src => src.DurationDays)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.IsActive, src => src.IsActive);

        // Map from EditPlanDTO to EditPlanViewModel
        config.NewConfig<EditPlanDTO, EditPlanViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DurationDays, src => src.DurationDays)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.IsActive, src => src.IsActive);

        // Map from EditPlanViewModel to EditPlanDTO
        config.NewConfig<EditPlanViewModel, EditPlanDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DurationDays, src => src.DurationDays)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }
}
