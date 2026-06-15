namespace GymSystem.Infrastructure.QueryService;

public class IndexSessionReadModel
{
    public int Id { get; init; }

    public string Description { get; init; } = default!;

    public string CategoryName { get; init; } = default!;
    public string TrainerName { get; init; } = default!;

    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public int MaxCapacity { get; set; }
    public int AvailableSlots { get; set; }

    public string Status { get; init; } = default!;
}
