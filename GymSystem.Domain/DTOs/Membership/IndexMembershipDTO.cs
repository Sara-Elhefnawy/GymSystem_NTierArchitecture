namespace GymSystem.Domain.DTOs.Membership;

public class IndexMembershipDTO
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = default!;
    public int PlanId { get; set; }
    public string PlanName { get; set; } = default!;

    public string? Photo { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
