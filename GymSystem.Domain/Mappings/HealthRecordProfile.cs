using AutoMapper;
using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;

namespace GymSystem.Domain.Mappings;

public class HealthRecordProfile : Profile
{
    public HealthRecordProfile()
    {
        // Map from CreateHealthRecordDTO to HealthRecord
        CreateMap<CreateHealthRecordDTO, HealthRecord>()
            .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src =>
                Enum.Parse<BloodType>(src.BloodType, true)))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
            .ForMember(dest => dest.Note, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Notes) ? null : src.Notes.Trim()))
            .ForMember(dest => dest.LastUpdate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MemberId, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore());

        // Map from HealthRecord to DetailsHealthRecordDTO
        CreateMap<HealthRecord, DetailsHealthRecordDTO>()
            .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src => src.BloodType.ToString()))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Note) ? "No notes available" : src.Note));
    }
}
