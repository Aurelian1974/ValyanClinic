using ValyanClinic.Domain.Models;
using ValyanClinic.Application.Models;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Service interface pentru business logic Personal
/// Rich service cu validari, business rules si domain events
/// </summary>
public interface IPersonalService
{
    Task<PersonalPagedResult> GetPersonalAsync(PersonalSearchRequest request);
    
    Task<Personal?> GetPersonalByIdAsync(Guid id);
    
    Task<PersonalResult> CreatePersonalAsync(Personal personal, string utilizator);
    
    Task<PersonalResult> UpdatePersonalAsync(Personal personal, string utilizator);
    
    Task<PersonalResult> DeletePersonalAsync(Guid id, string utilizator);
    
    Task<PersonalStatistics> GetStatisticsAsync();
    
    Task<PersonalDropdownOptions> GetDropdownOptionsAsync();
    
    Task<PersonalValidationResult> ValidatePersonalAsync(Personal personal, bool isUpdate = false);
    
    /// <summary>
    /// Genereaza urmatorul cod de angajat disponibil (EMP001, EMP002, etc.)
    /// </summary>
    Task<string> GetNextCodAngajatAsync();
}

/// <summary>
/// Request model pentru search si paginare
/// </summary>
public record PersonalSearchRequest(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchText = null,
    string? Departament = null,
    string? Status = null,
    string SortColumn = "Nume",
    string SortDirection = "ASC"
);

/// <summary>
/// Result model pentru operatii CRUD
/// </summary>
public class PersonalResult
{
    public bool IsSuccess { get; init; }
    public Personal? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> ValidationErrors { get; init; } = new();
    
    public static PersonalResult Success(Personal data) => new()
    {
        IsSuccess = true,
        Data = data
    };
    
    public static PersonalResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
    
    public static PersonalResult ValidationFailure(List<string> validationErrors) => new()
    {
        IsSuccess = false,
        ValidationErrors = validationErrors
    };
}

/// <summary>
/// Result model pentru paginare
/// </summary>
public record PersonalPagedResult(
    IEnumerable<Personal> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Model pentru statistici
/// </summary>
public record PersonalStatistics(
    int TotalPersonal,
    int PersonalActiv,
    int PersonalInactiv
);

/// <summary>
/// Model pentru optiuni dropdown
/// </summary>
public record PersonalDropdownOptions(
    IEnumerable<DropdownItem> Departamente,
    IEnumerable<DropdownItem> Functii,
    IEnumerable<DropdownItem> Judete
);

/// <summary>
/// Result pentru validare Personal
/// </summary>
public class PersonalValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; init; } = new();
    
    public static PersonalValidationResult Success() => new();
    
    public static PersonalValidationResult Failure(params string[] errors) => new()
    {
        Errors = errors.ToList()
    };
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
}
