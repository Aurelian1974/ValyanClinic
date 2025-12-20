using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru ICD10Code - Clasificarea Internationala a Bolilor
/// </summary>
public interface IICD10Repository
{
    /// <summary>
    /// Cauta coduri ICD-10 pentru autocomplete
    /// </summary>
    Task<IEnumerable<ICD10Code>> SearchAsync(
        string searchTerm,
     string? category = null,
     bool onlyCommon = false,
        int maxResults = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine un cod ICD-10 dupa ID
    /// </summary>
    Task<ICD10Code?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine un cod ICD-10 dupa codul exact
    /// </summary>
    Task<ICD10Code?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine toate codurile dintr-o categorie
    /// </summary>
    Task<IEnumerable<ICD10Code>> GetByCategoryAsync(
        string category,
        bool onlyLeafNodes = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine codurile COMUNE (cele mai folosite)
    /// </summary>
    Task<IEnumerable<ICD10Code>> GetCommonCodesAsync(
        string? category = null,
        int maxResults = 50,
 CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine codurile copil pentru un cod parinte
    /// </summary>
    Task<IEnumerable<ICD10Code>> GetChildCodesAsync(
   string parentCode,
   CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine toate categoriile distincte
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine statistici pentru dashboard
    /// </summary>
    Task<(int totalCodes, int commonCodes, int categories)> GetStatisticsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica daca un cod ICD-10 este valid
    /// </summary>
    Task<bool> IsValidCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import bulk codes (pentru populare initiala)
    /// </summary>
    Task<int> BulkImportAsync(
        IEnumerable<ICD10Code> codes,
    CancellationToken cancellationToken = default);
}
