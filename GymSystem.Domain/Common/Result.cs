namespace GymSystem.Domain.Common;

public abstract class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error message");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new SuccessResult();
    public static Result Fail(string error) => new FailureResult(error);

    public static Result<T> Ok<T>(T value) => new SuccessResult<T>(value);
    public static Result<T> Fail<T>(string error) => new FailureResult<T>(error);

    // For validation errors with field-specific messages
    public static Result ValidationError(string error, Dictionary<string, string> fieldErrors = null)
        => new ValidationErrorResult(error, fieldErrors);

    public static Result<T> ValidationError<T>(string error, Dictionary<string, string> fieldErrors = null)
        => new ValidationErrorResult<T>(error, fieldErrors);
}

// Success result
public class SuccessResult : Result
{
    public SuccessResult() : base(true, string.Empty) { }
}

// Failure result
public class FailureResult : Result
{
    public FailureResult(string error) : base(false, error) { }
}

// Validation error result (with field-specific errors)
public class ValidationErrorResult : Result
{
    public Dictionary<string, string> FieldErrors { get; }

    public ValidationErrorResult(string error, Dictionary<string, string> fieldErrors = null)
        : base(false, error)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
    }
}

// Generic version
public abstract class Result<T> : Result
{
    protected Result(bool isSuccess, string error) : base(isSuccess, error) { }

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
    public FailureResult(string error) : base(false, error) { }

    public override T Value => throw new InvalidOperationException("Cannot access value of a failure result");
}

public class ValidationErrorResult<T> : Result<T>
{
    public Dictionary<string, string> FieldErrors { get; }

    public ValidationErrorResult(string error, Dictionary<string, string> fieldErrors = null)
        : base(false, error)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
    }

    public override T Value => throw new InvalidOperationException("Cannot access value of a validation error result");
}