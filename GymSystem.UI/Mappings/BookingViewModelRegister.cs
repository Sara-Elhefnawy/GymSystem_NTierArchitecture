using GymSystem.Domain.DTOs.Booking;
using GymSystem.UI.ViewModels.Booking;
using Mapster;

namespace GymSystem.UI.Mappings;

public class BookingViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from IndexBookingDTO to IndexBookingViewModel
        config.NewConfig<IndexBookingDTO, IndexBookingViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CategoryName, src => src.CategoryName)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.TrainerName, src => src.TrainerName)
            .Map(dest => dest.DateDisplay, src => src.DateDisplay)
            .Map(dest => dest.TimeRangeDisplay, src => src.TimeRangeDisplay)
            .Map(dest => dest.Duration, src => src.Duration)
            .Map(dest => dest.Capacity, src => src.Capacity)
            .Map(dest => dest.AvailableSlots, src => src.AvailableSlots)
            .Map(dest => dest.Status, src => src.Status);

        // Map from SessionInBookingDTO to SessionInBookingViewModel
        config.NewConfig<SessionInBookingDTO, SessionInBookingViewModel>()
            .Map(dest => dest.MemberId, src => src.MemberId)
            .Map(dest => dest.MemberName, src => src.MemberName)
            .Map(dest => dest.SessionId, src => src.SessionId)
            .Map(dest => dest.IsAttended, src => src.IsAttended)
            .Map(dest => dest.BookingDate, src => src.BookingDate);

        // Map from CreateBookingViewModel to CreateBookingDTO
        config.NewConfig<CreateBookingViewModel, CreateBookingDTO>()
            .Map(dest => dest.SessionId, src => src.SessionId)
            .Map(dest => dest.MemberId, src => src.MemberId);
    }
}
