using FluentValidation;
using ValyanClinic.Domain.Validators;
using ValyanClinic.Application.Validators;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ValyanClinic.Application.Validators;

/// <summary>
/// Service centralizat pentru toate validările aplicației
/// Folosește FluentValidation pentru validări consistente și extensibile
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validează un obiect folosind validatorul corespunzător
    /// </summary>
    Task<ValidationResult> ValidateAsync<T>(T entity) where T : class;
    
    /// <summary>
    /// Validează un obiect pentru operațiunea de creare
    /// </summary>
    Task<ValidationResult> ValidateForCreateAsync<T>(T entity) where T : class;
    
    /// <summary>
    /// Validează un obiect pentru operațiunea de actualizare
    /// </summary>
    Task<ValidationResult> ValidateForUpdateAsync<T>(T entity) where T : class;
    
    /// <summary>
    /// Validează cu un validator specificat
    /// </summary>
    Task<ValidationResult> ValidateWithValidatorAsync<T>(T entity, IValidator<T> validator) where T : class;
}

/// <summary>
/// Implementarea service-ului de validare
/// </summary>
public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(IServiceProvider serviceProvider, ILogger<ValidationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAsync<T>(T entity) where T : class
    {
        var validator = GetValidator<T>();
        if (validator == null)
        {
            _logger.LogWarning("No validator found for type {Type}", typeof(T).Name);
            return ValidationResult.Success();
        }

        var result = await validator.ValidateAsync(entity);
        LogValidationResult(entity, result);
        
        return new ValidationResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage,
                AttemptedValue = e.AttemptedValue?.ToString(),
                ErrorCode = e.ErrorCode
            }).ToList()
        };
    }

    public async Task<ValidationResult> ValidateForCreateAsync<T>(T entity) where T : class
    {
        var validator = GetCreateValidator<T>() ?? GetValidator<T>();
        if (validator == null)
        {
            _logger.LogWarning("No create validator found for type {Type}", typeof(T).Name);
            return ValidationResult.Success();
        }

        var result = await validator.ValidateAsync(entity);
        LogValidationResult(entity, result, "CREATE");
        
        return new ValidationResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage,
                AttemptedValue = e.AttemptedValue?.ToString(),
                ErrorCode = e.ErrorCode
            }).ToList()
        };
    }

    public async Task<ValidationResult> ValidateForUpdateAsync<T>(T entity) where T : class
    {
        var validator = GetUpdateValidator<T>() ?? GetValidator<T>();
        if (validator == null)
        {
            _logger.LogWarning("No update validator found for type {Type}", typeof(T).Name);
            return ValidationResult.Success();
        }

        var result = await validator.ValidateAsync(entity);
        LogValidationResult(entity, result, "UPDATE");
        
        return new ValidationResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage,
                AttemptedValue = e.AttemptedValue?.ToString(),
                ErrorCode = e.ErrorCode
            }).ToList()
        };
    }

    public async Task<ValidationResult> ValidateWithValidatorAsync<T>(T entity, IValidator<T> validator) where T : class
    {
        var result = await validator.ValidateAsync(entity);
        LogValidationResult(entity, result, "CUSTOM");
        
        return new ValidationResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage,
                AttemptedValue = e.AttemptedValue?.ToString(),
                ErrorCode = e.ErrorCode
            }).ToList()
        };
    }

    /// <summary>
    /// Obține validatorul de bază pentru un tip
    /// </summary>
    private IValidator<T>? GetValidator<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(Personal) => _serviceProvider.GetService(typeof(PersonalValidator)) as IValidator<T>,
            "User" => _serviceProvider.GetService(typeof(UserValidator)) as IValidator<T>, // Evit ambiguitatea
            nameof(Patient) => _serviceProvider.GetService(typeof(PatientValidator)) as IValidator<T>,
            nameof(LoginRequest) => _serviceProvider.GetService(typeof(LoginRequestValidator)) as IValidator<T>,
            _ => _serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>
        };
    }

    /// <summary>
    /// Obține validatorul pentru operațiunea de creare
    /// </summary>
    private IValidator<T>? GetCreateValidator<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(Personal) => _serviceProvider.GetService(typeof(PersonalCreateValidator)) as IValidator<T>,
            "User" => _serviceProvider.GetService(typeof(UserCreateValidator)) as IValidator<T>, // Evit ambiguitatea
            nameof(Patient) => _serviceProvider.GetService(typeof(PatientCreateValidator)) as IValidator<T>,
            _ => null
        };
    }

    /// <summary>
    /// Obține validatorul pentru operațiunea de actualizare
    /// </summary>
    private IValidator<T>? GetUpdateValidator<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(Personal) => _serviceProvider.GetService(typeof(PersonalUpdateValidator)) as IValidator<T>,
            "User" => _serviceProvider.GetService(typeof(UserUpdateValidator)) as IValidator<T>, // Evit ambiguitatea
            nameof(Patient) => _serviceProvider.GetService(typeof(PatientUpdateValidator)) as IValidator<T>,
            _ => null
        };
    }

    /// <summary>
    /// Loghează rezultatul validării
    /// </summary>
    private void LogValidationResult<T>(T entity, FluentValidation.Results.ValidationResult result, string operation = "VALIDATE")
    {
        if (result.IsValid)
        {
            _logger.LogInformation("Validation {Operation} PASSED for {EntityType}", 
                operation, typeof(T).Name);
        }
        else
        {
            _logger.LogWarning("Validation {Operation} FAILED for {EntityType} with {ErrorCount} errors: {Errors}",
                operation, typeof(T).Name, result.Errors.Count, 
                string.Join(", ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
        }
    }
}

/// <summary>
/// Rezultatul validării
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public string ErrorMessage => string.Join("; ", Errors.Select(e => e.ErrorMessage));
    public bool HasErrors => Errors.Any();

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string errorMessage) => new() 
    { 
        IsValid = false, 
        Errors = [new ValidationError { ErrorMessage = errorMessage }] 
    };
    public static ValidationResult Failure(List<ValidationError> errors) => new() 
    { 
        IsValid = false, 
        Errors = errors 
    };
}

/// <summary>
/// Eroare de validare
/// </summary>
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? AttemptedValue { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Extension methods pentru ValidationResult
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// Convertește ValidationResult la boolean
    /// </summary>
    public static bool IsSuccess(this ValidationResult result) => result.IsValid;

    /// <summary>
    /// Convertește ValidationResult la string cu toate erorile
    /// </summary>
    public static string GetErrorsAsString(this ValidationResult result)
    {
        if (result.IsValid) return string.Empty;
        
        return string.Join(Environment.NewLine, 
            result.Errors.Select(e => $"• {e.PropertyName}: {e.ErrorMessage}"));
    }

    /// <summary>
    /// Obține erorile pentru o proprietate specifică
    /// </summary>
    public static List<ValidationError> GetErrorsForProperty(this ValidationResult result, string propertyName)
    {
        return result.Errors.Where(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Verifică dacă o proprietate specifică are erori
    /// </summary>
    public static bool HasErrorForProperty(this ValidationResult result, string propertyName)
    {
        return result.Errors.Any(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }
}
