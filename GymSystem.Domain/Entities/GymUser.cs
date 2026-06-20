using GymSystem.Domain.Entities.Enums;

namespace GymSystem.Domain.Entities;

public abstract class GymUser : BaseEntity
{
    public string Name { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public DateOnly DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    public Address Address { get; set; } = default!;
}
