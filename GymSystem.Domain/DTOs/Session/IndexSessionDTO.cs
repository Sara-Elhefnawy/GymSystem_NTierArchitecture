namespace GymSystem.Domain.DTOs.Session;

public class IndexSessionDTO
{
    public int Id { get; set; }

    public string Specialty { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string TrainerName { get; set; } = default!;

    public string StartDate { get; set; } = default!;

    public TimeSpan TimeRange { get; set; }

    public string Duration { get; set; } = default!;

    public int Capacity { get; set; }

    public int AvailableSlots { get; set; }

    public string Status { get; set; } = default!;
}
