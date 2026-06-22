using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.UI.ViewModels.Member;
using Mapster;

namespace GymSystem.UI.Mappings;

public class MemberViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
         // Map from IndexMemberDTO to IndexMemberViewModel
        config.NewConfig<IndexMemberDTO, IndexMemberViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Photo, src => src.Photo)
            .Map(dest => dest.Gender, src => src.Gender);

        // Map from CreateMemberViewModel to CreateMemberDTO
        config.NewConfig<CreateMemberViewModel, CreateMemberDTO>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.Street, src => src.Street)
            .Map(dest => dest.Photo, src => src.Photo)
            .Map(dest => dest.HealthRecord, src => src.HealthRecord.Adapt<CreateHealthRecordDTO>());

        // Map from DetailsMemberDTO to DetailsMemberViewModel
        config.NewConfig<DetailsMemberDTO, DetailsMemberViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Photo, src => src.Photo)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.MembershipStartDate, src => src.MembershipStartDate)
            .Map(dest => dest.MembershipEndDate, src => src.MembershipEndDate)
            .Map(dest => dest.PlanName, src => src.PlanName)
            .Map(dest => dest.Address, src => src.Address);

         // Map from EditMemberViewModel to EditMemberDTO
        config.NewConfig<EditMemberViewModel, EditMemberDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.Street, src => src.Street);

         // Map from DeleteMemberDTO to DeleteMemberViewModel
        config.NewConfig<DeleteMemberDTO, DeleteMemberViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Photo, src => src.Photo);
    }
}
