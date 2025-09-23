using System.Data;
using Dapper;
using ValyanClinic.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository implementation pentru DepartamentMedical folosind Dapper si Stored Procedures
/// IMPORTANT: incarca departamentele medicale DOAR din baza de date, NU din enum-uri statice
/// Toate departamentele medicale sunt dinamice si se pot adauga/modifica fara rebuild
/// </summary>
public class DepartamentMedicalRepository : IDepartamentMedicalRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DepartamentMedicalRepository> _logger;

    public DepartamentMedicalRepository(IDbConnection connection, ILogger<DepartamentMedicalRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<IEnumerable<DepartamentMedical>> GetAllDepartamenteMedicaleAsync()
    {
        try
        {
            _logger.LogInformation("Getting all medical departments from database");

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Tip", "Medical");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetByTip",
                parameters,
                commandType: CommandType.StoredProcedure);

            var departamente = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Retrieved {Count} medical departments", departamente.Count);

            return departamente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all medical departments");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetCategoriiMedicaleAsync()
    {
        try
        {
            _logger.LogInformation("Getting medical categories from database");

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Tip", "Categorie");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetByTip",
                parameters,
                commandType: CommandType.StoredProcedure);

            var categorii = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Retrieved {Count} medical categories", categorii.Count);

            return categorii;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medical categories");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSpecializariMedicaleAsync()
    {
        try
        {
            _logger.LogInformation("Getting medical specializations from database");

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Tip", "Specializare");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetByTip",
                parameters,
                commandType: CommandType.StoredProcedure);

            var specializari = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Retrieved {Count} medical specializations", specializari.Count);

            return specializari;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medical specializations");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSubspecializariMedicaleAsync()
    {
        try
        {
            _logger.LogInformation("Getting medical subspecializations from database");

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Tip", "Subspecializare");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetByTip",
                parameters,
                commandType: CommandType.StoredProcedure);

            var subspecializari = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Retrieved {Count} medical subspecializations", subspecializari.Count);

            return subspecializari;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medical subspecializations");
            throw;
        }
    }

    public async Task<DepartamentMedical?> GetDepartamentMedicalByIdAsync(Guid departamentId)
    {
        try
        {
            _logger.LogInformation("Getting medical department by ID: {DepartamentId}", departamentId);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@DepartamentID", departamentId);

            var result = await _connection.QueryFirstOrDefaultAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result != null ? MapDtoToDepartamentMedical(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving medical department by ID: {DepartamentId}", departamentId);
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSpecializariPentruCategoriaAsync(Guid categorieId)
    {
        try
        {
            _logger.LogInformation("Getting specializations for category: {CategorieId}", categorieId);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ParentID", categorieId);
            parameters.Add("@TipCopii", "Specializare");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetChildren",
                parameters,
                commandType: CommandType.StoredProcedure);

            var specializari = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Retrieved {Count} specializations for category {CategorieId}", specializari.Count, categorieId);

            return specializari;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving specializations for category: {CategorieId}", categorieId);
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSubspecializariPentruSpecializareaAsync(Guid specializareId)
    {
        try
        {
            _logger.LogInformation("Getting subspecializations for specialization: {SpecializareId}", specializareId);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ParentID", specializareId);
            parameters.Add("@TipCopii", "Subspecializare");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_GetChildren",
                parameters,
                commandType: CommandType.StoredProcedure);

            var subspecializari = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Retrieved {Count} subspecializations for specialization {SpecializareId}", subspecializari.Count, specializareId);

            return subspecializari;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subspecializations for specialization: {SpecializareId}", specializareId);
            throw;
        }
    }

    public async Task<bool> DepartamentMedicalExistsAsync(Guid departamentId)
    {
        try
        {
            _logger.LogInformation("Checking if medical department exists: {DepartamentId}", departamentId);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@DepartamentID", departamentId);

            var result = await _connection.QueryFirstAsync<bool>(
                "sp_Departamente_CheckExists",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking medical department existence: {DepartamentId}", departamentId);
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> SearchDepartamenteMedicaleAsync(string searchText)
    {
        try
        {
            _logger.LogInformation("Searching medical departments with text: {SearchText}", searchText);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", searchText);
            parameters.Add("@Tip", "Medical");

            var result = await _connection.QueryAsync<DepartamentMedicalDto>(
                "sp_Departamente_Search",
                parameters,
                commandType: CommandType.StoredProcedure);

            var departamente = result.Select(MapDtoToDepartamentMedical).ToList();

            _logger.LogInformation("Found {Count} medical departments matching '{SearchText}'", departamente.Count, searchText);

            return departamente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching medical departments with text: {SearchText}", searchText);
            throw;
        }
    }

    #region Connection Management

    /// <summary>
    /// Ensures the connection is open and ready for use
    /// Handles connection state issues and provides retry logic
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

    private static DepartamentMedical MapDtoToDepartamentMedical(DepartamentMedicalDto dto)
    {
        return new DepartamentMedical
        {
            DepartamentID = dto.DepartamentID,
            Nume = dto.Nume,
            Tip = dto.Tip
        };
    }

    #endregion

    #region DTOs for Dapper Mapping

    private class DepartamentMedicalDto
    {
        public Guid DepartamentID { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
    }

    #endregion

    public async Task<bool> TestDatabaseConnectionAsync()
    {
        try
        {
            await EnsureConnectionOpenAsync();
            
            // Test 1: Basic connection test
            var basicTest = await _connection.QuerySingleAsync<int>("SELECT 1");
            
            // Test 2: Database name
            var dbName = await _connection.QuerySingleAsync<string>("SELECT DB_NAME()");
            
            // Test 3: Check Departamente table
            var departamenteTableExists = await _connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departamente'");
            
            if (departamenteTableExists == 0)
            {
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return false;
        }
    }
}
