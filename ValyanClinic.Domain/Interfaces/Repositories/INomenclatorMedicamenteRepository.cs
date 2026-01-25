using ValyanClinic.Domain.Models;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru accesarea nomenclatorului de medicamente ANM.
/// Abstractizează accesul la baza de date pentru operațiile CRUD pe Medicamente.
/// </summary>
/// <remarks>
/// Stored Procedures folosite:
/// - Medicamente_Search
/// - Medicamente_GetByCod
/// - Medicamente_Upsert
/// - Medicamente_GetStats
/// - Medicamente_DeactivateOld
/// - Medicamente_SyncLog_Start
/// - Medicamente_SyncLog_Complete
/// </remarks>
public interface INomenclatorMedicamenteRepository
{
    /// <summary>
    /// Caută medicamente după termen de căutare.
    /// Folosește stored procedure Medicamente_Search.
    /// </summary>
    /// <param name="searchTerm">Termen de căutare (min 2 caractere)</param>
    /// <param name="maxResults">Număr maxim de rezultate</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de medicamente găsite</returns>
    Task<List<MedicamentNomenclator>> SearchAsync(
        string searchTerm,
        int maxResults,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține un medicament după codul CIM.
    /// Folosește stored procedure Medicamente_GetByCod.
    /// </summary>
    /// <param name="codCIM">Codul CIM al medicamentului</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Medicamentul găsit sau null</returns>
    Task<MedicamentNomenclator?> GetByCodeAsync(
        string codCIM,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert sau update medicament în nomenclator.
    /// Folosește stored procedure Medicamente_Upsert.
    /// </summary>
    /// <param name="medicament">Datele medicamentului</param>
    /// <param name="sursaFisier">Numele fișierului sursă (pentru audit)</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>True dacă e nou (insert), False dacă e update</returns>
    Task<bool> UpsertAsync(
        MedicamentNomenclator medicament,
        string sursaFisier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține statistici despre nomenclator.
    /// Folosește stored procedure Medicamente_GetStats.
    /// </summary>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Statistici nomenclator</returns>
    Task<NomenclatorStats> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dezactivează medicamentele care nu sunt în fișierul curent.
    /// Folosește stored procedure Medicamente_DeactivateOld.
    /// </summary>
    /// <param name="currentFileName">Numele fișierului curent</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Numărul de înregistrări dezactivate</returns>
    Task<int> DeactivateOldRecordsAsync(
        string currentFileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pornește un nou log de sincronizare.
    /// Folosește stored procedure Medicamente_SyncLog_Start.
    /// </summary>
    /// <param name="sourceUrl">URL-ul sursei de sincronizare</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>ID-ul log-ului creat</returns>
    Task<Guid> StartSyncLogAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalizează log-ul de sincronizare.
    /// Folosește stored procedure Medicamente_SyncLog_Complete.
    /// </summary>
    /// <param name="logId">ID-ul log-ului</param>
    /// <param name="status">Status sincronizare ("Success" sau "Failed")</param>
    /// <param name="result">Rezultatul sincronizării</param>
    /// <param name="fileName">Numele fișierului importat</param>
    /// <param name="fileSize">Dimensiunea fișierului</param>
    /// <param name="errorMessage">Mesaj de eroare (opțional)</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    Task CompleteSyncLogAsync(
        Guid logId,
        string status,
        SyncResult result,
        string? fileName,
        long? fileSize,
        string? errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține data ultimei sincronizări reușite.
    /// Interogare directă pe tabela Medicamente_SyncLog.
    /// </summary>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Data ultimei sincronizări sau null</returns>
    Task<DateTime?> GetLastSuccessfulSyncDateAsync(CancellationToken cancellationToken = default);
}
