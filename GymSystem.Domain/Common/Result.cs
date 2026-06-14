namespace GymSystem.Domain.Common;

public abstract class Result : IResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string? ErrorKey { get; }

    protected Result(bool isSuccess, string? error, string? errorKey = null)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error message");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message");

        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
        ErrorKey = errorKey;
    }

    public static Result Ok() => new SuccessResult();
    public static Result Fail(string error, string? errorKey = null) => new FailureResult(error, errorKey);

    public static Result<T> Ok<T>(T value) => new SuccessResult<T>(value);
    public static Result<T> Fail<T>(string error, string? errorKey = null) => new FailureResult<T>(error, errorKey);

    // For validation errors with field-specific messages
    public static Result ValidationError(string error, Dictionary<string, string> fieldErrors = null, string? errorKey = null)
        => new ValidationErrorResult(error, fieldErrors, errorKey);

    public static Result<T> ValidationError<T>(string error, Dictionary<string, string> fieldErrors = null, string? errorKey = null)
        => new ValidationErrorResult<T>(error, fieldErrors, errorKey);
}

// Success result
public class SuccessResult : Result
{
    public SuccessResult() : base(true, string.Empty) { }
}

// Failure result
public class FailureResult : Result
{
    public FailureResult(string error, string? errorKey = null) : base(false, error, errorKey) { }
}

// Validation error result (with field-specific errors)
public class ValidationErrorResult : Result
{
    public Dictionary<string, string> FieldErrors { get; }

    public ValidationErrorResult(string error, Dictionary<string, string> fieldErrors = null, string? errorKey = null)
        : base(false, error, errorKey)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
    }
}

// Generic version
public abstract class Result<T> : Result
{
    protected Result(bool isSuccess, string error, string? errorKey = null) : base(isSuccess, error, errorKey) { }

    public abstract T Value { get; }
}

public class SuccessResult<T> : Result<T>
{
    private readonly T _value;

    public SuccessResult(T value) : base(true, string.Empty)
    {
        _value = value;
    }

    public override T Value => _value;
}

public class FailureResult<T> : Result<T>
{
    public FailureResult(string error, string? errorKey = null) : base(false, error, errorKey) { }

    public override T Value => throw new InvalidOperationException("Cannot access value of a failure result");
}

public class ValidationErrorResult<T> : Result<T>
{
    public Dictionary<string, string> FieldErrors { get; }

    public ValidationErrorResult(string error, Dictionary<string, string> fieldErrors = null, string? errorKey = null)
        : base(false, error, errorKey)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
    }

    public override T Value => throw new InvalidOperationException("Cannot access value of a validation error result");
}