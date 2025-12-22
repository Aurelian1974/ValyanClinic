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
