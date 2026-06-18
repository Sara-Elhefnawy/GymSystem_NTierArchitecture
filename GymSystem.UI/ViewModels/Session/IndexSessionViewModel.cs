using GymSystem.Domain.DTOs.Session.Enums;

namespace GymSystem.UI.ViewModels.Session;

public class IndexSessionViewModel
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string TrainerName { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int MaxCapacity { get; set; }
    public int AvailableSlots { get; set; }

    public SessionStatus Status { get; set; }

    public TimeSpan Duration { get; set; }

    public string HeaderClass => Status switch
    {
        SessionStatus.Upcoming => "bg-primary-color",
        SessionStatus.Ongoing => "bg-success",
        SessionStatus.Completed => "bg-secondary",
        _ => "bg-secondary"
    };
}
