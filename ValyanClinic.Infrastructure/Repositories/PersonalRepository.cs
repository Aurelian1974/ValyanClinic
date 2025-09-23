using System.Data;
using Dapper;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository implementation pentru Personal folosind Dapper si Stored Procedures
/// Conform best practices: fara SQL queries in cod, doar SP calls
/// </summary>
public class PersonalRepository : IPersonalRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<PersonalRepository> _logger;

    public PersonalRepository(IDbConnection connection, ILogger<PersonalRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<(IEnumerable<Personal> Data, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? status = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC")
    {
        try
        {
            _logger.LogInformation("Getting personal data with pagination. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchText", searchText);
            parameters.Add("@Departament", departament);
            parameters.Add("@Status", status);
            parameters.Add("@SortColumn", sortColumn);
            parameters.Add("@SortDirection", sortDirection);

            using var multi = await _connection.QueryMultipleAsync(
                "sp_Personal_GetAll", 
                parameters, 
                commandType: CommandType.StoredProcedure);

            var data = await multi.ReadAsync<PersonalDto>();
            var totalCount = await multi.ReadSingleAsync<int>();

            var personalList = data.Select(MapDtoToPersonal).ToList();

            return (personalList, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal data");
            throw;
        }
    }

    public async Task<Personal?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting personal by ID: {PersonalId}", id);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Id_Personal", id);

            var result = await _connection.QueryFirstOrDefaultAsync<PersonalDto>(
                "sp_Personal_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result != null ? MapDtoToPersonal(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal by ID: {PersonalId}", id);
            throw;
        }
    }

    public async Task<Personal> CreateAsync(Personal personal, string creatDe)
    {
        try
        {
            _logger.LogInformation("Creating new personal: {Nume} {Prenume}", personal.Nume, personal.Prenume);

            await EnsureConnectionOpenAsync();

            var parameters = MapPersonalToParameters(personal);
            parameters.Add("@Id_Personal", personal.Id_Personal == Guid.Empty ? null : personal.Id_Personal);
            parameters.Add("@Creat_De", creatDe);

            var result = await _connection.QueryFirstAsync<PersonalDto>(
                "sp_Personal_Create",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120); // 2 minute timeout

            var mappedResult = MapDtoToPersonal(result);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            if (ex is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(ex, "SQL Error creating personal: Number={Number}, Severity={Severity}, State={State}", 
                    sqlEx.Number, sqlEx.Class, sqlEx.State);
            }
            
            _logger.LogError(ex, "Error creating personal: {Nume} {Prenume}", personal.Nume, personal.Prenume);
            throw;
        }
    }

    public async Task<Personal> UpdateAsync(Personal personal, string modificatDe)
    {
        try
        {
            _logger.LogInformation("Updating personal: {PersonalId}", personal.Id_Personal);

            await EnsureConnectionOpenAsync();

            var parameters = MapPersonalToParameters(personal);
            parameters.Add("@Id_Personal", personal.Id_Personal);
            parameters.Add("@Modificat_De", modificatDe);

            var result = await _connection.QueryFirstAsync<PersonalDto>(
                "sp_Personal_Update",
                parameters,
                commandType: CommandType.StoredProcedure);

            var mappedResult = MapDtoToPersonal(result);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal: {PersonalId}", personal.Id_Personal);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, string modificatDe)
    {
        try
        {
            _logger.LogInformation("Soft deleting personal: {PersonalId}", id);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Id_Personal", id);
            parameters.Add("@Modificat_De", modificatDe);

            var result = await _connection.QueryFirstAsync<int>(
                "sp_Personal_Delete",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal: {PersonalId}", id);
            throw;
        }
    }

    public async Task<(bool CnpExists, bool CodAngajatExists)> CheckUniqueAsync(
        string cnp, 
        string codAngajat, 
        Guid? excludeId = null)
    {
        try
        {
            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@CNP", cnp);
            parameters.Add("@Cod_Angajat", codAngajat);
            parameters.Add("@ExcludeId", excludeId);

            var result = await _connection.QueryFirstAsync<(bool CNP_Exists, bool CodAngajat_Exists)>(
                "sp_Personal_CheckUnique",
                parameters,
                commandType: CommandType.StoredProcedure);

            return (result.CNP_Exists, result.CodAngajat_Exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking uniqueness for CNP: {CNP}, Cod: {CodAngajat}", cnp, codAngajat);
            throw;
        }
    }

    public async Task<(int TotalPersonal, int PersonalActiv, int PersonalInactiv)> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting personal statistics");

            await EnsureConnectionOpenAsync();

            var result = await _connection.QueryAsync<StatisticDto>(
                "sp_Personal_GetStatistics",
                commandType: CommandType.StoredProcedure);

            var stats = result.ToDictionary(x => x.StatisticName, x => x.Value);

            return (
                stats.GetValueOrDefault("Total Personal", 0),
                stats.GetValueOrDefault("Personal Activ", 0),
                stats.GetValueOrDefault("Personal Inactiv", 0)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal statistics");
            throw;
        }
    }

    public async Task<IEnumerable<(string Value, string Text)>> GetDepartamenteAsync()
    {
        try
        {
            await EnsureConnectionOpenAsync();
            
            var result = await _connection.QueryAsync<DropdownOption>(
                "SELECT DISTINCT Departament as Value, Departament as Text FROM Personal WHERE Departament IS NOT NULL ORDER BY Departament");

            return result.Select(x => (x.Value, x.Text));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departamente");
            throw;
        }
    }

    public async Task<IEnumerable<(string Value, string Text)>> GetFunctiiAsync()
    {
        try
        {
            await EnsureConnectionOpenAsync();
            
            var result = await _connection.QueryAsync<DropdownOption>(
                "SELECT DISTINCT Functia as Value, Functia as Text FROM Personal WHERE Functia IS NOT NULL ORDER BY Functia");

            return result.Select(x => (x.Value, x.Text));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving functii");
            throw;
        }
    }

    public async Task<IEnumerable<(string Value, string Text)>> GetJudeteAsync()
    {
        try
        {
            await EnsureConnectionOpenAsync();
            
            var result = await _connection.QueryAsync<DropdownOption>(
                "SELECT DISTINCT Judet_Domiciliu as Value, Judet_Domiciliu as Text FROM Personal WHERE Judet_Domiciliu IS NOT NULL ORDER BY Judet_Domiciliu");

            return result.Select(x => (x.Value, x.Text));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving judete");
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

    private static Personal MapDtoToPersonal(PersonalDto dto)
    {
        return new Personal
        {
            Id_Personal = dto.Id_Personal,
            Cod_Angajat = dto.Cod_Angajat,
            CNP = dto.CNP,
            Nume = dto.Nume,
            Prenume = dto.Prenume,
            Nume_Anterior = dto.Nume_Anterior,
            Data_Nasterii = dto.Data_Nasterii,
            Locul_Nasterii = dto.Locul_Nasterii,
            Nationalitate = dto.Nationalitate ?? "Romana",
            Cetatenie = dto.Cetatenie ?? "Romana",
            Telefon_Personal = dto.Telefon_Personal,
            Telefon_Serviciu = dto.Telefon_Serviciu,
            Email_Personal = dto.Email_Personal,
            Email_Serviciu = dto.Email_Serviciu,
            Adresa_Domiciliu = dto.Adresa_Domiciliu,
            Judet_Domiciliu = dto.Judet_Domiciliu,
            Oras_Domiciliu = dto.Oras_Domiciliu,
            Cod_Postal_Domiciliu = dto.Cod_Postal_Domiciliu,
            Adresa_Resedinta = dto.Adresa_Resedinta,
            Judet_Resedinta = dto.Judet_Resedinta,
            Oras_Resedinta = dto.Oras_Resedinta,
            Cod_Postal_Resedinta = dto.Cod_Postal_Resedinta,
            Stare_Civila = ParseEnum<StareCivila>(dto.Stare_Civila),
            Functia = dto.Functia,
            Departament = ParseEnum<Departament>(dto.Departament),
            Serie_CI = dto.Serie_CI,
            Numar_CI = dto.Numar_CI,
            Eliberat_CI_De = dto.Eliberat_CI_De,
            Data_Eliberare_CI = dto.Data_Eliberare_CI,
            Valabil_CI_Pana = dto.Valabil_CI_Pana,
            Status_Angajat = ParseEnum<StatusAngajat>(dto.Status_Angajat) ?? StatusAngajat.Activ,
            Observatii = dto.Observatii,
            Data_Crearii = dto.Data_Crearii,
            Data_Ultimei_Modificari = dto.Data_Ultimei_Modificari,
            Creat_De = dto.Creat_De,
            Modificat_De = dto.Modificat_De
        };
    }

    private static DynamicParameters MapPersonalToParameters(Personal personal)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Cod_Angajat", personal.Cod_Angajat);
        parameters.Add("@CNP", personal.CNP);
        parameters.Add("@Nume", personal.Nume);
        parameters.Add("@Prenume", personal.Prenume);
        parameters.Add("@Nume_Anterior", personal.Nume_Anterior);
        parameters.Add("@Data_Nasterii", personal.Data_Nasterii);
        parameters.Add("@Locul_Nasterii", personal.Locul_Nasterii);
        parameters.Add("@Nationalitate", personal.Nationalitate ?? "Romana");
        parameters.Add("@Cetatenie", personal.Cetatenie ?? "Romana");
        parameters.Add("@Telefon_Personal", personal.Telefon_Personal);
        parameters.Add("@Telefon_Serviciu", personal.Telefon_Serviciu);
        parameters.Add("@Email_Personal", personal.Email_Personal);
        parameters.Add("@Email_Serviciu", personal.Email_Serviciu);
        parameters.Add("@Adresa_Domiciliu", personal.Adresa_Domiciliu);
        parameters.Add("@Judet_Domiciliu", personal.Judet_Domiciliu);
        parameters.Add("@Oras_Domiciliu", personal.Oras_Domiciliu);
        parameters.Add("@Cod_Postal_Domiciliu", personal.Cod_Postal_Domiciliu);
        parameters.Add("@Adresa_Resedinta", personal.Adresa_Resedinta);
        parameters.Add("@Judet_Resedinta", personal.Judet_Resedinta);
        parameters.Add("@Oras_Resedinta", personal.Oras_Resedinta);
        parameters.Add("@Cod_Postal_Resedinta", personal.Cod_Postal_Resedinta);
        parameters.Add("@Stare_Civila", personal.Stare_Civila?.ToString());
        parameters.Add("@Functia", personal.Functia);
        parameters.Add("@Departament", personal.Departament?.ToString());
        parameters.Add("@Serie_CI", personal.Serie_CI);
        parameters.Add("@Numar_CI", personal.Numar_CI);
        parameters.Add("@Eliberat_CI_De", personal.Eliberat_CI_De);
        parameters.Add("@Data_Eliberare_CI", personal.Data_Eliberare_CI);
        parameters.Add("@Valabil_CI_Pana", personal.Valabil_CI_Pana);
        parameters.Add("@Status_Angajat", personal.Status_Angajat.ToString());
        parameters.Add("@Observatii", personal.Observatii);
        
        return parameters;
    }

    private static T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return Enum.TryParse<T>(value, true, out var result) ? result : null;
    }

    #endregion

    #region DTOs for Dapper Mapping

    private class PersonalDto
    {
        public Guid Id_Personal { get; set; }
        public string Cod_Angajat { get; set; } = string.Empty;
        public string CNP { get; set; } = string.Empty;
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string? Nume_Anterior { get; set; }
        public DateTime Data_Nasterii { get; set; }
        public string? Locul_Nasterii { get; set; }
        public string? Nationalitate { get; set; }
        public string? Cetatenie { get; set; }
        public string? Telefon_Personal { get; set; }
        public string? Telefon_Serviciu { get; set; }
        public string? Email_Personal { get; set; }
        public string? Email_Serviciu { get; set; }
        public string Adresa_Domiciliu { get; set; } = string.Empty;
        public string Judet_Domiciliu { get; set; } = string.Empty;
        public string Oras_Domiciliu { get; set; } = string.Empty;
        public string? Cod_Postal_Domiciliu { get; set; }
        public string? Adresa_Resedinta { get; set; }
        public string? Judet_Resedinta { get; set; }
        public string? Oras_Resedinta { get; set; }
        public string? Cod_Postal_Resedinta { get; set; }
        public string? Stare_Civila { get; set; }
        public string Functia { get; set; } = string.Empty;
        public string? Departament { get; set; }
        public string? Serie_CI { get; set; }
        public string? Numar_CI { get; set; }
        public string? Eliberat_CI_De { get; set; }
        public DateTime? Data_Eliberare_CI { get; set; }
        public DateTime? Valabil_CI_Pana { get; set; }
        public string Status_Angajat { get; set; } = "Activ";
        public string? Observatii { get; set; }
        public DateTime Data_Crearii { get; set; }
        public DateTime Data_Ultimei_Modificari { get; set; }
        public string? Creat_De { get; set; }
        public string? Modificat_De { get; set; }
    }

    private class StatisticDto
    {
        public string StatisticName { get; set; } = string.Empty;
        public int Value { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }

    private class DropdownOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
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
            
            // Test 3: Check Personal table
            var personalTableExists = await _connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Personal'");
            
            if (personalTableExists == 0)
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

    /// <summary>
    /// Genereaza urmatorul cod de angajat automat (EMP001, EMP002, etc.)
    /// </summary>
    public async Task<string> GetNextCodAngajatAsync()
    {
        try
        {
            _logger.LogInformation("Generating next employee code");

            await EnsureConnectionOpenAsync();

            // Query pentru a gasi ultimul cod de angajat
            var lastCodAngajat = await _connection.QueryFirstOrDefaultAsync<string>(
                @"SELECT TOP 1 Cod_Angajat 
                  FROM Personal 
                  WHERE Cod_Angajat LIKE 'EMP%' 
                    AND LEN(Cod_Angajat) = 6
                    AND ISNUMERIC(SUBSTRING(Cod_Angajat, 4, 3)) = 1
                  ORDER BY CAST(SUBSTRING(Cod_Angajat, 4, 3) AS INT) DESC");

            if (string.IsNullOrEmpty(lastCodAngajat))
            {
                // Primul angajat in sistem
                _logger.LogInformation("No existing employee codes found, starting with EMP001");
                return "EMP001";
            }

            // Extrage numarul din ultimul cod (ex: EMP029 -> 029 -> 29)
            var lastNumberString = lastCodAngajat.Substring(3); // ultimele 3 cifre
            if (!int.TryParse(lastNumberString, out var lastNumber))
            {
                _logger.LogWarning("Could not parse last employee code: {LastCode}, defaulting to EMP001", lastCodAngajat);
                return "EMP001";
            }

            // Genereaza urmatorul numar
            var nextNumber = lastNumber + 1;
            var nextCode = $"EMP{nextNumber:D3}"; // Format cu 3 cifre: EMP001, EMP002, etc.

            _logger.LogInformation("Generated next employee code: {NextCode} (previous was {LastCode})", nextCode, lastCodAngajat);
            
            return nextCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next employee code");
            throw;
        }
    }
}
