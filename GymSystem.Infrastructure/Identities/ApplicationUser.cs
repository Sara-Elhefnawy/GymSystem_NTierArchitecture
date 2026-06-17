using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Infrastructure.Identities;

public class ApplicationUser : IdentityUser<int>
{
    [StringLength(100)]
    public string FullName { get; set; } = default!;
}
