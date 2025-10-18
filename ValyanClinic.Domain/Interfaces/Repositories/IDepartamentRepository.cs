using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface IDepartamentRepository
{
    Task<Departament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Departament>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        Guid? idTipDepartament = null,
        string sortColumn = "DenumireDepartament",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    
    Task<int> GetCountAsync(
        string? searchText = null,
        Guid? idTipDepartament = null,
        CancellationToken cancellationToken = default);
    
    Task<Guid> CreateAsync(Departament departament, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Departament departament, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
