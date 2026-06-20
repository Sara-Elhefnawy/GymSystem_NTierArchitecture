using AutoMapper;
using GymSystem.Domain.DTOs.Booking;
using GymSystem.Domain.DTOs.CheckIn;
using GymSystem.Domain.Entities;

namespace GymSystem.Domain.Mappings;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        // Map from Session to IndexBookingDTO
        CreateMap<Session, IndexBookingDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                src.Category != null ? src.Category.Name : "Uncategorized"))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                src.Description ?? string.Empty))
            .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src =>
                src.Trainer != null ? src.Trainer.Name : "Unassigned"))
            .ForMember(dest => dest.DateDisplay, opt => opt.MapFrom(src =>
                src.StartDate.ToString("dddd, MMM dd, yyyy")))
            .ForMember(dest => dest.TimeRangeDisplay, opt => opt.MapFrom(src =>
                $"{src.StartDate:hh:mm tt} - {src.EndDate:hh:mm tt}"))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src =>
                $"{src.EndDate.Subtract(src.StartDate).TotalMinutes} min"))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                GetSessionStatus(src)))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src =>
                src.Capacity - (src.Bookings != null ? src.Bookings.Count(b => !b.IsDeleted) : 0)));

        // Map from Booking to SessionInBookingDTO
        CreateMap<Booking, SessionInBookingDTO>()
            .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src =>
                src.Member != null ? src.Member.Name : "Unknown"))
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
            .ForMember(dest => dest.IsAttended, opt => opt.MapFrom(src => src.IsAttended))
            .ForMember(dest => dest.AttendanceMarkedAt, opt => opt.MapFrom(src => src.AttendanceMarkedAt))
            .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate));

        // Map from CreateBookingDTO to Booking
        CreateMap<CreateBookingDTO, Booking>()
            .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
            .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.IsAttended, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.AttendanceMarkedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Member, opt => opt.Ignore())
            .ForMember(dest => dest.Session, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // Map to ResultCheckInDTO (from multiple sources)
        CreateMap<Member, ResultCheckInDTO>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SessionName, opt => opt.Ignore())
            .ForMember(dest => dest.IsAlreadyAttended, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.WasAutoBooked, opt => opt.MapFrom(src => false));
    }

    // Helper method to calculate session status
    private static string GetSessionStatus(Session session)
    {
        var now = DateTime.Now;
        if (now < session.StartDate)
            return "Upcoming";
        else if (now >= session.StartDate && now <= session.EndDate)
            return "Ongoing";
        else
            return "Completed";
    }
}
