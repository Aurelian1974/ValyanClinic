using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DevSupport.ICD10Translate;

/// <summary>
/// Tool pentru traducerea descrierilor ICD-10 din engleză în română
/// Citește din baza de date, traduce și salvează înapoi
/// </summary>
public class ICD10Translator
{
    private readonly string _connectionString;
    private readonly ICD10TranslationService _translationService;
    private readonly int _batchSize;
    private readonly bool _verbose;

    // Statistici
    private int _totalCodes;
    private int _translatedCodes;
    private int _failedCodes;
    private int _skippedCodes;

    public ICD10Translator(
        string connectionString, 
        ICD10TranslationService translationService,
        int batchSize = 20,
        bool verbose = true)
    {
        _connectionString = connectionString;
        _translationService = translationService;
        _batchSize = batchSize;
        _verbose = verbose;
    }

    /// <summary>
    /// Traduce un număr limitat de coduri
    /// </summary>
    public async Task TranslateLimitedAsync(int maxCount)
    {
        Log("========================================");
        Log($"  TRADUCERE LIMITATĂ - {maxCount} CODURI");
        Log("========================================");
        Log($"Batch size: {_batchSize}");
        Log("");

        var startTime = DateTime.Now;
        _translatedCodes = 0;
        _failedCodes = 0;
        _skippedCodes = 0;

        try
        {
            var translated = 0;
            while (translated < maxCount)
            {
                var batchCount = Math.Min(_batchSize, maxCount - translated);
                var batch = await GetUntranslatedBatchAsync(batchCount);
                
                if (batch.Count == 0)
                {
                    Log("✅ Nu mai sunt coduri de tradus!");
                    break;
                }

                await TranslateBatchAsync(batch);
                translated += batch.Count;
                
                Log($"   Progres: {translated}/{maxCount}");
            }

            var duration = DateTime.Now - startTime;
            PrintStatistics(duration);
        }
        catch (Exception ex)
        {
            LogError($"Eroare la traducere: {ex.Message}");
        }
    }

    /// <summary>
    /// Traduce toate codurile care nu au încă traducere
    /// </summary>
    public async Task TranslateAllUntranslatedAsync()
    {
        Log("========================================");
        Log("  ICD-10 TRANSLATION TOOL v1.1");
        Log("========================================");
        Log($"Batch size: {_batchSize}");
        Log($"Database: {GetDatabaseName()}");
        Log("");

        var startTime = DateTime.Now;
        _translatedCodes = 0;
        _failedCodes = 0;
        _skippedCodes = 0;

        try
        {
            var untranslatedCount = await GetUntranslatedCountAsync();
            Log($"📋 Coduri de tradus: {untranslatedCount}");

            if (untranslatedCount == 0)
            {
                Log("✅ Toate codurile sunt deja traduse!");
                return;
            }

            Log("");
            Log("🔄 Începe traducerea...");
            Log("   (Apasă Ctrl+C pentru a opri)");
            Log("");

            var offset = 0;
            while (offset < untranslatedCount)
            {
                var batch = await GetUntranslatedBatchAsync(_batchSize);
                
                if (batch.Count == 0)
                    break;

                await TranslateBatchAsync(batch);
                
                offset += batch.Count;
                
                var progress = (double)offset / untranslatedCount * 100;
                var eta = EstimateTimeRemaining(startTime, offset, untranslatedCount);
                Log($"   Progres: {offset}/{untranslatedCount} ({progress:F1}%) - ETA: {eta}");
            }

            var duration = DateTime.Now - startTime;
            PrintStatistics(duration);
        }
        catch (Exception ex)
        {
            LogError($"Eroare la traducere: {ex.Message}");
            LogError(ex.StackTrace ?? "");
        }
    }

    /// <summary>
    /// Traduce codurile dintr-o categorie specifică
    /// </summary>
    public async Task TranslateByCategoryAsync(string category)
    {
        Log($"🔄 Traducere categorie: {category}");

        _translatedCodes = 0;
        _failedCodes = 0;
        _skippedCodes = 0;

        var startTime = DateTime.Now;
        var codes = await GetCodesByCategoryAsync(category);
        Log($"   Găsite {codes.Count} coduri netraduse");

        if (codes.Count == 0)
        {
            Log("✅ Toate codurile din această categorie sunt traduse!");
            return;
        }

        for (int i = 0; i < codes.Count; i += _batchSize)
        {
            var batch = codes.Skip(i).Take(_batchSize).ToList();
            await TranslateBatchAsync(batch);
            
            var progress = (double)(i + batch.Count) / codes.Count * 100;
            Log($"   Progres: {i + batch.Count}/{codes.Count} ({progress:F1}%)");
        }

        var duration = DateTime.Now - startTime;
        Log($"✅ Categorie {category} tradusă în {duration.TotalMinutes:F1} minute");
        PrintStatistics(duration);
    }

    /// <summary>
    /// Traduce codurile comune (IsCommon = 1)
    /// </summary>
    public async Task TranslateCommonCodesAsync()
    {
        Log("🔄 Traducere coduri comune...");

        _translatedCodes = 0;
        _failedCodes = 0;
        _skippedCodes = 0;

        var startTime = DateTime.Now;
        var codes = await GetCommonCodesAsync();
        Log($"   Găsite {codes.Count} coduri comune netraduse");

        if (codes.Count == 0)
        {
            Log("✅ Toate codurile comune sunt traduse!");
            return;
        }

        for (int i = 0; i < codes.Count; i += _batchSize)
        {
            var batch = codes.Skip(i).Take(_batchSize).ToList();
            await TranslateBatchAsync(batch);
        }

        var duration = DateTime.Now - startTime;
        Log($"✅ Coduri comune traduse în {duration.TotalSeconds:F0} secunde");
        PrintStatistics(duration);
    }

    /// <summary>
    /// Afișează statistici despre traduceri
    /// </summary>
    public async Task ShowStatisticsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var stats = new Dictionary<string, int>();

        stats["Total"] = await ExecuteScalarAsync(connection, 
            "SELECT COUNT(*) FROM ICD10_Codes WHERE IsActive = 1");

        stats["Traduse"] = await ExecuteScalarAsync(connection, 
            "SELECT COUNT(*) FROM ICD10_Codes WHERE IsTranslated = 1 AND IsActive = 1");

        stats["Netraduse"] = await ExecuteScalarAsync(connection, 
            "SELECT COUNT(*) FROM ICD10_Codes WHERE (IsTranslated = 0 OR IsTranslated IS NULL) AND IsActive = 1");

        stats["Comune"] = await ExecuteScalarAsync(connection, 
            "SELECT COUNT(*) FROM ICD10_Codes WHERE IsCommon = 1 AND IsActive = 1");

        stats["Comune traduse"] = await ExecuteScalarAsync(connection, 
            "SELECT COUNT(*) FROM ICD10_Codes WHERE IsCommon = 1 AND IsTranslated = 1 AND IsActive = 1");

        Log("");
        Log("📊 STATISTICI TRADUCERE ICD-10");
        Log("========================================");
        Log($"  Total coduri:      {stats["Total"]:N0}");
        Log($"  Traduse:           {stats["Traduse"]:N0} ({(stats["Total"] > 0 ? (double)stats["Traduse"] / stats["Total"] * 100 : 0):F1}%)");
        Log($"  Netraduse:         {stats["Netraduse"]:N0}");
        Log($"  Coduri comune:     {stats["Comune"]:N0}");
        Log($"  Comune traduse:    {stats["Comune traduse"]:N0}");
        Log("========================================");
        Log("");

        var categoryStats = await GetCategoryStatisticsAsync(connection);
        
        Log("📊 Per categorie:");
        Log("─────────────────────────────────────────────");
        Log($"{"Categorie",-20} {"Total",8} {"Traduse",8} {"Procent",8}");
        Log("─────────────────────────────────────────────");
        
        foreach (var cat in categoryStats)
        {
            var percent = cat.Total > 0 ? (double)cat.Translated / cat.Total * 100 : 0;
            Log($"{cat.Category,-20} {cat.Total,8:N0} {cat.Translated,8:N0} {percent,7:F1}%");
        }
        Log("─────────────────────────────────────────────");
    }

    #region Private Methods

    private void PrintStatistics(TimeSpan duration)
    {
        Log("");
        Log("========================================");
        Log("✅ TRADUCERE COMPLETĂ!");
        Log("========================================");
        Log($"Durată: {duration.TotalMinutes:F1} minute");
        Log("");
        Log("Statistici sesiune:");
        Log($"  ✅ Traduse:    {_translatedCodes}");
        Log($"  ⏭️ Omise:      {_skippedCodes}");
        Log($"  ❌ Eșuate:     {_failedCodes}");
        Log("");
    }

    private string EstimateTimeRemaining(DateTime startTime, int completed, int total)
    {
        if (completed == 0) return "calculare...";
        
        var elapsed = DateTime.Now - startTime;
        var rate = completed / elapsed.TotalSeconds;
        var remaining = (total - completed) / rate;
        
        if (remaining < 60) return $"{remaining:F0} sec";
        if (remaining < 3600) return $"{remaining / 60:F0} min";
        return $"{remaining / 3600:F1} ore";
    }

    private async Task<int> GetUntranslatedCountAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        return await ExecuteScalarAsync(connection,
            "SELECT COUNT(*) FROM ICD10_Codes WHERE (IsTranslated = 0 OR IsTranslated IS NULL) AND IsActive = 1");
    }

    private async Task<List<ICD10CodeToTranslate>> GetUntranslatedBatchAsync(int count)
    {
        var results = new List<ICD10CodeToTranslate>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP (@Count) 
                ICD10_ID, 
                Code, 
                ShortDescriptionEn,
                LongDescriptionEn
            FROM ICD10_Codes 
            WHERE (IsTranslated = 0 OR IsTranslated IS NULL) 
              AND IsActive = 1
              AND ShortDescriptionEn IS NOT NULL
            ORDER BY 
                IsCommon DESC,
                HierarchyLevel,
                Code";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Count", count);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new ICD10CodeToTranslate
            {
                Id = reader.GetGuid(0),
                Code = reader.GetString(1),
                ShortDescriptionEn = reader.IsDBNull(2) ? null : reader.GetString(2),
                LongDescriptionEn = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return results;
    }

    private async Task<List<ICD10CodeToTranslate>> GetCodesByCategoryAsync(string category)
    {
        var results = new List<ICD10CodeToTranslate>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT ICD10_ID, Code, ShortDescriptionEn, LongDescriptionEn
            FROM ICD10_Codes 
            WHERE Category = @Category
              AND (IsTranslated = 0 OR IsTranslated IS NULL)
              AND IsActive = 1
            ORDER BY Code";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Category", category);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new ICD10CodeToTranslate
            {
                Id = reader.GetGuid(0),
                Code = reader.GetString(1),
                ShortDescriptionEn = reader.IsDBNull(2) ? null : reader.GetString(2),
                LongDescriptionEn = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return results;
    }

    private async Task<List<ICD10CodeToTranslate>> GetCommonCodesAsync()
    {
        var results = new List<ICD10CodeToTranslate>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT ICD10_ID, Code, ShortDescriptionEn, LongDescriptionEn
            FROM ICD10_Codes 
            WHERE IsCommon = 1
              AND (IsTranslated = 0 OR IsTranslated IS NULL)
              AND IsActive = 1
            ORDER BY Code";

        using var cmd = new SqlCommand(sql, connection);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new ICD10CodeToTranslate
            {
                Id = reader.GetGuid(0),
                Code = reader.GetString(1),
                ShortDescriptionEn = reader.IsDBNull(2) ? null : reader.GetString(2),
                LongDescriptionEn = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return results;
    }

    private async Task TranslateBatchAsync(List<ICD10CodeToTranslate> batch)
    {
        var textsToTranslate = batch
            .Where(c => !string.IsNullOrWhiteSpace(c.ShortDescriptionEn))
            .Select(c => c.ShortDescriptionEn!)
            .ToList();

        if (textsToTranslate.Count == 0)
        {
            _skippedCodes += batch.Count;
            return;
        }

        var translations = await _translationService.TranslateBatchAsync(textsToTranslate);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var translationIndex = 0;
        foreach (var code in batch)
        {
            if (string.IsNullOrWhiteSpace(code.ShortDescriptionEn))
            {
                _skippedCodes++;
                continue;
            }

            var translation = translationIndex < translations.Count ? translations[translationIndex] : null;
            translationIndex++;

            if (!string.IsNullOrWhiteSpace(translation))
            {
                await UpdateTranslationAsync(connection, code.Id, translation);
                _translatedCodes++;
            }
            else
            {
                _failedCodes++;
            }
        }
    }

    private async Task UpdateTranslationAsync(SqlConnection connection, Guid id, string translation)
    {
        var sql = @"
            UPDATE ICD10_Codes 
            SET ShortDescriptionRo = @TranslationRo,
                IsTranslated = 1,
                TranslatedAt = SYSUTCDATETIME(),
                TranslatedBy = 'AutoTranslate-Google',
                UpdatedAt = SYSUTCDATETIME()
            WHERE ICD10_ID = @Id";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@TranslationRo", translation.Length > 250 ? translation.Substring(0, 250) : translation);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<List<CategoryStats>> GetCategoryStatisticsAsync(SqlConnection connection)
    {
        var results = new List<CategoryStats>();

        var sql = @"
            SELECT 
                ISNULL(Category, 'Necategorizat') as Category,
                COUNT(*) as Total,
                SUM(CASE WHEN IsTranslated = 1 THEN 1 ELSE 0 END) as Translated
            FROM ICD10_Codes
            WHERE IsActive = 1
            GROUP BY Category
            ORDER BY COUNT(*) DESC";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new CategoryStats
            {
                Category = reader.GetString(0),
                Total = reader.GetInt32(1),
                Translated = reader.GetInt32(2)
            });
        }

        return results;
    }

    private async Task<int> ExecuteScalarAsync(SqlConnection connection, string sql)
    {
        using var cmd = new SqlCommand(sql, connection);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private string GetDatabaseName()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        return builder.InitialCatalog ?? "Unknown";
    }

    private void Log(string message)
    {
        Console.WriteLine(message);
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {message}");
        Console.ResetColor();
    }

    #endregion

    #region Helper Classes

    private class ICD10CodeToTranslate
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string? ShortDescriptionEn { get; set; }
        public string? LongDescriptionEn { get; set; }
    }

    private class CategoryStats
    {
        public string Category { get; set; } = "";
        public int Total { get; set; }
        public int Translated { get; set; }
    }

    #endregion
}
