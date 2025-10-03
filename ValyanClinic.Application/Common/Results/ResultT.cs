namespace ValyanClinic.Application.Common.Results;

/// <summary>
/// Generic Result pattern implementation cu valoare de return
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private set; }
    
    protected Result(bool isSuccess, T? value, List<string> errors, string? successMessage = null)
        : base(isSuccess, errors, successMessage)
    {
        Value = value;
    }
    
    public static Result<T> Success(T value, string? message = null)
        => new(true, value, new List<string>(), message);
    
    public new static Result<T> Failure(string error)
        => new(false, default, new List<string> { error });
    
    public new static Result<T> Failure(params string[] errors)
        => new(false, default, errors.ToList());
    
    public new static Result<T> Failure(List<string> errors)
        => new(false, default, errors);
    
    public new static Result<T> Failure(Exception exception)
        => new(false, default, new List<string> { exception.Message });
    
    /// <summary>
    /// Mapeaza rezultatul la un alt tip
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Errors);
            
        try
        {
            var newValue = mapper(Value!);
            return Result<TNew>.Success(newValue, SuccessMessage);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex.Message);
        }
    }
    
    /// <summary>
    /// Executa o actiune daca rezultatul este success
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Value != null)
            action(Value);
        return this;
    }
    
    /// <summary>
    /// Executa o actiune daca rezultatul este failure
    /// </summary>
    public new Result<T> OnFailure(Action<List<string>> action)
    {
        if (IsFailure)
            action(Errors);
        return this;
    }
}
