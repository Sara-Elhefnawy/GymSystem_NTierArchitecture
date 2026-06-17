using GymSystem.Domain.DTOs.Category;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<IndexCategoryDTO>>> GetAllAsync(CancellationToken ct = default);
}
