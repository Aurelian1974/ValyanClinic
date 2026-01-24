using System.Data;

namespace ValyanClinic.Domain.Interfaces.Data;

/// <summary>
/// Factory interface pentru crearea conexiunilor la baza de date.
/// Abstracție care permite testarea și înlocuirea implementărilor de data access.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Crează o nouă conexiune la baza de date.
    /// </summary>
    /// <returns>Conexiune IDbConnection configurată.</returns>
    IDbConnection CreateConnection();

    /// <summary>
    /// Testează conectivitatea la baza de date.
    /// </summary>
    /// <param name="cancellationToken">Token pentru anularea operației.</param>
    /// <returns>True dacă conexiunea este validă, false altfel.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
