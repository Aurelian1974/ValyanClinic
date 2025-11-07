using ValyanClinic.Domain.Entities.Settings;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface IUserSessionRepository
{
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid utilizatorId, CancellationToken cancellationToken = default);
    Task<(Guid SessionId, string SessionToken)> CreateAsync(Guid utilizatorId, string adresaIp, string userAgent, string dispozitiv, CancellationToken cancellationToken = default);
    Task<bool> UpdateActivityAsync(string sessionToken, CancellationToken cancellationToken = default);
  Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
