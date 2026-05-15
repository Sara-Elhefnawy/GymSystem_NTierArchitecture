namespace GymSystem.Infrastructure.Entities;

public class Membership : BaseEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }  // StartDate + Plan.DurationDays

    // A member can have only ONE active membership at a time
    // Only active plans can be assigned
    // Status is computed: Active if EndDate > Now, else Expired
    // Cancellation deletes the membership record

    public int MemberId { get; set; }
    public Member Member { get; set; } = default!;

    public int PlanId { get; set; }
    public Plan Plan { get; set; } = default!;
}