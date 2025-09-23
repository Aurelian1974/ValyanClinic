using ValyanClinic.Domain.Models;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository interface pentru operatiile cu PersonalMedical
/// Foloseste Dapper si Stored Procedures pentru performance optimal
/// Similar cu IPersonalRepository dar adaptat pentru tabela PersonalMedical
/// </summary>
public interface IPersonalMedicalRepository
{
    Task<(IEnumerable<PersonalMedical> Data, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? pozitie = null,
        string? status = null,
        bool? areSpecializare = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC");

    Task<PersonalMedical?> GetByIdAsync(Guid id);
    
    Task<PersonalMedical> CreateAsync(PersonalMedical personalMedical, string creatDe);
    
    Task<PersonalMedical> UpdateAsync(PersonalMedical personalMedical, string modificatDe);
    
    Task<bool> DeleteAsync(Guid id, string modificatDe);
    
    /// <summary>
    /// Verifică unicitatea numărului de licență medicală
    /// </summary>
    Task<bool> CheckLicentaUnicityAsync(string numarLicenta, Guid? excludeId = null);
    
    /// <summary>
    /// Verifică unicitatea email-ului în sistemul medical
    /// </summary>
    Task<bool> CheckEmailUnicityAsync(string email, Guid? excludeId = null);
    
    /// <summary>
    /// Verifică dacă personalul medical are programări active
    /// </summary>
    Task<bool> CheckActiveAppointmentsAsync(Guid personalId);
    
    /// <summary>
    /// Statistici pentru personalul medical
    /// </summary>
    Task<(int TotalPersonalMedical, int PersonalMedicalActiv, int PersonalMedicalInactiv, 
           int TotalDoctori, int TotalAsistenti, int TotalTehnicianiMedicali)> GetStatisticsAsync();
    
    /// <summary>
    /// Distribuția personalului medical pe departamente
    /// </summary>
    Task<Dictionary<string, int>> GetDistributiePerDepartamentAsync();
    
    /// <summary>
    /// Distribuția personalului medical pe specializări
    /// </summary>
    Task<Dictionary<string, int>> GetDistributiePerSpecializareAsync();
    
    // Test method for debugging database connectivity issues
    Task<bool> TestDatabaseConnectionAsync();
}
