using GymSystem.Domain.DTOs.Membership;
using GymSystem.Domain.Entities;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class MembershipRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from Membership to IndexMembershipDTO
        config.NewConfig<Membership, IndexMembershipDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.MemberId, (src => src.MemberId))
            .Map(dest => dest.MemberName, (src =>
                src.Member != null ? src.Member.Name : "Unknown"))
            .Map(dest => dest.PlanName, (src =>
                src.Plan != null ? src.Plan.Name : "Unknown"))
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate))
            .Map(dest => dest.Photo, (src =>
                src.Member != null ? src.Member.Photo : null));

        // Map from CreateMembershipDTO to Membership
        config.NewConfig<CreateMembershipDTO, Membership>()
            .Map(dest => dest.MemberId, (src => src.MemberId))
            .Map(dest => dest.PlanId, (src => src.PlanId))
            .Map(dest => dest.StartDate, (src => DateOnly.FromDateTime(DateTime.Now)));
    }
}
