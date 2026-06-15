using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.UI.ViewModels.Session;

public class CreateSessionViewModel
{
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Trainer is required")]
    public int TrainerId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = default!;

    [Range(1, 25, ErrorMessage = "Capacity must be between 1 and 25")]
    public int Capacity { get; set; } = 25;

    public DateTime StartDate { get; set; } = DateTime.Now;

    public DateTime EndDate { get; set; } = DateTime.Now.AddHours(1);

    public SelectList? CategoryList { get; set; }
    public SelectList? TrainerList { get; set; }
}
