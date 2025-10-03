using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru Personal - ALINIAT CU SP-URI REALE
/// </summary>
public interface IPersonalRepository
{
    // Queries
    Task<Personal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Personal>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? status = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    
    Task<int> GetCountAsync(
        string? searchText = null,
        string? departament = null,
        string? status = null,
        CancellationToken cancellationToken = default);
    
    Task<(string name, int value, string iconClass, string colorClass)[]> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
    
    Task<(string[] departamente, string[] functii, string[] judete)> GetDropdownOptionsAsync(
        CancellationToken cancellationToken = default);
    
    // Commands
    Task<Guid> CreateAsync(Personal personal, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Personal personal, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string modificatDe, CancellationToken cancellationToken = default);
    
    // Validation
    Task<(bool cnpExists, bool codAngajatExists)> CheckUniqueAsync(
        string? cnp = null,
        string? codAngajat = null,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
