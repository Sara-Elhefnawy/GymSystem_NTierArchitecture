using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Session;

namespace GymSystem.Domain.Services;

public interface ISessionService
{
    Task<Result<IReadOnlyList<IndexSessionDTO>>> GetAllAsync(CancellationToken ct = default);

    Task<Result<CreateSessionDTO>> GetCreateFormAsync(CancellationToken st = default);
    Task<Result<CreateSessionDTO>> PrepareCreateFormAsync(CreateSessionDTO model, CancellationToken ct = default);
    Task<Result> CreateAsync(CreateSessionDTO model, CancellationToken cancellationToken = default);

    Task<Result<DetailsSessionDTO>> GetDetailsAsync(int id, CancellationToken ct = default);

}
