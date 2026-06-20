using GymSystem.Domain.DTOs.Session.Enums;

namespace GymSystem.Domain.DTOs.Session;

public class DetailsSessionDTO
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AvailableSlots { get; set; }
    public int Capacity { get; set; }
    public SessionStatus Status { get; set; }
}
