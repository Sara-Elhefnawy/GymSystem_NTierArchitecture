using GymSystem.Domain.DTOs.Session.Enums;

namespace GymSystem.Domain.DTOs.Session;

public class DetailsSessionDTO
{
    public string CategoryName { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string TrainerName { get; init; } = null!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int AvailableSlots { get; init; }
    public int MaxCapacity { get; init; }
    public SessionStatus Status { get; init; }
}
