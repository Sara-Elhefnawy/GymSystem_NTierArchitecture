using GymSystem.Domain.DTOs.Booking;
using GymSystem.Domain.DTOs.CheckIn;
using GymSystem.Domain.Entities;
using Mapster;

namespace GymSystem.Domain.Mappings;

public class BookingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from Session to IndexBookingDTO
        config.NewConfig<Session, IndexBookingDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CategoryName, src =>
                src.Category != null ? src.Category.Name : "Uncategorized")
            .Map(dest => dest.Description, src =>
                src.Description ?? string.Empty)
            .Map(dest => dest.TrainerName, src =>
                src.Trainer != null ? src.Trainer.Name : "Unassigned")
            .Map(dest => dest.DateDisplay, src =>
                src.StartDate.ToString("dddd, MMM dd, yyyy"))
            .Map(dest => dest.TimeRangeDisplay, src =>
                $"{src.StartDate:hh:mm tt} - {src.EndDate:hh:mm tt}")
            .Map(dest => dest.Duration, src =>
                $"{(int)(src.EndDate - src.StartDate).TotalMinutes} min")
            .Map(dest => dest.Capacity, src => src.Capacity)
            .Map(dest => dest.Status, src =>
                DateTime.Now < src.StartDate ? "Upcoming" :
                DateTime.Now >= src.StartDate && DateTime.Now <= src.EndDate ? "Ongoing" :
                "Completed")
            .Map(dest => dest.AvailableSlots, src =>
                src.Capacity - (src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0));

        // Map from Booking to SessionInBookingDTO
        config.NewConfig<Booking, SessionInBookingDTO>()
            .Map(dest => dest.MemberId, (src => src.MemberId))
            .Map(dest => dest.MemberName, (src =>
                src.Member != null ? src.Member.Name : "Unknown"))
            .Map(dest => dest.SessionId, (src => src.SessionId))
            .Map(dest => dest.IsAttended, (src => src.IsAttended))
            .Map(dest => dest.AttendanceMarkedAt, (src => src.AttendanceMarkedAt))
            .Map(dest => dest.BookingDate, (src => src.BookingDate));

        // Map from CreateBookingDTO to Booking
        config.NewConfig<CreateBookingDTO, Booking>()
            .Map(dest => dest.MemberId, (src => src.MemberId))
            .Map(dest => dest.SessionId, (src => src.SessionId))
            .Map(dest => dest.BookingDate, (src => DateTime.Now))
            .Map(dest => dest.IsAttended, (src => false));

        // Map to ResultCheckInDTO (from multiple sources)
        config.NewConfig<Member, ResultCheckInDTO>()
            .Map(dest => dest.MemberName, (src => src.Name))
            .Map(dest => dest.IsAlreadyAttended, (src => false))
            .Map(dest => dest.WasAutoBooked, (src => false));
    }
}
