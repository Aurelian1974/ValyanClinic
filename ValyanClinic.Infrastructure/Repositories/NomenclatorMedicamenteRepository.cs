using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Services.Medicamente;
using ValyanClinic.Domain.Interfaces.Data;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Implementare repository pentru nomenclatorul de medicamente ANM.
/// Accesează baza de date prin stored procedures.
/// </summary>
public class NomenclatorMedicamenteRepository : INomenclatorMedicamenteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NomenclatorMedicamenteRepository> _logger;

    public NomenclatorMedicamenteRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<NomenclatorMedicamenteRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<List<MedicamentNomenclator>> SearchAsync(
        string searchTerm,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        var results = new List<MedicamentNomenclator>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("Medicamente_Search", (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@SearchTerm", searchTerm);
        command.Parameters.AddWithValue("@MaxResults", maxResults);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapFromReader(reader));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<MedicamentNomenclator?> GetByCodeAsync(
        string codCIM,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("Medicamente_GetByCod", (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@CodCIM", codCIM);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapFromReader(reader);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> UpsertAsync(
        MedicamentNomenclator medicament,
        string sursaFisier,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("Medicamente_Upsert", (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@CodCIM", medicament.CodCIM);
        command.Parameters.AddWithValue("@DenumireComerciala", medicament.DenumireComerciala);
        command.Parameters.AddWithValue("@DCI", (object?)medicament.DCI ?? DBNull.Value);
        command.Parameters.AddWithValue("@FormaFarmaceutica", (object?)medicament.FormaFarmaceutica ?? DBNull.Value);
        command.Parameters.AddWithValue("@Concentratie", (object?)medicament.Concentratie ?? DBNull.Value);
        command.Parameters.AddWithValue("@FirmaTaraProducatoareAPP", (object?)medicament.FirmaTaraProducatoareAPP ?? DBNull.Value);
        command.Parameters.AddWithValue("@FirmaTaraDetinatoareAPP", (object?)medicament.FirmaTaraDetinatoareAPP ?? DBNull.Value);
        command.Parameters.AddWithValue("@CodATC", (object?)medicament.CodATC ?? DBNull.Value);
        command.Parameters.AddWithValue("@ActiuneTerapeutica", (object?)medicament.ActiuneTerapeutica ?? DBNull.Value);
        command.Parameters.AddWithValue("@Prescriptie", (object?)medicament.Prescriptie ?? DBNull.Value);
        command.Parameters.AddWithValue("@NrDataAmbalajAPP", (object?)medicament.NrDataAmbalajAPP ?? DBNull.Value);
        command.Parameters.AddWithValue("@Ambalaj", (object?)medicament.Ambalaj ?? DBNull.Value);
        command.Parameters.AddWithValue("@VolumAmbalaj", (object?)medicament.VolumAmbalaj ?? DBNull.Value);
        command.Parameters.AddWithValue("@ValabilitateAmbalaj", (object?)medicament.ValabilitateAmbalaj ?? DBNull.Value);
        command.Parameters.AddWithValue("@Bulina", (object?)medicament.Bulina ?? DBNull.Value);
        command.Parameters.AddWithValue("@Diez", (object?)medicament.Diez ?? DBNull.Value);
        command.Parameters.AddWithValue("@Stea", (object?)medicament.Stea ?? DBNull.Value);
        command.Parameters.AddWithValue("@Triunghi", (object?)medicament.Triunghi ?? DBNull.Value);
        command.Parameters.AddWithValue("@Dreptunghi", (object?)medicament.Dreptunghi ?? DBNull.Value);
        command.Parameters.AddWithValue("@DataActualizare", (object?)medicament.DataActualizare ?? DBNull.Value);
        command.Parameters.AddWithValue("@SursaFisier", sursaFisier);

        var isNewParam = new SqlParameter("@IsNew", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        command.Parameters.Add(isNewParam);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return (bool)isNewParam.Value;
    }

    /// <inheritdoc />
    public async Task<NomenclatorStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("Medicamente_GetStats", (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new NomenclatorStats
            {
                TotalActive = reader.GetInt32(reader.GetOrdinal("TotalActive")),
                TotalInactive = reader.GetInt32(reader.GetOrdinal("TotalInactive")),
                TotalDCI = reader.GetInt32(reader.GetOrdinal("TotalDCI")),
                TotalProducatori = reader.GetInt32(reader.GetOrdinal("TotalProducatori")),
                UltimaActualizare = reader.IsDBNull(reader.GetOrdinal("UltimaActualizare"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("UltimaActualizare")),
                UltimaSincronizareReusita = reader.IsDBNull(reader.GetOrdinal("UltimaSincronizareReusita"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("UltimaSincronizareReusita"))
            };
        }

        return new NomenclatorStats();
    }

    /// <inheritdoc />
    public async Task<int> DeactivateOldRecordsAsync(
        string currentFileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_DeactivateOld", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@SursaFisier", currentFileName);

            var countParam = new SqlParameter("@DeactivatedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(countParam);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return (int)countParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nu s-au putut dezactiva înregistrările vechi");
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> StartSyncLogAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_SyncLog_Start", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@SursaURL", sourceUrl);

            var logIdParam = new SqlParameter("@LogId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output };
            command.Parameters.Add(logIdParam);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return (Guid)logIdParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nu s-a putut crea log-ul de sincronizare");
            return Guid.Empty;
        }
    }

    /// <inheritdoc />
    public async Task CompleteSyncLogAsync(
        Guid logId,
        string status,
        SyncResult result,
        string? fileName,
        long? fileSize,
        string? errorMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_SyncLog_Complete", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@LogId", logId);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@TotalRecords", (object?)result.TotalRecords ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecordsAdded", (object?)result.RecordsAdded ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecordsUpdated", (object?)result.RecordsUpdated ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecordsDeactivated", (object?)result.RecordsDeactivated ?? DBNull.Value);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);
            command.Parameters.AddWithValue("@NumeFisier", (object?)fileName ?? DBNull.Value);
            command.Parameters.AddWithValue("@DimensiuneFisier", (object?)fileSize ?? DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nu s-a putut actualiza log-ul de sincronizare");
        }
    }

    /// <inheritdoc />
    public async Task<DateTime?> GetLastSuccessfulSyncDateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Verifică dacă există tabela SyncLog
            var checkTableSql = @"
                SELECT TOP 1 [DataEnd]
                FROM [dbo].[Medicamente_SyncLog]
                WHERE [Status] = 'Success'
                ORDER BY [Id] DESC";

            await using var command = new SqlCommand(checkTableSql, (SqlConnection)connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            return (DateTime)result;
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name (tabla nu există)
        {
            _logger.LogWarning("Tabela Medicamente_SyncLog nu există");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la verificarea ultimei sincronizări");
            return null;
        }
    }

    #region Private Mapping Methods

    private static MedicamentNomenclator MapFromReader(SqlDataReader reader)
    {
        return new MedicamentNomenclator
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            CodCIM = reader.GetString(reader.GetOrdinal("CodCIM")),
            DenumireComerciala = reader.GetString(reader.GetOrdinal("DenumireComerciala")),
            DCI = GetNullableString(reader, "DCI"),
            FormaFarmaceutica = GetNullableString(reader, "FormaFarmaceutica"),
            Concentratie = GetNullableString(reader, "Concentratie"),
            FirmaTaraProducatoareAPP = GetNullableString(reader, "FirmaTaraProducatoareAPP"),
            FirmaTaraDetinatoareAPP = GetNullableString(reader, "FirmaTaraDetinatoareAPP"),
            CodATC = GetNullableString(reader, "CodATC"),
            ActiuneTerapeutica = GetNullableString(reader, "ActiuneTerapeutica"),
            Prescriptie = GetNullableString(reader, "Prescriptie"),
            NrDataAmbalajAPP = GetNullableString(reader, "NrDataAmbalajAPP"),
            Ambalaj = GetNullableString(reader, "Ambalaj"),
            VolumAmbalaj = GetNullableString(reader, "VolumAmbalaj"),
            ValabilitateAmbalaj = GetNullableString(reader, "ValabilitateAmbalaj"),
            Bulina = GetNullableString(reader, "Bulina"),
            Diez = GetNullableString(reader, "Diez"),
            Stea = GetNullableString(reader, "Stea"),
            Triunghi = GetNullableString(reader, "Triunghi"),
            Dreptunghi = GetNullableString(reader, "Dreptunghi"),
            DataActualizare = GetNullableString(reader, "DataActualizare"),
            DataImport = reader.GetDateTime(reader.GetOrdinal("DataImport")),
            DataUltimaActualizare = reader.GetDateTime(reader.GetOrdinal("DataUltimaActualizare")),
            Activ = reader.GetBoolean(reader.GetOrdinal("Activ"))
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    #endregion
}
