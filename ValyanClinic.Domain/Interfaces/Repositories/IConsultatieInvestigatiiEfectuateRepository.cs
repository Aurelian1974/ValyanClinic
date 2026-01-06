using ValyanClinic.Domain.Entities.Investigatii;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru Investigații Imagistice Efectuate
/// </summary>
public interface IConsultatieInvestigatieImagisticaEfectuataRepository
{
    /// <summary>
    /// Creează o nouă investigație imagistică efectuată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieInvestigatieImagisticaEfectuata investigatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate investigațiile imagistice efectuate pentru un pacient
    /// </summary>
    Task<IEnumerable<ConsultatieInvestigatieImagisticaEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate investigațiile imagistice efectuate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieInvestigatieImagisticaEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o investigație imagistică efectuată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieInvestigatieImagisticaEfectuata investigatie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o investigație imagistică efectuată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o investigație după ID
    /// </summary>
    Task<ConsultatieInvestigatieImagisticaEfectuata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pentru Explorări Funcționale Efectuate
/// </summary>
public interface IConsultatieExplorareEfectuataRepository
{
    /// <summary>
    /// Creează o nouă explorare funcțională efectuată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieExplorareEfectuata explorare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate explorările funcționale efectuate pentru un pacient
    /// </summary>
    Task<IEnumerable<ConsultatieExplorareEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate explorările funcționale efectuate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieExplorareEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o explorare funcțională efectuată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieExplorareEfectuata explorare, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o explorare funcțională efectuată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o explorare după ID
    /// </summary>
    Task<ConsultatieExplorareEfectuata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pentru Endoscopii Efectuate
/// </summary>
public interface IConsultatieEndoscopieEfectuataRepository
{
    /// <summary>
    /// Creează o nouă endoscopie efectuată
    /// </summary>
    Task<Guid> CreateAsync(ConsultatieEndoscopieEfectuata endoscopie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate endoscopiile efectuate pentru un pacient
    /// </summary>
    Task<IEnumerable<ConsultatieEndoscopieEfectuata>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține toate endoscopiile efectuate pentru o consultație
    /// </summary>
    Task<IEnumerable<ConsultatieEndoscopieEfectuata>> GetByConsultatieIdAsync(Guid consultatieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează o endoscopie efectuată
    /// </summary>
    Task<bool> UpdateAsync(ConsultatieEndoscopieEfectuata endoscopie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge o endoscopie efectuată
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o endoscopie după ID
    /// </summary>
    Task<ConsultatieEndoscopieEfectuata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
