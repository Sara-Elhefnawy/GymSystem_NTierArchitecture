using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Category;
using GymSystem.Domain.DTOs.Session.Lookups;
using GymSystem.Infrastructure.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork uow, ILogger<CategoryService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<IndexCategoryDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var items = await _uow.Categories.GetAllAsync(ct);
            var dtos = items.Select(m =>
            {
                return new IndexCategoryDTO
                {
                    Name = m.Name,
                    RequiredSpecialty = m.RequiredSpecialty.ToString()
                };
            }).ToList();

            return Result.Ok<IReadOnlyList<IndexCategoryDTO>>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return Result.Fail<IReadOnlyList<IndexCategoryDTO>>("Failed to retrieve categories", "DATABASE_ERROR");
        }
    }

    public async Task<IReadOnlyList<CategoryLookupDTO>> GetCategoryLookupAsync(CancellationToken ct = default)
    {
        var categories = await _uow.Categories.GetAllAsync(ct);

        return categories.Select(c => new CategoryLookupDTO
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();
    }

}
