using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Validators;

namespace ValyanClinic.Components.Shared.Validation;

/// <summary>
/// Component helper pentru validarea FluentValidation in Blazor
/// Integreaza ValidationService cu componentele UI
/// </summary>
public class FluentValidationHelper<T> : ComponentBase where T : class
{
    [Inject] private IValidationService ValidationService { get; set; } = default!;
    [Inject] private ILogger<FluentValidationHelper<T>> Logger { get; set; } = default!;

    [Parameter] public T Model { get; set; } = default!;
    [Parameter] public EventCallback<ValidationResult> OnValidationCompleted { get; set; }
    [Parameter] public EventCallback<List<ValidationError>> OnValidationFailed { get; set; }
    [Parameter] public EventCallback OnValidationSuccess { get; set; }

    /// <summary>
    /// Valideaza modelul folosind validatorul de baza
    /// </summary>
    public async Task<ValidationResult> ValidateAsync()
    {
        try
        {
            Logger.LogInformation("Validating model of type: {ModelType}", typeof(T).Name);
            
            var result = await ValidationService.ValidateAsync(Model);
            
            await OnValidationCompleted.InvokeAsync(result);
            
            if (result.IsValid)
            {
                Logger.LogInformation("Validation passed for {ModelType}", typeof(T).Name);
                await OnValidationSuccess.InvokeAsync();
            }
            else
            {
                Logger.LogWarning("Validation failed for {ModelType} with {ErrorCount} errors", 
                    typeof(T).Name, result.Errors.Count);
                await OnValidationFailed.InvokeAsync(result.Errors);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during validation for {ModelType}", typeof(T).Name);
            var errorResult = ValidationResult.Failure("Eroare la validarea datelor");
            await OnValidationCompleted.InvokeAsync(errorResult);
            return errorResult;
        }
    }

    /// <summary>
    /// Valideaza pentru operatiunea de creare
    /// </summary>
    public async Task<ValidationResult> ValidateForCreateAsync()
    {
        try
        {
            Logger.LogInformation("Validating model for CREATE operation: {ModelType}", typeof(T).Name);
            
            var result = await ValidationService.ValidateForCreateAsync(Model);
            
            await OnValidationCompleted.InvokeAsync(result);
            
            if (result.IsValid)
            {
                Logger.LogInformation("CREATE validation passed for {ModelType}", typeof(T).Name);
                await OnValidationSuccess.InvokeAsync();
            }
            else
            {
                Logger.LogWarning("CREATE validation failed for {ModelType} with {ErrorCount} errors", 
                    typeof(T).Name, result.Errors.Count);
                await OnValidationFailed.InvokeAsync(result.Errors);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during CREATE validation for {ModelType}", typeof(T).Name);
            var errorResult = ValidationResult.Failure("Eroare la validarea datelor pentru creare");
            await OnValidationCompleted.InvokeAsync(errorResult);
            return errorResult;
        }
    }

    /// <summary>
    /// Valideaza pentru operatiunea de actualizare
    /// </summary>
    public async Task<ValidationResult> ValidateForUpdateAsync()
    {
        try
        {
            Logger.LogInformation("Validating model for UPDATE operation: {ModelType}", typeof(T).Name);
            
            var result = await ValidationService.ValidateForUpdateAsync(Model);
            
            await OnValidationCompleted.InvokeAsync(result);
            
            if (result.IsValid)
            {
                Logger.LogInformation("UPDATE validation passed for {ModelType}", typeof(T).Name);
                await OnValidationSuccess.InvokeAsync();
            }
            else
            {
                Logger.LogWarning("UPDATE validation failed for {ModelType} with {ErrorCount} errors", 
                    typeof(T).Name, result.Errors.Count);
                await OnValidationFailed.InvokeAsync(result.Errors);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during UPDATE validation for {ModelType}", typeof(T).Name);
            var errorResult = ValidationResult.Failure("Eroare la validarea datelor pentru actualizare");
            await OnValidationCompleted.InvokeAsync(errorResult);
            return errorResult;
        }
    }

    /// <summary>
    /// Valideaza un singur camp
    /// </summary>
    public async Task<bool> ValidatePropertyAsync(string propertyName)
    {
        try
        {
            var result = await ValidationService.ValidateAsync(Model);
            return !result.HasErrorForProperty(propertyName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating property {PropertyName} for {ModelType}", propertyName, typeof(T).Name);
            return false;
        }
    }

    /// <summary>
    /// Obtine mesajul de eroare pentru o proprietate specifica
    /// </summary>
    public async Task<string?> GetPropertyErrorAsync(string propertyName)
    {
        try
        {
            var result = await ValidationService.ValidateAsync(Model);
            var errors = result.GetErrorsForProperty(propertyName);
            return errors.FirstOrDefault()?.ErrorMessage;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting property error for {PropertyName} on {ModelType}", propertyName, typeof(T).Name);
            return "Eroare la validare";
        }
    }

    /// <summary>
    /// Obtine toate erorile pentru o proprietate specifica
    /// </summary>
    public async Task<List<string>> GetPropertyErrorsAsync(string propertyName)
    {
        try
        {
            var result = await ValidationService.ValidateAsync(Model);
            var errors = result.GetErrorsForProperty(propertyName);
            return errors.Select(e => e.ErrorMessage).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting property errors for {PropertyName} on {ModelType}", propertyName, typeof(T).Name);
            return ["Eroare la validare"];
        }
    }
}

/// <summary>
/// Extension methods pentru facilitarea validarii in componente
/// </summary>
public static class BlazorValidationExtensions
{
    /// <summary>
    /// Extension method pentru validare rapida in componente
    /// </summary>
    public static async Task<ValidationResult> ValidateAsync<T>(this T model, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateAsync(model);
    }

    /// <summary>
    /// Verifica daca un model este valid
    /// </summary>
    public static async Task<bool> IsValidAsync<T>(this T model, IServiceProvider serviceProvider) where T : class
    {
        var result = await model.ValidateAsync(serviceProvider);
        return result.IsValid;
    }

    /// <summary>
    /// Obtine toate erorile de validare ca string
    /// </summary>
    public static async Task<string> GetValidationErrorsAsync<T>(this T model, IServiceProvider serviceProvider) where T : class
    {
        var result = await model.ValidateAsync(serviceProvider);
        return result.GetErrorsAsString();
    }

    /// <summary>
    /// Validare cu callback pentru erori
    /// </summary>
    public static async Task ValidateWithCallbackAsync<T>(
        this T model, 
        IServiceProvider serviceProvider, 
        Func<ValidationResult, Task> onResult) where T : class
    {
        var result = await model.ValidateAsync(serviceProvider);
        await onResult(result);
    }
}
