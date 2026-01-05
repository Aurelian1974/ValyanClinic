using System.Text.RegularExpressions;
using ValyanClinic.Application.Features.Analize.Models;

namespace ValyanClinic.Application.Services.Analize.PdfAnalizeParsers;

/// <summary>
/// Parser universal care încearcă să detecteze formatul automat
/// Funcționează pentru majoritatea laboratoarelor românești
/// </summary>
public class UniversalParser : BaseLaboratorParser
{
    public override string Key => "universal";
    public override string Name => "Universal (Auto-detect)";
    public override string Description => "Detectează automat formatul laboratorului";

    // Pattern-uri pentru diferite formate
    private static readonly Regex PatternTabel = new(
        @"^(.+?)\s+(\d+[.,]?\d*)\s+([a-zA-Z/%µ\.\^0-9]+)\s+(\d+[.,]?\d*\s*[-–]\s*\d+[.,]?\d*)",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    private static readonly Regex PatternEgal = new(
        @"^(.+?)\s*=\s*([<>]?\d+[.,]?\d*)\s*([a-zA-Z/%µ\.\^0-9]+)?\s*(\[[^\]]+\])?",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    private static readonly Regex PatternSimplificat = new(
        @"^([A-ZĂÂÎȘȚa-zăâîșț\s\(\)]+)\s+(\d+[.,]?\d*)\s+([a-zA-Z/%µ\.\^0-9/]+)?\s*([\[\(]?\d+[.,]?\d*\s*[-–]\s*\d+[.,]?\d*[\]\)]?)?",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    public override ParsePdfResult Parse(string text, string fileName)
    {
        var result = new ParsePdfResult
        {
            Success = true,
            Laborator = DetectLaborator(text),
            NumarBuletin = ExtractNumarBuletin(text),
            DataRecoltare = ExtractDataRecoltare(text),
            PacientNume = ExtractNumePacient(text),
            PacientCnp = ExtractCNP(text)
        };

        var analize = new List<AnalizaParsataDto>();
        var currentCategorie = "GENERAL";
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var processedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.Length < 3)
                continue;

            // Skip linii header/footer
            if (IsHeaderOrFooter(line))
                continue;

            // Detectare categorie
            var newCategorie = DetectCategorie(line, currentCategorie);
            if (newCategorie != currentCategorie)
            {
                currentCategorie = newCategorie;
                continue;
            }

            // Încercare parsare cu diferite pattern-uri
            var analiza = TryParseLineUniversal(line, currentCategorie);
            if (analiza != null && !string.IsNullOrWhiteSpace(analiza.NumeAnaliza))
            {
                // Evită duplicate
                var key = $"{analiza.NumeAnaliza}_{analiza.Rezultat}";
                if (!processedNames.Contains(key))
                {
                    processedNames.Add(key);
                    analize.Add(analiza);
                }
            }
        }

        result.Analize = analize;
        result.TotalAnalize = analize.Count;
        result.AnalizeAnormale = analize.Count(a => a.EsteAnormal);

        if (analize.Count == 0)
        {
            result.Warnings.Add("Nu s-au putut extrage analize din PDF. Verificați formatul documentului.");
        }

        return result;
    }

    private AnalizaParsataDto? TryParseLineUniversal(string line, string categorie)
    {
        // Încercăm Pattern 1: Tabel clasic
        var match = PatternTabel.Match(line);
        if (match.Success)
        {
            return CreateAnaliza(
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value,
                match.Groups[4].Value,
                categorie
            );
        }

        // Încercăm Pattern 2: Cu egal (Regina Maria style)
        match = PatternEgal.Match(line);
        if (match.Success && line.Contains('='))
        {
            return CreateAnaliza(
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value,
                match.Groups[4].Value,
                categorie
            );
        }

        // Încercăm Pattern 3: Simplificat
        match = PatternSimplificat.Match(line);
        if (match.Success)
        {
            var nume = match.Groups[1].Value.Trim();
            // Verificăm că e un nume valid (nu doar numere sau caractere speciale)
            if (nume.Length > 2 && Regex.IsMatch(nume, @"[A-Za-zĂÂÎȘȚăâîșț]{2,}"))
            {
                return CreateAnaliza(
                    nume,
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    match.Groups[4].Value,
                    categorie
                );
            }
        }

        return null;
    }

    private AnalizaParsataDto CreateAnaliza(string nume, string valoare, string? unitate, string? interval, string categorie)
    {
        var (valText, valNumeric) = ParseNumeric(valoare);
        var (min, max) = ParseInterval(interval ?? "");
        var (isAnormal, direction) = CheckAnormal(valNumeric, min, max);

        // Extrage cod din nume dacă există (ex: "Hemoglobina (HGB)")
        string? cod = null;
        var codMatch = Regex.Match(nume, @"\(([A-Z]{2,10})\)");
        if (codMatch.Success)
        {
            cod = codMatch.Groups[1].Value;
        }

        return new AnalizaParsataDto
        {
            Categorie = categorie,
            NumeAnaliza = CleanNumeAnaliza(nume),
            CodAnaliza = cod,
            Rezultat = valText,
            RezultatNumeric = valNumeric,
            UnitateMasura = (unitate ?? "").Trim(),
            IntervalMin = min,
            IntervalMax = max,
            IntervalText = (interval ?? "").Trim(),
            EsteAnormal = isAnormal,
            DirectieAnormal = direction
        };
    }

    private string CleanNumeAnaliza(string nume)
    {
        // Curăță numele de analiză
        nume = Regex.Replace(nume, @"\s+", " ").Trim();
        nume = Regex.Replace(nume, @"^\d+\.\s*", ""); // Elimină numerotare
        nume = Regex.Replace(nume, @"\*+$", "").Trim(); // Elimină asteriscuri
        return nume;
    }

    private bool IsHeaderOrFooter(string line)
    {
        var lowerLine = line.ToLowerInvariant();
        var skipPatterns = new[]
        {
            "pagina", "page", "data:", "ora:", "validat", "semnat",
            "laborator", "adresa:", "telefon:", "fax:", "email:",
            "www.", "http", ".ro", ".com", "copyright",
            "rezultat", "unitate", "interval", "referință", "valoare",
            "medic", "doctor", "asistent"
        };

        return skipPatterns.Any(p => lowerLine.Contains(p)) && line.Length < 80;
    }

    private string DetectLaborator(string text)
    {
        var detectors = new Dictionary<string, string[]>
        {
            { "Regina Maria", new[] { "Regina Maria", "reginamaria.ro" } },
            { "Synevo", new[] { "SYNEVO", "synevo.ro" } },
            { "MedLife", new[] { "MedLife", "medlife.ro" } },
            { "Bioclinica", new[] { "Bioclinica", "bioclinica.ro" } },
            { "Clinica Sante", new[] { "Clinica Sante", "clinica-sante.ro", "analizeonline.ro" } },
            { "SmartLabs", new[] { "SmartLabs", "erpos" } },
            { "Elite Medical", new[] { "Elite Medical", "poliana.ro" } },
            { "Gral Medical", new[] { "Gral Medical", "gfrg.ro" } },
            { "Sanador", new[] { "Sanador", "sanador.ro" } }
        };

        foreach (var (lab, patterns) in detectors)
        {
            if (patterns.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase)))
                return lab;
        }

        return "Necunoscut";
    }
}
