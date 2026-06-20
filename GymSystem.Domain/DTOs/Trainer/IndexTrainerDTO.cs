namespace GymSystem.Domain.DTOs.Trainer;

public class IndexTrainerDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Specialty { get; set; } = default!;
}
