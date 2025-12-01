using ValyanClinic.Domain.Entities.Settings;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for user session management.
/// Handles session tracking, activity monitoring, and cleanup operations.
/// </summary>
/// <remarks>
/// User sessions are created when users log in and track:
/// - Session tokens (for stateless authentication if needed)
/// - IP addresses and device information (for security audit)
/// - Activity timestamps (for automatic timeout)
/// - Expiration dates (for cleanup)
/// 
/// Session lifecycle:
/// 1. CreateAsync() - Create session on login
/// 2. UpdateActivityAsync() - Update on each request (optional, for activity tracking)
/// 3. EndSessionAsync() - Close session on logout or admin action
/// 4. CleanupExpiredAsync() - Background job to clean expired sessions
/// </remarks>
public interface IUserSessionRepository
{
    /// <summary>
    /// Gets all active sessions (legacy method).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active UserSession entities</returns>
    /// <remarks>
    /// Note: Consider using GetActiveSessionsWithDetailsAsync() instead
    /// as it provides more information (username, email, role).
    /// 
    /// This method queries VW_ActiveSessions view which filters
    /// sessions where EsteActiva = 1.
    /// </remarks>
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active sessions with user details (joined with Utilizatori table).
    /// </summary>
    /// <param name="utilizatorId">Filter by specific user ID (null for all users)</param>
    /// <param name="doarExpiraInCurand">Filter sessions expiring soon (within 15 minutes)</param>
    /// <param name="sortColumn">Column name for sorting (DataCreare, DataUltimaActivitate, DataExpirare, Username, AdresaIP)</param>
    /// <param name="sortDirection">Sort direction: "ASC" or "DESC" (default: DESC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Collection of tuples containing session and user data:
    /// - SessionID: Unique session identifier
    /// - UtilizatorID: User identifier
    /// - Username: Username for display
    /// - Email: User email
    /// - Rol: User role
    /// - SessionToken: Unique session token
    /// - AdresaIP: Client IP address
    /// - UserAgent: Browser/device user agent
    /// - Dispozitiv: Device type (Mobile, Tablet, Desktop)
    /// - DataCreare: Session creation timestamp
    /// - DataUltimaActivitate: Last activity timestamp
    /// - DataExpirare: Session expiration timestamp
    /// - EsteActiva: Session active flag
    /// </returns>
    /// <remarks>
    /// Use cases:
    /// - Admin dashboard: Monitor all active sessions
    /// - User profile: Show user's active sessions (pass utilizatorId)
    /// - Security: Identify sessions about to expire (doarExpiraInCurand = true)
    /// 
    /// Performance:
    /// - JOIN with Utilizatori table for user data
    /// - Filtered by EsteActiva = 1 (active sessions only)
    /// - Parameterized query prevents SQL injection
    /// - Sorted by specified column and direction
    /// </remarks>
    Task<IEnumerable<(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions for a specific user (active and inactive).
    /// </summary>
    /// <param name="utilizatorId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of UserSession entities for the specified user</returns>
    /// <remarks>
    /// Returns both active and inactive sessions.
    /// Queries VW_ActiveSessions view filtered by UtilizatorID.
    /// 
    /// Use cases:
    /// - User profile: Show login history
    /// - Security audit: Review user's session activity
    /// </remarks>
    Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid utilizatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user session.
    /// </summary>
    /// <param name="utilizatorId">User identifier</param>
    /// <param name="adresaIp">Client IP address</param>
    /// <param name="userAgent">Browser/device user agent string</param>
    /// <param name="dispozitiv">Device type (Mobile, Tablet, Desktop)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Tuple containing:
    /// - SessionId: Unique session identifier (GUID)
    /// - SessionToken: Unique session token (string, for stateless auth)
    /// </returns>
    /// <remarks>
    /// Called after successful login (AuthenticationController.Login).
    /// 
    /// Uses stored procedure SP_CreateUserSession which:
    /// - Generates unique SessionID (GUID)
    /// - Generates unique SessionToken (string, for JWT or stateless auth)
    /// - Sets DataCreare and DataUltimaActivitate to current timestamp
    /// - Sets DataExpirare based on configured session timeout
    /// - Sets EsteActiva = 1
    /// 
    /// Returns values via OUTPUT parameters:
    /// - @SessionID: Generated session ID
    /// - @SessionToken: Generated session token
    /// 
    /// Security:
    /// - IP address logged for audit trail
    /// - User agent logged for device tracking
    /// - Device type identified for statistics
    /// </remarks>
    Task<(Guid SessionId, string SessionToken)> CreateAsync(
        Guid utilizatorId,
        string adresaIp,
        string userAgent,
        string dispozitiv,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last activity timestamp for a session.
    /// </summary>
    /// <param name="sessionToken">Unique session token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if session was found and updated, false otherwise</returns>
    /// <remarks>
    /// Called on each request to extend session lifetime (sliding expiration).
    /// 
    /// Uses stored procedure SP_UpdateSessionActivity which:
    /// - Finds session by SessionToken
    /// - Updates DataUltimaActivitate to current timestamp
    /// - May extend DataExpirare (sliding expiration)
    /// 
    /// Performance:
    /// - Fast lookup by indexed SessionToken
    /// - Single UPDATE statement
    /// - No data returned (fire-and-forget operation)
    /// 
    /// Note: Optional operation. Can be called periodically
    /// (e.g., every 5 minutes) instead of on every request
    /// to reduce database load.
    /// </remarks>
    Task<bool> UpdateActivityAsync(string sessionToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a session (forced logout).
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if session was found and closed, false otherwise</returns>
    /// <remarks>
    /// Use cases:
    /// - User logout: Close current session
    /// - Admin action: Force logout user from specific session
    /// - Security: Close suspicious session
    /// 
    /// Operation:
    /// - Sets EsteActiva = 0 (inactive)
    /// - Updates DataUltimaActivitate to current timestamp
    /// - Does NOT delete session (for audit trail)
    /// 
    /// Effect:
    /// - User will be logged out on next request
    /// - Session no longer appears in active sessions list
    /// - Historical data preserved for audit
    /// </remarks>
    Task<bool> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired sessions (background maintenance task).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of sessions cleaned up</returns>
    /// <remarks>
    /// Should be called periodically (e.g., daily via background job).
    /// 
    /// Uses stored procedure SP_CleanupExpiredSessions which:
    /// - Finds sessions where DataExpirare &lt; GETDATE()
    /// - Sets EsteActiva = 0 (if not already)
    /// - May delete very old sessions (configurable retention period)
    /// - Returns count of affected sessions
    /// 
    /// Performance:
    /// - Bulk operation (UPDATE or DELETE)
    /// - Indexed by DataExpirare for fast lookup
    /// - Run during off-peak hours if possible
    /// 
    /// Scheduling:
    /// - Recommended: Daily at 2 AM
    /// - Alternative: Every 6 hours for strict security
    /// </remarks>
    Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session statistics for dashboard and monitoring.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Tuple containing:
    /// - TotalActive: Total number of active sessions
    /// - ExpiraInCurand: Sessions expiring soon (within 15 minutes)
    /// - InactiviAzi: Sessions that became inactive today
    /// </returns>
    /// <remarks>
    /// Use cases:
    /// - Admin dashboard: Display session statistics
    /// - Monitoring: Alert if TotalActive unusually high (potential attack)
    /// - Capacity planning: Track usage patterns
    /// 
    /// Query details:
    /// - TotalActive: COUNT(*) WHERE EsteActiva = 1
    /// - ExpiraInCurand: COUNT(*) WHERE EsteActiva = 1 AND DATEDIFF(MINUTE, GETDATE(), DataExpirare) &lt; 15
    /// - InactiviAzi: COUNT(*) WHERE EsteActiva = 0 AND CAST(DataUltimaActivitate AS DATE) = CAST(GETDATE() AS DATE)
    /// 
    /// Performance:
    /// - Three separate COUNT queries
    /// - Fast on indexed columns (EsteActiva, DataExpirare, DataUltimaActivitate)
    /// - Consider caching result for 1-5 minutes
    /// </remarks>
    Task<(int TotalActive, int ExpiraInCurand, int InactiviAzi)> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}
