using System.Text.RegularExpressions;
using ValyanClinic.Application.Features.Analize.Models;

namespace ValyanClinic.Application.Services.Analize.PdfAnalizeParsers;

/// <summary>
/// Parser abstract de bază pentru laboratoare
/// Conține metodele comune de extragere
/// </summary>
public abstract class BaseLaboratorParser : ILaboratorParser
{
    public abstract string Key { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }

    // Categorii standard
    protected static readonly string[] Categorii = 
    {
        "HEMATOLOGIE", "BIOCHIMIE", "IMUNOLOGIE", "SEROLOGIE",
        "COAGULARE", "HORMONI", "ENDOCRINOLOGIE", "MARKERI TUMORALI",
        "ANALIZE DE URINA", "SUMAR URINA", "VSH", "EXAMEN URINA",
        "LIPIDE", "HEPATIC", "RENAL", "TIROIDĂ"
    };

    // Unități de măsură cunoscute
    protected static readonly string[] UnitatiMasura = 
    {
        "g/dL", "g/dl", "g/L", "mg/dL", "mg/dl", "mg/L",
        "µg/dL", "ng/mL", "pg/mL", "pg/ml", "ng/ml",
        "mmol/L", "mmol/l", "µmol/L", "nmol/L", "pmol/L",
        "mU/L", "U/L", "U/l", "IU/L", "mIU/mL", "µIU/mL",
        "mil./µL", "mii/µL", "x10^6/µl", "x10^3/µl",
        "*10^6/µl", "*10^6/µL", "x10^9/L", "x10^12/L",
        "/mm³", "/mm3", "mm/h", "sec", "s",
        "%", "fl", "fL", "pg", "µm³", "µm^3",
        "mEq/L", "mg%", "UI/mL", "10^3/uL", "10^6/uL"
    };

    public abstract ParsePdfResult Parse(string text, string fileName);

    /// <summary>
    /// Extrage numărul buletinului
    /// </summary>
    protected virtual string ExtractNumarBuletin(string text)
    {
        var patterns = new[]
        {
            @"[Nn]r\.?\s*[Bb]uletin[:\s]+(\d+)",
            @"[Bb]uletin\s*[Nn]r\.?[:\s]+(\d+)",
            @"[Nn]r\.?[:\s]+(\d{5,12})",
            @"Numar\s*raport[:\s]+(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
        }
        return "";
    }

    /// <summary>
    /// Extrage data recoltării
    /// </summary>
    protected virtual string ExtractDataRecoltare(string text)
    {
        var patterns = new[]
        {
            @"[Rr]ecolt(?:are|at)[:\s]+(\d{2}[./]\d{2}[./]\d{4})",
            @"[Dd]ata\s+recolt[aă](?:re|rii)?[:\s]+(\d{2}[./]\d{2}[./]\d{4})",
            @"(\d{2}[./]\d{2}[./]\d{4})\s*[-–]\s*[Rr]ecolt"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
        }
        return "";
    }

    /// <summary>
    /// Extrage numele pacientului
    /// </summary>
    protected virtual string ExtractNumePacient(string text)
    {
        var match = Regex.Match(text, @"[Nn]ume[/\s]?[Pp]renume[:\s]+([A-ZĂÂÎȘȚ][A-ZĂÂÎȘȚa-zăâîșț\s\-]+)", RegexOptions.Multiline);
        if (match.Success)
            return match.Groups[1].Value.Trim();
        return "";
    }

    /// <summary>
    /// Extrage CNP-ul pacientului
    /// </summary>
    protected virtual string ExtractCNP(string text)
    {
        var match = Regex.Match(text, @"\b(\d{13})\b");
        if (match.Success)
            return match.Groups[1].Value;
        return "";
    }

    /// <summary>
    /// Detectează categoria din linie
    /// </summary>
    protected virtual string DetectCategorie(string line, string currentCategorie)
    {
        var lineUpper = line.ToUpperInvariant().Trim();
        
        // Verificăm dacă linia conține o categorie
        foreach (var cat in Categorii)
        {
            if (lineUpper.Contains(cat) && line.Length < 60)
                return cat;
        }
        
        return currentCategorie;
    }

    /// <summary>
    /// Parsează o valoare numerică
    /// </summary>
    protected virtual (string text, decimal? numeric) ParseNumeric(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ("", null);

        text = text.Trim().Replace(',', '.');
        var cleanText = text.Replace("<", "").Replace(">", "").Replace("=", "").Trim();

        if (decimal.TryParse(cleanText, System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return (text, result);
        }

        return (text, null);
    }

    /// <summary>
    /// Parsează intervalul de referință
    /// </summary>
    protected virtual (decimal? min, decimal? max) ParseInterval(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, null);

        // Pattern: [min - max] sau (min - max)
        var match = Regex.Match(text, @"[\[(]?([<>]?\d+[.,]?\d*)\s*[-–]\s*([<>]?\d+[.,]?\d*)[\])]?");
        if (match.Success)
        {
            var minStr = match.Groups[1].Value.Replace(',', '.').Replace("<", "").Replace(">", "");
            var maxStr = match.Groups[2].Value.Replace(',', '.').Replace("<", "").Replace(">", "");

            decimal.TryParse(minStr, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var min);
            decimal.TryParse(maxStr, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var max);

            return (min, max);
        }

        // Pattern: < valoare
        match = Regex.Match(text, @"^[<]\s*(\d+[.,]?\d*)$");
        if (match.Success)
        {
            var maxStr = match.Groups[1].Value.Replace(',', '.');
            if (decimal.TryParse(maxStr, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var max))
            {
                return (null, max);
            }
        }

        return (null, null);
    }

    /// <summary>
    /// Verifică dacă valoarea este în afara limitelor
    /// </summary>
    protected virtual (bool isAnormal, string? direction) CheckAnormal(decimal? value, decimal? min, decimal? max)
    {
        if (!value.HasValue)
            return (false, null);

        if (min.HasValue && value < min)
            return (true, "LOW");

        if (max.HasValue && value > max)
            return (true, "HIGH");

        return (false, null);
    }

    /// <summary>
    /// Găsește unitatea de măsură în text
    /// </summary>
    protected virtual string FindUnitate(string text)
    {
        foreach (var um in UnitatiMasura)
        {
            if (text.Contains(um, StringComparison.OrdinalIgnoreCase))
                return um;
        }
        return "";
    }
}
