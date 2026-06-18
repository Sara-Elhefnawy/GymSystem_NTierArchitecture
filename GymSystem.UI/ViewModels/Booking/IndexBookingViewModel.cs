namespace GymSystem.UI.ViewModels.Booking;

public class IndexBookingViewModel
{
    //public int SessionId { get; set; }
    public string? SessionName { get; set; }
    //public int MemberId { get; set; }
    public string MemberName { get; set; } = default!;

    public DateTime BookingDate { get; set; }

    public bool IsAttended { get; set; }
}
