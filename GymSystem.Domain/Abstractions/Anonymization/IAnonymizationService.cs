namespace GymSystem.Domain.Abstractions.Anonymization;

public interface IAnonymizationService
{
    void Anonymize(object entity);
}
