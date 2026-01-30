using Dapper;
using System.Data;
using ValyanClinic.Domain.Interfaces.Data;
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
    /// Procesează și formatează excepțiile SQL pentru utilizator
    /// </summary>
    private Exception ProcessSqlException(SqlException ex)
    {
        _logger?.LogError(ex, "SQL Exception occurred: Number={Number}, Severity={Severity}, State={State}, Message={Message}",
       ex.Number, ex.Class, ex.State, ex.Message);

        // Returnează mesaje user-friendly pentru erori cunoscute
        return ex.Number switch
        {
            2627 => new ArgumentException("Un element cu aceste date există deja în sistem."),
            547 => new ArgumentException("Nu se poate șterge elementul deoarece este folosit în alte părți ale sistemului."),
            515 => new ArgumentException("Unul sau mai multe câmpuri obligatorii nu au fost completate."),
            2 => new ArgumentException("Procedura stocată nu a fost găsită. Contactați administratorul sistemului."),
            208 => new ArgumentException("Numele obiectului din baza de date nu este invalid. Contactați administratorul sistemului."),
            18456 => new ArgumentException("Eroare de autentificare la baza de date. Contactați administratorul sistemului."),
            50001 => new ArgumentException(ex.Message), // Custom error din stored procedures
            _ when ex.Message.Contains("exista deja") => new ArgumentException(ex.Message),
            _ when ex.Message.Contains("duplicate") => new ArgumentException("Un element cu aceste date există deja în sistem."),
            _ when ex.Message.Contains("foreign key") => new ArgumentException("Nu se poate șterge elementul deoarece este folosit în alte părți ale sistemului."),
            _ when ex.Message.Contains("timeout") => new TimeoutException("Operația a durat prea mult timp. Încearcați din nou."),
            _ => new ArgumentException($"Eroare la operația cu baza de date: {ex.Message}")
        };
    }

    /// <summary>
    /// Executa un stored procedure si returneaza primul rezultat CU RETRY
    /// </summary>
    protected async Task<T?> QueryFirstOrDefaultAsync<T>(
        string storedProcedure,
  object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("🔄 BASE: Starting QueryFirstOrDefaultAsync for {StoredProcedure}", storedProcedure);

        _logger?.LogInformation("🔄 BASE: Creating connection...");
        using var connection = _connectionFactory.CreateConnection();
        try
        {
            _logger?.LogInformation("🔄 BASE: Opening connection...");
            await EnsureConnectionOpenAsync(connection, cancellationToken);

            _logger?.LogInformation("🔄 BASE: Executing stored procedure {StoredProcedure}...", storedProcedure);

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<T>(
               new CommandDefinition(
                     storedProcedure,
                   parameters,
                 commandType: CommandType.StoredProcedure,
                      commandTimeout: 30,
                      cancellationToken: cancellationToken));

                _logger?.LogInformation("🔄 BASE: Dapper call completed, result: {ResultType}",
                        result?.GetType().Name ?? "null");

                return result;
            }
            catch (SqlException sqlEx) when (!IsTransientError(sqlEx))
            {
                _logger?.LogError(sqlEx, "🔥 BASE: SqlException - Number={Number}, Message={Message}",
                      sqlEx.Number, sqlEx.Message);

                _logger?.LogInformation("🔄 BASE: About to process and throw SqlException...");
                var processedException = ProcessSqlException(sqlEx);
                _logger?.LogInformation("🔄 BASE: ProcessSqlException returned: {Type} with message: {Message}",
                      processedException.GetType().Name, processedException.Message);

                _logger?.LogInformation("🔄 BASE: About to throw processed exception...");
                throw processedException;
            }
            catch (SqlException sqlEx)
            {
                _logger?.LogError(sqlEx, "🔥 BASE: SqlException (transient) - Number={Number}", sqlEx.Number);
                throw; // Re-throw pentru debugging
            }
        }
        finally
        {
            _logger?.LogInformation("🔄 BASE: Finally block - closing connection...");
            await CloseConnectionSafelyAsync(connection);
            _logger?.LogInformation("🔄 BASE: Finally block - connection closed successfully");
        }
    }

    /// <summary>
    /// Executa un stored procedure si returneaza un singur rezultat CU RETRY
    /// </summary>
    protected async Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (SqlException ex) when (!IsTransientError(ex))
        {
            throw ProcessSqlException(ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error in QuerySingleOrDefaultAsync for {StoredProcedure}", storedProcedure);
            throw;
        }
    }

    /// <summary>
    /// Executa un stored procedure si returneaza o lista de rezultate CU RETRY
    /// </summary>
    protected async Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
   object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (SqlException ex) when (!IsTransientError(ex))
        {
            throw ProcessSqlException(ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error in QueryAsync for {StoredProcedure}", storedProcedure);
            throw;
        }
    }

    /// <summary>
    /// Executa un stored procedure fara a returna rezultate CU RETRY
    /// </summary>
    protected async Task<int> ExecuteAsync(
          string storedProcedure,
     object? parameters = null,
          CancellationToken cancellationToken = default)
    {
        try
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
        catch (SqlException ex) when (!IsTransientError(ex))
        {
            throw ProcessSqlException(ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error in ExecuteAsync for {StoredProcedure}", storedProcedure);
            throw;
        }
    }

    /// <summary>
    /// Executa un stored procedure si returneaza un scalar CU RETRY
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(
        string storedProcedure,
  object? parameters = null,
  CancellationToken cancellationToken = default)
    {
        try
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
        catch (SqlException ex) when (!IsTransientError(ex))
        {
            throw ProcessSqlException(ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error in ExecuteScalarAsync for {StoredProcedure}", storedProcedure);
            throw;
        }
    }

    /// <summary>
    /// Executa multiple query-uri in cadrul aceleasi conexiuni CU RETRY
    /// </summary>
    protected async Task<T> ExecuteInTransactionAsync<T>(
    Func<IDbConnection, IDbTransaction, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (SqlException ex) when (!IsTransientError(ex))
        {
            throw ProcessSqlException(ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error in ExecuteInTransactionAsync");
            throw;
        }
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
