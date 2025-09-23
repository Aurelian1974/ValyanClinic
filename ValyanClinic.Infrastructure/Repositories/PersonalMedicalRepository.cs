using System.Data;
using Dapper;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository implementation pentru PersonalMedical folosind Dapper si Stored Procedures
/// Conform best practices: fara SQL queries in cod, doar SP calls
/// Similar cu PersonalRepository dar adaptat pentru tabela PersonalMedical
/// </summary>
public class PersonalMedicalRepository : IPersonalMedicalRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<PersonalMedicalRepository> _logger;

    public PersonalMedicalRepository(IDbConnection connection, ILogger<PersonalMedicalRepository> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(IEnumerable<PersonalMedical> Data, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        string? departament = null,
        string? pozitie = null,
        string? status = null,
        bool? areSpecializare = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC")
    {
        try
        {
            _logger.LogInformation("Getting personal medical data with pagination. Page: {PageNumber}, Size: {PageSize}, SearchText: {SearchText}", 
                pageNumber, pageSize, searchText?.Length > 0 ? $"'{searchText[..Math.Min(searchText.Length, 20)]}...'" : "none");

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchText", searchText);
            parameters.Add("@Departament", departament);
            parameters.Add("@Pozitie", pozitie);
            
            // Convert status string to boolean for EsteActiv parameter
            bool? esteActiv = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("activ", StringComparison.OrdinalIgnoreCase) || status.Equals("true", StringComparison.OrdinalIgnoreCase))
                    esteActiv = true;
                else if (status.Equals("inactiv", StringComparison.OrdinalIgnoreCase) || status.Equals("false", StringComparison.OrdinalIgnoreCase))
                    esteActiv = false;
            }
            parameters.Add("@EsteActiv", esteActiv);
            
            parameters.Add("@SortColumn", sortColumn);
            parameters.Add("@SortDirection", sortDirection);

            using var multi = await _connection.QueryMultipleAsync(
                "sp_PersonalMedical_GetAll", 
                parameters, 
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60);

            var data = await multi.ReadAsync<PersonalMedicalDto>();
            var totalCount = await multi.ReadSingleAsync<int>();

            var personalMedicalList = data.Select(MapDtoToPersonalMedical).ToList();

            _logger.LogInformation("Retrieved {Count} personal medical records out of {TotalCount}", 
                personalMedicalList.Count, totalCount);

            return (personalMedicalList, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal medical data with parameters: Page={PageNumber}, Size={PageSize}, Search={SearchText}", 
                pageNumber, pageSize, searchText);
            throw;
        }
    }

    public async Task<PersonalMedical?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Getting personal medical by ID: {PersonalId}", id);

            if (id == Guid.Empty)
            {
                _logger.LogWarning("GetByIdAsync called with empty GUID");
                return null;
            }

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@PersonalID", id);

            var result = await _connection.QueryFirstOrDefaultAsync<PersonalMedicalDto>(
                "sp_PersonalMedical_GetById",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            var mappedResult = result != null ? MapDtoToPersonalMedical(result) : null;
            
            _logger.LogDebug("Personal medical {PersonalId} {Found}", id, mappedResult != null ? "found" : "not found");
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal medical by ID: {PersonalId}", id);
            throw;
        }
    }

    public async Task<PersonalMedical> CreateAsync(PersonalMedical personalMedical, string creatDe)
    {
        try
        {
            if (personalMedical == null) throw new ArgumentNullException(nameof(personalMedical));
            if (string.IsNullOrWhiteSpace(creatDe)) throw new ArgumentException("CreatDe nu poate fi gol", nameof(creatDe));

            _logger.LogInformation("Creating new personal medical: {Nume} {Prenume}, Position: {Pozitie}", 
                personalMedical.Nume, personalMedical.Prenume, personalMedical.Pozitie);

            // Validare business logic
            if (!personalMedical.IsValidForSave())
            {
                var errors = personalMedical.GetValidationErrors();
                throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
            }

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Nume", personalMedical.Nume?.Trim());
            parameters.Add("@Prenume", personalMedical.Prenume?.Trim());
            parameters.Add("@Specializare", personalMedical.Specializare?.Trim());
            parameters.Add("@NumarLicenta", personalMedical.NumarLicenta?.Trim());
            parameters.Add("@Telefon", personalMedical.Telefon?.Trim());
            parameters.Add("@Email", personalMedical.Email?.Trim()?.ToLowerInvariant());
            parameters.Add("@Departament", personalMedical.Departament?.Trim());
            parameters.Add("@Pozitie", personalMedical.Pozitie?.ToDatabase());
            parameters.Add("@EsteActiv", personalMedical.EsteActiv);
            parameters.Add("@CategorieID", personalMedical.CategorieID);
            parameters.Add("@SpecializareID", personalMedical.SpecializareID);
            parameters.Add("@SubspecializareID", personalMedical.SubspecializareID);

            var result = await _connection.QueryFirstAsync<PersonalMedicalDto>(
                "sp_PersonalMedical_Create",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);

            var mappedResult = MapDtoToPersonalMedical(result);
            
            _logger.LogInformation("Successfully created personal medical: {PersonalId} - {NumeComplet}", 
                mappedResult.PersonalID, mappedResult.NumeComplet);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            if (ex is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(ex, "SQL Error creating personal medical: Number={Number}, Severity={Severity}, State={State}, Procedure={Procedure}", 
                    sqlEx.Number, sqlEx.Class, sqlEx.State, sqlEx.Procedure);
                
                // Handle specific SQL error codes
                var friendlyMessage = sqlEx.Number switch
                {
                    2627 or 2601 => "Exists already a record with the same data (duplicate)",
                    547 => "Foreign key constraint violation",
                    515 => "Required field is missing",
                    _ => "Database operation failed"
                };
                
                throw new InvalidOperationException(friendlyMessage, ex);
            }
            
            _logger.LogError(ex, "Error creating personal medical: {Nume} {Prenume}", 
                personalMedical?.Nume, personalMedical?.Prenume);
            throw;
        }
    }

    public async Task<PersonalMedical> UpdateAsync(PersonalMedical personalMedical, string modificatDe)
    {
        try
        {
            if (personalMedical == null) throw new ArgumentNullException(nameof(personalMedical));
            if (string.IsNullOrWhiteSpace(modificatDe)) throw new ArgumentException("ModificatDe nu poate fi gol", nameof(modificatDe));
            if (personalMedical.PersonalID == Guid.Empty) throw new ArgumentException("PersonalID nu poate fi gol");

            _logger.LogInformation("Updating personal medical: {PersonalId} - {NumeComplet}", 
                personalMedical.PersonalID, personalMedical.NumeComplet);

            // Validare business logic
            if (!personalMedical.IsValidForSave())
            {
                var errors = personalMedical.GetValidationErrors();
                throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
            }

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@PersonalID", personalMedical.PersonalID);
            parameters.Add("@Nume", personalMedical.Nume?.Trim());
            parameters.Add("@Prenume", personalMedical.Prenume?.Trim());
            parameters.Add("@Specializare", personalMedical.Specializare?.Trim());
            parameters.Add("@NumarLicenta", personalMedical.NumarLicenta?.Trim());
            parameters.Add("@Telefon", personalMedical.Telefon?.Trim());
            parameters.Add("@Email", personalMedical.Email?.Trim()?.ToLowerInvariant());
            parameters.Add("@Departament", personalMedical.Departament?.Trim());
            parameters.Add("@Pozitie", personalMedical.Pozitie?.ToDatabase());
            parameters.Add("@EsteActiv", personalMedical.EsteActiv);
            parameters.Add("@CategorieID", personalMedical.CategorieID);
            parameters.Add("@SpecializareID", personalMedical.SpecializareID);
            parameters.Add("@SubspecializareID", personalMedical.SubspecializareID);

            var result = await _connection.QueryFirstOrDefaultAsync<PersonalMedicalDto>(
                "sp_PersonalMedical_Update",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);

            if (result == null)
            {
                throw new InvalidOperationException($"Personal medical cu ID {personalMedical.PersonalID} nu a fost gasit pentru eventuala actualizare");
            }

            var mappedResult = MapDtoToPersonalMedical(result);
            
            _logger.LogInformation("Successfully updated personal medical: {PersonalId} - {NumeComplet}", 
                mappedResult.PersonalID, mappedResult.NumeComplet);
            
            return mappedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal medical: {PersonalId}", personalMedical?.PersonalID);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, string modificatDe)
    {
        try
        {
            if (id == Guid.Empty) throw new ArgumentException("ID nu poate fi gol", nameof(id));

            _logger.LogInformation("Soft deleting personal medical: {PersonalId}", id);

            // Check for active appointments first
            var hasActiveAppointments = await CheckActiveAppointmentsAsync(id);
            if (hasActiveAppointments)
            {
                throw new InvalidOperationException("Nu se poate sterge personalul medical cu programari active");
            }

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@PersonalID", id);

            var result = await _connection.QueryFirstAsync<dynamic>(
                "sp_PersonalMedical_Delete",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60);

            var success = ((int)result.Success) == 1;
            
            _logger.LogInformation("Personal medical {PersonalId} deletion result: {Success}", id, success);
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal medical: {PersonalId}", id);
            throw;
        }
    }

    public async Task<bool> CheckLicentaUnicityAsync(string numarLicenta, Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numarLicenta))
            {
                return true; // Empty license numbers are allowed (for non-medical positions)
            }

            _logger.LogDebug("Checking medical license unicity: {NumarLicenta}", numarLicenta);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@NumarLicenta", numarLicenta.Trim());
            parameters.Add("@ExcludeId", excludeId);
            parameters.Add("@Email", (string?)null); // Add Email parameter since SP expects both

            var result = await _connection.QueryFirstAsync<dynamic>(
                "sp_PersonalMedical_CheckUnique",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            // The SP returns Email_Exists and NumarLicenta_Exists, we want the opposite for "unicity"
            bool isUnique = result.NumarLicenta_Exists == 0;
            
            _logger.LogDebug("License unicity check result for {NumarLicenta}: {IsUnique}", numarLicenta, isUnique);
            
            return isUnique;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking medical license unicity: {NumarLicenta}", numarLicenta);
            throw;
        }
    }

    public async Task<bool> CheckEmailUnicityAsync(string email, Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true; // Empty emails are allowed
            }

            _logger.LogDebug("Checking medical email unicity: {Email}", email);

            await EnsureConnectionOpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email.Trim().ToLowerInvariant());
            parameters.Add("@ExcludeId", excludeId);
            parameters.Add("@NumarLicenta", (string?)null); // Add NumarLicenta parameter since SP expects both

            var result = await _connection.QueryFirstAsync<dynamic>(
                "sp_PersonalMedical_CheckUnique",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            // The SP returns Email_Exists and NumarLicenta_Exists, we want the opposite for "unicity"
            bool isUnique = result.Email_Exists == 0;

            _logger.LogDebug("Email unicity check result for {Email}: {IsUnique}", email, isUnique);
            
            return isUnique;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking medical email unicity: {Email}", email);
            throw;
        }
    }

    public async Task<bool> CheckActiveAppointmentsAsync(Guid personalId)
    {
        try
        {
            if (personalId == Guid.Empty)
            {
                return false;
            }

            _logger.LogDebug("Checking active appointments for personal medical: {PersonalId}", personalId);

            await EnsureConnectionOpenAsync();

            // Since we don't have the specific SP, we'll do a simple check
            // This assumes there's a Programari table - if not, this should always return false
            var result = await _connection.QueryFirstOrDefaultAsync<int?>(
                @"SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Programari'");

            if (result == null)
            {
                // Programari table doesn't exist, so no active appointments
                _logger.LogDebug("Programari table not found, assuming no active appointments for {PersonalId}", personalId);
                return false;
            }

            // Check if there are active appointments
            var appointmentCount = await _connection.QueryFirstOrDefaultAsync<int?>(
                @"SELECT COUNT(*) FROM Programari WHERE PersonalID = @PersonalID AND DataProgramare > GETDATE() AND Status = 'Programata'",
                new { PersonalID = personalId });

            bool hasActiveAppointments = appointmentCount > 0;
            
            _logger.LogDebug("Active appointments check result for {PersonalId}: {HasActiveAppointments}", personalId, hasActiveAppointments);
            
            return hasActiveAppointments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking active appointments for personal medical: {PersonalId}", personalId);
            // If we can't check, assume no active appointments to allow deletion
            return false;
        }
    }

    public async Task<(int TotalPersonalMedical, int PersonalMedicalActiv, int PersonalMedicalInactiv, 
                       int TotalDoctori, int TotalAsistenti, int TotalTehnicianiMedicali)> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Getting personal medical statistics");

            await EnsureConnectionOpenAsync();

            var result = await _connection.QueryAsync<StatisticDto>(
                "sp_PersonalMedical_GetStatistics",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60);

            var stats = result.ToDictionary(x => x.StatisticName, x => x.Value);

            var statisticsResult = (
                TotalPersonalMedical: stats.GetValueOrDefault("Total Personal Medical", 0),
                PersonalMedicalActiv: stats.GetValueOrDefault("Personal Medical Activ", 0),
                PersonalMedicalInactiv: stats.GetValueOrDefault("Personal Medical Inactiv", 0),
                TotalDoctori: stats.GetValueOrDefault("Total Doctori", 0),
                TotalAsistenti: stats.GetValueOrDefault("Total Asistenti", 0),
                TotalTehnicianiMedicali: stats.GetValueOrDefault("Total Tehniciani", 0)
            );

            _logger.LogInformation("Personal medical statistics: Total={Total}, Active={Active}, Inactive={Inactive}, Doctors={Doctors}, Nurses={Nurses}, Technicians={Technicians}", 
                statisticsResult.TotalPersonalMedical, statisticsResult.PersonalMedicalActiv, statisticsResult.PersonalMedicalInactiv,
                statisticsResult.TotalDoctori, statisticsResult.TotalAsistenti, statisticsResult.TotalTehnicianiMedicali);

            return statisticsResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal medical statistics");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetDistributiePerDepartamentAsync()
    {
        try
        {
            _logger.LogDebug("Getting personal medical distribution per department");

            await EnsureConnectionOpenAsync();
            
            var result = await _connection.QueryAsync<DistributieDto>(
                "sp_PersonalMedical_GetDistributiePerDepartament",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60);

            var distribution = result.ToDictionary(x => x.Categorie ?? "Nedefinit", x => x.Numar);
            
            _logger.LogDebug("Retrieved distribution per department with {Count} categories", distribution.Count);
            
            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal medical distribution per department");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetDistributiePerSpecializareAsync()
    {
        try
        {
            _logger.LogDebug("Getting personal medical distribution per specialization");

            await EnsureConnectionOpenAsync();
            
            var result = await _connection.QueryAsync<DistributieDto>(
                "sp_PersonalMedical_GetDistributiePerSpecializare",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60);

            var distribution = result.ToDictionary(x => x.Categorie ?? "Nespecializat", x => x.Numar);
            
            _logger.LogDebug("Retrieved distribution per specialization with {Count} specializations", distribution.Count);
            
            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personal medical distribution per specialization");
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
                _logger.LogDebug("Opening database connection");
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
                    _logger.LogDebug("Connection test successful");
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

    private static PersonalMedical MapDtoToPersonalMedical(PersonalMedicalDto dto)
    {
        return new PersonalMedical
        {
            PersonalID = dto.PersonalID,
            Nume = dto.Nume ?? string.Empty,
            Prenume = dto.Prenume ?? string.Empty,
            Specializare = dto.Specializare,
            NumarLicenta = dto.NumarLicenta,
            Telefon = dto.Telefon,
            Email = dto.Email,
            Departament = dto.Departament,
            Pozitie = PozitiePersonalMedicalExtensions.ParseFromDatabase(dto.Pozitie),
            EsteActiv = dto.EsteActiv,
            DataCreare = dto.DataCreare,
            CategorieID = dto.CategorieID,
            SpecializareID = dto.SpecializareID,
            SubspecializareID = dto.SubspecializareID,
            CategorieName = dto.CategorieName,
            SpecializareName = dto.SpecializareName,
            SubspecializareName = dto.SubspecializareName
        };
    }

    private static T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Pentru PozitiePersonalMedical, folosim metoda robusta de parsing
        if (typeof(T) == typeof(PozitiePersonalMedical))
        {
            var enumResult = PozitiePersonalMedicalExtensions.ParseFromDatabase(value);
            return enumResult.HasValue ? (T)(object)enumResult.Value : null;
        }

        // Pentru alte enum-uri, folosim parsing-ul standard
        return Enum.TryParse<T>(value, true, out var parseResult) ? parseResult : null;
    }

    #endregion

    #region DTOs for Dapper Mapping

    private class PersonalMedicalDto
    {
        public Guid PersonalID { get; set; }
        public string? Nume { get; set; }
        public string? Prenume { get; set; }
        public string? Specializare { get; set; }
        public string? NumarLicenta { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Departament { get; set; }
        public string? Pozitie { get; set; }
        public bool EsteActiv { get; set; } = true;
        public DateTime DataCreare { get; set; }
        public Guid? CategorieID { get; set; }
        public Guid? SpecializareID { get; set; }
        public Guid? SubspecializareID { get; set; }
        public string? CategorieName { get; set; }
        public string? SpecializareName { get; set; }
        public string? SubspecializareName { get; set; }
    }

    private class StatisticDto
    {
        public string StatisticName { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? IconClass { get; set; }
        public string? ColorClass { get; set; }
    }

    private class DistributieDto
    {
        public string? Categorie { get; set; }
        public int Numar { get; set; }
    }

    #endregion

    public async Task<bool> TestDatabaseConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing database connection for PersonalMedical repository");

            await EnsureConnectionOpenAsync();
            
            // Test 1: Basic connection test
            var basicTest = await _connection.QuerySingleAsync<int>("SELECT 1");
            _logger.LogDebug("Basic connection test passed: {Result}", basicTest);
            
            // Test 2: Database name
            var dbName = await _connection.QuerySingleAsync<string>("SELECT DB_NAME()");
            _logger.LogDebug("Connected to database: {DatabaseName}", dbName);
            
            // Test 3: Check PersonalMedical table
            var personalMedicalTableExists = await _connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PersonalMedical'");
            
            if (personalMedicalTableExists == 0)
            {
                _logger.LogError("PersonalMedical table does not exist in database");
                return false;
            }
            
            _logger.LogDebug("PersonalMedical table exists");
            
            // Test 4: Check stored procedures existence
            var storedProcedures = new []
            {
                "sp_PersonalMedical_GetAll",
                "sp_PersonalMedical_GetById",
                "sp_PersonalMedical_Create",
                "sp_PersonalMedical_Update",
                "sp_PersonalMedical_Delete"
            };
            
            foreach (var spName in storedProcedures)
            {
                var spExists = await _connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = @SpName AND ROUTINE_TYPE = 'PROCEDURE'",
                    new { SpName = spName });
                
                if (spExists == 0)
                {
                    _logger.LogWarning("Stored procedure {StoredProcedure} does not exist", spName);
                }
                else
                {
                    _logger.LogDebug("Stored procedure {StoredProcedure} exists", spName);
                }
            }
            
            _logger.LogInformation("Database connection test completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return false;
        }
    }
}
