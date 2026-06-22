using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class TrainerDTORegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from CreateTrainerDTO to Trainer
        config.NewConfig<CreateTrainerDTO, Trainer>()
            .Map(dest => dest.Name, (src => src.Name.Trim()))
            .Map(dest => dest.Email, (src => src.Email.Trim().ToLowerInvariant()))
            .Map(dest => dest.Phone, (src => src.Phone.Trim()))
            .Map(dest => dest.Gender, (src =>
                Enum.Parse<Gender>(src.Gender, true)))
            .Map(dest => dest.Specialty, (src =>
                Enum.Parse<Specialty>(src.Specialties, true)))
            .Ignore(dest => dest.Address);

        // Map from Trainer to IndexTrainerDTO
        config.NewConfig<Trainer, IndexTrainerDTO>()
            .Map(dest => dest.Specialty, src => src.Specialty)
            .Map(dest => dest.Name, (src => src.Name))
            .Map(dest => dest.Email, (src => src.Email))
            .Map(dest => dest.Phone, (src => src.Phone))
            .Map(dest => dest.Id, (src => src.Id));

        // Map from Trainer to DetailsTrainerDTO
        config.NewConfig<Trainer, DetailsTrainerDTO>()
            .Map(dest => dest.Specialty, (src => src.Specialty.ToString()))
            .Map(dest => dest.Name, (src => src.Name))
            .Map(dest => dest.Email, (src => src.Email))
            .Map(dest => dest.Phone, (src => src.Phone))
            .Map(dest => dest.Id, (src => src.Id))
            .Ignore(dest => dest.Address);

        // Map from Trainer to EditTrainerDTO
        config.NewConfig<Trainer, EditTrainerDTO>()
            .Map(dest => dest.Email, (src => src.Email))
            .Map(dest => dest.Phone, (src => src.Phone))
            .Map(dest => dest.Specialty, (src => src.Specialty.ToString()))
            .Ignore(dest => dest.BuildingNumber)
            .Ignore(dest => dest.City)
            .Ignore(dest => dest.Street);

        // Map from Trainer to DeleteTrainerDTO
        config.NewConfig<Trainer, DeleteTrainerDTO>()
            .Map(dest => dest.Name, (src => src.Name))
            .Map(dest => dest.Id, (src => src.Id));

        // Map from EditTrainerDTO to Trainer (for updates)
        config.NewConfig<EditTrainerDTO, Trainer>()
            .Map(dest => dest.Email, (src => src.Email.Trim().ToLowerInvariant()))
            .Map(dest => dest.Phone, (src => src.Phone.Trim()))
            .Map(dest => dest.Specialty, (src =>
                Enum.Parse<Specialty>(src.Specialty, true)))
            .Ignore(dest => dest.Address);
    }
}
