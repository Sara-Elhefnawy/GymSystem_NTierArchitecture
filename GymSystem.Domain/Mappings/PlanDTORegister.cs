using GymSystem.Domain.DTOs.Plan;
using GymSystem.Domain.Entities;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class PlanDTORegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from Plan to IndexPlanDTO
        config.NewConfig<Plan, IndexPlanDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.Name, (src => src.Name))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.DurationDays, (src => src.DurationDays))
            .Map(dest => dest.IsActive, (src => src.IsActive))
            .Map(dest => dest.Price, (src => src.Price));

        // Map from Plan to DetailsPlanDTO
        config.NewConfig<Plan, DetailsPlanDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.Name, (src => src.Name))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.DurationDays, (src => src.DurationDays))
            .Map(dest => dest.IsActive, (src => src.IsActive))
            .Map(dest => dest.Price, (src => src.Price));

        // Map from Plan to EditPlanDTO
        config.NewConfig<Plan, EditPlanDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.Name, (src => src.Name))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.DurationDays, (src => src.DurationDays))
            .Map(dest => dest.Price, (src => src.Price))
            .Map(dest => dest.IsActive, (src => src.IsActive));

        // Map from EditPlanDTO to Plan (for updates)
        config.NewConfig<EditPlanDTO, Plan>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description.Trim())
            .Map(dest => dest.DurationDays, src => src.DurationDays)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }
}
