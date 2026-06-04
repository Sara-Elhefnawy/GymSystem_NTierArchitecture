using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Services;

public class AnonymizationService : IAnonymizationService
{
    public void Anonymize(object entity)
    {
        switch (entity)
        {
            case Member member:
                if (!string.IsNullOrEmpty(member.Email))
                    member.Email = $"{member.Id}_deleted_{Guid.NewGuid():N}";

                break;

            case Trainer trainer:
                if (!string.IsNullOrEmpty(trainer.Email))
                    trainer.Email = $"{trainer.Id}_deleted_{Guid.NewGuid():N}";

                break;
        }
    }
}
