using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Category;
using GymSystem.Domain.DTOs.Session.Lookups;

namespace GymSystem.Domain.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<IndexCategoryDTO>>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<CategoryLookupDTO>> GetCategoryLookupAsync(CancellationToken ct = default);
}
