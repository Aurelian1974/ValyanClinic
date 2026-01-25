using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru operațiile de draft ale consultațiilor medicale.
/// Gestionează consultații incomplete/în curs de completare.
/// </summary>
public interface IConsultatieDraftRepository
{
    /// <summary>
    /// Salvează draft consultație (auto-save cu câmpuri esențiale).
    /// Permite salvarea incrementală a consultației pe măsură ce medicul o completează.
    /// <summary>
/// Saves a draft of the provided consultation and returns its identifier.
/// </summary>
/// <param name="consultatie">The consultation draft to persist (partial or in-progress data).</param>
/// <returns>The Guid that identifies the saved draft.</returns>
    Task<Guid> SaveDraftAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută o consultație draft (nefinalizată) pentru un pacient.
    /// Folosit pentru a preveni crearea de consultații duplicate.
    /// </summary>
    /// <param name="pacientId">ID-ul pacientului</param>
    /// <param name="medicId">ID-ul medicului (opțional)</param>
    /// <param name="dataConsultatie">Data consultației (implicit azi)</param>
    /// <param name="programareId">ID-ul programării (opțional)</param>
    /// <summary>
        /// Retrieves an unfinalized draft consultation for the specified patient, optionally filtered by clinician, consultation date, and appointment.
        /// </summary>
        /// <param name="pacientId">Identifier of the patient to find the draft for.</param>
        /// <param name="medicId">Optional identifier of the clinician to narrow the search.</param>
        /// <param name="dataConsultatie">Optional consultation date to match; if null, the search defaults to today's date.</param>
        /// <param name="programareId">Optional appointment identifier to narrow the search.</param>
        /// <param name="cancellationToken">Token to observe while waiting for the operation to complete.</param>
        /// <returns>The matching draft consultation, or null if none is found.</returns>
    Task<Consultatie?> GetDraftByPacientAsync(
        Guid pacientId,
        Guid? medicId = null,
        DateTime? dataConsultatie = null,
        Guid? programareId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalizează consultație (cu validări și update status programare).
    /// Marchează consultația ca finalizată și actualizează statusul programării asociate.
    /// <summary>
/// Finalize a draft consultation and update its state and related scheduling information.
/// </summary>
/// <param name="consultatieId">Identifier of the draft consultation to finalize.</param>
/// <param name="durataMinute">Duration in minutes to assign to the finalized consultation.</param>
/// <param name="modificatDe">Identifier of the user who performs the finalization.</param>
/// <param name="cancellationToken">Token to observe while waiting for the operation to complete.</param>
/// <returns>`true` if the consultation was finalized and related status updated, `false` otherwise.</returns>
    Task<bool> FinalizeAsync(Guid consultatieId, int durataMinute, Guid modificatDe, CancellationToken cancellationToken = default);
}