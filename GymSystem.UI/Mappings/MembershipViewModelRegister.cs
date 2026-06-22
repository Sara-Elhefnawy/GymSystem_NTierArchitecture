using GymSystem.Domain.DTOs.Membership;
using GymSystem.UI.ViewModels.Memberships;
using Mapster;

namespace GymSystem.UI.Mappings;

public class MembershipViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from IndexMembershipDTO to IndexMembershipViewModel
        config.NewConfig<IndexMembershipDTO, IndexMembershipViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.MemberId, src => src.MemberId)
            .Map(dest => dest.MemberName, src => src.MemberName)
            .Map(dest => dest.PlanName, src => src.PlanName)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Photo, src => src.Photo);

        // Map from CreateMembershipViewModel to CreateMembershipDTO
        config.NewConfig<CreateMembershipViewModel, CreateMembershipDTO>()
            .Map(dest => dest.MemberId, src => src.MemberId)
            .Map(dest => dest.PlanId, src => src.PlanId);
    }
}
