using Dapper;
using System.Data;
using ValyanClinic.Infrastructure.Data;
using Polly;
using Polly.Retry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Base repository cu functionalitati comune Dapper + Connection Resilience
/// </summary>
public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory _connectionFactory;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger? _logger;

    protected BaseRepository(IDbConnectionFactory connectionFactory, ILogger? logger = null)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        
        // Configurare retry policy pentru erori de conexiune
        _retryPolicy = Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .Or<InvalidOperationException>(ex => ex.Message.Contains("connection"))
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger?.LogWarning(
                        "DB Retry {RetryCount} dupa {Seconds}s: {Message}", 
                        retryCount, 
                        timeSpan.TotalSeconds, 
                        exception.Message);
                });
    }

    /// <summary>
    /// Verifică dacă eroarea SQL este tranzientă (poate fi reîncercată)
    /// </summary>
    private static bool IsTransientError(SqlException ex)
    {
        // Coduri de eroare SQL care indică probleme tranziente
        var transientErrors = new[]
        {
            -1,     // Timeout
            -2,     // Connection timeout
            1205,   // Deadlock
            233,    // Connection initialization error
            10053,  // Transport-level error
            10054,  // Transport-level error
            10060,  // Network error
            40197,  // Service error
            40501,  // Service busy
            40613,  // Database unavailable
            49918,  // Cannot process request
            49919,  // Cannot process create/update request
            49920   // Cannot process create/update request
        };

        return transientErrors.Contains(ex.Number);
    }

    /// <summary>
    /// Executa un stored procedure si returneaza un singur rezultat CU RETRY
    /// </summary>
    protected async Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                await EnsureConnectionOpenAsync(connection, cancellationToken);
                
                return await connection.QuerySingleOrDefaultAsync<T>(
                    new CommandDefinition(
                        storedProcedure,
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 30,
                        cancellationToken: cancellationToken));
            }
            finally
            {
                await CloseConnectionSafelyAsync(connection);
            }
        });
    }

    /// <summary>
    /// Executa un stored procedure si returneaza primul rezultat CU RETRY
    /// </summary>
    protected async Task<T?> QueryFirstOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                await EnsureConnectionOpenAsync(connection, cancellationToken);
                
                return await connection.QueryFirstOrDefaultAsync<T>(
                    new CommandDefinition(
                        storedProcedure,
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 30,
                        cancellationToken: cancellationToken));
            }
            finally
            {
                await CloseConnectionSafelyAsync(connection);
            }
        });
    }

    /// <summary>
    /// Executa un stored procedure si returneaza o lista de rezultate CU RETRY
    /// </summary>
    protected async Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                await EnsureConnectionOpenAsync(connection, cancellationToken);
                
                return await connection.QueryAsync<T>(
                    new CommandDefinition(
                        storedProcedure,
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 30,
                        cancellationToken: cancellationToken));
            }
            finally
            {
                await CloseConnectionSafelyAsync(connection);
            }
        });
    }

    /// <summary>
    /// Executa un stored procedure fara a returna rezultate CU RETRY
    /// </summary>
    protected async Task<int> ExecuteAsync(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                await EnsureConnectionOpenAsync(connection, cancellationToken);
                
                return await connection.ExecuteAsync(
                    new CommandDefinition(
                        storedProcedure,
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 30,
                        cancellationToken: cancellationToken));
            }
            finally
            {
                await CloseConnectionSafelyAsync(connection);
            }
        });
    }

    /// <summary>
    /// Executa un stored procedure si returneaza un scalar CU RETRY
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                await EnsureConnectionOpenAsync(connection, cancellationToken);
                
                return await connection.ExecuteScalarAsync<T>(
                    new CommandDefinition(
                        storedProcedure,
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 30,
                        cancellationToken: cancellationToken));
            }
            finally
            {
                await CloseConnectionSafelyAsync(connection);
            }
        });
    }

    /// <summary>
    /// Executa multiple query-uri in cadrul aceleiasi conexiuni CU RETRY
    /// </summary>
    protected async Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();
            IDbTransaction? transaction = null;
            
            try
            {
                await EnsureConnectionOpenAsync(connection, cancellationToken);
                transaction = connection.BeginTransaction();

                var result = await operation(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
                await CloseConnectionSafelyAsync(connection);
            }
        });
    }

    /// <summary>
    /// Asigură că conexiunea este deschisă înainte de a executa comenzi
    /// </summary>
    private async Task EnsureConnectionOpenAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State == ConnectionState.Closed)
        {
            if (connection is SqlConnection sqlConnection)
            {
                await sqlConnection.OpenAsync(cancellationToken);
                _logger?.LogDebug("Database connection opened");
            }
            else
            {
                connection.Open();
            }
        }
        else if (connection.State == ConnectionState.Broken)
        {
            _logger?.LogWarning("Database connection was broken, reconnecting...");
            connection.Close();
            
            if (connection is SqlConnection sqlConnection)
            {
                await sqlConnection.OpenAsync(cancellationToken);
                _logger?.LogDebug("Database connection reopened after broken state");
            }
            else
            {
                connection.Open();
            }
        }
    }

    /// <summary>
    /// Închide conexiunea în mod sigur, gestionând toate erorile
    /// </summary>
    private async Task CloseConnectionSafelyAsync(IDbConnection connection)
    {
        if (connection == null) return;

        try
        {
            if (connection.State != ConnectionState.Closed)
            {
                if (connection is SqlConnection sqlConnection)
                {
                    await sqlConnection.CloseAsync();
                    _logger?.LogDebug("Database connection closed");
                }
                else
                {
                    connection.Close();
                }
                
                // CRITICAL: Clear connection pool pentru a preveni conexiuni stale
                if (connection is SqlConnection sql)
                {
                    SqlConnection.ClearPool(sql);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error closing database connection");
            // Nu re-throw - lăsăm using să facă cleanup
        }
    }
}
