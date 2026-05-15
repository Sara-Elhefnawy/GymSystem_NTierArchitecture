namespace GymSystem.Infrastructure.Entities;

public class Session : BaseEntity
{
    public string Description { get; set; } = default!;

    public int Capacity { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;

    public DateTime EndDate { get; set; }

    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = default!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public ICollection<Booking> Bookings { get; set; } = [];
}