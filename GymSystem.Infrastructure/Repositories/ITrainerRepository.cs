using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public interface ITrainerRepository : IRepository<Trainer>
{
    Task<bool> IsEmailTakenAsync(string normalizedEmail, int? excludeMemberId = null, CancellationToken ct = default);
    Task<bool> IsPhoneTakenAsync(string phone, int? excludeMemberId = null, CancellationToken ct = default);

}
