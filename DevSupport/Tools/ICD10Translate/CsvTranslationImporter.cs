using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DevSupport.ICD10Translate;

/// <summary>
/// Importă traduceri din fișiere CSV în baza de date ICD-10
/// Util pentru traduceri făcute manual sau din alte surse
/// </summary>
public class CsvTranslationImporter
{
    private readonly string _connectionString;

    public CsvTranslationImporter(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Importă traduceri din fișier CSV
    /// Format acceptat:
    ///   Code,ShortDescriptionRo
    ///   sau
    ///   Code,ShortDescriptionEn,ShortDescriptionRo
    /// </summary>
    public async Task ImportFromCsvAsync(string csvPath)
    {
        Console.WriteLine($"📁 Import din: {csvPath}");
        Console.WriteLine();

        var lines = await File.ReadAllLinesAsync(csvPath);
        
        if (lines.Length == 0)
        {
            Console.WriteLine("❌ Fișierul este gol!");
            return;
        }

        // Detectează formatul (2 sau 3 coloane)
        var firstLine = lines[0];
        var hasHeader = firstLine.ToLower().Contains("code") || firstLine.ToLower().Contains("description");
        var startIndex = hasHeader ? 1 : 0;

        var translations = new List<(string Code, string TranslationRo)>();
        var parseErrors = 0;

        for (int i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var parsed = ParseCsvLine(line);
            
            if (parsed.HasValue)
            {
                translations.Add(parsed.Value);
            }
            else
            {
                parseErrors++;
                if (parseErrors <= 5)
                {
                    Console.WriteLine($"⚠️ Linie {i + 1} nu poate fi parsată: {line.Substring(0, Math.Min(50, line.Length))}...");
                }
            }
        }

        Console.WriteLine($"📊 Găsite {translations.Count} traduceri valide");
        if (parseErrors > 0)
        {
            Console.WriteLine($"⚠️ {parseErrors} linii cu erori de parsare");
        }
        Console.WriteLine();

        if (translations.Count == 0)
        {
            Console.WriteLine("❌ Nu sunt traduceri de importat!");
            return;
        }

        // Import în baza de date
        var imported = 0;
        var notFound = 0;
        var errors = 0;

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var (code, translationRo) in translations)
        {
            try
            {
                var rowsAffected = await UpdateTranslationAsync(connection, code, translationRo);
                
                if (rowsAffected > 0)
                {
                    imported++;
                    if (imported % 100 == 0)
                    {
                        Console.Write($"\r   Importate: {imported}");
                    }
                }
                else
                {
                    notFound++;
                }
            }
            catch (Exception ex)
            {
                errors++;
                if (errors <= 5)
                {
                    Console.WriteLine($"❌ Eroare pentru {code}: {ex.Message}");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("✅ IMPORT COMPLET!");
        Console.WriteLine("========================================");
        Console.WriteLine($"  ✅ Importate:   {imported}");
        Console.WriteLine($"  ⏭️ Negăsite:    {notFound}");
        Console.WriteLine($"  ❌ Erori:       {errors}");
        Console.WriteLine("========================================");
    }

    /// <summary>
    /// Exportă codurile netraduse într-un CSV pentru traducere manuală
    /// </summary>
    public async Task ExportUntranslatedToCsvAsync(string csvPath, int? maxCount = null)
    {
        Console.WriteLine($"📁 Export în: {csvPath}");

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = maxCount.HasValue
            ? $"SELECT TOP ({maxCount.Value}) Code, ShortDescriptionEn FROM ICD10_Codes WHERE (IsTranslated = 0 OR IsTranslated IS NULL) AND IsActive = 1 ORDER BY IsCommon DESC, Code"
            : "SELECT Code, ShortDescriptionEn FROM ICD10_Codes WHERE (IsTranslated = 0 OR IsTranslated IS NULL) AND IsActive = 1 ORDER BY IsCommon DESC, Code";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        var lines = new List<string> { "Code,ShortDescriptionEn,ShortDescriptionRo" };

        while (await reader.ReadAsync())
        {
            var code = reader.GetString(0);
            var descEn = reader.IsDBNull(1) ? "" : reader.GetString(1);
            
            // Escape pentru CSV
            descEn = EscapeCsvField(descEn);
            
            lines.Add($"{code},{descEn},");
        }

        await File.WriteAllLinesAsync(csvPath, lines);

        Console.WriteLine($"✅ Exportate {lines.Count - 1} coduri pentru traducere");
    }

    private (string Code, string TranslationRo)? ParseCsvLine(string line)
    {
        var parts = SplitCsvLine(line);
        
        if (parts.Count < 2)
            return null;

        // Format: Code,ShortDescriptionRo
        if (parts.Count == 2)
        {
            var code = parts[0].Trim();
            var translationRo = parts[1].Trim();
            
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(translationRo))
            {
                return (code, translationRo);
            }
        }
        // Format: Code,ShortDescriptionEn,ShortDescriptionRo
        else if (parts.Count >= 3)
        {
            var code = parts[0].Trim();
            var translationRo = parts[2].Trim();
            
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(translationRo))
            {
                return (code, translationRo);
            }
        }

        return null;
    }

    private List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = "";

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }

    private string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    private async Task<int> UpdateTranslationAsync(SqlConnection connection, string code, string translationRo)
    {
        var sql = @"
            UPDATE ICD10_Codes 
            SET ShortDescriptionRo = @TranslationRo,
                IsTranslated = 1,
                TranslatedAt = SYSUTCDATETIME(),
                TranslatedBy = 'CSV-Import',
                UpdatedAt = SYSUTCDATETIME()
            WHERE Code = @Code";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Code", code);
        cmd.Parameters.AddWithValue("@TranslationRo", translationRo.Length > 250 ? translationRo.Substring(0, 250) : translationRo);
        
        return await cmd.ExecuteNonQueryAsync();
    }
}
