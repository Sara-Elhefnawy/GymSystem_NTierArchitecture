using AutoMapper;
using GymSystem.Domain.DTOs.Membership;
using GymSystem.Infrastructure.Entities;

namespace GymSystem.Domain.Mappings;

public class MembershipProfile : Profile
{
    public MembershipProfile()
    {
        // Map from Membership to IndexMembershipDTO
        CreateMap<Membership, IndexMembershipDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src =>
                src.Member != null ? src.Member.Name : "Unknown"))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src =>
                src.Plan != null ? src.Plan.Name : "Unknown"))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src =>
                src.Member != null ? src.Member.Photo : null));

        // Map from CreateMembershipDTO to Membership
        CreateMap<CreateMembershipDTO, Membership>()
            .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
            .ForMember(dest => dest.PlanId, opt => opt.MapFrom(src => src.PlanId))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(DateTime.Now)))
            .ForMember(dest => dest.EndDate, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
