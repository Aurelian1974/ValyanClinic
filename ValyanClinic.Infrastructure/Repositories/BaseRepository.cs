using Dapper;
using System.Data;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Base repository cu functionalitati comune Dapper
/// </summary>
public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory _connectionFactory;

    protected BaseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Executa un stored procedure si returneaza un singur rezultat
    /// </summary>
    protected async Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(
            storedProcedure,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Executa un stored procedure si returneaza primul rezultat
    /// </summary>
    protected async Task<T?> QueryFirstOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(
            storedProcedure,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Executa un stored procedure si returneaza o lista de rezultate
    /// </summary>
    protected async Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(
            storedProcedure,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Executa un stored procedure fara a returna rezultate
    /// </summary>
    protected async Task<int> ExecuteAsync(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            storedProcedure,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Executa un stored procedure si returneaza un scalar
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<T>(
            storedProcedure,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Executa multiple query-uri in cadrul aceleiasi conexiuni
    /// </summary>
    protected async Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var result = await operation(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
