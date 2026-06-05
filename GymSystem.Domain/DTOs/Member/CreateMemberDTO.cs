using GymSystem.Domain.DTOs.HealthRecord;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Domain.DTOs.Member;

public class CreateMemberDTO
{
    [Required(ErrorMessage = "Name is requiqred")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage ="Name can only contain letters and spaces")]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage ="invalid email format")]
    [MaxLength(100)]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Invalid Egyptian phone number")]
    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; } = default!;

    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = default!;

    [Required(ErrorMessage = "Building number is required")]
    [Range(1, 9000, ErrorMessage ="Building Numbers must be greater than 0")]
    public int BuildingNumber { get; set; }

    [Required(ErrorMessage = "Street is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage ="Street must be between 2 and 100")]
    [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Street can only contain letters and spaces")]
    public string Street { get; set; } = default!;

    [Required(ErrorMessage = "City is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage ="City must be between 2 and 100")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "City can only contain letters and spaces")]
    public string City { get; set; } = default!;


    [Required(ErrorMessage = "HealthRecord is required")]
    public CreateHealthRecordDTO HealthRecord { get; set; } = new CreateHealthRecordDTO();

}
