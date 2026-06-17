namespace GymSystem.Shared.Common;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure => !IsSuccess;
    string? Error { get; }
    string? ErrorKey { get; }
}
