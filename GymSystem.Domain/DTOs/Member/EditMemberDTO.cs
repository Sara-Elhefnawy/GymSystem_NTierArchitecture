using System.ComponentModel.DataAnnotations;

namespace GymSystem.Domain.DTOs.Member;

public class EditMemberDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Photo { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [RegularExpression(@"^(010|011|012|015)\d{8}$",
        ErrorMessage = "Invalid Egyptian phone number")]
    public string Phone { get; set; } = default!;

    [Required] 
    public int BuildingNumber { get; set; }

    [Required] 
    public string Street { get; set; } = default!;

    [Required] 
    public string City { get; set; } = default!;
}
