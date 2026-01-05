using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru analize medicale recomandate în timpul consultației.
/// Separat de IConsultatieAnalizaMedicalaRepository (pentru import/rezultate).
/// </summary>
public interface IConsultatieAnalizaRecomandataRepository
{
    /// <summary>
    /// Creează o nouă analiză recomandată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieAnalizaRecomandataEntity analiza, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate analizele recomandate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieAnalizaRecomandataEntity>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o analiză recomandată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o analiză recomandată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieAnalizaRecomandataEntity analiza, CancellationToken cancellationToken = default);
}
