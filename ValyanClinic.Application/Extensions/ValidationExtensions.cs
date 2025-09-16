using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ValyanClinic.Domain.Validators;
using ValyanClinic.Application.Validators;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Extensions;

/// <summary>
/// Extension methods pentru înregistrarea validatorilor FluentValidation
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Înregistrează toate validatorii FluentValidation în DI container
    /// </summary>
    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        // === ÎNREGISTRARE VALIDATION SERVICE ===
        services.AddScoped<IValidationService, ValidationService>();

        // === VALIDATORI PENTRU DOMAIN MODELS ===
        
        // Personal validators
        services.AddScoped<IValidator<Personal>, PersonalValidator>();
        services.AddScoped<PersonalValidator>();
        services.AddScoped<PersonalCreateValidator>();
        services.AddScoped<PersonalUpdateValidator>();

        // User validators - folosesc doar Models.User pentru a evita ambiguitatea
        services.AddScoped<IValidator<ValyanClinic.Domain.Models.User>, UserValidator>();
        services.AddScoped<UserValidator>();
        services.AddScoped<UserCreateValidator>();
        services.AddScoped<UserUpdateValidator>();

        // Patient validators
        services.AddScoped<IValidator<Patient>, PatientValidator>();
        services.AddScoped<PatientValidator>();
        services.AddScoped<PatientCreateValidator>();
        services.AddScoped<PatientUpdateValidator>();

        // Authentication validators
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<LoginRequestValidator>();
        services.AddScoped<ChangePasswordRequestValidator>();
        services.AddScoped<ResetPasswordRequestValidator>();

        // === CONFIGURARE FLUENT VALIDATION ===
        
        // Adaugă FluentValidation cu detectare automată
        services.AddValidatorsFromAssembly(typeof(PersonalValidator).Assembly, ServiceLifetime.Scoped);
        services.AddValidatorsFromAssembly(typeof(ValidationService).Assembly, ServiceLifetime.Scoped);

        return services;
    }

    /// <summary>
    /// Configurează opțiunile FluentValidation pentru întreaga aplicație
    /// </summary>
    public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false; // Folosim mesaje custom
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop; // Oprește la prima eroare per regulă
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue; // Continuă cu următoarea proprietate

        return services;
    }

    /// <summary>
    /// Adaugă validatori custom specifici pentru entități complexe
    /// </summary>
    public static IServiceCollection AddCustomValidators(this IServiceCollection services)
    {
        // === VALIDATORI PENTRU OPERAȚIUNI DE BUSINESS ===
        
        // Exemple pentru validatori de business logic
        // services.AddScoped<AppointmentBusinessValidator>();
        // services.AddScoped<MedicalRecordBusinessValidator>();
        // services.AddScoped<PrescriptionBusinessValidator>();

        // === VALIDATORI PENTRU DTO-URI ȘI VIEW MODELS ===
        
        // Când vom avea DTO-uri specifice, le vom adăuga aici
        // services.AddScoped<IValidator<PersonalDto>, PersonalDtoValidator>();
        // services.AddScoped<IValidator<UserDto>, UserDtoValidator>();

        return services;
    }

    /// <summary>
    /// Extension method pentru configurarea completă a validării
    /// </summary>
    public static IServiceCollection AddValyanClinicValidation(this IServiceCollection services)
    {
        return services
            .AddFluentValidationServices()
            .ConfigureFluentValidation()
            .AddCustomValidators();
    }
}

/// <summary>
/// Helper class pentru obținerea rapidă a validatorilor
/// </summary>
public static class ValidatorHelper
{
    /// <summary>
    /// Obține validatorul pentru un tip specificat
    /// </summary>
    public static IValidator<T>? GetValidator<T>(IServiceProvider serviceProvider) where T : class
    {
        return serviceProvider.GetService<IValidator<T>>();
    }

    /// <summary>
    /// Obține validatorul de creare pentru un tip specificat
    /// </summary>
    public static IValidator<T>? GetCreateValidator<T>(IServiceProvider serviceProvider) where T : class
    {
        return typeof(T).Name switch
        {
            nameof(Personal) => serviceProvider.GetService<PersonalCreateValidator>() as IValidator<T>,
            "User" => serviceProvider.GetService<UserCreateValidator>() as IValidator<T>, // Evit ambiguitatea
            nameof(Patient) => serviceProvider.GetService<PatientCreateValidator>() as IValidator<T>,
            _ => serviceProvider.GetService<IValidator<T>>()
        };
    }

    /// <summary>
    /// Obține validatorul de actualizare pentru un tip specificat
    /// </summary>
    public static IValidator<T>? GetUpdateValidator<T>(IServiceProvider serviceProvider) where T : class
    {
        return typeof(T).Name switch
        {
            nameof(Personal) => serviceProvider.GetService<PersonalUpdateValidator>() as IValidator<T>,
            "User" => serviceProvider.GetService<UserUpdateValidator>() as IValidator<T>, // Evit ambiguitatea
            nameof(Patient) => serviceProvider.GetService<PatientUpdateValidator>() as IValidator<T>,
            _ => serviceProvider.GetService<IValidator<T>>()
        };
    }

    /// <summary>
    /// Validează rapid un obiect și returnează rezultatul
    /// </summary>
    public static async Task<ValidationResult> QuickValidateAsync<T>(this T entity, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateAsync(entity);
    }

    /// <summary>
    /// Validează rapid un obiect pentru creare
    /// </summary>
    public static async Task<ValidationResult> QuickValidateForCreateAsync<T>(this T entity, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateForCreateAsync(entity);
    }

    /// <summary>
    /// Validează rapid un obiect pentru actualizare
    /// </summary>
    public static async Task<ValidationResult> QuickValidateForUpdateAsync<T>(this T entity, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateForUpdateAsync(entity);
    }
}
