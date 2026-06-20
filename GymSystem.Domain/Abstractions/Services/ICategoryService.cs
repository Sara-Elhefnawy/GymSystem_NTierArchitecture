using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Category;

namespace GymSystem.Domain.Abstractions.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<IndexCategoryDTO>>> GetAllAsync(CancellationToken ct = default);
}
