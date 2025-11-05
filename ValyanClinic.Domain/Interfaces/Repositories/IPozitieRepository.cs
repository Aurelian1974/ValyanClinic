using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface IPozitieRepository
{
    Task<Pozitie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Pozitie>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        bool? esteActiv = null,
        string sortColumn = "Denumire",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    
    Task<int> GetCountAsync(
        string? searchText = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default);
    
    Task<Guid> CreateAsync(Pozitie pozitie, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Pozitie pozitie, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
