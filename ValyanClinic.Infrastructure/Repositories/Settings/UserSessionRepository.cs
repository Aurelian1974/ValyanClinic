using Dapper;
using System.Data;
using Microsoft.Extensions.Logging;
using ValyanClinic.Domain.Entities.Settings;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Data;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories.Settings;

/// <summary>
/// Repository implementation for user session management using Dapper and SQL Server.
/// Provides session tracking, activity monitoring, and cleanup operations.
/// </summary>
/// <remarks>
/// Implementation details:
/// - Uses Dapper for fast data access with minimal overhead
/// - Leverages stored procedures for complex operations
/// - Uses views (VW_ActiveSessions) for simple queries
/// - Includes error handling and logging for diagnostics
/// 
/// Performance considerations:
/// - Connection pooling managed by IDbConnectionFactory
/// - Parameterized queries prevent SQL injection
/// - Indexed columns (SessionToken, EsteActiva, DataExpirare) for fast lookups
/// - Bulk operations for cleanup
/// 
/// Thread safety:
/// - Stateless repository (no shared mutable state)
/// - Each method creates its own connection (disposable pattern)
/// - Safe for concurrent calls
/// </remarks>
public class UserSessionRepository : IUserSessionRepository
{
    #region Constants

    /// <summary>
    /// Threshold in minutes for "expiring soon" sessions
    /// </summary>
    private const int EXPIRING_SOON_THRESHOLD_MINUTES = 15;

    /// <summary>
    /// Default sort direction
    /// </summary>
    private const string DEFAULT_SORT_DIRECTION = "DESC";

    /// <summary>
    /// Default sort column
    /// </summary>
    private const string DEFAULT_SORT_COLUMN = "DataUltimaActivitate";

    /// <summary>
    /// Valid columns for sorting (whitelist to prevent SQL injection)
    /// </summary>
    private static readonly string[] VALID_SORT_COLUMNS =
    {
        "DataCreare",
        "DataUltimaActivitate",
        "DataExpirare",
        "Username",
        "AdresaIP"
    };

    #endregion

    #region Dependencies

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<UserSessionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSessionRepository"/> class.
    /// </summary>
    /// <param name="connectionFactory">Factory for creating database connections</param>
    /// <param name="logger">Logger for diagnostics and audit trail</param>
    public UserSessionRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<UserSessionRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    #endregion

    #region Query Methods

    /// <inheritdoc />
    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<UserSession>(
                "SELECT * FROM VW_ActiveSessions ORDER BY DataUltimaActivitate DESC");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            throw;
        }
    }

    /// <inheritdoc />
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
        string sortColumn = DEFAULT_SORT_COLUMN,
        string sortDirection = DEFAULT_SORT_DIRECTION,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Validate and sanitize sort parameters (defense in depth)
            var safeColumn = VALID_SORT_COLUMNS.Contains(sortColumn)
                ? sortColumn
                : DEFAULT_SORT_COLUMN;
            var safeDirection = sortDirection.ToUpper() == "ASC" ? "ASC" : DEFAULT_SORT_DIRECTION;

            // Use stored procedure instead of inline SQL
            var parameters = new
            {
                UtilizatorID = utilizatorId,
                DoarExpiraInCurand = doarExpiraInCurand,
                SortColumn = safeColumn,
                SortDirection = safeDirection
            };

            var results = await connection.QueryAsync<dynamic>(
                "SP_GetActiveSessionsWithDetails",
                parameters,
                commandType: CommandType.StoredProcedure);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions with details for user: {UtilizatorId}", utilizatorId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid utilizatorId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<UserSession>(
                "SELECT * FROM VW_ActiveSessions WHERE UtilizatorID = @UtilizatorID",
                new { UtilizatorID = utilizatorId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for user: {UtilizatorId}", utilizatorId);
            throw;
        }
    }

    #endregion

    #region Command Methods

    /// <inheritdoc />
    public async Task<(Guid SessionId, string SessionToken)> CreateAsync(
        Guid utilizatorId,
        string adresaIp,
        string userAgent,
        string dispozitiv,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Setup OUTPUT parameters for stored procedure
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

            _logger.LogInformation("Session created successfully for user: {UtilizatorId}, SessionID: {SessionId}",
                utilizatorId, sessionId);

            return (sessionId, sessionToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UtilizatorId}", utilizatorId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateActivityAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var result = await connection.ExecuteAsync(
                "SP_UpdateSessionActivity",
                new { SessionToken = sessionToken },
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session activity for token: {SessionToken}", sessionToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Use stored procedure instead of inline SQL
            var result = await connection.ExecuteAsync(
                "SP_EndSession",
                new { SessionID = sessionId },
                commandType: CommandType.StoredProcedure);

            if (result > 0)
            {
                _logger.LogInformation("Session ended successfully: {SessionId}", sessionId);
            }
            else
            {
                _logger.LogWarning("Attempted to end non-existent session: {SessionId}", sessionId);
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(
                "SP_CleanupExpiredSessions",
                commandType: CommandType.StoredProcedure);

            if (count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired sessions", count);
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
            throw;
        }
    }

    #endregion

    #region Statistics Methods

    /// <inheritdoc />
    public async Task<(int TotalActive, int ExpiraInCurand, int InactiviAzi)> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Use stored procedure instead of inline SQL
            var result = await connection.QuerySingleAsync<dynamic>(
                "SP_GetSessionStatistics",
                commandType: CommandType.StoredProcedure);

            return (
                (int)result.TotalActive,
                (int)result.ExpiraInCurand,
                (int)result.InactiviAzi
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session statistics");
            throw;
        }
    }

    #endregion
}
