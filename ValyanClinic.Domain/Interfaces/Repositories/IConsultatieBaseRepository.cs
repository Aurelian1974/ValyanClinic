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
    /// <summary>
/// Creates a new consultation record from the provided Consultatie entity and persists it.
/// </summary>
/// <param name="consultatie">The consultation entity to create.</param>
/// <returns>The Guid of the newly created consultation.</returns>
    Task<Guid> CreateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    // ==================== READ ====================

    /// <summary>
    /// Obține o consultație după ID
    /// <summary>
/// Retrieves the consultation with the specified identifier.
/// </summary>
/// <param name="consultatieId">Identifier of the consultation to retrieve.</param>
/// <returns>The consultation with the specified identifier, or null if no matching consultation exists.</returns>
    Task<Consultatie?> GetByIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate consultațiile pentru un pacient
    /// <summary>
/// Retrieves all consultations for the specified patient.
/// </summary>
/// <param name="pacientId">The patient's unique identifier.</param>
/// <param name="cancellationToken">A token to cancel the operation.</param>
/// <returns>An enumerable of consultations associated with the patient; empty if none are found.</returns>
    Task<IEnumerable<Consultatie>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate consultațiile pentru un medic
    /// <summary>
/// Retrieves all consultations associated with the specified doctor.
/// </summary>
/// <param name="medicId">The identifier of the doctor whose consultations to retrieve.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>An enumerable of consultations for the specified doctor; empty if none are found.</returns>
    Task<IEnumerable<Consultatie>> GetByMedicIdAsync(Guid medicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține consultația pentru o programare specifică
    /// <summary>
/// Retrieves the consultation associated with a specific appointment.
/// </summary>
/// <param name="programareId">The identifier of the appointment.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>The consultation linked to the given appointment, or `null` if none exists.</returns>
    Task<Consultatie?> GetByProgramareIdAsync(Guid programareId, CancellationToken cancellationToken = default);

    // ==================== UPDATE ====================

    /// <summary>
    /// Actualizează o consultație existentă (toate câmpurile)
    /// <summary>
/// Updates an existing consultation with the provided values.
/// </summary>
/// <param name="consultatie">The consultation entity containing updated values; its Id identifies which record to update.</param>
/// <param name="cancellationToken">Token to observe while waiting for the operation to complete.</param>
/// <returns>`true` if the consultation was updated, `false` otherwise.</returns>
    Task<bool> UpdateAsync(Consultatie consultatie, CancellationToken cancellationToken = default);

    // ==================== DELETE ====================

    /// <summary>
    /// Șterge o consultație (soft delete)
    /// <summary>
/// Performs a soft delete of the consultation identified by the given ID.
/// </summary>
/// <param name="consultatieId">The identifier of the consultation to delete.</param>
/// <returns>`true` if the consultation was marked as deleted, `false` otherwise.</returns>
    Task<bool> DeleteAsync(Guid consultatieId, CancellationToken cancellationToken = default);
}