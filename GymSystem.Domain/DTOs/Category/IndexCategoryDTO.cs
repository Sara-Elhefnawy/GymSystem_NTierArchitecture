namespace GymSystem.Domain.DTOs.Category;

public class IndexCategoryDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string RequiredSpecialty { get; set; } = default!;
}
