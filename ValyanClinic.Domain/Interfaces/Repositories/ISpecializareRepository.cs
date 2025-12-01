using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface ISpecializareRepository
{
    Task<Specializare?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Specializare>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? categorie = null,
        bool? esteActiv = null,
        string sortColumn = "Denumire",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(
        string? searchText = null,
        string? categorie = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Specializare specializare, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Specializare specializare, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetCategoriiAsync(CancellationToken cancellationToken = default);

    // Metodă specializată pentru dropdown-uri (returnează TOATE specializările active)
    Task<IEnumerable<(Guid Id, string Denumire, string? Categorie)>> GetDropdownOptionsAsync(
        string? categorie = null,
        bool esteActiv = true,
        CancellationToken cancellationToken = default);
}
