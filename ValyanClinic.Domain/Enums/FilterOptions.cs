using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Domain.Enums;

/// <summary>
/// Optiuni pentru filtrarea statusurilor in UI
/// inlocuieste magic strings cu enum type-safe
/// </summary>
public enum StatusFilterOption
{
    [Display(Name = "Toate statusurile")]
    All = 0,
    
    [Display(Name = "Activ")]
    Activ = 1,
    
    [Display(Name = "Inactiv")]
    Inactiv = 2,
    
    [Display(Name = "Suspendat")]
    Suspendat = 3
}

/// <summary>
/// Optiuni pentru filtrarea rolurilor in UI
/// </summary>
public enum RoleFilterOption
{
    [Display(Name = "Toate rolurile")]
    All = 0,
    
    [Display(Name = "Administrator")]
    Administrator = 1,
    
    [Display(Name = "Medic")]
    Medic = 2,
    
    [Display(Name = "Asistent Medical")]
    AsistentMedical = 3,
    
    [Display(Name = "Receptioner")]
    Receptioner = 4,
    
    [Display(Name = "Manager")]
    Manager = 5,
    
    [Display(Name = "Operator")]
    Operator = 6
}

/// <summary>
/// Optiuni pentru filtrarea departamentelor
/// </summary>
public enum DepartmentFilterOption
{
    [Display(Name = "Toate departamentele")]
    All = 0,
    
    [Display(Name = "Cardiologie")]
    Cardiologie = 1,
    
    [Display(Name = "Neurologie")]
    Neurologie = 2,
    
    [Display(Name = "Pediatrie")]
    Pediatrie = 3,
    
    [Display(Name = "Chirurgie")]
    Chirurgie = 4,
    
    [Display(Name = "Radiologie")]
    Radiologie = 5,
    
    [Display(Name = "Laborator")]
    Laborator = 6,
    
    [Display(Name = "Administrare")]
    Administrare = 7
}

/// <summary>
/// Optiuni pentru perioada de activitate
/// </summary>
public enum ActivityPeriodOption
{
    [Display(Name = "Orice perioada")]
    Any = 0,
    
    [Display(Name = "Astazi")]
    Today = 1,
    
    [Display(Name = "Ultima saptamana")]
    LastWeek = 2,
    
    [Display(Name = "Ultima luna")]
    LastMonth = 3,
    
    [Display(Name = "Ultimele 3 luni")]
    Last3Months = 4,
    
    [Display(Name = "Ultimele 6 luni")]
    Last6Months = 5,
    
    [Display(Name = "Ultimul an")]
    LastYear = 6
}

/// <summary>
/// Status pentru medicatie
/// </summary>
public enum MedicationStatus
{
    [Display(Name = "Activ")]
    Active = 1,
    
    [Display(Name = "Inactiv")]
    Inactive = 2,
    
    [Display(Name = "Suspendat")]
    Suspended = 3,
    
    [Display(Name = "Expirat")]
    Expired = 4
}

/// <summary>
/// Tip personal medical pentru filtrare
/// </summary>
public enum TipPersonalMedical
{
    [Display(Name = "Toate tipurile")]
    All = 0,
    
    [Display(Name = "Medic Primar")]
    MedicPrimar = 1,
    
    [Display(Name = "Medic Rezident")]
    MedicRezident = 2,
    
    [Display(Name = "Asistent Sef")]
    AsistentSef = 3,
    
    [Display(Name = "Asistent")]
    Asistent = 4,
    
    [Display(Name = "Tehnician")]
    Tehnician = 5,
    
    [Display(Name = "Secretar Medical")]
    SecretarMedical = 6
}

/// <summary>
/// Extension methods pentru obtinerea display name-urilor
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Obtine display name-ul pentru un enum value
    /// </summary>
    public static string GetDisplayName(this Enum enumValue)
    {
        var attribute = enumValue.GetType()
            .GetField(enumValue.ToString())
            ?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return attribute?.Name ?? enumValue.ToString();
    }
    
    /// <summary>
    /// Obtine toate valorile enum-ului ca lista de optiuni pentru dropdown
    /// </summary>
    public static List<FilterOption<TEnum>> GetFilterOptions<TEnum>() 
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(value => new FilterOption<TEnum>
            {
                Value = value,
                Text = value.GetDisplayName()
            })
            .ToList();
    }
}

/// <summary>
/// Clasa helper pentru optiuni dropdown
/// </summary>
public class FilterOption<T>
{
    public T Value { get; set; } = default!;
    public string Text { get; set; } = string.Empty;
    
    public FilterOption() { }
    
    public FilterOption(T value, string text)
    {
        Value = value;
        Text = text;
    }
}
