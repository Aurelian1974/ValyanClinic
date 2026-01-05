using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru nomenclator analize medicale
/// </summary>
public interface IAnalizaMedicalaRepository
{
    // ==================== SEARCH & FILTER ====================
    
    /// <summary>
    /// Căutare analize în nomenclator cu filtre
    /// </summary>
    Task<(IEnumerable<AnalizaMedicala> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        Guid? categorieId,
        Guid? laboratorId,
        bool doarActive = true,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține analiză din nomenclator după ID
    /// </summary>
    Task<AnalizaMedicala?> GetByIdAsync(Guid analizaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate categoriile cu număr de analize
    /// </summary>
    Task<IEnumerable<AnalizaMedicalaCategorie>> GetCategoriiAsync(
        bool doarActive = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate laboratoarele
    /// </summary>
    Task<IEnumerable<AnalizaMedicalaLaborator>> GetLaboratoareAsync(
        bool doarActive = true,
        CancellationToken cancellationToken = default);

    // ==================== AUTOCOMPLETE ====================
    
    /// <summary>
    /// Autocomplete pentru nume analiză (top 10 matches)
    /// </summary>
    Task<IEnumerable<AnalizaMedicala>> AutocompleteAsync(
        string searchTerm,
        int maxResults = 10,
        CancellationToken cancellationToken = default);
}
