using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Application.Services.Medicamente;

/// <summary>
/// Implementare serviciu nomenclator medicamente.
/// Descarcă și importă nomenclatorul ANM din Excel.
/// Utilizează Repository Pattern pentru accesul la date.
/// </summary>
public class NomenclatorMedicamenteService : INomenclatorMedicamenteService
{
    private readonly ILogger<NomenclatorMedicamenteService> _logger;
    private readonly INomenclatorMedicamenteRepository _repository;
    private readonly HttpClient _httpClient;

    private const string ANM_NOMENCLATOR_URL = "https://nomenclator.anm.ro/files/nomenclator.xlsx";
    private const int SYNC_INTERVAL_DAYS = 7; // Sincronizare săptămânală

    public NomenclatorMedicamenteService(
        ILogger<NomenclatorMedicamenteService> logger,
        INomenclatorMedicamenteRepository repository,
        HttpClient httpClient)
    {
        _logger = logger;
        _repository = repository;
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
            var results = await _repository.SearchAsync(searchTerm, maxResults, cancellationToken);
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
            var medicament = await _repository.GetByCodeAsync(codCIM, cancellationToken);
            return Result<MedicamentNomenclator?>.Success(medicament);
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
        Guid logId = Guid.Empty;

        try
        {
            _logger.LogInformation("Începe sincronizarea nomenclatorului ANM...");

            // 1. Start log
            logId = await _repository.StartSyncLogAsync(ANM_NOMENCLATOR_URL, cancellationToken);

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
            syncResult.RecordsDeactivated = await _repository.DeactivateOldRecordsAsync(fileName, cancellationToken);

            syncResult.Success = true;
            stopwatch.Stop();
            syncResult.Duration = stopwatch.Elapsed;

            // 5. Complete log
            if (logId != Guid.Empty)
            {
                await _repository.CompleteSyncLogAsync(logId, "Success", syncResult, fileName, excelBytes.Length, null, cancellationToken);
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

            if (logId != Guid.Empty)
            {
                await _repository.CompleteSyncLogAsync(logId, "Failed", syncResult, null, null, ex.Message, cancellationToken);
            }

            return Result<SyncResult>.Failure($"Eroare la sincronizare: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<NomenclatorStats>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _repository.GetStatsAsync(cancellationToken);
            return Result<NomenclatorStats>.Success(stats);
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
                // Fallback: verifică direct ultima sincronizare
                _logger.LogWarning("Nu s-au putut obține statisticile. Se verifică direct ultima sincronizare...");
                return await CheckNeedsSyncDirectAsync(cancellationToken);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la verificarea necesității sincronizării");
            // În caz de eroare, încercăm verificarea directă
            return await CheckNeedsSyncDirectAsync(cancellationToken);
        }
    }

    #region Private Methods

    /// <summary>
    /// Verifică direct necesitatea sincronizării folosind repository.
    /// Folosit ca fallback când stored procedure-ul GetStats nu există.
    /// </summary>
    private async Task<bool> CheckNeedsSyncDirectAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lastSync = await _repository.GetLastSuccessfulSyncDateAsync(cancellationToken);

            if (!lastSync.HasValue)
            {
                _logger.LogInformation("Nu există sincronizări anterioare. Se va sincroniza nomenclatorul.");
                return true;
            }

            var daysSinceLastSync = (DateTime.Now - lastSync.Value).TotalDays;

            _logger.LogInformation(
                "Ultima sincronizare reușită: {LastSync} (acum {Days:F1} zile). Interval necesar: {Interval} zile.",
                lastSync.Value, daysSinceLastSync, SYNC_INTERVAL_DAYS);

            return daysSinceLastSync >= SYNC_INTERVAL_DAYS;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la verificarea directă a ultimei sincronizări. NU se va forța sincronizarea.");
            // În caz de eroare totală, NU forțăm sincronizarea pentru a evita update-uri inutile
            return false;
        }
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

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Parsare coloane exacte din Excel ANM (1-20)
                var medicament = new MedicamentNomenclator
                {
                    CodCIM = GetCellValue(row, 1) ?? "",
                    DenumireComerciala = GetCellValue(row, 2) ?? "",
                    DCI = GetCellValue(row, 3),
                    FormaFarmaceutica = GetCellValue(row, 4),
                    Concentratie = GetCellValue(row, 5),
                    FirmaTaraProducatoareAPP = GetCellValue(row, 6),
                    FirmaTaraDetinatoareAPP = GetCellValue(row, 7),
                    CodATC = GetCellValue(row, 8),
                    ActiuneTerapeutica = GetCellValue(row, 9),
                    Prescriptie = GetCellValue(row, 10),
                    NrDataAmbalajAPP = GetCellValue(row, 11),
                    Ambalaj = GetCellValue(row, 12),
                    VolumAmbalaj = GetCellValue(row, 13),
                    ValabilitateAmbalaj = GetCellValue(row, 14),
                    Bulina = GetCellValue(row, 15),
                    Diez = GetCellValue(row, 16),
                    Stea = GetCellValue(row, 17),
                    Triunghi = GetCellValue(row, 18),
                    Dreptunghi = GetCellValue(row, 19),
                    DataActualizare = GetCellValue(row, 20)
                };

                if (string.IsNullOrWhiteSpace(medicament.CodCIM) || string.IsNullOrWhiteSpace(medicament.DenumireComerciala))
                {
                    continue;
                }

                // Folosește repository pentru upsert
                var isNew = await _repository.UpsertAsync(medicament, fileName, cancellationToken);

                if (isNew)
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

    #endregion
}
