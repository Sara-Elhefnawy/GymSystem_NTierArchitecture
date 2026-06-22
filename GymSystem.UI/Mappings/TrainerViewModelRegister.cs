using GymSystem.Domain.DTOs.Trainer;
using GymSystem.UI.ViewModels.Trainer;
using Mapster;

namespace GymSystem.UI.Mappings;

public class TrainerViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from Map from IndexTrainerDTO to IndexTrainerViewModel
        config.NewConfig<IndexTrainerDTO, IndexTrainerViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Specialty, src => src.Specialty.ToString());

        // Map from Map from CreateTrainerViewModel to CreateTrainerDTO
        config.NewConfig<CreateTrainerViewModel, CreateTrainerDTO>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.Street, src => src.Street)
            .Map(dest => dest.Specialties, src => src.Specialties);

        // Map from Map from DetailsTrainerDTO to DetailsTrainerViewModel
        config.NewConfig<DetailsTrainerDTO, DetailsTrainerViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Specialty, src => src.Specialty);

        // Map from Map from EditTrainerViewModel to EditTrainerDTO
        config.NewConfig<EditTrainerViewModel, EditTrainerDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.Street, src => src.Street)
            .Map(dest => dest.Specialty, src => src.Specialty);

        // Map from Map from DeleteTrainerDTO to DeleteTrainerViewModel
        config.NewConfig<DeleteTrainerDTO, DeleteTrainerViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);
    }
}
