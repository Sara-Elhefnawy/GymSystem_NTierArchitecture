public class DetailsMemberViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Photo { get; set; }

    public string Email { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public string Gender { get; set; } = default!;

    public DateOnly DateOfBirth { get; set; }
    // "12 - El Tahrir - Giza"
    public string Address { get; set; } = default!;

    public string PlanName { get; set; } = default!;

    public DateOnly? MembershipStartDate { get; set; }

    public DateOnly? MembershipEndDate { get; set; }
}

