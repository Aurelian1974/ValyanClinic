using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ValyanClinic.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqlConnectionFactory>? _logger;
    private int _connectionCount = 0;

    public SqlConnectionFactory(string connectionString, ILogger<SqlConnectionFactory>? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    public IDbConnection CreateConnection()
    {
        var connectionId = Interlocked.Increment(ref _connectionCount);

        _logger?.LogDebug("Creating database connection #{ConnectionId}", connectionId);

        var connection = new SqlConnection(_connectionString);

        // Event handlers pentru monitoring
        connection.StateChange += (sender, args) =>
        {
            _logger?.LogDebug(
                "Connection #{ConnectionId} state changed: {OldState} → {NewState}",
                connectionId,
                args.OriginalState,
                args.CurrentState);
        };

        return connection;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            _logger?.LogInformation("Database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Database connection test failed");
            return false;
        }
    }
}
