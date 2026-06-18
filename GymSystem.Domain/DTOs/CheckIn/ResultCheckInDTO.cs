namespace GymSystem.Domain.DTOs.CheckIn;

public class ResultCheckInDTO
{
    public string MemberName { get; set; } = default!;
    public string SessionName { get; set; } = default!;
    public bool IsAlreadyAttended { get; set; }
    public bool WasAutoBooked { get; set; }
}
