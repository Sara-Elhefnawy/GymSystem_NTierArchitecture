using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.Infrastructure.Identities;

public class ApplicationRole : IdentityRole<int>
{
    [StringLength(50)]
    public string DisplayName { get; set; } = default!;
}
