using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Models;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Interface for Personal Medical Service
/// Manages medical staff data and operations
/// </summary>
public interface IPersonalMedicalService
{
    /// <summary>
    /// Gets personal medical data with search and filtering
    /// </summary>
    Task<PersonalMedicalPagedResult> GetPersonalMedicalAsync(PersonalMedicalSearchRequest request);

    /// <summary>
    /// Gets personal medical by ID
    /// </summary>
    Task<PersonalMedical?> GetPersonalMedicalByIdAsync(Guid id);

    /// <summary>
    /// Creates new personal medical record
    /// </summary>
    Task<PersonalMedicalResult> CreatePersonalMedicalAsync(PersonalMedical personalMedical, string utilizator);

    /// <summary>
    /// Updates existing personal medical record
    /// </summary>
    Task<PersonalMedicalResult> UpdatePersonalMedicalAsync(PersonalMedical personalMedical, string utilizator);

    /// <summary>
    /// Deletes personal medical record
    /// </summary>
    Task<PersonalMedicalResult> DeletePersonalMedicalAsync(Guid id, string utilizator);

    /// <summary>
    /// Gets statistics for personal medical
    /// </summary>
    Task<PersonalMedicalStatistics> GetStatisticsAsync();

    /// <summary>
    /// Gets dropdown options for medical staff forms
    /// </summary>
    Task<PersonalMedicalDropdownOptions> GetDropdownOptionsAsync();

    /// <summary>
    /// Validates medical license
    /// </summary>
    Task<bool> ValidateLicentaMedicalaAsync(string numarLicenta, string pozitie);

    /// <summary>
    /// Gets required certifications for medical staff position
    /// </summary>
    Task<List<string>> GetCertificariNecesareAsync(Guid personalId);

    /// <summary>
    /// Validates personal medical data with medical-specific rules
    /// </summary>
    Task<PersonalMedicalValidationResult> ValidatePersonalMedicalAsync(PersonalMedical personalMedical, bool isUpdate = false);
}

/// <summary>
/// Search request for personal medical data
/// </summary>
public record PersonalMedicalSearchRequest(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchText = null,
    string? Departament = null,
    PozitiePersonalMedical? Pozitie = null,
    bool? Status = null,
    bool? AreSpecializare = null,
    string SortColumn = "Nume",
    string SortDirection = "ASC"
);

/// <summary>
/// Paged result for personal medical
/// </summary>
public record PersonalMedicalPagedResult(
    IEnumerable<PersonalMedical> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Result wrapper for personal medical operations
/// </summary>
public class PersonalMedicalResult
{
    public bool IsSuccess { get; private set; }
    public PersonalMedical? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> ValidationErrors { get; private set; } = new();

    public static PersonalMedicalResult Success(PersonalMedical? data = null) =>
        new() { IsSuccess = true, Data = data };

    public static PersonalMedicalResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };

    public static PersonalMedicalResult ValidationFailure(List<string> errors) =>
        new() { IsSuccess = false, ValidationErrors = errors, ErrorMessage = "Validation failed" };
}

/// <summary>
/// Statistics for personal medical
/// </summary>
public record PersonalMedicalStatistics(
    int Total,
    int Activ,
    int Inactiv,
    int Doctori,
    int AsistentiMedicali,
    int Tehnicieni,
    Dictionary<string, int> DistributiePerDepartament,
    Dictionary<string, int> DistributiePerSpecializare
);

/// <summary>
/// Dropdown options for personal medical forms
/// </summary>
public record PersonalMedicalDropdownOptions(
    IEnumerable<DropdownItem> DepartamenteMedicale,
    IEnumerable<DropdownItem> CategoriiMedicale,
    IEnumerable<DropdownItem> SpecializariMedicale,
    IEnumerable<DropdownItem> SubspecializariMedicale,
    IEnumerable<DropdownItem> PozitiiMedicale
);

/// <summary>
/// Validation result for personal medical specific validations
/// </summary>
public class PersonalMedicalValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; init; } = new();
    
    public static PersonalMedicalValidationResult Success() => new();
    
    public static PersonalMedicalValidationResult Failure(params string[] errors) => new()
    {
        Errors = errors.ToList()
    };
    
    public static PersonalMedicalValidationResult Failure(string error) => new()
    {
        Errors = new List<string> { error }
    };
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
}
