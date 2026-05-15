using GymSystem.Infrastructure.Entities.Enums;

namespace GymSystem.Infrastructure.Entities;

public class Trainer : GymUser
{
    public Specialty Specialty { get; set; }

    public DateTime HireDate { get; set; } = DateTime.Now;

    // Cannot delete a trainer who has future sessions
    // Specialty drives which sessions they can lead

    public ICollection<Session> Sessions { get; set; } = [];
}
