using ValyanClinic.Domain.Entities.Investigatii;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru Investigații Imagistice Recomandate
/// </summary>
public interface IConsultatieInvestigatieImagisticaRecomandataRepository
{
    /// <summary>
    /// Creează o nouă investigație imagistică recomandată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieInvestigatieImagisticaRecomandata investigatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate investigațiile imagistice recomandate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieInvestigatieImagisticaRecomandata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o investigație imagistică recomandată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieInvestigatieImagisticaRecomandata investigatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o investigație imagistică recomandată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează statusul unei investigații
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid id, string status, Guid modificatDe, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pentru Explorări Funcționale Recomandate
/// </summary>
public interface IConsultatieExplorareRecomandataRepository
{
    /// <summary>
    /// Creează o nouă explorare funcțională recomandată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieExplorareRecomandata explorare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate explorările funcționale recomandate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieExplorareRecomandata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o explorare funcțională recomandată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieExplorareRecomandata explorare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o explorare funcțională recomandată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează statusul unei explorări
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid id, string status, Guid modificatDe, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pentru Endoscopii Recomandate
/// </summary>
public interface IConsultatieEndoscopieRecomandataRepository
{
    /// <summary>
    /// Creează o nouă endoscopie recomandată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieEndoscopieRecomandata endoscopie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate endoscopiile recomandate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieEndoscopieRecomandata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o endoscopie recomandată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieEndoscopieRecomandata endoscopie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o endoscopie recomandată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează statusul unei endoscopii
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid id, string status, Guid modificatDe, CancellationToken cancellationToken = default);
}
