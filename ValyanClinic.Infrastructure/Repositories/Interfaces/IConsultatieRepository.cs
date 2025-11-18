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
    /// Actualizeaza o consultatie existenta
    /// </summary>
    Task<bool> UpdateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sterge o consultatie (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid consultatieId, CancellationToken cancellationToken = default);
}
