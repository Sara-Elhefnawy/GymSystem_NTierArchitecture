namespace GymSystem.UI.ViewModels.Session;

public class DeleteSessionViewModel
{
    public int Id { get; set; }
    public string Specialty { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BookedCount { get; set; }
    public int MaxCapacity { get; set; }
    public string Status { get; set; } = default!;
    public bool CanDelete { get; set; }

    public string CapacityDisplay => $"{BookedCount} / {MaxCapacity} spots";
}
