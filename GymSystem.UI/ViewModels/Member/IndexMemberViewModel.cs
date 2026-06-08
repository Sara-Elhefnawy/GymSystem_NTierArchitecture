namespace GymSystem.UI.ViewModels.Member;

public class IndexMemberViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Photo { get; set; }
    public string Gender { get; set; } = default!;
}
