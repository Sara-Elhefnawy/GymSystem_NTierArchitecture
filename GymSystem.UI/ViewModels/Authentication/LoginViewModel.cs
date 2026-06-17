using System.ComponentModel.DataAnnotations;

namespace GymSystem.UI.ViewModels.Authentication;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email Is Required")]
    [EmailAddress(ErrorMessage ="Invalid email")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password Is Required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;
    public bool RememberMe { get; set; }
}
