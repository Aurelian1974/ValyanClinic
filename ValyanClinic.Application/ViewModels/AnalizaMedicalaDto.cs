namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// DTO pentru afișarea unei analize medicale
/// </summary>
public class AnalizaMedicalaDto
{
    public Guid Id { get; set; }
    public Guid PacientId { get; set; }
    public Guid? ConsultatieId { get; set; }
    
    // Date document sursă
    public DateTime DataDocument { get; set; }
    public string? NumeDocument { get; set; }
    public string? SursaDocument { get; set; }
    public Guid? BatchId { get; set; }
    
    // Detalii analiză
    public string? Categorie { get; set; }
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? Rezultat { get; set; }
    public string? UnitateMasura { get; set; }
    public string? IntervalReferinta { get; set; }
    public bool InAfaraLimitelor { get; set; }
    
    // Audit
    public DateTime DataCrearii { get; set; }
}

/// <summary>
/// Grupare analize pe document/batch
/// </summary>
public class AnalizeMedicaleGroupDto
{
    public DateTime DataDocument { get; set; }
    public string? NumeDocument { get; set; }
    public string? SursaDocument { get; set; }
    public Guid? BatchId { get; set; }
    
    public List<AnalizaMedicalaDto> Analize { get; set; } = new();
    
    // Statistici
    public int TotalAnalize => Analize.Count;
    public int AnormalCount => Analize.Count(a => a.InAfaraLimitelor);
    public bool HasAbnormalValues => AnormalCount > 0;
}

/// <summary>
/// Rezultat comparație între două analize (anterioară vs. actuală)
/// </summary>
public class AnalizaComparatieDto
{
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? UnitateMasura { get; set; }
    public string? IntervalReferinta { get; set; }
    
    // Valori anterioare
    public string? ValoareAnterioara { get; set; }
    public decimal? ValoareAnterioaraNumeric { get; set; }
    public bool AnterioaraInAfaraLimitelor { get; set; }
    public DateTime? DataAnterioara { get; set; }
    
    // Valori actuale
    public string? ValoareActuala { get; set; }
    public decimal? ValoareActualaNumeric { get; set; }
    public bool ActualaInAfaraLimitelor { get; set; }
    public DateTime? DataActuala { get; set; }
    
    // Trend și comparație
    public TrendComparatie Trend { get; set; } = TrendComparatie.Nedeterminat;
    public decimal? DiferentaAbsoluta { get; set; }
    public decimal? DiferentaProcentuala { get; set; }
    public string? MesajComparatie { get; set; }
    
    // Flags pentru afișare
    public bool ExistaAnterior => !string.IsNullOrEmpty(ValoareAnterioara);
    public bool ExistaActual => !string.IsNullOrEmpty(ValoareActuala);
    public bool EsteNou => !ExistaAnterior && ExistaActual;
    public bool ADisparut => ExistaAnterior && !ExistaActual;
}

/// <summary>
/// Direcția trendului între două valori
/// </summary>
public enum TrendComparatie
{
    Nedeterminat = 0,
    Crescut = 1,
    Scazut = 2,
    Stabil = 3,
    Imbunatatit = 4,  // A revenit în limite normale
    Inrautatit = 5    // A ieșit din limite normale
}

/// <summary>
/// Rezultat complet al comparației între două seturi de analize
/// </summary>
public class ComparatieAnalizeMedicaleDto
{
    public DateTime DataSetAnterior { get; set; }
    public string? NumeDocumentAnterior { get; set; }
    
    public DateTime DataSetActual { get; set; }
    public string? NumeDocumentActual { get; set; }
    
    public List<AnalizaComparatieDto> Comparatii { get; set; } = new();
    
    // Statistici
    public int TotalAnalize => Comparatii.Count;
    public int AnalizeImbunatatite => Comparatii.Count(c => c.Trend == TrendComparatie.Imbunatatit);
    public int AnalizeInrautatite => Comparatii.Count(c => c.Trend == TrendComparatie.Inrautatit);
    public int AnalizeNoi => Comparatii.Count(c => c.EsteNou);
    public int AnalizeDisparute => Comparatii.Count(c => c.ADisparut);
}
