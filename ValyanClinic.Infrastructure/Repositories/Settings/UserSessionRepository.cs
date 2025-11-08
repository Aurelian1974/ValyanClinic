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

    // ✅ NOU: Obține sesiuni active cu detalii utilizator (JOIN)
  public async Task<IEnumerable<(
    Guid SessionID,
        Guid UtilizatorID,
   string Username,
   string Email,
        string Rol,
   string SessionToken,
     string AdresaIP,
    string? UserAgent,
  string? Dispozitiv,
    DateTime DataCreare,
DateTime DataUltimaActivitate,
   DateTime DataExpirare,
bool EsteActiva
 )>> GetActiveSessionsWithDetailsAsync(
    Guid? utilizatorId = null,
 bool doarExpiraInCurand = false,
    string sortColumn = "DataUltimaActivitate",
       string sortDirection = "DESC",
CancellationToken cancellationToken = default)
    {
  using var connection = _connectionFactory.CreateConnection();
    
        // Build WHERE clause
     var where = "WHERE us.EsteActiva = 1";
    if (utilizatorId.HasValue)
      where += " AND us.UtilizatorID = @UtilizatorID";
       if (doarExpiraInCurand)
   where += " AND DATEDIFF(MINUTE, GETDATE(), us.DataExpirare) < 15";
   
     // Build ORDER BY clause
     var orderBy = sortDirection.ToUpper() == "ASC" ? "ASC" : "DESC";
    var validColumns = new[] { "DataCreare", "DataUltimaActivitate", "DataExpirare", "Username", "AdresaIP" };
      var safeColumn = validColumns.Contains(sortColumn) ? sortColumn : "DataUltimaActivitate";
        
    var query = $@"
 SELECT 
   us.SessionID,
    us.UtilizatorID,
    u.Username,
   u.Email,
   u.Rol,
    us.SessionToken,
  us.AdresaIP,
    us.UserAgent,
     us.Dispozitiv,
      us.DataCreare,
    us.DataUltimaActivitate,
        us.DataExpirare,
       us.EsteActiva
 FROM UserSessions us
    INNER JOIN Utilizatori u ON us.UtilizatorID = u.UtilizatorID
   {where}
        ORDER BY us.{safeColumn} {orderBy}";
     
    var results = await connection.QueryAsync<dynamic>(query, new { UtilizatorID = utilizatorId });
  
        return results.Select(r => (
     (Guid)r.SessionID,
     (Guid)r.UtilizatorID,
     (string)r.Username,
       (string)r.Email,
 (string)r.Rol,
      (string)r.SessionToken,
            (string)r.AdresaIP,
   (string?)r.UserAgent,
    (string?)r.Dispozitiv,
    (DateTime)r.DataCreare,
        (DateTime)r.DataUltimaActivitate,
   (DateTime)r.DataExpirare,
 (bool)r.EsteActiva
   ));
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
    
       // ✅ SP folosește OUTPUT parameters pentru SessionID și SessionToken
    var parameters = new DynamicParameters();
       parameters.Add("@UtilizatorID", utilizatorId);
     parameters.Add("@AdresaIP", adresaIp);
       parameters.Add("@UserAgent", userAgent);
    parameters.Add("@Dispozitiv", dispozitiv);
    parameters.Add("@SessionToken", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
       parameters.Add("@SessionID", dbType: DbType.Guid, direction: ParameterDirection.Output);
      
       await connection.ExecuteAsync(
       "SP_CreateUserSession",
         parameters,
     commandType: CommandType.StoredProcedure);
  
  var sessionId = parameters.Get<Guid>("@SessionID");
        var sessionToken = parameters.Get<string>("@SessionToken");
  
  return (sessionId, sessionToken);
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

    // ✅ NOU: Închide o sesiune (logout forțat)
public async Task<bool> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
{
     using var connection = _connectionFactory.CreateConnection();
   
       var result = await connection.ExecuteAsync(@"
    UPDATE UserSessions 
         SET EsteActiva = 0, 
     DataUltimaActivitate = GETDATE()
WHERE SessionID = @SessionID",
       new { SessionID = sessionId });
  
     return result > 0;
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
  return await connection.ExecuteScalarAsync<int>(
"SP_CleanupExpiredSessions",
     commandType: CommandType.StoredProcedure);
    }
  
  // ✅ NOU: Obține statistici sesiuni
    public async Task<(int TotalActive, int ExpiraInCurand, int InactiviAzi)> GetStatisticsAsync(
  CancellationToken cancellationToken = default)
 {
   using var connection = _connectionFactory.CreateConnection();
     
var query = @"
     SELECT 
          (SELECT COUNT(*) FROM UserSessions WHERE EsteActiva = 1) AS TotalActive,
   (SELECT COUNT(*) FROM UserSessions 
         WHERE EsteActiva = 1 AND DATEDIFF(MINUTE, GETDATE(), DataExpirare) < 15) AS ExpiraInCurand,
     (SELECT COUNT(*) FROM UserSessions 
 WHERE EsteActiva = 0 AND CAST(DataUltimaActivitate AS DATE) = CAST(GETDATE() AS DATE)) AS InactiviAzi";
   
      var result = await connection.QuerySingleAsync<dynamic>(query);
    
 return (
       (int)result.TotalActive,
      (int)result.ExpiraInCurand,
(int)result.InactiviAzi
        );
    }
}
