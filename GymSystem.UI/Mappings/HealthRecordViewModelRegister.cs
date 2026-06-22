using GymSystem.Domain.DTOs.HealthRecord;
using Mapster;

namespace GymSystem.UI.Mappings;

public class HealthRecordViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from CreateHealthRecordViewModel to CreateHealthRecordDTO
        config.NewConfig<CreateHealthRecordViewModel, CreateHealthRecordDTO>()
            .Map(dest => dest.BloodType, src => src.BloodType)
            .Map(dest => dest.Height, src => src.Height)
            .Map(dest => dest.Weight, src => src.Weight)
            .Map(dest => dest.Notes, src => src.Note ?? string.Empty);

        // Map from DetailsHealthRecordDTO to DetailsHealthRecordViewModel
        config.NewConfig<DetailsHealthRecordDTO, DetailsHealthRecordViewModel>()
            .Map(dest => dest.BloodType, src => src.BloodType)
            .Map(dest => dest.Height, src => src.Height)
            .Map(dest => dest.Weight, src => src.Weight)
            .Map(dest => dest.Notes, src => string.IsNullOrWhiteSpace(src.Notes) ? "No notes available" : src.Notes);
    }
}
