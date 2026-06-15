using GymSystem.Infrastructure.Entities.Enums;

namespace GymSystem.Infrastructure.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = default!;

    public Specialty RequiredSpecialty { get; set; }

    public ICollection<Session> Sessions { get; set; } = [];
}
