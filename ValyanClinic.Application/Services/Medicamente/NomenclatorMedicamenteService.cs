using System.Data;
using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Services.Medicamente;

/// <summary>
/// Implementare serviciu nomenclator medicamente.
/// Descarcă și importă nomenclatorul ANM din Excel.
/// </summary>
public class NomenclatorMedicamenteService : INomenclatorMedicamenteService
{
    private readonly ILogger<NomenclatorMedicamenteService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    private const string ANM_NOMENCLATOR_URL = "https://nomenclator.anm.ro/files/nomenclator.xlsx";
    private const int SYNC_INTERVAL_DAYS = 7; // Sincronizare săptămânală

    public NomenclatorMedicamenteService(
        ILogger<NomenclatorMedicamenteService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MedicamentNomenclator>>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            return Result<IReadOnlyList<MedicamentNomenclator>>.Success(Array.Empty<MedicamentNomenclator>());
        }

        try
        {
            var results = new List<MedicamentNomenclator>();
            
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_Search", connection)
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

            return Result<IReadOnlyList<MedicamentNomenclator>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la căutarea medicamentelor pentru '{SearchTerm}'", searchTerm);
            return Result<IReadOnlyList<MedicamentNomenclator>>.Failure($"Eroare la căutare: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<MedicamentNomenclator?>> GetByCodeAsync(
        string codCIM,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codCIM))
        {
            return Result<MedicamentNomenclator?>.Success(null);
        }

        try
        {
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_GetByCod", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@CodCIM", codCIM);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return Result<MedicamentNomenclator?>.Success(MapFromReader(reader));
            }

            return Result<MedicamentNomenclator?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obținerea medicamentului cu cod '{CodCIM}'", codCIM);
            return Result<MedicamentNomenclator?>.Failure($"Eroare: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<SyncResult>> SyncFromANMAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var syncResult = new SyncResult();
        Guid? logId = null;

        try
        {
            _logger.LogInformation("Începe sincronizarea nomenclatorului ANM...");

            // 1. Start log
            logId = await StartSyncLogAsync(ANM_NOMENCLATOR_URL, cancellationToken);

            // 2. Download Excel
            _logger.LogInformation("Descărcare fișier de la {Url}", ANM_NOMENCLATOR_URL);
            var excelBytes = await DownloadExcelAsync(cancellationToken);
            
            if (excelBytes == null || excelBytes.Length == 0)
            {
                throw new InvalidOperationException("Fișierul descărcat este gol.");
            }

            _logger.LogInformation("Fișier descărcat: {Size} bytes", excelBytes.Length);

            // 3. Parse Excel și import
            var fileName = $"nomenclator_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var (added, updated) = await ImportExcelAsync(excelBytes, fileName, cancellationToken);

            syncResult.RecordsAdded = added;
            syncResult.RecordsUpdated = updated;
            syncResult.TotalRecords = added + updated;

            // 4. Dezactivează medicamentele vechi
            syncResult.RecordsDeactivated = await DeactivateOldRecordsAsync(fileName, cancellationToken);

            syncResult.Success = true;
            stopwatch.Stop();
            syncResult.Duration = stopwatch.Elapsed;

            // 5. Complete log
            if (logId.HasValue)
            {
                await CompleteSyncLogAsync(logId.Value, "Success", syncResult, fileName, excelBytes.Length, cancellationToken);
            }

            _logger.LogInformation(
                "Sincronizare completă: {Added} adăugate, {Updated} actualizate, {Deactivated} dezactivate în {Duration}",
                syncResult.RecordsAdded, syncResult.RecordsUpdated, syncResult.RecordsDeactivated, syncResult.Duration);

            return Result<SyncResult>.Success(syncResult);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            syncResult.Success = false;
            syncResult.ErrorMessage = ex.Message;
            syncResult.Duration = stopwatch.Elapsed;

            _logger.LogError(ex, "Eroare la sincronizarea nomenclatorului ANM");

            if (logId.HasValue)
            {
                await CompleteSyncLogAsync(logId.Value, "Failed", syncResult, null, null, cancellationToken, ex.Message);
            }

            return Result<SyncResult>.Failure($"Eroare la sincronizare: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<NomenclatorStats>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_GetStats", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var stats = new NomenclatorStats
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
                return Result<NomenclatorStats>.Success(stats);
            }

            return Result<NomenclatorStats>.Success(new NomenclatorStats());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obținerea statisticilor nomenclatorului");
            return Result<NomenclatorStats>.Failure($"Eroare: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<bool> NeedsSyncAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statsResult = await GetStatsAsync(cancellationToken);
            if (!statsResult.IsSuccess || statsResult.Value == null)
            {
                return true; // Dacă nu putem verifica, trebuie sync
            }

            var stats = statsResult.Value;
            
            // Dacă nu avem date sau ultima sincronizare e prea veche
            if (stats.TotalActive == 0)
            {
                return true;
            }

            if (!stats.UltimaSincronizareReusita.HasValue)
            {
                return true;
            }

            var daysSinceLastSync = (DateTime.Now - stats.UltimaSincronizareReusita.Value).TotalDays;
            return daysSinceLastSync >= SYNC_INTERVAL_DAYS;
        }
        catch
        {
            return true;
        }
    }

    #region Private Methods

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    private async Task<byte[]?> DownloadExcelAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(ANM_NOMENCLATOR_URL, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private async Task<(int added, int updated)> ImportExcelAsync(
        byte[] excelBytes, 
        string fileName,
        CancellationToken cancellationToken)
    {
        int added = 0;
        int updated = 0;

        using var stream = new MemoryStream(excelBytes);
        using var workbook = new XLWorkbook(stream);
        
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1); // Skip header

        await using var connection = new SqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Parsare coloane exacte din Excel ANM (1-20)
                // Col 1: Cod CIM, Col 2: Denumire comerciala, Col 3: DCI, Col 4: Forma farmaceutica
                // Col 5: Concentratie, Col 6: Firma/tara producatoare APP, Col 7: Firma/tara detinatoare APP
                // Col 8: Cod ATC, Col 9: Actiune terapeutica, Col 10: Prescriptie
                // Col 11: Nr/data ambalaj APP, Col 12: Ambalaj, Col 13: Volum ambalaj
                // Col 14: Valabilitate ambalaj, Col 15: Bulina, Col 16: Diez
                // Col 17: Stea, Col 18: Triunghi, Col 19: Dreptunghi, Col 20: Data actualizare
                var codCIM = GetCellValue(row, 1);
                var denumireComerciala = GetCellValue(row, 2);
                var dci = GetCellValue(row, 3);
                var formaFarmaceutica = GetCellValue(row, 4);
                var concentratie = GetCellValue(row, 5);
                var firmaTaraProducatoareAPP = GetCellValue(row, 6);
                var firmaTaraDetinatoareAPP = GetCellValue(row, 7);
                var codATC = GetCellValue(row, 8);
                var actiuneTerapeutica = GetCellValue(row, 9);
                var prescriptie = GetCellValue(row, 10);
                var nrDataAmbalajAPP = GetCellValue(row, 11);
                var ambalaj = GetCellValue(row, 12);
                var volumAmbalaj = GetCellValue(row, 13);
                var valabilitateAmbalaj = GetCellValue(row, 14);
                var bulina = GetCellValue(row, 15);
                var diez = GetCellValue(row, 16);
                var stea = GetCellValue(row, 17);
                var triunghi = GetCellValue(row, 18);
                var dreptunghi = GetCellValue(row, 19);
                var dataActualizare = GetCellValue(row, 20);

                if (string.IsNullOrWhiteSpace(codCIM) || string.IsNullOrWhiteSpace(denumireComerciala))
                {
                    continue;
                }

                await using var command = new SqlCommand("Medicamente_Upsert", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@CodCIM", codCIM);
                command.Parameters.AddWithValue("@DenumireComerciala", denumireComerciala);
                command.Parameters.AddWithValue("@DCI", (object?)dci ?? DBNull.Value);
                command.Parameters.AddWithValue("@FormaFarmaceutica", (object?)formaFarmaceutica ?? DBNull.Value);
                command.Parameters.AddWithValue("@Concentratie", (object?)concentratie ?? DBNull.Value);
                command.Parameters.AddWithValue("@FirmaTaraProducatoareAPP", (object?)firmaTaraProducatoareAPP ?? DBNull.Value);
                command.Parameters.AddWithValue("@FirmaTaraDetinatoareAPP", (object?)firmaTaraDetinatoareAPP ?? DBNull.Value);
                command.Parameters.AddWithValue("@CodATC", (object?)codATC ?? DBNull.Value);
                command.Parameters.AddWithValue("@ActiuneTerapeutica", (object?)actiuneTerapeutica ?? DBNull.Value);
                command.Parameters.AddWithValue("@Prescriptie", (object?)prescriptie ?? DBNull.Value);
                command.Parameters.AddWithValue("@NrDataAmbalajAPP", (object?)nrDataAmbalajAPP ?? DBNull.Value);
                command.Parameters.AddWithValue("@Ambalaj", (object?)ambalaj ?? DBNull.Value);
                command.Parameters.AddWithValue("@VolumAmbalaj", (object?)volumAmbalaj ?? DBNull.Value);
                command.Parameters.AddWithValue("@ValabilitateAmbalaj", (object?)valabilitateAmbalaj ?? DBNull.Value);
                command.Parameters.AddWithValue("@Bulina", (object?)bulina ?? DBNull.Value);
                command.Parameters.AddWithValue("@Diez", (object?)diez ?? DBNull.Value);
                command.Parameters.AddWithValue("@Stea", (object?)stea ?? DBNull.Value);
                command.Parameters.AddWithValue("@Triunghi", (object?)triunghi ?? DBNull.Value);
                command.Parameters.AddWithValue("@Dreptunghi", (object?)dreptunghi ?? DBNull.Value);
                command.Parameters.AddWithValue("@DataActualizare", (object?)dataActualizare ?? DBNull.Value);
                command.Parameters.AddWithValue("@SursaFisier", fileName);
                
                var isNewParam = new SqlParameter("@IsNew", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(isNewParam);

                await command.ExecuteNonQueryAsync(cancellationToken);

                if ((bool)isNewParam.Value)
                    added++;
                else
                    updated++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Eroare la importul rândului {RowNumber}", row.RowNumber());
            }
        }

        return (added, updated);
    }

    private static string? GetCellValue(IXLRow row, int column)
    {
        var cell = row.Cell(column);
        if (cell.IsEmpty()) return null;
        
        var value = cell.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private async Task<Guid?> StartSyncLogAsync(string sourceUrl, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_SyncLog_Start", connection)
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
            return null;
        }
    }

    private async Task CompleteSyncLogAsync(
        Guid logId,
        string status,
        SyncResult result,
        string? fileName,
        long? fileSize,
        CancellationToken cancellationToken,
        string? errorMessage = null)
    {
        try
        {
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_SyncLog_Complete", connection)
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

    private async Task<int> DeactivateOldRecordsAsync(string currentFileName, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("Medicamente_DeactivateOld", connection)
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
