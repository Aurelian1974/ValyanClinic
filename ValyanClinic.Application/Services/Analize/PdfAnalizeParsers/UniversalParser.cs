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

    // Pattern-uri pentru diferite formate de analize
    // Format Clinica Sante: "Hemoglobina (HGB) 10.6 g/dL [11.5 - 16]"
    private static readonly Regex PatternClinicaSante = new(
        @"^(.+?)\s+(\d+[.,]?\d*)\s+([a-zA-Z%µ\^0-9/\s]+?)\s+\[(\d+[.,]?\d*)\s*[-–]\s*(\d+[.,]?\d*)\]",
        RegexOptions.Compiled
    );

    // Format cu < sau > pentru referință: "Colesterol seric total 172.51 mg/dL <200"
    private static readonly Regex PatternCuComparator = new(
        @"^(.+?)\s+(\d+[.,]?\d*)\s+([a-zA-Z%µ\^0-9/\s]+?)\s+([<>]=?\s*\d+[.,]?\d*)",
        RegexOptions.Compiled
    );

    // Format urina cu text: "pH 5.5 [4.6 - 8]" sau "Bilirubina Negativ mg/dL Negativ"
    private static readonly Regex PatternUrinaText = new(
        @"^(.+?)\s+(Negativ|Pozitiv|Normal|Rar[aăe]?|Frecvent[eă]?|Absent[eă]?|Prezent[eă]?)\s+([a-zA-Z/%µ\.\^0-9]+)?\s*(Negativ|Pozitiv|Normal)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Format cu egal (Regina Maria): "Glucoza = 95 mg/dL [70-110]"
    private static readonly Regex PatternEgal = new(
        @"^(.+?)\s*=\s*([<>]?\d+[.,]?\d*)\s*([a-zA-Z/%µ\.\^0-9]+)?\s*\[?(\d+[.,]?\d*)?\s*[-–]?\s*(\d+[.,]?\d*)?\]?",
        RegexOptions.Compiled
    );

    // Pattern simplu pentru valori fără interval explicit: "VSH 15 mm/h"
    private static readonly Regex PatternSimplu = new(
        @"^([A-ZĂÂÎȘȚa-zăâîșț\s\(\)\-/]+?)\s+(\d+[.,]?\d*)\s+([a-zA-Z%µ\^0-9/]+)",
        RegexOptions.Compiled
    );

    // Lista de analize cunoscute pentru validare
    private static readonly HashSet<string> AnalizeComune = new(StringComparer.OrdinalIgnoreCase)
    {
        "Hemoglobina", "HGB", "Hematocrit", "HCT", "Hematii", "RBC", "Leucocite", "WBC",
        "Trombocite", "PLT", "MCV", "MCH", "MCHC", "RDW", "MPV", "PCT", "PDW",
        "Neutrofile", "Limfocite", "Monocite", "Eosinofile", "Basofile", "VSH",
        "Glicemie", "Glucoza", "Colesterol", "Trigliceride", "HDL", "LDL",
        "TGO", "TGP", "AST", "ALT", "GGT", "Bilirubina", "Uree", "Creatinina",
        "Acid uric", "Proteine", "Albumina", "Fibrinogen", "INR", "PT", "APTT",
        "Fier", "Sideremie", "Feritina", "Transferina", "Vitamina", "Calciu",
        "Magneziu", "Sodiu", "Potasiu", "Clor", "TSH", "T3", "T4", "FT3", "FT4",
        "pH", "Densitate", "Urobilinogen", "Nitriti", "Sediment"
    };

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

            // Skip linii header/footer/metadata
            if (ShouldSkipLine(line))
                continue;

            // Detectare categorie
            var newCategorie = DetectCategorie(line, currentCategorie);
            if (newCategorie != currentCategorie && IsCategoryLine(line))
            {
                currentCategorie = newCategorie;
                continue;
            }

            // Încercare parsare cu diferite pattern-uri
            var analiza = TryParseLineUniversal(line, currentCategorie);
            if (analiza != null && IsValidAnaliza(analiza))
            {
                // Evită duplicate
                var key = $"{analiza.NumeAnaliza}_{analiza.Rezultat}".ToLowerInvariant();
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

    private bool IsCategoryLine(string line)
    {
        // Verifică dacă linia este doar o categorie (fără valori numerice)
        var upperLine = line.ToUpperInvariant();
        return Categories.Contains(upperLine) || 
               (upperLine.Length < 40 && !Regex.IsMatch(line, @"\d+[.,]\d+"));
    }

    private bool IsValidAnaliza(AnalizaParsataDto analiza)
    {
        if (string.IsNullOrWhiteSpace(analiza.NumeAnaliza))
            return false;

        var nume = analiza.NumeAnaliza.Trim();
        
        // Lungime minimă 2 caractere
        if (nume.Length < 2)
            return false;

        // Trebuie să conțină cel puțin 2 litere consecutive
        if (!Regex.IsMatch(nume, @"[A-Za-zĂÂÎȘȚăâîșț]{2,}"))
            return false;

        // Verifică dacă conține cuvinte cheie de analiză
        var numeWords = nume.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool hasKnownAnaliza = numeWords.Any(w => AnalizeComune.Any(a => 
            w.StartsWith(a, StringComparison.OrdinalIgnoreCase) || 
            a.StartsWith(w, StringComparison.OrdinalIgnoreCase)));

        // Sau are valoare numerică validă
        bool hasNumericValue = analiza.RezultatNumeric.HasValue && analiza.RezultatNumeric > 0;

        return hasKnownAnaliza || hasNumericValue;
    }

    private AnalizaParsataDto? TryParseLineUniversal(string line, string categorie)
    {
        // Pattern 1: Clinica Sante format cu interval [min - max]
        var match = PatternClinicaSante.Match(line);
        if (match.Success)
        {
            return CreateAnalizaFromMatch(match, categorie, "interval");
        }

        // Pattern 2: Cu comparator <200 sau >90
        match = PatternCuComparator.Match(line);
        if (match.Success)
        {
            return CreateAnalizaFromMatch(match, categorie, "comparator");
        }

        // Pattern 3: Urina cu text (Negativ/Pozitiv)
        match = PatternUrinaText.Match(line);
        if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return CreateAnalizaFromMatch(match, categorie, "text");
        }

        // Pattern 4: Cu egal
        if (line.Contains('='))
        {
            match = PatternEgal.Match(line);
            if (match.Success)
            {
                return CreateAnalizaFromMatch(match, categorie, "egal");
            }
        }

        // Pattern 5: Simplu - doar dacă are unitate de măsură validă
        match = PatternSimplu.Match(line);
        if (match.Success)
        {
            var unitate = match.Groups[3].Value.Trim();
            if (IsValidUnit(unitate))
            {
                return CreateAnalizaFromMatch(match, categorie, "simplu");
            }
        }

        return null;
    }

    private AnalizaParsataDto CreateAnalizaFromMatch(Match match, string categorie, string patternType)
    {
        string nume = "";
        string valoare = "";
        string? unitate = null;
        decimal? min = null;
        decimal? max = null;
        string? intervalText = null;

        switch (patternType)
        {
            case "interval":
                // Groups: 1=nume, 2=valoare, 3=unitate, 4=min, 5=max
                nume = match.Groups[1].Value;
                valoare = match.Groups[2].Value;
                unitate = match.Groups[3].Value;
                min = ParseNumeric(match.Groups[4].Value).numeric;
                max = ParseNumeric(match.Groups[5].Value).numeric;
                intervalText = $"[{match.Groups[4].Value} - {match.Groups[5].Value}]";
                break;

            case "comparator":
                // Groups: 1=nume, 2=valoare, 3=unitate, 4=comparator
                nume = match.Groups[1].Value;
                valoare = match.Groups[2].Value;
                unitate = match.Groups[3].Value;
                intervalText = match.Groups[4].Value;
                var compMatch = Regex.Match(match.Groups[4].Value, @"([<>]=?)\s*(\d+[.,]?\d*)");
                if (compMatch.Success)
                {
                    var op = compMatch.Groups[1].Value;
                    var val = ParseNumeric(compMatch.Groups[2].Value).numeric;
                    if (op.Contains('<'))
                        max = val;
                    else if (op.Contains('>'))
                        min = val;
                }
                break;

            case "text":
                // Groups: 1=nume, 2=valoare_text, 3=unitate, 4=referinta
                nume = match.Groups[1].Value;
                valoare = match.Groups[2].Value;
                unitate = match.Groups[3].Value;
                intervalText = match.Groups[4].Value;
                break;

            case "egal":
                // Groups: 1=nume, 2=valoare, 3=unitate, 4=min, 5=max
                nume = match.Groups[1].Value;
                valoare = match.Groups[2].Value;
                unitate = match.Groups[3].Value;
                if (!string.IsNullOrEmpty(match.Groups[4].Value))
                    min = ParseNumeric(match.Groups[4].Value).numeric;
                if (!string.IsNullOrEmpty(match.Groups[5].Value))
                    max = ParseNumeric(match.Groups[5].Value).numeric;
                break;

            case "simplu":
                // Groups: 1=nume, 2=valoare, 3=unitate
                nume = match.Groups[1].Value;
                valoare = match.Groups[2].Value;
                unitate = match.Groups[3].Value;
                break;
        }

        var (valText, valNumeric) = ParseNumeric(valoare);
        var (isAnormal, direction) = CheckAnormal(valNumeric, min, max);

        // Extrage cod din nume (ex: "Hemoglobina (HGB)")
        string? cod = null;
        var codMatch = Regex.Match(nume, @"\(([A-Z0-9\-]{2,10})\)");
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
            UnitateMasura = CleanUnit(unitate),
            IntervalMin = min,
            IntervalMax = max,
            IntervalText = intervalText?.Trim() ?? "",
            EsteAnormal = isAnormal,
            DirectieAnormal = direction
        };
    }

    private string CleanNumeAnaliza(string nume)
    {
        // Curăță numele de analiză
        nume = Regex.Replace(nume, @"\s+", " ").Trim();
        nume = Regex.Replace(nume, @"^\d+\.\s*", ""); // Elimină numerotare
        nume = Regex.Replace(nume, @"[\*\^]+$", "").Trim(); // Elimină asteriscuri și accente
        nume = Regex.Replace(nume, @"^\s*\^\s*", "").Trim(); // Elimină ^ de la început
        return nume;
    }

    private string CleanUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return "";
        
        // Curăță și normalizează unitatea
        var cleaned = unit.Trim();
        // Înlocuiește caractere corupte cu µ
        cleaned = Regex.Replace(cleaned, @"┬╡", "µ");
        return cleaned;
    }

    private bool IsValidUnit(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return false;

        // Unități valide comune
        var validUnits = new[] {
            "g/dL", "mg/dL", "ng/mL", "pg/mL", "µg/dL", "U/L", "mU/L",
            "%", "mm/h", "mm3", "µm3", "µm^3", "x10^", "/mm3",
            "mL/min", "mg/L", "g/L", "mmol/L", "µmol/L", "mEq/L",
            "Ery", "Leu", "sec", "s"
        };

        return validUnits.Any(u => unit.Contains(u, StringComparison.OrdinalIgnoreCase)) ||
               Regex.IsMatch(unit, @"^[a-zA-Z%/\^0-9µ]+$");
    }

    private bool ShouldSkipLine(string line)
    {
        var lowerLine = line.ToLowerInvariant();
        
        // Skip exact patterns
        var skipExact = new[]
        {
            "analize rezultate um interval biologic de referinta",
            "tip substanta:", "metoda:", "validat de", "verificat",
            "autorizat", "semnat", "data tiparirii:", "pagina",
            "buletin de analize", "nume/prenume:", "varsta:", "cnp:",
            "tel:", "adresa:", "contract:", "medic trimitator",
            "recoltat la:", "data cerere:", "data receptie:",
            "adresa unitate", "fl-5.8", "certificat", "acreditat",
            "specialisti", "multumim", "clinica sante", "laborator",
            "str.", "www.", "http", ".ro", ".com",
            "normal:", "moderat crescut:", "crescut:", "ridicat",
            "optim", "borderline", "risc", "sarcina:", "saptamana",
            "la termen:", "postpartum", "categorii", "insuficienta"
        };

        if (skipExact.Any(s => lowerLine.Contains(s)))
            return true;

        // Skip linii care sunt doar numere sau caractere speciale
        if (Regex.IsMatch(line, @"^[\d\s\.\,\-\:\;\(\)]+$"))
            return true;

        // Skip linii foarte scurte fără litere
        if (line.Length < 5 && !Regex.IsMatch(line, @"[A-Za-z]{2,}"))
            return true;

        // Skip linii cu caractere grafice (semnaturi electronice)
        if (Regex.IsMatch(line, @"[ΓûêΓûÇΓûäΓû]{3,}"))
            return true;

        return false;
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
