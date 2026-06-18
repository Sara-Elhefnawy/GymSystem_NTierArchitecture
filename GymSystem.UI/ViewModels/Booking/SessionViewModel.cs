namespace GymSystem.UI.ViewModels.Booking;

public class SessionViewModel
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public string DateDisplay { get; set; } = default!;
    public string TimeRangeDisplay { get; set; } = default!;
    public string Duration { get; set; } = default!;
    public int Capacity { get; set; }
    public int AvailableSlots { get; set; }
    public string Status { get; set; } = default!;
}
