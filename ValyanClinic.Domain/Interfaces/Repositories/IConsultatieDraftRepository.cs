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
    /// </summary>
    Task<Guid> SaveDraftAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută o consultație draft (nefinalizată) pentru un pacient.
    /// Folosit pentru a preveni crearea de consultații duplicate.
    /// </summary>
    /// <param name="pacientId">ID-ul pacientului</param>
    /// <param name="medicId">ID-ul medicului (opțional)</param>
    /// <param name="dataConsultatie">Data consultației (implicit azi)</param>
    /// <param name="programareId">ID-ul programării (opțional)</param>
    /// <returns>Consultația draft existentă sau null</returns>
    Task<Consultatie?> GetDraftByPacientAsync(
        Guid pacientId,
        Guid? medicId = null,
        DateTime? dataConsultatie = null,
        Guid? programareId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalizează consultație (cu validări și update status programare).
    /// Marchează consultația ca finalizată și actualizează statusul programării asociate.
    /// </summary>
    Task<bool> FinalizeAsync(Guid consultatieId, int durataMinute, Guid modificatDe, CancellationToken cancellationToken = default);
}
