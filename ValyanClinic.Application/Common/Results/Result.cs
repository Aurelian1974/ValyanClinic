namespace ValyanClinic.Application.Common.Results;

/// <summary>
/// Result pattern fara valoare pentru operatii simple
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; protected set; } = new();
    public string? SuccessMessage { get; protected set; }

    protected Result(bool isSuccess, List<string> errors, string? successMessage = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<string>();
        SuccessMessage = successMessage;
    }

    public static Result Success(string? message = null)
        => new(true, new List<string>(), message);

    public static Result Failure(string error)
        => new(false, new List<string> { error });

    public static Result Failure(params string[] errors)
        => new(false, errors.ToList());

    public static Result Failure(List<string> errors)
        => new(false, errors);

    public static Result Failure(Exception exception)
        => new(false, new List<string> { exception.Message });

    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;
    public string ErrorsAsString => string.Join("; ", Errors);
    public bool HasErrors => Errors.Any();

    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    public Result OnFailure(Action<List<string>> action)
    {
        if (IsFailure)
            action(Errors);
        return this;
    }
}
