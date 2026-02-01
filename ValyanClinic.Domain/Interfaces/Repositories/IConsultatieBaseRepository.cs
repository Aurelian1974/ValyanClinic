using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru operațiile CRUD de bază ale consultațiilor medicale.
/// Respectă Interface Segregation Principle - doar operații core.
/// </summary>
public interface IConsultatieBaseRepository
{
    // ==================== CREATE ====================

    /// <summary>
    /// Creează o consultație nouă în baza de date
    /// </summary>
    Task<Guid> CreateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    // ==================== READ ====================

    /// <summary>
    /// Obține o consultație după ID
    /// </summary>
    Task<Consultatie?> GetByIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate consultațiile pentru un pacient
    /// </summary>
    Task<IEnumerable<Consultatie>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate consultațiile pentru un medic
    /// </summary>
    Task<IEnumerable<Consultatie>> GetByMedicIdAsync(Guid medicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține consultația pentru o programare specifică
    /// </summary>
    Task<Consultatie?> GetByProgramareIdAsync(Guid programareId, CancellationToken cancellationToken = default);

    // ==================== UPDATE ====================

    /// <summary>
    /// Actualizează o consultație existentă (toate câmpurile)
    /// </summary>
    Task<bool> UpdateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    // ==================== DELETE ====================

    /// <summary>
    /// Șterge o consultație (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    // ==================== SECTION UPDATES (Upsert) ====================

    /// <summary>
    /// Upsert Motiv Prezentare - insert sau update
    /// </summary>
    Task UpsertMotivePrezentareAsync(Guid consultatieId, ConsultatieMotivePrezentare motivePrezentare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert Antecedente - insert sau update
    /// </summary>
    Task UpsertAntecedenteAsync(Guid consultatieId, ConsultatieAntecedente antecedente, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert Examen Obiectiv - insert sau update
    /// </summary>
    Task UpsertExamenObiectivAsync(Guid consultatieId, ConsultatieExamenObiectiv examenObiectiv, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert Investigații - insert sau update
    /// </summary>
    Task UpsertInvestigatiiAsync(Guid consultatieId, ConsultatieInvestigatii investigatii, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert Diagnostic - insert sau update
    /// </summary>
    Task UpsertDiagnosticAsync(Guid consultatieId, ConsultatieDiagnostic diagnostic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert Tratament - insert sau update
    /// </summary>
    Task UpsertTratamentAsync(Guid consultatieId, ConsultatieTratament tratament, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert Concluzii - insert sau update
    /// </summary>
    Task UpsertConcluziiAsync(Guid consultatieId, ConsultatieConcluzii concluzii, CancellationToken cancellationToken = default);

    /// <summary>
    /// Înlocuiește toate medicamentele pentru o consultație
    /// </summary>
    Task ReplaceMedicamenteAsync(Guid consultatieId, IEnumerable<ConsultatieMedicament> medicamente, string creatDe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sincronizează diagnosticele secundare pentru o consultație
    /// </summary>
    Task SyncDiagnosticeSecundareAsync(Guid consultatieId, IEnumerable<ConsultatieDiagnosticSecundar> diagnostice, string modificatDe, CancellationToken cancellationToken = default);
}
