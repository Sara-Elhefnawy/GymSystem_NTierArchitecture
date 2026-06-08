using GymSystem.Infrastructure.Entities.Enums;

namespace GymSystem.Infrastructure.Entities;

public class HealthRecord : BaseEntity
{
    public decimal Height { get; set; }
    public decimal Weight { get; set; }

    public BloodType BloodType { get; set; }

    public string? Note { get; set; }

    public DateTime LastUpdate { get; set; }


    public int MemberId { get; set; }
    public Member Member { get; set; } = default!;

}