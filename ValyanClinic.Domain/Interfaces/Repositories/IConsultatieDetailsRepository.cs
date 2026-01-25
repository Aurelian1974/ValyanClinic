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
    /// <summary>
/// Upserts the "Motive prezentare" (Section I) detail of a consultation's medical letter.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose "Motive prezentare" section will be inserted or updated.</param>
/// <param name="entity">The data for the "Motive prezentare" section to persist.</param>
    Task UpsertMotivePrezentareAsync(Guid consultatieId, ConsultatieMotivePrezentare entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieAntecedente (1:1).
    /// Secțiunea II din Scrisoarea Medicală - antecedente medicale (AHC, AF, APP).
    /// <summary>
/// Upserts the "Antecedente" (medical history) section for the specified consultation.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose antecedents are being upserted.</param>
/// <param name="entity">The antecedents data to insert or update for the consultation.</param>
    Task UpsertAntecedenteAsync(Guid consultatieId, ConsultatieAntecedente entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieExamenObiectiv (1:1).
    /// Secțiunea III din Scrisoarea Medicală - examen fizic și semne vitale.
    /// <summary>
/// Upserts the "Examen obiectiv" (physical examination and vital signs) section for the specified consultation.
/// </summary>
/// <param name="consultatieId">The consultation's unique identifier.</param>
/// <param name="entity">The physical examination and vital signs data to insert or update for the consultation.</param>
    Task UpsertExamenObiectivAsync(Guid consultatieId, ConsultatieExamenObiectiv entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieInvestigatii (1:1).
    /// Secțiunea IV din Scrisoarea Medicală - investigații recomandate/efectuate.
    /// <summary>
/// Upserts the "Investigatii" section of a consultation's medical letter (insert if missing, otherwise update).
/// </summary>
/// <param name="consultatieId">Identifier of the consultation whose Investigatii section will be upserted.</param>
/// <param name="entity">The Investigatii data to insert or update for the consultation.</param>
    Task UpsertInvestigatiiAsync(Guid consultatieId, ConsultatieInvestigatii entity);

    /// <summary>
    /// Upsert (INSERT/UPDATE) ConsultatieConcluzii (1:1).
    /// Secțiunea IX din Scrisoarea Medicală - concluzii și prognostic.
    /// <summary>
/// Inserează sau actualizează secțiunea "Concluzii" (Anexa 43, Secțiunea IX) asociată unei consultații.
/// </summary>
/// <param name="consultatieId">Identificatorul consultației pentru care se face upsert-ul.</param>
/// <param name="entity">Datele secțiunii de concluzii și prognostic ce trebuie salvate.</param>
    Task UpsertConcluziiAsync(Guid consultatieId, ConsultatieConcluzii entity);
}