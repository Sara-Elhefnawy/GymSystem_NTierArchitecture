using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Category;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class CategoryService(IUnitOfWork uow, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<Result<IReadOnlyList<IndexCategoryDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var items = await uow.Categories.GetAllAsync(ct);

            Console.WriteLine($"Found {items?.Count() ?? 0} categories");

            if (items == null || !items.Any())
            {
                Console.WriteLine("WARNING: No categories found in database!");
                return Result.Ok<IReadOnlyList<IndexCategoryDTO>>(new List<IndexCategoryDTO>());
            }

            var dtos = items.Select(m => new IndexCategoryDTO
            {
                Id = m.Id,
                Name = m.Name,
                RequiredSpecialty = m.RequiredSpecialty.ToString()
            }).ToList();

            return Result.Ok<IReadOnlyList<IndexCategoryDTO>>(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all categories");
            return Result.Fail<IReadOnlyList<IndexCategoryDTO>>("Failed to retrieve categories", "DATABASE_ERROR");
        }
    }
}
