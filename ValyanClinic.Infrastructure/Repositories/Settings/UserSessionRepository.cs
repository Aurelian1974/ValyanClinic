using Dapper;
using System.Data;
using ValyanClinic.Domain.Entities.Settings;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Settings;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserSessionRepository(IDbConnectionFactory connectionFactory)
    {
   _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
   using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserSession>(
 "SELECT * FROM VW_ActiveSessions ORDER BY DataUltimaActivitate DESC");
    }

    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid utilizatorId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserSession>(
            "SELECT * FROM VW_ActiveSessions WHERE UtilizatorID = @UtilizatorID",
    new { UtilizatorID = utilizatorId });
    }

public async Task<(Guid SessionId, string SessionToken)> CreateAsync(Guid utilizatorId, string adresaIp,
        string userAgent, string dispozitiv, CancellationToken cancellationToken = default)
 {
 using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(
    "SP_CreateUserSession",
  new { UtilizatorID = utilizatorId, AdresaIP = adresaIp, UserAgent = userAgent, Dispozitiv = dispozitiv },
commandType: CommandType.StoredProcedure);
   return (result?.SessionID ?? Guid.Empty, result?.SessionToken ?? string.Empty);
    }

    public async Task<bool> UpdateActivityAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
  using var connection = _connectionFactory.CreateConnection();
     var result = await connection.ExecuteAsync(
       "SP_UpdateSessionActivity",
    new { SessionToken = sessionToken },
   commandType: CommandType.StoredProcedure);
     return result > 0;
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
   using var connection = _connectionFactory.CreateConnection();
    return await connection.ExecuteScalarAsync<int>(
      "SP_CleanupExpiredSessions",
       commandType: CommandType.StoredProcedure);
    }
}
