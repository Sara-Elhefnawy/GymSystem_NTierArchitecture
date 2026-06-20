using AutoMapper;
using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.Entities;
using GymSystem.Domain.QueryService;

namespace GymSystem.Domain.Mappings;

public class SessionProfile : Profile
{
    public SessionProfile()
    {
        // ✅ Map from IndexSessionReadModel to IndexSessionDTO
        CreateMap<IndexSessionReadModel, IndexSessionDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.TrainerName))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.MaxCapacity))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.AvailableSlots))
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // Map from CreateSessionDTO to Session
        CreateMap<CreateSessionDTO, Session>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
            .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Trainer, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // ✅ Map from EditSessionDTO to Session (for updates)
        CreateMap<EditSessionDTO, Session>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            // These properties should not be updated via Edit
            .ForMember(dest => dest.Capacity, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Trainer, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Map from Session to IndexSessionDTO
        CreateMap<Session, IndexSessionDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src =>
                src.Trainer != null ? src.Trainer.Name : string.Empty))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src =>
                src.Capacity - (src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0)))
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // Map from Session to DetailsSessionDTO
        CreateMap<Session, DetailsSessionDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src =>
                src.Trainer != null ? src.Trainer.Name : string.Empty))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src =>
                src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0))
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // Map from Session to EditSessionDTO
        CreateMap<Session, EditSessionDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

        // Map from Session to DeleteSessionDTO
        CreateMap<Session, DeleteSessionDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src =>
                src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src =>
                src.Trainer != null ? src.Trainer.Name : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.BookedCount, opt => opt.MapFrom(src =>
                src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CanDelete, opt => opt.Ignore());
    }
}
