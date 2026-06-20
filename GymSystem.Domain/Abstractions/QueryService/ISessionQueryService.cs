using GymSystem.Domain.QueryService;

namespace GymSystem.Domain.Abstractions.QueryService;

public interface ISessionQueryService
{
    Task<IReadOnlyList<IndexSessionReadModel>> GetIndexItemsAsync(
        CancellationToken ct = default);
}
