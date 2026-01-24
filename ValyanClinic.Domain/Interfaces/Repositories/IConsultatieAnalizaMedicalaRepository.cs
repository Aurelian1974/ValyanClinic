using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru analize medicale în cadrul consultației
/// (analize prescrise și rezultate)
/// </summary>
public interface IConsultatieAnalizaMedicalaRepository
{
    // ==================== CRUD OPERATIONS ====================
    
    /// <summary>
    /// Creează analiză prescrisă în consultație
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieAnalizaMedicala analiza, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează analiză (ex: adăugare rezultate)
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieAnalizaMedicala analiza, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge analiză prescrisă
    /// </summary>
    Task<bool> DeleteAsync(Guid analizaId, CancellationToken cancellationToken = default);

    // ==================== QUERY OPERATIONS ====================
    
    /// <summary>
    /// Obține analiză după ID cu detalii
    /// </summary>
    Task<ConsultatieAnalizaMedicala?> GetByIdAsync(Guid analizaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate analizele pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieAnalizaMedicala>> GetByConsultatieIdAsync(
        Guid consultatieId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține analize pentru un pacient (istoric complet)
    /// </summary>
    Task<IEnumerable<ConsultatieAnalizaMedicala>> GetByPacientIdAsync(
        Guid pacientId,
        bool doarCuRezultate = false,
        DateTime? dataStart = null,
        DateTime? dataEnd = null,
        CancellationToken cancellationToken = default);

    // ==================== DETALII (PARAMETRI) ====================
    
    /// <summary>
    /// Adaugă detaliu (parametru individual) pentru o analiză
    /// </summary>
    Task<Guid> AddDetaliu(ConsultatieAnalizaDetaliu detaliu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate detaliile pentru o analiză
    /// </summary>
    Task<IEnumerable<ConsultatieAnalizaDetaliu>> GetDetaliiByAnalizaIdAsync(
        Guid analizaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează status analiză (Recomandata → Programata → Finalizata)
    /// </summary>
    Task<bool> UpdateStatusAsync(
        Guid analizaId,
        string newStatus,
        CancellationToken cancellationToken = default);

    // ==================== OPERATIONS WITH DETAILS ====================

    /// <summary>
    /// Obține toate analizele pentru o consultație împreună cu detaliile (single query optimized)
    /// </summary>
    Task<IEnumerable<ConsultatieAnalizaMedicala>> GetByConsultatieIdWithDetailsAsync(
        Guid consultatieId,
        CancellationToken cancellationToken = default);
}
