using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository implementation pentru Localitati folosind Dapper si Stored Procedures
/// Conform best practices: fara SQL queries in cod, doar SP calls
/// </summary>
public class LocalitateRepository : ILocalitateRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<LocalitateRepository> _logger;

    public LocalitateRepository(IDbConnection connection, ILogger<LocalitateRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<IEnumerable<Localitate>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all localitati");

            await EnsureConnectionOpenAsync();

            var result = await _connection.QueryAsync<LocalitateDto>(
                "sp_Localitati_GetAll",
                commandType: CommandType.StoredProcedure);

            var localitati = result.Select(MapDtoToLocalitate).ToList();
            
            _logger.LogDebug("Retrieved {Count} localitati", localitati.Count);
            return localitati;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all localitati");
            throw;
        }
    }

    public async Task<Localitate?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting localitate by ID: {LocalitateId}", id);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@IdOras", id);

            var result = await _connection.QueryFirstOrDefaultAsync<LocalitateDto>(
                "sp_Localitati_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result != null ? MapDtoToLocalitate(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localitate by ID: {LocalitateId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Localitate>> GetByJudetIdAsync(int judetId)
    {
        try
        {
            _logger.LogDebug("Getting localitati by judet ID: {JudetId}", judetId);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@IdJudet", judetId);

            var result = await _connection.QueryAsync<LocalitateDto>(
                "sp_Localitati_GetByJudetId",
                parameters,
                commandType: CommandType.StoredProcedure);

            var localitati = result.Select(MapDtoToLocalitate).ToList();
            
            _logger.LogDebug("Retrieved {Count} localitati for judet {JudetId}", localitati.Count, judetId);
            return localitati;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localitati by judet ID: {JudetId}", judetId);
            throw;
        }
    }

    public async Task<IEnumerable<Localitate>> GetByJudetIdOrderedAsync(int judetId)
    {
        try
        {
            _logger.LogDebug("Getting localitati by judet ID ordered: {JudetId}", judetId);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@IdJudet", judetId);

            var result = await _connection.QueryAsync<LocalitateDto>(
                "sp_Localitati_GetByJudetIdOrdered",
                parameters,
                commandType: CommandType.StoredProcedure);

            var localitati = result.Select(MapDtoToLocalitate).ToList();
            
            _logger.LogDebug("Retrieved {Count} ordered localitati for judet {JudetId}", localitati.Count, judetId);
            return localitati;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ordered localitati by judet ID: {JudetId}", judetId);
            throw;
        }
    }

    #region Connection Management

    /// <summary>
    /// Ensures the connection is open and ready for use
    /// Copy from PersonalRepository pentru consistency
    /// </summary>
    private async Task EnsureConnectionOpenAsync()
    {
        try
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open(); 
            }
            else if (_connection.State == ConnectionState.Broken)
            {
                _logger.LogWarning("Connection is broken, closing and reopening");
                _connection.Close();
                _connection.Open(); 
            }
            else if (_connection.State == ConnectionState.Open)
            {
                // Connection is already open - verify it's working with a simple test
                try
                {
                    var testResult = await _connection.QuerySingleAsync<int>("SELECT 1");
                }
                catch (Exception testEx)
                {
                    _logger.LogWarning(testEx, "Open connection test failed, reopening");
                    _connection.Close();
                    _connection.Open(); 
                }
            }

            // Add a small delay to ensure connection is fully ready
            await Task.Delay(10);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure connection is open");
            throw;
        }
    }

    #endregion

    #region Private Mapping Methods

    private static Localitate MapDtoToLocalitate(LocalitateDto dto)
    {
        return new Localitate
        {
            IdOras = dto.IdOras,
            LocalitateGuid = dto.LocalitateGuid,
            IdJudet = dto.IdJudet,
            Nume = dto.Nume,
            Siruta = dto.Siruta,
            IdTipLocalitate = dto.IdTipLocalitate,
            CodLocalitate = dto.CodLocalitate
        };
    }

    #endregion

    #region DTOs for Dapper Mapping

    private class LocalitateDto
    {
        public int IdOras { get; set; }
        public Guid LocalitateGuid { get; set; }
        public int IdJudet { get; set; }
        public string Nume { get; set; } = string.Empty;
        public int Siruta { get; set; }
        public int? IdTipLocalitate { get; set; }
        public string CodLocalitate { get; set; } = string.Empty;
    }

    #endregion
}
