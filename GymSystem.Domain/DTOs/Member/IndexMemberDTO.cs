namespace GymSystem.Domain.DTOs.Member;

public class IndexMemberDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Photo { get; set; }
    public string Gender { get; set; } = default!;

}
