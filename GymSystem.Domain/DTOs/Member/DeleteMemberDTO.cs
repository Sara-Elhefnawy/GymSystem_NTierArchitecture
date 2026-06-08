namespace GymSystem.Domain.DTOs.Member;

public class DeleteMemberDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Photo { get; set; }
}
