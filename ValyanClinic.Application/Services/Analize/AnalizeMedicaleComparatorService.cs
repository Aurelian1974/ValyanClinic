using System.Text.RegularExpressions;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Application.Services.Analize;

/// <summary>
/// Serviciu pentru compararea a două seturi de analize medicale
/// Folosit pentru a vedea evoluția pacientului în timp
/// </summary>
public interface IAnalizeMedicaleComparatorService
{
    /// <summary>
    /// Compară două grupuri de analize și returnează rezultatul comparației
    /// </summary>
    ComparatieAnalizeMedicaleDto CompareGroups(
        AnalizeMedicaleGroupDto anterioare, 
        AnalizeMedicaleGroupDto actuale);
    
    /// <summary>
    /// Compară două liste de analize
    /// </summary>
    List<AnalizaComparatieDto> CompareAnalize(
        List<AnalizaMedicalaDto> anterioare,
        List<AnalizaMedicalaDto> actuale);
}

public class AnalizeMedicaleComparatorService : IAnalizeMedicaleComparatorService
{
    // Toleranță pentru a considera două valori "stabile" (±2%)
    private const decimal TolerancePercent = 2m;

    public ComparatieAnalizeMedicaleDto CompareGroups(
        AnalizeMedicaleGroupDto anterioare, 
        AnalizeMedicaleGroupDto actuale)
    {
        var result = new ComparatieAnalizeMedicaleDto
        {
            DataSetAnterior = anterioare.DataDocument,
            NumeDocumentAnterior = anterioare.NumeDocument,
            DataSetActual = actuale.DataDocument,
            NumeDocumentActual = actuale.NumeDocument,
            Comparatii = CompareAnalize(anterioare.Analize, actuale.Analize)
        };

        return result;
    }

    public List<AnalizaComparatieDto> CompareAnalize(
        List<AnalizaMedicalaDto> anterioare,
        List<AnalizaMedicalaDto> actuale)
    {
        var comparatii = new List<AnalizaComparatieDto>();
        var processedActuale = new HashSet<Guid>();

        // 1. Pentru fiecare analiză anterioară, caută corespondentul actual
        foreach (var anterior in anterioare)
        {
            var actual = FindMatchingAnaliza(anterior, actuale, processedActuale);
            
            var comparatie = CreateComparatie(anterior, actual);
            comparatii.Add(comparatie);
            
            if (actual != null)
            {
                processedActuale.Add(actual.Id);
            }
        }

        // 2. Adaugă analizele noi (care nu au corespondent anterior)
        foreach (var actual in actuale.Where(a => !processedActuale.Contains(a.Id)))
        {
            var comparatie = CreateComparatie(null, actual);
            comparatii.Add(comparatie);
        }

        // Sortare: pe categorii, apoi pe nume
        return comparatii
            .OrderBy(c => c.Categorie ?? "ZZZ")
            .ThenBy(c => c.NumeAnaliza)
            .ToList();
    }

    /// <summary>
    /// Găsește analiza corespunzătoare din lista actuală
    /// </summary>
    private AnalizaMedicalaDto? FindMatchingAnaliza(
        AnalizaMedicalaDto anterior,
        List<AnalizaMedicalaDto> actuale,
        HashSet<Guid> processedIds)
    {
        // Normalizează numele pentru comparație
        var numeNormalizat = NormalizeNumeAnaliza(anterior.NumeAnaliza);
        
        // Caută potrivire exactă după nume normalizat
        var match = actuale
            .Where(a => !processedIds.Contains(a.Id))
            .FirstOrDefault(a => NormalizeNumeAnaliza(a.NumeAnaliza) == numeNormalizat);

        if (match != null)
            return match;

        // Caută potrivire fuzzy (conține același cuvânt cheie)
        var keywords = ExtractKeywords(anterior.NumeAnaliza);
        match = actuale
            .Where(a => !processedIds.Contains(a.Id))
            .FirstOrDefault(a => keywords.Any(k => 
                a.NumeAnaliza.Contains(k, StringComparison.OrdinalIgnoreCase)));

        return match;
    }

    /// <summary>
    /// Normalizează numele analizei pentru comparație
    /// </summary>
    private string NormalizeNumeAnaliza(string nume)
    {
        if (string.IsNullOrWhiteSpace(nume))
            return string.Empty;

        // Elimină paranteze cu cod (ex: "Hemoglobina (HGB)" -> "Hemoglobina")
        var result = Regex.Replace(nume, @"\s*\([^)]+\)\s*", " ");
        
        // Normalizare: lowercase, elimină spații multiple
        result = result.ToLowerInvariant().Trim();
        result = Regex.Replace(result, @"\s+", " ");
        
        return result;
    }

    /// <summary>
    /// Extrage cuvintele cheie din numele analizei
    /// </summary>
    private List<string> ExtractKeywords(string nume)
    {
        var keywords = new List<string>();
        
        // Extrage cod din paranteze (ex: HGB, WBC)
        var codMatch = Regex.Match(nume, @"\(([A-Z0-9]{2,10})\)");
        if (codMatch.Success)
        {
            keywords.Add(codMatch.Groups[1].Value);
        }

        // Adaugă primul cuvânt semnificativ (>3 caractere)
        var words = nume.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var significantWord = words.FirstOrDefault(w => w.Length > 3 && !w.StartsWith("("));
        if (!string.IsNullOrEmpty(significantWord))
        {
            keywords.Add(significantWord);
        }

        return keywords;
    }

    /// <summary>
    /// Creează obiectul de comparație
    /// </summary>
    private AnalizaComparatieDto CreateComparatie(
        AnalizaMedicalaDto? anterior,
        AnalizaMedicalaDto? actual)
    {
        var comparatie = new AnalizaComparatieDto
        {
            NumeAnaliza = actual?.NumeAnaliza ?? anterior?.NumeAnaliza ?? "",
            Categorie = actual?.Categorie ?? anterior?.Categorie,
            UnitateMasura = actual?.UnitateMasura ?? anterior?.UnitateMasura,
            IntervalReferinta = actual?.IntervalReferinta ?? anterior?.IntervalReferinta
        };

        // Valori anterioare
        if (anterior != null)
        {
            comparatie.ValoareAnterioara = anterior.Rezultat;
            comparatie.ValoareAnterioaraNumeric = ParseNumericValue(anterior.Rezultat);
            comparatie.AnterioaraInAfaraLimitelor = anterior.InAfaraLimitelor;
            comparatie.DataAnterioara = anterior.DataDocument;
        }

        // Valori actuale
        if (actual != null)
        {
            comparatie.ValoareActuala = actual.Rezultat;
            comparatie.ValoareActualaNumeric = ParseNumericValue(actual.Rezultat);
            comparatie.ActualaInAfaraLimitelor = actual.InAfaraLimitelor;
            comparatie.DataActuala = actual.DataDocument;
        }

        // Calculează trend și diferențe
        CalculateTrendAndDifference(comparatie);

        return comparatie;
    }

    /// <summary>
    /// Parsează valoarea numerică din rezultat
    /// </summary>
    private decimal? ParseNumericValue(string? rezultat)
    {
        if (string.IsNullOrWhiteSpace(rezultat))
            return null;

        // Extrage prima valoare numerică
        var match = Regex.Match(rezultat, @"([<>]?\d+[.,]?\d*)");
        if (match.Success)
        {
            var valStr = match.Groups[1].Value
                .Replace(",", ".")
                .TrimStart('<', '>');
            
            if (decimal.TryParse(valStr, 
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var val))
            {
                return val;
            }
        }

        return null;
    }

    /// <summary>
    /// Calculează trendul și diferențele dintre valori
    /// </summary>
    private void CalculateTrendAndDifference(AnalizaComparatieDto comparatie)
    {
        // Dacă nu există ambele valori numerice
        if (!comparatie.ValoareAnterioaraNumeric.HasValue || 
            !comparatie.ValoareActualaNumeric.HasValue)
        {
            // Verifică dacă sunt valori text
            if (!string.IsNullOrEmpty(comparatie.ValoareAnterioara) && 
                !string.IsNullOrEmpty(comparatie.ValoareActuala))
            {
                // Compară text (ex: Negativ vs Pozitiv)
                if (comparatie.ValoareAnterioara.Equals(comparatie.ValoareActuala, 
                    StringComparison.OrdinalIgnoreCase))
                {
                    comparatie.Trend = TrendComparatie.Stabil;
                    comparatie.MesajComparatie = "Neschimbat";
                }
                else
                {
                    // Verifică îmbunătățire/înrăutățire pentru valori calitative
                    comparatie.Trend = DetermineQualitativeTrend(
                        comparatie.ValoareAnterioara, 
                        comparatie.ValoareActuala,
                        comparatie.AnterioaraInAfaraLimitelor,
                        comparatie.ActualaInAfaraLimitelor);
                    comparatie.MesajComparatie = $"{comparatie.ValoareAnterioara} → {comparatie.ValoareActuala}";
                }
            }
            else
            {
                comparatie.Trend = TrendComparatie.Nedeterminat;
            }
            return;
        }

        var valAnterior = comparatie.ValoareAnterioaraNumeric.Value;
        var valActual = comparatie.ValoareActualaNumeric.Value;

        // Calculează diferențe
        comparatie.DiferentaAbsoluta = valActual - valAnterior;
        
        if (valAnterior != 0)
        {
            comparatie.DiferentaProcentuala = Math.Round(
                ((valActual - valAnterior) / valAnterior) * 100, 1);
        }

        // Determină trend bazat pe diferența procentuală
        if (Math.Abs(comparatie.DiferentaProcentuala ?? 0) <= TolerancePercent)
        {
            comparatie.Trend = TrendComparatie.Stabil;
            comparatie.MesajComparatie = "≈ Stabil";
        }
        else if (valActual > valAnterior)
        {
            // Verifică dacă e îmbunătățire sau înrăutățire
            if (comparatie.AnterioaraInAfaraLimitelor && !comparatie.ActualaInAfaraLimitelor)
            {
                comparatie.Trend = TrendComparatie.Imbunatatit;
                comparatie.MesajComparatie = $"↑ +{comparatie.DiferentaProcentuala}% (revenit în limite)";
            }
            else if (!comparatie.AnterioaraInAfaraLimitelor && comparatie.ActualaInAfaraLimitelor)
            {
                comparatie.Trend = TrendComparatie.Inrautatit;
                comparatie.MesajComparatie = $"↑ +{comparatie.DiferentaProcentuala}% (ieșit din limite!)";
            }
            else
            {
                comparatie.Trend = TrendComparatie.Crescut;
                comparatie.MesajComparatie = $"↑ +{comparatie.DiferentaProcentuala}%";
            }
        }
        else
        {
            // Verifică dacă e îmbunătățire sau înrăutățire
            if (comparatie.AnterioaraInAfaraLimitelor && !comparatie.ActualaInAfaraLimitelor)
            {
                comparatie.Trend = TrendComparatie.Imbunatatit;
                comparatie.MesajComparatie = $"↓ {comparatie.DiferentaProcentuala}% (revenit în limite)";
            }
            else if (!comparatie.AnterioaraInAfaraLimitelor && comparatie.ActualaInAfaraLimitelor)
            {
                comparatie.Trend = TrendComparatie.Inrautatit;
                comparatie.MesajComparatie = $"↓ {comparatie.DiferentaProcentuala}% (ieșit din limite!)";
            }
            else
            {
                comparatie.Trend = TrendComparatie.Scazut;
                comparatie.MesajComparatie = $"↓ {comparatie.DiferentaProcentuala}%";
            }
        }
    }

    /// <summary>
    /// Determină trendul pentru valori calitative (text)
    /// </summary>
    private TrendComparatie DetermineQualitativeTrend(
        string anterior, 
        string actual,
        bool anteriorAnormal,
        bool actualAnormal)
    {
        // Dacă a trecut de la anormal la normal = îmbunătățire
        if (anteriorAnormal && !actualAnormal)
            return TrendComparatie.Imbunatatit;
        
        // Dacă a trecut de la normal la anormal = înrăutățire
        if (!anteriorAnormal && actualAnormal)
            return TrendComparatie.Inrautatit;

        // Verifică tranziții cunoscute
        var anteriorLower = anterior.ToLowerInvariant();
        var actualLower = actual.ToLowerInvariant();

        // Pozitiv/Negativ
        if (anteriorLower.Contains("pozitiv") && actualLower.Contains("negativ"))
            return TrendComparatie.Imbunatatit;
        if (anteriorLower.Contains("negativ") && actualLower.Contains("pozitiv"))
            return TrendComparatie.Inrautatit;

        // Frecvent -> Rar = îmbunătățire (pentru sediment)
        if (anteriorLower.Contains("frecvent") && actualLower.Contains("rar"))
            return TrendComparatie.Imbunatatit;
        if (anteriorLower.Contains("rar") && actualLower.Contains("frecvent"))
            return TrendComparatie.Inrautatit;

        return TrendComparatie.Nedeterminat;
    }
}
