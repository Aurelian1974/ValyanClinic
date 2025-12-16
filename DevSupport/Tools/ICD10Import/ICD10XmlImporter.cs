using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace DevSupport.ICD10Import;

/// <summary>
/// Tool pentru importarea codurilor ICD-10 din fișierul XML în baza de date
/// Parsează icd10cm_tabular_2026.xml și inserează în schema v2
/// </summary>
public class ICD10XmlImporter
{
    private readonly string _connectionString;
    private readonly string _xmlFilePath;
    private readonly bool _verbose;

    // Cache pentru ID-uri generate
    private readonly Dictionary<int, Guid> _chapterIds = new();
    private readonly Dictionary<string, Guid> _sectionIds = new();
    private readonly Dictionary<string, Guid> _codeIds = new();

    // Statistici
    private int _chaptersInserted = 0;
    private int _sectionsInserted = 0;
    private int _codesInserted = 0;
    private int _inclusionTermsInserted = 0;
    private int _exclusionsInserted = 0;
    private int _codingInstructionsInserted = 0;
    private int _notesInserted = 0;

    public ICD10XmlImporter(string connectionString, string xmlFilePath, bool verbose = true)
    {
        _connectionString = connectionString;
        _xmlFilePath = xmlFilePath;
        _verbose = verbose;
    }

    /// <summary>
    /// Execută importul complet din XML în baza de date
    /// </summary>
    public async Task ExecuteImportAsync()
    {
        Log("========================================");
        Log("  ICD-10 XML IMPORT TOOL v2.0");
        Log("========================================");
        Log($"XML File: {_xmlFilePath}");
        Log($"Database: {GetDatabaseName()}");
        Log("");

        if (!File.Exists(_xmlFilePath))
        {
            LogError($"Fișierul XML nu există: {_xmlFilePath}");
            return;
        }

        var startTime = DateTime.Now;

        try
        {
            Log("📖 Încărcare XML...");
            var doc = XDocument.Load(_xmlFilePath);
            var root = doc.Root;

            if (root == null)
            {
                LogError("XML-ul nu are element root valid.");
                return;
            }

            Log($"✅ XML încărcat. Versiune: {root.Attribute("version")?.Value ?? "N/A"}");
            Log("");

            // Procesează capitolele
            var chapters = root.Elements("chapter").ToList();
            Log($"📋 Găsite {chapters.Count} capitole");
            Log("");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Dezactivează FK-uri temporar pentru performanță
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_Codes NOCHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_InclusionTerms NOCHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_Exclusions NOCHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_CodingInstructions NOCHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_Notes NOCHECK CONSTRAINT ALL");

            foreach (var chapter in chapters)
            {
                await ProcessChapterAsync(connection, chapter);
            }

            // Actualizează ParentId-urile
            Log("");
            Log("🔗 Actualizare relații părinte-copil...");
            await UpdateParentIdsAsync(connection);

            // Reactivează FK-uri
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_Codes WITH CHECK CHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_InclusionTerms WITH CHECK CHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_Exclusions WITH CHECK CHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_CodingInstructions WITH CHECK CHECK CONSTRAINT ALL");
            await ExecuteNonQueryAsync(connection, "ALTER TABLE ICD10_Notes WITH CHECK CHECK CONSTRAINT ALL");

            var duration = DateTime.Now - startTime;

            Log("");
            Log("========================================");
            Log("✅ IMPORT COMPLET!");
            Log("========================================");
            Log($"Durată: {duration.TotalMinutes:F1} minute");
            Log("");
            Log("Statistici:");
            Log($"  📋 Capitole:           {_chaptersInserted}");
            Log($"  📋 Secțiuni:           {_sectionsInserted}");
            Log($"  📋 Coduri:             {_codesInserted}");
            Log($"  📋 Termeni includere:  {_inclusionTermsInserted}");
            Log($"  📋 Excluderi:          {_exclusionsInserted}");
            Log($"  📋 Instrucțiuni:       {_codingInstructionsInserted}");
            Log($"  📋 Note:               {_notesInserted}");
            Log("");
        }
        catch (Exception ex)
        {
            LogError($"Eroare la import: {ex.Message}");
            LogError(ex.StackTrace ?? "");
        }
    }

    /// <summary>
    /// Procesează un capitol și toate elementele sale
    /// </summary>
    private async Task ProcessChapterAsync(SqlConnection connection, XElement chapter)
    {
        var name = chapter.Element("name")?.Value ?? "";
        var desc = chapter.Element("desc")?.Value ?? "";

        // Extrage numărul capitolului și range-ul
        var chapterNumber = ExtractChapterNumber(name);
        var (rangeStart, rangeEnd) = ExtractCodeRange(name);

        Log($"📖 Capitol {chapterNumber}: {desc.Substring(0, Math.Min(50, desc.Length))}...");

        // Inserează capitolul
        var chapterId = Guid.NewGuid();
        _chapterIds[chapterNumber] = chapterId;

        var insertChapter = @"
            INSERT INTO ICD10_Chapters 
                (ChapterId, ChapterNumber, CodeRangeStart, CodeRangeEnd, DescriptionEn, Version)
            VALUES 
                (@ChapterId, @ChapterNumber, @CodeRangeStart, @CodeRangeEnd, @DescriptionEn, '2026')";

        using (var cmd = new SqlCommand(insertChapter, connection))
        {
            cmd.Parameters.AddWithValue("@ChapterId", chapterId);
            cmd.Parameters.AddWithValue("@ChapterNumber", chapterNumber);
            cmd.Parameters.AddWithValue("@CodeRangeStart", rangeStart);
            cmd.Parameters.AddWithValue("@CodeRangeEnd", rangeEnd);
            cmd.Parameters.AddWithValue("@DescriptionEn", desc);
            await cmd.ExecuteNonQueryAsync();
        }
        _chaptersInserted++;

        // Procesează secțiunile
        var sectionGroups = chapter.Elements("sectionIndex").Elements("sectionRef").ToList();
        var sections = chapter.Elements("section").ToList();

        foreach (var section in sections)
        {
            await ProcessSectionAsync(connection, section, chapterId);
        }
    }

    /// <summary>
    /// Procesează o secțiune și toate diagnosticele sale
    /// </summary>
    private async Task ProcessSectionAsync(SqlConnection connection, XElement section, Guid chapterId)
    {
        var sectionId = section.Attribute("id")?.Value ?? "";
        var desc = section.Element("desc")?.Value ?? sectionId;

        // Extrage range-ul din id (ex: "A00-A09")
        var (rangeStart, rangeEnd) = ExtractCodeRange(sectionId);

        if (_verbose)
            Log($"  📁 Secțiune: {sectionId}");

        // Inserează secțiunea
        var sectionGuid = Guid.NewGuid();
        _sectionIds[sectionId] = sectionGuid;

        var insertSection = @"
            INSERT INTO ICD10_Sections 
                (SectionId, ChapterId, SectionCode, CodeRangeStart, CodeRangeEnd, DescriptionEn)
            VALUES 
                (@SectionId, @ChapterId, @SectionCode, @CodeRangeStart, @CodeRangeEnd, @DescriptionEn)";

        using (var cmd = new SqlCommand(insertSection, connection))
        {
            cmd.Parameters.AddWithValue("@SectionId", sectionGuid);
            cmd.Parameters.AddWithValue("@ChapterId", chapterId);
            cmd.Parameters.AddWithValue("@SectionCode", sectionId);
            cmd.Parameters.AddWithValue("@CodeRangeStart", rangeStart);
            cmd.Parameters.AddWithValue("@CodeRangeEnd", rangeEnd);
            cmd.Parameters.AddWithValue("@DescriptionEn", desc);
            await cmd.ExecuteNonQueryAsync();
        }
        _sectionsInserted++;

        // Procesează diagnosticele din secțiune
        var diags = section.Elements("diag").ToList();
        foreach (var diag in diags)
        {
            await ProcessDiagAsync(connection, diag, chapterId, sectionGuid, null, 0);
        }
    }

    /// <summary>
    /// Procesează recursiv un diagnostic și copiii săi
    /// </summary>
    private async Task ProcessDiagAsync(
        SqlConnection connection, 
        XElement diag, 
        Guid chapterId, 
        Guid? sectionId, 
        string? parentCode, 
        int level)
    {
        var code = diag.Element("name")?.Value ?? "";
        var desc = diag.Element("desc")?.Value ?? "";

        if (string.IsNullOrEmpty(code)) return;

        // Verifică dacă are copii (nu e leaf node)
        var childDiags = diag.Elements("diag").ToList();
        var isLeafNode = !childDiags.Any();

        // Generează GUID și salvează în cache
        var codeId = Guid.NewGuid();
        _codeIds[code] = codeId;

        // Determină categoria bazată pe primul caracter
        var category = DetermineCategory(code);

        // Inserează codul
        var insertCode = @"
            INSERT INTO ICD10_Codes 
                (ICD10_ID, ChapterId, SectionId, Code, FullCode, ShortDescriptionEn, 
                 ParentCode, HierarchyLevel, IsLeafNode, IsBillable, Category, Version)
            VALUES 
                (@ICD10_ID, @ChapterId, @SectionId, @Code, @FullCode, @ShortDescriptionEn,
                 @ParentCode, @HierarchyLevel, @IsLeafNode, @IsBillable, @Category, '2026')";

        using (var cmd = new SqlCommand(insertCode, connection))
        {
            cmd.Parameters.AddWithValue("@ICD10_ID", codeId);
            cmd.Parameters.AddWithValue("@ChapterId", chapterId);
            cmd.Parameters.AddWithValue("@SectionId", (object?)sectionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Code", code);
            cmd.Parameters.AddWithValue("@FullCode", code);
            cmd.Parameters.AddWithValue("@ShortDescriptionEn", desc.Length > 250 ? desc.Substring(0, 250) : desc);
            cmd.Parameters.AddWithValue("@ParentCode", (object?)parentCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HierarchyLevel", level);
            cmd.Parameters.AddWithValue("@IsLeafNode", isLeafNode);
            cmd.Parameters.AddWithValue("@IsBillable", isLeafNode); // Presupunem că leaf nodes sunt facturabile
            cmd.Parameters.AddWithValue("@Category", category);
            await cmd.ExecuteNonQueryAsync();
        }
        _codesInserted++;

        // Procesează elementele asociate
        await ProcessInclusionTermsAsync(connection, diag, codeId);
        await ProcessExclusionsAsync(connection, diag, codeId);
        await ProcessCodingInstructionsAsync(connection, diag, codeId);
        await ProcessNotesAsync(connection, diag, codeId);

        // Procesează recursiv copiii
        foreach (var childDiag in childDiags)
        {
            await ProcessDiagAsync(connection, childDiag, chapterId, sectionId, code, level + 1);
        }

        // Afișează progresul la fiecare 1000 de coduri
        if (_codesInserted % 1000 == 0)
        {
            Log($"    ... {_codesInserted} coduri procesate");
        }
    }

    /// <summary>
    /// Procesează termenii de includere (includes, inclusionTerm)
    /// </summary>
    private async Task ProcessInclusionTermsAsync(SqlConnection connection, XElement diag, Guid codeId)
    {
        var sortOrder = 0;

        // Procesează <includes>
        foreach (var includes in diag.Elements("includes"))
        {
            foreach (var note in includes.Elements("note"))
            {
                await InsertInclusionTermAsync(connection, codeId, "includes", note.Value, sortOrder++);
            }
        }

        // Procesează <inclusionTerm>
        foreach (var inclusionTerm in diag.Elements("inclusionTerm"))
        {
            foreach (var note in inclusionTerm.Elements("note"))
            {
                await InsertInclusionTermAsync(connection, codeId, "inclusionTerm", note.Value, sortOrder++);
            }
        }
    }

    private async Task InsertInclusionTermAsync(SqlConnection connection, Guid codeId, string termType, string text, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var insert = @"
            INSERT INTO ICD10_InclusionTerms (InclusionId, ICD10_ID, TermType, TermTextEn, SortOrder)
            VALUES (NEWID(), @ICD10_ID, @TermType, @TermTextEn, @SortOrder)";

        using var cmd = new SqlCommand(insert, connection);
        cmd.Parameters.AddWithValue("@ICD10_ID", codeId);
        cmd.Parameters.AddWithValue("@TermType", termType);
        cmd.Parameters.AddWithValue("@TermTextEn", text.Length > 1000 ? text.Substring(0, 1000) : text);
        cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
        await cmd.ExecuteNonQueryAsync();
        _inclusionTermsInserted++;
    }

    /// <summary>
    /// Procesează excluderile (excludes1, excludes2)
    /// </summary>
    private async Task ProcessExclusionsAsync(SqlConnection connection, XElement diag, Guid codeId)
    {
        var sortOrder = 0;

        foreach (var excludeType in new[] { "excludes1", "excludes2" })
        {
            foreach (var excludes in diag.Elements(excludeType))
            {
                foreach (var note in excludes.Elements("note"))
                {
                    var text = note.Value;
                    var referencedCode = ExtractReferencedCode(text);

                    await InsertExclusionAsync(connection, codeId, excludeType, text, referencedCode, sortOrder++);
                }
            }
        }
    }

    private async Task InsertExclusionAsync(SqlConnection connection, Guid codeId, string exclusionType, string text, string? referencedCode, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var insert = @"
            INSERT INTO ICD10_Exclusions (ExclusionId, ICD10_ID, ExclusionType, NoteTextEn, ReferencedCode, SortOrder)
            VALUES (NEWID(), @ICD10_ID, @ExclusionType, @NoteTextEn, @ReferencedCode, @SortOrder)";

        using var cmd = new SqlCommand(insert, connection);
        cmd.Parameters.AddWithValue("@ICD10_ID", codeId);
        cmd.Parameters.AddWithValue("@ExclusionType", exclusionType);
        cmd.Parameters.AddWithValue("@NoteTextEn", text.Length > 1000 ? text.Substring(0, 1000) : text);
        cmd.Parameters.AddWithValue("@ReferencedCode", (object?)referencedCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
        await cmd.ExecuteNonQueryAsync();
        _exclusionsInserted++;
    }

    /// <summary>
    /// Procesează instrucțiunile de codificare (codeFirst, useAdditionalCode, codeAlso)
    /// </summary>
    private async Task ProcessCodingInstructionsAsync(SqlConnection connection, XElement diag, Guid codeId)
    {
        var sortOrder = 0;

        foreach (var instructionType in new[] { "codeFirst", "useAdditionalCode", "codeAlso" })
        {
            foreach (var instruction in diag.Elements(instructionType))
            {
                var text = instruction.Value;
                if (string.IsNullOrWhiteSpace(text))
                {
                    // Încearcă să ia din note
                    foreach (var note in instruction.Elements("note"))
                    {
                        text = note.Value;
                        var referencedCode = ExtractReferencedCode(text);
                        await InsertCodingInstructionAsync(connection, codeId, instructionType, text, referencedCode, sortOrder++);
                    }
                }
                else
                {
                    var referencedCode = ExtractReferencedCode(text);
                    await InsertCodingInstructionAsync(connection, codeId, instructionType, text, referencedCode, sortOrder++);
                }
            }
        }
    }

    private async Task InsertCodingInstructionAsync(SqlConnection connection, Guid codeId, string instructionType, string text, string? referencedCode, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var insert = @"
            INSERT INTO ICD10_CodingInstructions (InstructionId, ICD10_ID, InstructionType, InstructionTextEn, ReferencedCode, SortOrder)
            VALUES (NEWID(), @ICD10_ID, @InstructionType, @InstructionTextEn, @ReferencedCode, @SortOrder)";

        using var cmd = new SqlCommand(insert, connection);
        cmd.Parameters.AddWithValue("@ICD10_ID", codeId);
        cmd.Parameters.AddWithValue("@InstructionType", instructionType);
        cmd.Parameters.AddWithValue("@InstructionTextEn", text.Length > 1000 ? text.Substring(0, 1000) : text);
        cmd.Parameters.AddWithValue("@ReferencedCode", (object?)referencedCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
        await cmd.ExecuteNonQueryAsync();
        _codingInstructionsInserted++;
    }

    /// <summary>
    /// Procesează notele
    /// </summary>
    private async Task ProcessNotesAsync(SqlConnection connection, XElement diag, Guid codeId)
    {
        var sortOrder = 0;

        // Note la nivel de element <notes>
        foreach (var notes in diag.Elements("notes"))
        {
            foreach (var note in notes.Elements("note"))
            {
                await InsertNoteAsync(connection, codeId, "general", note.Value, sortOrder++);
            }
        }

        // Note directe
        foreach (var note in diag.Elements("note"))
        {
            await InsertNoteAsync(connection, codeId, "general", note.Value, sortOrder++);
        }
    }

    private async Task InsertNoteAsync(SqlConnection connection, Guid codeId, string noteType, string text, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var insert = @"
            INSERT INTO ICD10_Notes (NoteId, ICD10_ID, NoteType, NoteTextEn, SortOrder)
            VALUES (NEWID(), @ICD10_ID, @NoteType, @NoteTextEn, @SortOrder)";

        using var cmd = new SqlCommand(insert, connection);
        cmd.Parameters.AddWithValue("@ICD10_ID", codeId);
        cmd.Parameters.AddWithValue("@NoteType", noteType);
        cmd.Parameters.AddWithValue("@NoteTextEn", text.Length > 2000 ? text.Substring(0, 2000) : text);
        cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
        await cmd.ExecuteNonQueryAsync();
        _notesInserted++;
    }

    /// <summary>
    /// Actualizează ParentId-urile bazat pe ParentCode
    /// </summary>
    private async Task UpdateParentIdsAsync(SqlConnection connection)
    {
        var update = @"
            UPDATE c
            SET c.ParentId = p.ICD10_ID
            FROM ICD10_Codes c
            INNER JOIN ICD10_Codes p ON c.ParentCode = p.Code
            WHERE c.ParentCode IS NOT NULL AND c.ParentId IS NULL";

        using var cmd = new SqlCommand(update, connection);
        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        Log($"✅ {rowsAffected} relații părinte-copil actualizate");
    }

    // ==================== HELPER METHODS ====================

    private int ExtractChapterNumber(string name)
    {
        // Extrage numărul din formatul "1" sau "01"
        if (int.TryParse(name.Trim(), out var num))
            return num;
        return 0;
    }

    private (string start, string end) ExtractCodeRange(string text)
    {
        // Extrage range-ul din formate: "A00-B99", "(A00-B99)"
        var match = System.Text.RegularExpressions.Regex.Match(
            text, @"([A-Z]\d{2,3}(?:\.\d+)?)\s*-\s*([A-Z]\d{2,3}(?:\.\d+)?)");

        if (match.Success)
            return (match.Groups[1].Value, match.Groups[2].Value);

        // Dacă nu e range, returnează același cod pentru ambele
        var singleCode = System.Text.RegularExpressions.Regex.Match(text, @"([A-Z]\d{2,3})");
        if (singleCode.Success)
            return (singleCode.Groups[1].Value, singleCode.Groups[1].Value);

        return (text, text);
    }

    private string? ExtractReferencedCode(string text)
    {
        // Extrage codul referențiat din text (ex: "see also A00.1")
        var match = System.Text.RegularExpressions.Regex.Match(
            text, @"\(([A-Z]\d{2}(?:\.\d+)?)\)");

        return match.Success ? match.Groups[1].Value : null;
    }

    private string DetermineCategory(string code)
    {
        if (string.IsNullOrEmpty(code)) return "Necategorizat";

        var prefix = code[0];
        return prefix switch
        {
            'A' or 'B' => "Infectioase",
            'C' => "Neoplasme",
            'D' when code.StartsWith("D0") || code.StartsWith("D1") || 
                     code.StartsWith("D2") || code.StartsWith("D3") || 
                     code.StartsWith("D4") => "Neoplasme",
            'D' => "Sange",
            'E' => "Endocrin",
            'F' => "Mental",
            'G' => "Nervos",
            'H' when code.CompareTo("H60") < 0 => "Ochi",
            'H' => "Ureche",
            'I' => "Cardiovascular",
            'J' => "Respirator",
            'K' => "Digestiv",
            'L' => "Piele",
            'M' => "Musculo-scheletic",
            'N' => "Genito-urinar",
            'O' => "Obstetric",
            'P' => "Perinatal",
            'Q' => "Malformatii",
            'R' => "Simptome",
            'S' or 'T' => "Traumatisme",
            'V' or 'W' or 'X' or 'Y' => "Cauze externe",
            'Z' => "Factori sanatate",
            _ => "Alte"
        };
    }

    private async Task ExecuteNonQueryAsync(SqlConnection connection, string sql)
    {
        using var cmd = new SqlCommand(sql, connection);
        cmd.CommandTimeout = 300; // 5 minute timeout
        await cmd.ExecuteNonQueryAsync();
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
}
