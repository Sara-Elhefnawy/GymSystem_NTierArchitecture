namespace GymSystem.Domain.DTOs.Authentication;

public class LoginDTO
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; } = default!;
}
