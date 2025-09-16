using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ValyanClinic.Domain.Validators;
using ValyanClinic.Application.Validators;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Application.Extensions;

/// <summary>
/// Extension methods pentru inregistrarea validatorilor FluentValidation
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Inregistreaza toate validatorii FluentValidation in DI container
    /// </summary>
    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        // === INREGISTRARE VALIDATION SERVICE ===
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
        
        // Adauga FluentValidation cu detectare automata
        services.AddValidatorsFromAssembly(typeof(PersonalValidator).Assembly, ServiceLifetime.Scoped);
        services.AddValidatorsFromAssembly(typeof(ValidationService).Assembly, ServiceLifetime.Scoped);

        return services;
    }

    /// <summary>
    /// Configureaza optiunile FluentValidation pentru intreaga aplicatie
    /// </summary>
    public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false; // Folosim mesaje custom
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop; // Opreste la prima eroare per regula
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue; // Continua cu urmatoarea proprietate

        return services;
    }

    /// <summary>
    /// Adauga validatori custom specifici pentru entitati complexe
    /// </summary>
    public static IServiceCollection AddCustomValidators(this IServiceCollection services)
    {
        // === VALIDATORI PENTRU OPERATIUNI DE BUSINESS ===
        
        // Exemple pentru validatori de business logic
        // services.AddScoped<AppointmentBusinessValidator>();
        // services.AddScoped<MedicalRecordBusinessValidator>();
        // services.AddScoped<PrescriptionBusinessValidator>();

        // === VALIDATORI PENTRU DTO-URI SI VIEW MODELS ===
        
        // Cand vom avea DTO-uri specifice, le vom adauga aici
        // services.AddScoped<IValidator<PersonalDto>, PersonalDtoValidator>();
        // services.AddScoped<IValidator<UserDto>, UserDtoValidator>();

        return services;
    }

    /// <summary>
    /// Extension method pentru configurarea completa a validarii
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
/// Helper class pentru obtinerea rapida a validatorilor
/// </summary>
public static class ValidatorHelper
{
    /// <summary>
    /// Obtine validatorul pentru un tip specificat
    /// </summary>
    public static IValidator<T>? GetValidator<T>(IServiceProvider serviceProvider) where T : class
    {
        return serviceProvider.GetService<IValidator<T>>();
    }

    /// <summary>
    /// Obtine validatorul de creare pentru un tip specificat
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
    /// Obtine validatorul de actualizare pentru un tip specificat
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
    /// Valideaza rapid un obiect si returneaza rezultatul
    /// </summary>
    public static async Task<ValidationResult> QuickValidateAsync<T>(this T entity, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateAsync(entity);
    }

    /// <summary>
    /// Valideaza rapid un obiect pentru creare
    /// </summary>
    public static async Task<ValidationResult> QuickValidateForCreateAsync<T>(this T entity, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateForCreateAsync(entity);
    }

    /// <summary>
    /// Valideaza rapid un obiect pentru actualizare
    /// </summary>
    public static async Task<ValidationResult> QuickValidateForUpdateAsync<T>(this T entity, IServiceProvider serviceProvider) where T : class
    {
        var validationService = serviceProvider.GetRequiredService<IValidationService>();
        return await validationService.ValidateForUpdateAsync(entity);
    }
}
