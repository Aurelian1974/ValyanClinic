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
    /// </summary>
    Task UpsertDiagnosticAsync(Guid consultatieId, ConsultatieDiagnostic entity);

    /// <summary>
    /// Sync (DELETE + INSERT) ConsultatieDiagnosticSecundar (1:N) - Diagnostice Secundare.
    /// Șterge toate diagnosticele secundare existente și inserează noile (max 10).
    /// </summary>
    Task SyncDiagnosticeSecundareAsync(Guid consultatieId, IEnumerable<ConsultatieDiagnosticSecundar> diagnostice, Guid modificatDe);

    /// <summary>
    /// Obține toate diagnosticele secundare pentru o consultație.
    /// </summary>
    Task<IEnumerable<ConsultatieDiagnosticSecundar>> GetDiagnosticeSecundareAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    // ==================== TRATAMENT (Secțiunea VI din Scrisoarea Medicală) ====================

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieTratament (1:1).
    /// Secțiunea VI din Scrisoarea Medicală - tratament medicamentos și nemedicamentos.
    /// </summary>
    Task UpsertTratamentAsync(Guid consultatieId, ConsultatieTratament entity);

    /// <summary>
    /// Replace all medications for a consultation (delete + insert).
    /// Înlocuiește complet lista de medicamente prescrise.
    /// </summary>
    Task ReplaceMedicamenteAsync(Guid consultatieId, IEnumerable<ConsultatieMedicament> medicamente, Guid modificatDe);

    /// <summary>
    /// Obține toate medicamentele prescrise pentru o consultație.
    /// </summary>
    Task<IEnumerable<ConsultatieMedicament>> GetMedicamenteAsync(Guid consultatieId, CancellationToken cancellationToken = default);
}
