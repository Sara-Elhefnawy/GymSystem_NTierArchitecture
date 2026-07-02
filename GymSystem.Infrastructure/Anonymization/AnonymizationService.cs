using GymSystem.Domain.Abstractions.Anonymization;
using GymSystem.Domain.Entities;

namespace GymSystem.Infrastructure.Anonymization;

public class AnonymizationService : IAnonymizationService
{
    public void Anonymize(object entity)
    {
        switch (entity)
        {
            case Member member:
                if (!string.IsNullOrEmpty(member.Email))
                {
                    var anonymizedEmail = $"{member.Name}_deleted_{Guid.NewGuid():N}@anonymized.com";
                    member.Email = anonymizedEmail;

                    member.Phone = "01000000000";
                }
                break;

            case Trainer trainer:
                if (!string.IsNullOrEmpty(trainer.Email))
                {
                    var anonymizedEmail = $"{trainer.Name}_deleted_{Guid.NewGuid():N}@anonymized.com";
                    trainer.Email = anonymizedEmail;

                    trainer.Phone = "01000000000"; // valid-format placeholder, passes GymUser_PhoneCheck
                }
                break;
        }
    }
}
