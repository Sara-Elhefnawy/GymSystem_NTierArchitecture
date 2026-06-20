using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Session;

namespace GymSystem.Domain.Abstractions.Services;

public interface ISessionService
{
    Task<Result<IReadOnlyList<IndexSessionDTO>>> GetAllAsync(CancellationToken ct = default);

    Task<Result> CreateAsync(CreateSessionDTO model, CancellationToken cancellationToken = default);

    Task<Result<DetailsSessionDTO>> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<Result<EditSessionDTO>> GetForEditAsync(int id, CancellationToken ct = default);
    Task<Result> UpdateAsync(EditSessionDTO dto, CancellationToken ct = default);

    Task<Result<DeleteSessionDTO>> GetForDeleteAsync(int id, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
