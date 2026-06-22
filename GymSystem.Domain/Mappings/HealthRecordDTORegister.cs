using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class HealthRecordDTORegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from CreateHealthRecordDTO to HealthRecord
        config.NewConfig<CreateHealthRecordDTO, HealthRecord>()
            .Map(dest => dest.BloodType, src => Enum.Parse<BloodType>(src.BloodType, true))
            .Map(dest => dest.Weight, src => src.Weight)
            .Map(dest => dest.Height, src => src.Height)
            .Map(dest => dest.Note, src => src.Notes ?? string.Empty)
            .Map(dest => dest.LastUpdate, src => DateTime.UtcNow)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.MemberId)
            .Ignore(dest => dest.Member)
            .Ignore(dest => dest.IsDeleted);

        // Map from HealthRecord to DetailsHealthRecordDTO
        config.NewConfig<HealthRecord, DetailsHealthRecordDTO>()
            .Map(dest => dest.BloodType, src => src.BloodType.ToString())
            .Map(dest => dest.Height, src => src.Height)
            .Map(dest => dest.Weight, src => src.Weight)
            .Map(dest => dest.Notes, src =>
                string.IsNullOrWhiteSpace(src.Note) ? "No notes available" : src.Note);
    }
}
