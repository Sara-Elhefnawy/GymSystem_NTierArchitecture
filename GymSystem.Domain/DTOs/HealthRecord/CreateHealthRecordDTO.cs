using System.ComponentModel.DataAnnotations;

namespace GymSystem.Domain.DTOs.HealthRecord;

public class CreateHealthRecordDTO
{
    [Range(0.1, 300, ErrorMessage = "Height must be greater than 0 and less than 300")] 
    public decimal Height { get; set; }

    [Range(0.1, 500, ErrorMessage = "Weight must be greater than 0 and less than 500")] 
    public decimal Weight { get; set; }

    [Required(ErrorMessage = "Blood Type is required")]
    [StringLength(15, ErrorMessage = "Blood Type must be at most 15 characters")]
    public string BloodType { get; set; } = default!;

    [MaxLength(500)] 
    public string? Note { get; set; }
}