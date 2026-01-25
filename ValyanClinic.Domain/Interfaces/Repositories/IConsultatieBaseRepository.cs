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
}
