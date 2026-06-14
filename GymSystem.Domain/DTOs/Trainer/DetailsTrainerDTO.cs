namespace GymSystem.Domain.DTOs.Trainer;

public class DetailsTrainerDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Photo { get; set; }

    public string Specialty { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public DateOnly DateOfBirth { get; set; }

    public string Address { get; set; } = default!;
}
