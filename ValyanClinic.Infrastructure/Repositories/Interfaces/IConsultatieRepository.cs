using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface pentru gestionarea consultatiilor medicale
/// </summary>
public interface IConsultatieRepository
{
    /// <summary>
    /// Creaza o consultatie noua in baza de date
    /// </summary>
    Task<Guid> CreateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine o consultatie dupa ID
    /// </summary>
    Task<Consultatie?> GetByIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine toate consultatiile pentru un pacient
    /// </summary>
    Task<IEnumerable<Consultatie>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine toate consultatiile pentru un medic
    /// </summary>
    Task<IEnumerable<Consultatie>> GetByMedicIdAsync(Guid medicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtine consultatia pentru o programare
    /// </summary>
    Task<Consultatie?> GetByProgramareIdAsync(Guid programareId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizeaza o consultatie existenta (toate campurile)
    /// </summary>
    Task<bool> UpdateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salveaza draft consultatie (auto-save cu campuri esentiale)
    /// </summary>
    Task<Guid> SaveDraftAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalizeaza consultatie (cu validari si update status programare)
    /// </summary>
    Task<bool> FinalizeAsync(Guid consultatieId, int durataMinute, Guid modificatDe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sterge o consultatie (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută o consultație draft (nefinalizată) pentru un pacient
    /// Folosit pentru a preveni crearea de consultații duplicate
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
}
