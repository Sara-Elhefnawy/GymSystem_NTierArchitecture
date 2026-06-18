using GymSystem.Domain.DTOs.Session.Lookups;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Domain.DTOs.Session;

public class CreateSessionDTO
{
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Trainer is required")]
    public int TrainerId { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = default!;

    [Required]
    [Range(1, 25)]
    public int Capacity { get; set; } = 25;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
