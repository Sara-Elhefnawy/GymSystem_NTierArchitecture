namespace GymSystem.Domain.DTOs.Booking;

public class SessionInBookingDTO
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = default!;
    public int SessionId { get; set; }
    public DateTime? BookingDate { get; set; }
    public bool IsAttended { get; set; } = false;

    public DateTime? AttendanceMarkedAt { get; set; }
}
