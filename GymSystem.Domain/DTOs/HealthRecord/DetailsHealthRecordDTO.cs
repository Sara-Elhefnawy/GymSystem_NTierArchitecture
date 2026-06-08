namespace GymSystem.Domain.DTOs.HealthRecord;

public class DetailsHealthRecordDTO
{
    public decimal Height { get; set; }

    public decimal Weight { get; set; }

    public string BloodType { get; set; } = default!;

    public string? Notes { get; set; } = default!;
}
