using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class MemberDTORegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from CreateMemberDTO to Member
        config.NewConfig<CreateMemberDTO, Member>()
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Email, src => src.Email.Trim().ToLowerInvariant())
            .Map(dest => dest.Phone, src => src.Phone.Trim())
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Gender, src => Enum.Parse<Gender>(src.Gender, true))
            .Map(dest => dest.JoinDate, src => DateOnly.FromDateTime(DateTime.UtcNow))
            .Map(dest => dest.HealthRecord, src => src.HealthRecord)
            .Ignore(dest => dest.Address);

        // Map from Member to IndexMemberDTO
        config.NewConfig<Member, IndexMemberDTO>()
            .Map(dest => dest.Gender, src => src.Gender.ToString());

        // Map from Member to DetailsMemberDTO - FIXED
        config.NewConfig<Member, DetailsMemberDTO>()
            .Map(dest => dest.Gender, src => src.Gender.ToString())
            .Ignore(dest => dest.Address)
            .Ignore(dest => dest.MembershipStartDate!)
            .Ignore(dest => dest.MembershipEndDate!);


        // Map from Member to EditMemberDTO
        config.NewConfig<Member, EditMemberDTO>()
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Ignore(dest => dest.BuildingNumber)
            .Ignore(dest => dest.City)
            .Ignore(dest => dest.Street);

        // Map from Member to DeleteMemberDTO
        config.NewConfig<Member, DeleteMemberDTO>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Photo, src => src.Photo);

        // Map from EditMemberDTO to Member
        config.NewConfig<EditMemberDTO, Member>()
            .Map(dest => dest.Email, src => src.Email.Trim().ToLowerInvariant())
            .Map(dest => dest.Phone, src => src.Phone.Trim())
            .Ignore(dest => dest.Address);
    }
}
