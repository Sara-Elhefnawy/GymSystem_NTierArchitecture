namespace GymSystem.Domain.DTOs.Memberships;

public class IndexMembershipDTO
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = default!;
    public int PlanId { get; set; }
    public string PlanName { get; set; } = default!;

    public string? Photo { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; }
}
