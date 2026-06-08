using GymSystem.Domain.DTOs.Session;

namespace GymSystem.Domain.Services;

public interface ISessionService
{
    Task<IReadOnlyList<IndexSessionDTO>> GetAllAsync(CancellationToken ct = default);
}
