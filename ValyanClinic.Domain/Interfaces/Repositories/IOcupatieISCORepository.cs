using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru entitatea OcupatieISCO - Clasificarea ISCO-08
/// </summary>
public interface IOcupatieISCORepository
{
    /// <summary>
    /// Obține toate ocupațiile cu suport pentru paginare și filtrare
    /// </summary>
    Task<IEnumerable<OcupatieISCO>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? searchText = null,
        byte? nivelIerarhic = null,
        string? grupaMajora = null,
        bool? esteActiv = true,
        string sortColumn = "Cod_ISCO",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține numărul total de înregistrări pentru paginare
    /// </summary>
    Task<int> GetCountAsync(
        string? searchText = null,
        byte? nivelIerarhic = null,
        string? grupaMajora = null,
        bool? esteActiv = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o ocupație după ID
    /// </summary>
    Task<OcupatieISCO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o ocupație după cod ISCO
    /// </summary>
    Task<OcupatieISCO?> GetByCodISCOAsync(string codISCO, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține ocupațiile copil pentru o ocupație părinte
    /// </summary>
    Task<IEnumerable<OcupatieISCO>> GetCopiiAsync(string codParinte, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține grupele majore (nivel 1)
    /// </summary>
    Task<IEnumerable<OcupatieISCO>> GetGrupeMajoreAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Căutare ocupații cu scoring de relevanță
    /// </summary>
    Task<IEnumerable<(OcupatieISCO ocupatie, int scorRelevanta)>> SearchAsync(
        string searchText,
        byte? nivelIerarhic = null,
        int maxResults = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține statistici pentru dashboard
    /// </summary>
    Task<(string categorie, int numar, int active)[]> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține opțiuni pentru dropdown-uri (pentru UI)
    /// </summary>
    Task<(string value, string text)[]> GetDropdownOptionsAsync(
        byte nivelIerarhic = 4,
        string? grupaMajora = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creează o nouă ocupație
    /// </summary>
    Task<Guid> CreateAsync(OcupatieISCO ocupatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o ocupație existentă
    /// </summary>
    Task<bool> UpdateAsync(OcupatieISCO ocupatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o ocupație (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id, string modificatDe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifică dacă un cod ISCO este unic
    /// </summary>
    Task<bool> IsUniqueAsync(string codISCO, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
