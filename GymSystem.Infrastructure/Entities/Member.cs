namespace GymSystem.Infrastructure.Entities;

public class Member : GymUser
{
    public string? Photo { get; set; }   // Required at registration

    public DateTime JoinDate { get; set; } = DateTime.Now;

    // Cannot delete a member who has active bookings

    public HealthRecord? HealthRecord { get; set; }   // Health record is required at registration

    public ICollection<Membership> Memberships { get; set; } = [];

    public ICollection<Booking> Bookings { get; set; } = [];
}
