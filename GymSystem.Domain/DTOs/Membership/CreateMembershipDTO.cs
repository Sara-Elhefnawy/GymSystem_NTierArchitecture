namespace GymSystem.Domain.DTOs.Memberships;

public class CreateMembershipDTO
{
    public int PlanId { get; set; }
    public int MemberId { get; set; }
    public DateTime? StartDate { get; set; }
}
