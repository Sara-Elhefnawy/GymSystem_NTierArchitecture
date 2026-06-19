using AutoMapper;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;

namespace GymSystem.Domain.Mappings;

public class MemberProfile : Profile
{
    public MemberProfile()
    {
        // Map from CreateMemberDTO to Member
        CreateMap<CreateMemberDTO, Member>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLowerInvariant()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Trim()))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                Enum.Parse<Gender>(src.Gender, true)))
            .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(DateTime.UtcNow)))
            .ForMember(dest => dest.Photo, opt => opt.Ignore())
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
            {
                BuildingNumber = src.BuildingNumber,
                Street = src.Street.Trim(),
                City = src.City.Trim()
            }))
            // AutoMapper will use HealthRecordProfile to map this automatically
            .ForMember(dest => dest.HealthRecord, opt => opt.MapFrom(src => src.HealthRecord));

        // Map from Member to IndexMemberDTO
        CreateMap<Member, IndexMemberDTO>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()));

        // Map from Member to DetailsMemberDTO
        CreateMap<Member, DetailsMemberDTO>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                $"{src.Address.BuildingNumber} - {src.Address.Street} - {src.Address.City}"))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src =>
                src.Memberships.OrderByDescending(m => m.StartDate).FirstOrDefault() != null
                    ? src.Memberships.OrderByDescending(m => m.StartDate).First().Plan.Name
                    : "No Plan"))
            .ForMember(dest => dest.MembershipStartDate, opt => opt.MapFrom(src =>
                src.Memberships.OrderByDescending(m => m.StartDate).FirstOrDefault() != null
                    ? src.Memberships.OrderByDescending(m => m.StartDate).First().StartDate
                    : (DateOnly?)null))
            .ForMember(dest => dest.MembershipEndDate, opt => opt.MapFrom(src =>
                src.Memberships.OrderByDescending(m => m.StartDate).FirstOrDefault() != null
                    ? src.Memberships.OrderByDescending(m => m.StartDate).First().EndDate
                    : (DateOnly?)null));

        // Map from Member to EditMemberDTO
        CreateMap<Member, EditMemberDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City));

        // Map from Member to DeleteMemberDTO
        CreateMap<Member, DeleteMemberDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo));

        // Map from EditMemberDTO to Member
        CreateMap<EditMemberDTO, Member>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLowerInvariant()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Trim()))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
            {
                BuildingNumber = src.BuildingNumber,
                Street = src.Street.Trim(),
                City = src.City.Trim()
            }))
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.Gender, opt => opt.Ignore())
            .ForMember(dest => dest.Photo, opt => opt.Ignore())
            .ForMember(dest => dest.JoinDate, opt => opt.Ignore())
            .ForMember(dest => dest.HealthRecord, opt => opt.Ignore())
            .ForMember(dest => dest.Memberships, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore());
    }
}
