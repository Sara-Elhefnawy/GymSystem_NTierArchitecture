namespace GymSystem.Infrastructure.Entities;

public class Booking : BaseEntity
{
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public bool IsAttended { get; set; } = false;

    // Member must have an active membership to book    
    // Session must have available capacity(not full)
    // Member can't book the same session twice
    // Only future sessions can be booked
    // Only future bookings can be cancelled
    // Attendance can be marked only for ongoing sessions

    public int MemberId { get; set; }
    public Member Member { get; set; } = default!;

    public int SessionId { get; set; }
    public Session Session { get; set; } = default!;
}