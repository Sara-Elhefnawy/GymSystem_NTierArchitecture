namespace GymSystem.Infrastructure.QueryService;

public interface ISessionQueryService
{
    Task<IReadOnlyList<IndexSessionReadModel>> GetIndexItemsAsync(
        CancellationToken ct = default);
}
