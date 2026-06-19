using AutoMapper;
using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;

namespace GymSystem.Domain.Mappings;

public class TrainerProfile : Profile
{
    public TrainerProfile()
    {
        // Map from CreateTrainerDTO to Trainer
        CreateMap<CreateTrainerDTO, Trainer>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLowerInvariant()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Trim()))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                Enum.Parse<Gender>(src.Gender, true)))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src =>
                Enum.Parse<Specialty>(src.Specialties, true)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
            {
                BuildingNumber = src.BuildingNumber,
                Street = src.Street.Trim(),
                City = src.City.Trim()
            }));

        // Map from Trainer to IndexTrainerDTO
        CreateMap<Trainer, IndexTrainerDTO>()
            .ForMember(dest => dest.Specialties, opt => opt.MapFrom(src => src.Specialty.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // Map from Trainer to DetailsTrainerDTO
        CreateMap<Trainer, DetailsTrainerDTO>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                $"{src.Address.BuildingNumber} - {src.Address.Street} - {src.Address.City}"))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialty.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // Map from Trainer to EditTrainerDTO
        CreateMap<Trainer, EditTrainerDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialty.ToString()));

        // Map from Trainer to DeleteTrainerDTO
        CreateMap<Trainer, DeleteTrainerDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // Map from EditTrainerDTO to Trainer (for updates)
        CreateMap<EditTrainerDTO, Trainer>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLowerInvariant()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Trim()))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src =>
                Enum.Parse<Specialty>(src.Specialty, true)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
            {
                BuildingNumber = src.BuildingNumber,
                Street = src.Street.Trim(),
                City = src.City.Trim()
            }))
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.Gender, opt => opt.Ignore());
    }
}
