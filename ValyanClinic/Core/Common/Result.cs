namespace ValyanClinic.Core.Common;

public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public List<string> Errors { get; private init; } = new();
    public string? SuccessMessage { get; private init; }

    private Result(bool isSuccess, T? value, List<string> errors, string? successMessage = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
        SuccessMessage = successMessage;
    }

    public static Result<T> Success(T value, string? message = null) 
        => new(true, value, new List<string>(), message);

    public static Result<T> Failure(params string[] errors) 
        => new(false, default, errors.ToList());

    public static Result<T> Failure(List<string> errors) 
        => new(false, default, errors);
}

public class Result
{
    public bool IsSuccess { get; private init; }
    public List<string> Errors { get; private init; } = new();
    public string? SuccessMessage { get; private init; }

    private Result(bool isSuccess, List<string> errors, string? successMessage = null)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        SuccessMessage = successMessage;
    }

    public static Result Success(string? message = null) 
        => new(true, new List<string>(), message);

    public static Result Failure(params string[] errors) 
        => new(false, errors.ToList());

    public static Result Failure(List<string> errors) 
        => new(false, errors);
}