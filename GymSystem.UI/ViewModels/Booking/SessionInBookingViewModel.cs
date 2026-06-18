namespace GymSystem.UI.ViewModels.Booking;

public class SessionInBookingViewModel
{
    public int MemberId { get; set; }
    public int SessionId { get; set; }
    public string MemberName { get; set; } = default!;
    public DateTime? BookingDate { get; set; }
    public bool IsAttended { get; set; } = false;

    public DateTime? AttendanceMarkedAt { get; set; }
}
