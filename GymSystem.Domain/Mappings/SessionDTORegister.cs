using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.Entities;
using GymSystem.Domain.QueryService;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class SessionDTORegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from IndexSessionReadModel to IndexSessionDTO
        config.NewConfig<IndexSessionReadModel, IndexSessionDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.CategoryName, (src => src.CategoryName))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.TrainerName, (src => src.TrainerName))
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate))
            .Map(dest => dest.MaxCapacity, (src => src.MaxCapacity))
            .Map(dest => dest.AvailableSlots, (src => src.AvailableSlots));

        // Map from CreateSessionDTO to Session
        config.NewConfig<CreateSessionDTO, Session>()
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate))
            .Map(dest => dest.Capacity, (src => src.Capacity))
            .Map(dest => dest.CategoryId, (src => src.CategoryId))
            .Map(dest => dest.Description, (src => src.Description.Trim()))
            .Map(dest => dest.TrainerId, (src => src.TrainerId));

        // Map from EditSessionDTO to Session (for updates)
        config.NewConfig<EditSessionDTO, Session>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.TrainerId, (src => src.TrainerId))
            .Map(dest => dest.Description, (src => src.Description.Trim()))
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate));

        // Map from Session to IndexSessionDTO
        config.NewConfig<Session, IndexSessionDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.CategoryName, (src =>
                src.Category != null ? src.Category.Name : string.Empty))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.TrainerName, (src =>
                src.Trainer != null ? src.Trainer.Name : string.Empty))
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate))
            .Map(dest => dest.MaxCapacity, (src => src.Capacity))
            .Map(dest => dest.AvailableSlots, (src =>
                src.Capacity - (src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0)));

        // Map from Session to DetailsSessionDTO
        config.NewConfig<Session, DetailsSessionDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.CategoryName, (src =>
                src.Category != null ? src.Category.Name : string.Empty))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.TrainerName, (src =>
                src.Trainer != null ? src.Trainer.Name : string.Empty))
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate))
            .Map(dest => dest.Capacity, (src => src.Capacity))
            .Map(dest => dest.AvailableSlots, (src =>
                src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0));

        // Map from Session to EditSessionDTO
        config.NewConfig<Session, EditSessionDTO>()
            .Map(dest => dest.Id,  (src => src.Id))
            .Map(dest => dest.TrainerId,  (src => src.TrainerId))
            .Map(dest => dest.Description,  (src => src.Description))
            .Map(dest => dest.StartDate,  (src => src.StartDate))
            .Map(dest => dest.EndDate,  (src => src.EndDate));

        // Map from Session to DeleteSessionDTO
        config.NewConfig<Session, DeleteSessionDTO>()
            .Map(dest => dest.Id, (src => src.Id))
            .Map(dest => dest.Specialty, (src =>
                src.Category != null ? src.Category.Name : string.Empty))
            .Map(dest => dest.TrainerName, (src =>
                src.Trainer != null ? src.Trainer.Name : string.Empty))
            .Map(dest => dest.Description, (src => src.Description))
            .Map(dest => dest.StartDate, (src => src.StartDate))
            .Map(dest => dest.EndDate, (src => src.EndDate))
            .Map(dest => dest.BookedCount, (src =>
                src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0))
            .Map(dest => dest.MaxCapacity, (src => src.Capacity));
    }
}
