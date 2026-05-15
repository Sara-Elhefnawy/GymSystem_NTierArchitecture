namespace GymSystem.Infrastructure.Entities;

public class CheckIn : BaseEntity
{
    public DateTime CheckInTime { get; set; }

    public int MemberId { get; set; }
    public virtual Member? Member { get; set; }
}