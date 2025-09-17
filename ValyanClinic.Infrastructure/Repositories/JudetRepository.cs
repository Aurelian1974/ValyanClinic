using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository implementation pentru Judete folosind Dapper si Stored Procedures
/// Conform best practices: fara SQL queries in cod, doar SP calls
/// </summary>
public class JudetRepository : IJudetRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<JudetRepository> _logger;

    public JudetRepository(IDbConnection connection, ILogger<JudetRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<IEnumerable<Judet>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all judete");

            await EnsureConnectionOpenAsync();

            var result = await _connection.QueryAsync<JudetDto>(
                "sp_Judete_GetAll",
                commandType: CommandType.StoredProcedure);

            var judete = result.Select(MapDtoToJudet).ToList();
            
            _logger.LogDebug("Retrieved {Count} judete", judete.Count);
            return judete;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all judete");
            throw;
        }
    }

    public async Task<IEnumerable<Judet>> GetOrderedByNameAsync()
    {
        try
        {
            _logger.LogInformation("🚀 JudetRepository.GetOrderedByNameAsync() called");

            await EnsureConnectionOpenAsync();
            _logger.LogInformation("✅ Database connection ensured");

            _logger.LogInformation("📞 Executing stored procedure: sp_Judete_GetOrderedByName");
            
            var result = await _connection.QueryAsync<JudetDto>(
                "sp_Judete_GetOrderedByName",
                commandType: CommandType.StoredProcedure);

            var judete = result.Select(MapDtoToJudet).ToList();
            
            _logger.LogInformation("✅ JudetRepository retrieved {Count} judete from database", judete.Count);
            
            if (judete.Count > 0)
            {
                _logger.LogInformation("📋 Sample judete from DB: {Sample}", 
                    string.Join(", ", judete.Take(3).Select(j => $"{j.IdJudet}-{j.Nume}-{j.CodJudet}")));
            }
            else
            {
                _logger.LogError("💥 CRITICAL: Database returned 0 judete! Check if SP exists and table has data");
                
                // Test direct table query
                try
                {
                    var directCount = await _connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Judete");
                    _logger.LogInformation("📊 Direct table count: {Count} judete in Judete table", directCount);
                }
                catch (Exception testEx)
                {
                    _logger.LogError(testEx, "💥 FATAL: Cannot even count Judete table records");
                }
            }
            
            return judete;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 FATAL ERROR in JudetRepository.GetOrderedByNameAsync()");
            throw;
        }
    }

    public async Task<Judet?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting judet by ID: {JudetId}", id);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@IdJudet", id);

            var result = await _connection.QueryFirstOrDefaultAsync<JudetDto>(
                "sp_Judete_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result != null ? MapDtoToJudet(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving judet by ID: {JudetId}", id);
            throw;
        }
    }

    public async Task<Judet?> GetByCodAsync(string codJudet)
    {
        try
        {
            _logger.LogDebug("Getting judet by cod: {CodJudet}", codJudet);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@CodJudet", codJudet);

            var result = await _connection.QueryFirstOrDefaultAsync<JudetDto>(
                "sp_Judete_GetByCod",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result != null ? MapDtoToJudet(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving judet by cod: {CodJudet}", codJudet);
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

    private static Judet MapDtoToJudet(JudetDto dto)
    {
        return new Judet
        {
            IdJudet = dto.IdJudet,
            JudetGuid = dto.JudetGuid,
            CodJudet = dto.CodJudet,
            Nume = dto.Nume,
            Siruta = dto.Siruta,
            CodAuto = dto.CodAuto,
            Ordine = dto.Ordine
        };
    }

    #endregion

    #region DTOs for Dapper Mapping

    private class JudetDto
    {
        public int IdJudet { get; set; }
        public Guid JudetGuid { get; set; }
        public string CodJudet { get; set; } = string.Empty;
        public string Nume { get; set; } = string.Empty;
        public int? Siruta { get; set; }
        public string? CodAuto { get; set; }
        public int? Ordine { get; set; }
    }

    #endregion
}
