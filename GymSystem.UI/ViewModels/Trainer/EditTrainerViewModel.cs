using System.ComponentModel.DataAnnotations;

namespace GymSystem.UI.ViewModels.Trainer;

public class EditTrainerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    [EmailAddress]
    public string Email { get; set; } = default!;

    [RegularExpression(@"^(010|011|012|015)\d{8}$",
        ErrorMessage = "Invalid Egyptian phone number")]
    public string Phone { get; set; } = default!;

    public int BuildingNumber { get; set; }

    public string Street { get; set; } = default!;

    public string City { get; set; } = default!;

    public string Specialty { get; set; } = default!;
}
