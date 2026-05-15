using GymSystem.Infrastructure.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Entities;

public class HealthRecord
{
    public decimal Height { get; set; }
    public decimal Weight { get; set; }

    public BloodType BloodType { get; set; }

    public string? Note { get; set; }
}