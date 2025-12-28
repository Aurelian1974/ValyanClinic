using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru PersonalMedical - ALINIAT CU SP-URI REALE
/// </summary>
public interface IPersonalMedicalRepository
{
    // Queries
    Task<PersonalMedical?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PersonalMedical?> GetByPersonalIdAsync(Guid personalId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PersonalMedical>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? pozitie = null,
        bool? esteActiv = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(
        string? searchText = null,
        string? departament = null,
        string? pozitie = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default);

    // New: metadata & statistics for filters & dashboard (domain DTOs)
    Task<PersonalMedicalFilterMetadataDto> GetFilterMetadataAsync(CancellationToken cancellationToken = default);
    Task<PersonalMedicalStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);

    // Commands
    Task<Guid> CreateAsync(PersonalMedical personalMedical, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(PersonalMedical personalMedical, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
