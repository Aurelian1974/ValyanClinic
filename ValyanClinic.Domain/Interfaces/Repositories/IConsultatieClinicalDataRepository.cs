using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru gestionarea datelor clinice ale consultațiilor.
/// Diagnostic (ICD-10) și Tratament (medicamente).
/// </summary>
public interface IConsultatieClinicalDataRepository
{
    // ==================== DIAGNOSTIC (Secțiunea V din Scrisoarea Medicală) ====================

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieDiagnostic (1:1) - Diagnostic Principal.
    /// Secțiunea V din Scrisoarea Medicală - diagnostic principal cu cod ICD-10.
    /// <summary>
/// Upserts the consultation's principal diagnostic (Section V) identified by ICD-10 code.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose principal diagnostic will be upserted.</param>
/// <param name="entity">The principal diagnostic data to insert or update.</param>
    Task UpsertDiagnosticAsync(Guid consultatieId, ConsultatieDiagnostic entity);

    /// <summary>
    /// Sync (DELETE + INSERT) ConsultatieDiagnosticSecundar (1:N) - Diagnostice Secundare.
    /// Șterge toate diagnosticele secundare existente și inserează noile (max 10).
    /// <summary>
/// Replace the secondary diagnoses for a consultation with the supplied collection, recording who performed the modification.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose secondary diagnoses will be synchronized.</param>
/// <param name="diagnostice">The new set of secondary diagnoses to apply (up to 10 items).</param>
/// <param name="modificatDe">Identifier of the user who made the modification.</param>
    Task SyncDiagnosticeSecundareAsync(Guid consultatieId, IEnumerable<ConsultatieDiagnosticSecundar> diagnostice, Guid modificatDe);

    /// <summary>
    /// Obține toate diagnosticele secundare pentru o consultație.
    /// <summary>
/// Retrieves all secondary diagnoses associated with a consultation.
/// </summary>
/// <param name="consultatieId">The consultation identifier whose secondary diagnoses to retrieve.</param>
/// <returns>An enumerable of secondary diagnosis records for the specified consultation; empty if none exist.</returns>
    Task<IEnumerable<ConsultatieDiagnosticSecundar>> GetDiagnosticeSecundareAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    // ==================== TRATAMENT (Secțiunea VI din Scrisoarea Medicală) ====================

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieTratament (1:1).
    /// Secțiunea VI din Scrisoarea Medicală - tratament medicamentos și nemedicamentos.
    /// <summary>
/// Inserts or updates the consultation's principal treatment record (Section VI) so a single treatment is stored for the specified consultation.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose treatment will be created or updated.</param>
/// <param name="entity">Treatment data to upsert.</param>
    Task UpsertTratamentAsync(Guid consultatieId, ConsultatieTratament entity);

    /// <summary>
    /// Replace all medications for a consultation (delete + insert).
    /// Înlocuiește complet lista de medicamente prescrise.
    /// <summary>
/// Replace the entire set of prescribed medications for a consultation with the provided collection.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose medications will be replaced.</param>
/// <param name="medicamente">The complete collection of medications to store for the consultation; an empty collection removes all medications.</param>
/// <param name="modificatDe">Identifier of the user who performed the modification.</param>
    Task ReplaceMedicamenteAsync(Guid consultatieId, IEnumerable<ConsultatieMedicament> medicamente, Guid modificatDe);

    /// <summary>
    /// Obține toate medicamentele prescrise pentru o consultație.
    /// <summary>
/// Retrieves all medications prescribed for the specified consultation.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose medications are requested.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>An enumerable of prescribed medications for the consultation; empty if none are found.</returns>
    Task<IEnumerable<ConsultatieMedicament>> GetMedicamenteAsync(Guid consultatieId, CancellationToken cancellationToken = default);
}