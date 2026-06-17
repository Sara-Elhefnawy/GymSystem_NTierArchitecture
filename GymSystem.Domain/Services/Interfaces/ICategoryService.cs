using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Category;

namespace GymSystem.Domain.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<IndexCategoryDTO>>> GetAllAsync(CancellationToken ct = default);
}
