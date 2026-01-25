using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru gestionarea secțiunilor de detalii ale consultațiilor.
/// Operații Upsert (Insert/Update) pentru secțiunile Scrisorii Medicale (Anexa 43).
/// </summary>
public interface IConsultatieDetailsRepository
{
    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieMotivePrezentare (1:1).
    /// Secțiunea I din Scrisoarea Medicală - motivul prezentării pacientului.
    /// </summary>
    Task UpsertMotivePrezentareAsync(Guid consultatieId, ConsultatieMotivePrezentare entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieAntecedente (1:1).
    /// Secțiunea II din Scrisoarea Medicală - antecedente medicale (AHC, AF, APP).
    /// </summary>
    Task UpsertAntecedenteAsync(Guid consultatieId, ConsultatieAntecedente entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieExamenObiectiv (1:1).
    /// Secțiunea III din Scrisoarea Medicală - examen fizic și semne vitale.
    /// </summary>
    Task UpsertExamenObiectivAsync(Guid consultatieId, ConsultatieExamenObiectiv entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieInvestigatii (1:1).
    /// Secțiunea IV din Scrisoarea Medicală - investigații recomandate/efectuate.
    /// </summary>
    Task UpsertInvestigatiiAsync(Guid consultatieId, ConsultatieInvestigatii entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieConcluzii (1:1).
    /// Secțiunea IX din Scrisoarea Medicală - concluzii și prognostic.
    /// </summary>
    Task UpsertConcluziiAsync(Guid consultatieId, ConsultatieConcluzii entity);
}
