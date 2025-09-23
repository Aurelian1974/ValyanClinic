namespace ValyanClinic.Application.Common;

/// <summary>
/// Generic Result pattern implementation pentru ValyanClinic
/// Înlocuiește bool/exception pattern cu Result<T> type-safe
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public string? SuccessMessage { get; private set; }
    
    protected Result(bool isSuccess, T? value, List<string> errors, string? successMessage = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? new List<string>();
        SuccessMessage = successMessage;
    }
    
    /// <summary>
    /// Creează un rezultat de succes
    /// </summary>
    public static Result<T> Success(T value, string? message = null)
        => new(true, value, new List<string>(), message);
    
    /// <summary>
    /// Creează un rezultat de eșec cu o singură eroare
    /// </summary>
    public static Result<T> Failure(string error)
        => new(false, default, new List<string> { error });
    
    /// <summary>
    /// Creează un rezultat de eșec cu multiple erori
    /// </summary>
    public static Result<T> Failure(params string[] errors)
        => new(false, default, errors.ToList());
    
    /// <summary>
    /// Creează un rezultat de eșec cu o listă de erori
    /// </summary>
    public static Result<T> Failure(List<string> errors)
        => new(false, default, errors);
    
    /// <summary>
    /// Creează un rezultat de eșec de la un Exception
    /// </summary>
    public static Result<T> Failure(Exception exception)
        => new(false, default, new List<string> { exception.Message });
    
    /// <summary>
    /// Obține prima eroare sau string gol
    /// </summary>
    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;
    
    /// <summary>
    /// Obține toate erorile ca un string concatenat
    /// </summary>
    public string ErrorsAsString => string.Join("; ", Errors);
    
    /// <summary>
    /// Verifică dacă există erori
    /// </summary>
    public bool HasErrors => Errors.Any();
    
    /// <summary>
    /// Mapează rezultatul la un alt tip
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
    /// Combină cu alt rezultat
    /// </summary>
    public Result<T> Combine(Result<T> other)
    {
        if (IsSuccess && other.IsSuccess)
            return Success(Value!, SuccessMessage ?? other.SuccessMessage);
            
        var combinedErrors = new List<string>();
        if (IsFailure) combinedErrors.AddRange(Errors);
        if (other.IsFailure) combinedErrors.AddRange(other.Errors);
        
        return Failure(combinedErrors);
    }
    
    /// <summary>
    /// Execută o acțiune dacă rezultatul este success
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Value != null)
            action(Value);
        return this;
    }
    
    /// <summary>
    /// Execută o acțiune dacă rezultatul este failure
    /// </summary>
    public Result<T> OnFailure(Action<List<string>> action)
    {
        if (IsFailure)
            action(Errors);
        return this;
    }
}

/// <summary>
/// Result pattern fără valoare pentru operații simple
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; private set; } = new();
    public string? SuccessMessage { get; private set; }
    
    private Result(bool isSuccess, List<string> errors, string? successMessage = null)
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
    
    /// <summary>
    /// Convertește la Result<T>
    /// </summary>
    public Result<T> ToResult<T>(T value)
    {
        if (IsSuccess)
            return Result<T>.Success(value, SuccessMessage);
        return Result<T>.Failure(Errors);
    }
    
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

/// <summary>
/// Extension methods pentru Result pattern în Blazor
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Afișează toast notification pe baza rezultatului
    /// </summary>
    public static async Task ShowToastAsync<T>(this Result<T> result, 
        Func<string, Task> showSuccess, 
        Func<string, Task> showError)
    {
        if (result.IsSuccess && !string.IsNullOrEmpty(result.SuccessMessage))
            await showSuccess(result.SuccessMessage);
        else if (result.IsFailure)
            await showError(result.ErrorsAsString);
    }
    
    /// <summary>
    /// Afișează toast notification pentru Result simplu
    /// </summary>
    public static async Task ShowToastAsync(this Result result, 
        Func<string, Task> showSuccess, 
        Func<string, Task> showError)
    {
        if (result.IsSuccess && !string.IsNullOrEmpty(result.SuccessMessage))
            await showSuccess(result.SuccessMessage);
        else if (result.IsFailure)
            await showError(result.ErrorsAsString);
    }
    
    /// <summary>
    /// Convertește ValidationResult la Result<T>
    /// </summary>
    public static Result<T> ToResult<T>(this Application.Validators.ValidationResult validationResult, T value)
    {
        if (validationResult.IsValid)
            return Result<T>.Success(value);
            
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return Result<T>.Failure(errors);
    }
    
    /// <summary>
    /// Convertește Exception la Result<T>
    /// </summary>
    public static Result<T> ToResult<T>(this Exception exception)
    {
        return Result<T>.Failure(exception.Message);
    }
}

/// <summary>
/// Result-uri specifice pentru domain-ul aplicației
/// </summary>
public static class DomainResults
{
    /// <summary>
    /// Result pentru operații cu Personal
    /// </summary>
    public class PersonalResult : Result<Domain.Models.Personal>
    {
        private PersonalResult(bool isSuccess, Domain.Models.Personal? value, List<string> errors, string? successMessage = null)
            : base(isSuccess, value, errors, successMessage)
        {
        }
        
        public static new PersonalResult Success(Domain.Models.Personal personal, string? message = "Personal salvat cu succes")
        {
            var result = new PersonalResult(true, personal, new List<string>(), message);
            return result;
        }
        
        public static new PersonalResult Failure(string error)
        {
            var result = new PersonalResult(false, null, new List<string> { error });
            return result;
        }
        
        public static new PersonalResult Failure(List<string> errors)
        {
            var result = new PersonalResult(false, null, errors);
            return result;
        }
        
        public static PersonalResult ValidationFailure(List<string> validationErrors)
        {
            var result = new PersonalResult(false, null, validationErrors);
            return result;
        }
    }
    
    /// <summary>
    /// Result pentru operații cu User
    /// </summary>
    public class UserResult : Result<Domain.Models.User>
    {
        private UserResult(bool isSuccess, Domain.Models.User? value, List<string> errors, string? successMessage = null)
            : base(isSuccess, value, errors, successMessage)
        {
        }
        
        public static new UserResult Success(Domain.Models.User user, string? message = "Utilizator salvat cu succes")
        {
            var result = new UserResult(true, user, new List<string>(), message);
            return result;
        }
        
        public static new UserResult Failure(string error)
        {
            var result = new UserResult(false, null, new List<string> { error });
            return result;
        }
        
        public static new UserResult Failure(List<string> errors)
        {
            var result = new UserResult(false, null, errors);
            return result;
        }
    }
}
