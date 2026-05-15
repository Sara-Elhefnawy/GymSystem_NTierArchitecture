using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Entities;

[Owned]
public class Address
{
    public int BuildingNumber { get; set; }

    public string Street { get; set; } = default!;

    public string City { get; set; } = default!;
}