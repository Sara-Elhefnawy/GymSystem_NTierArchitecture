namespace GymSystem.Domain.Results;

public class CreateMemberResult
{
    public bool Succeeded { get; init; }
    public int? MemberId { get; init; }
    public Dictionary<string, string> Errors { get; init; } = new();

    public static CreateMemberResult Success(int memberId) => new()
    {
        Succeeded = true,
        MemberId = memberId
    };

    public static CreateMemberResult Failure(Dictionary<string, string> errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };

    public static CreateMemberResult Failure(string field, string message) => new()
    {
        Succeeded = false,
        Errors = new() { [field] = message }
    };

}