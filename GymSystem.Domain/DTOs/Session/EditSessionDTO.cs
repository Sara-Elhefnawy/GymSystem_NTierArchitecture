namespace GymSystem.Domain.DTOs.Session;

public class EditSessionDTO
{
    public int Id { get; set; }

    public int TrainerId { get; set; }

    public string Description { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool CanEdit { get; set; } = true;
}
