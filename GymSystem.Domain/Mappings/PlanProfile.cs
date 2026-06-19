using AutoMapper;
using GymSystem.Domain.DTOs.Plan;
using GymSystem.Infrastructure.Entities;

namespace GymSystem.Domain.Mappings;

public class PlanProfile : Profile
{
    public PlanProfile()
    {
        // Map from Plan to IndexPlanDTO
        CreateMap<Plan, IndexPlanDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.DurationDays, opt => opt.MapFrom(src => src.DurationDays))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

        // Map from Plan to DetailsPlanDTO
        CreateMap<Plan, DetailsPlanDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.DurationDays, opt => opt.MapFrom(src => src.DurationDays))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

        // Map from Plan to EditPlanDTO
        CreateMap<Plan, EditPlanDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.DurationDays, opt => opt.MapFrom(src => src.DurationDays))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        // Map from EditPlanDTO to Plan (for updates)
        CreateMap<EditPlanDTO, Plan>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
            .ForMember(dest => dest.DurationDays, opt => opt.MapFrom(src => src.DurationDays))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Memberships, opt => opt.Ignore());
    }
}
