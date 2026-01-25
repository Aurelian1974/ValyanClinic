using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Application.Services.Medicamente;

/// <summary>
/// Serviciu pentru gestionarea nomenclatorului de medicamente ANM.
/// </summary>
public interface INomenclatorMedicamenteService
{
    /// <summary>
    /// Caută medicamente pentru autocomplete.
    /// </summary>
    /// <param name="searchTerm">Termen de căutare (min 2 caractere)</param>
    /// <param name="maxResults">Număr maxim de rezultate</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de medicamente găsite</returns>
    Task<Result<IReadOnlyList<MedicamentNomenclator>>> SearchAsync(
        string searchTerm, 
        int maxResults = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține un medicament după codul CIM.
    /// </summary>
    Task<Result<MedicamentNomenclator?>> GetByCodeAsync(
        string codCIM, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sincronizează nomenclatorul cu sursa ANM (download + import).
    /// </summary>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Rezultatul sincronizării</returns>
    Task<Result<SyncResult>> SyncFromANMAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține statistici despre nomenclator.
    /// </summary>
    Task<Result<NomenclatorStats>> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifică dacă este necesară o sincronizare (bazat pe ultima dată de sync).
    /// </summary>
    Task<bool> NeedsSyncAsync(CancellationToken cancellationToken = default);
}
