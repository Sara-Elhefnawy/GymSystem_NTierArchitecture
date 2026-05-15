namespace GymSystem.Infrastructure.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;  // Plans are soft-deleted via IsActive = false

    // Cannot update or deactivate a plan with active memberships

    public ICollection<Membership> Memberships { get; set; } = [];
}
