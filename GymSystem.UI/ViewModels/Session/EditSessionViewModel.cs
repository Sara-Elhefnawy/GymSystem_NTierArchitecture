using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.UI.ViewModels.Session;

public class EditSessionViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Trainer is required")]
    public int TrainerId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Start Date & Time")]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "End Date & Time")]
    public DateTime EndDate { get; set; }

    public bool CanEdit { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }

    public SelectList? TrainerList { get; set; }

    public EditSessionViewModel()
    {
        TrainerList = new SelectList(new List<SelectListItem>());
    }
}
