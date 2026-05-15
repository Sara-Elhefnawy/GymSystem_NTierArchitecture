namespace GymSystem.Domain.DTOs;

public class CheckInDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
}

public class CreateCheckInDto
{
    public int MemberId { get; set; }
}
