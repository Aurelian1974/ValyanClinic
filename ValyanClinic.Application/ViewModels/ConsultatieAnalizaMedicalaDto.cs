namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// DTO pentru afișarea unei analize medicale din consultație
/// </summary>
public class ConsultatieAnalizaMedicalaDto
{
    public Guid Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    // Tipul Analizei
    public string TipAnaliza { get; set; } = string.Empty;
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }
    
    // Status
    public string StatusAnaliza { get; set; } = "Recomandata";
    
    // Date Programare/Efectuare
    public DateTime DataRecomandare { get; set; }
    public DateTime? DataProgramata { get; set; }
    public DateTime? DataEfectuare { get; set; }
    public string? LocEfectuare { get; set; }
    
    // Prioritate
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    
    // Indicatii
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiRecomandare { get; set; }
    
    // Rezultate
    public bool AreRezultate { get; set; }
    public DateTime? DataRezultate { get; set; }
    public string? ValoareRezultat { get; set; }
    public string? UnitatiMasura { get; set; }
    public decimal? ValoareNormalaMin { get; set; }
    public decimal? ValoareNormalaMax { get; set; }
    public bool EsteInAfaraLimitelor { get; set; }
    
    // Interpretare
    public string? InterpretareMedic { get; set; }
    public string? ConclusiiAnaliza { get; set; }
    
    // Documente
    public string? CaleFisierRezultat { get; set; }
    public string? TipFisier { get; set; }
    
    // Costuri
    public decimal? Pret { get; set; }
    public bool Decontat { get; set; }
    
    // Detalii (parametri individuali)
    public List<ConsultatieAnalizaDetaliuDto> Detalii { get; set; } = new();
}

/// <summary>
/// DTO pentru detaliile (parametri) unei analize
/// </summary>
public class ConsultatieAnalizaDetaliuDto
{
    public Guid Id { get; set; }
    public Guid AnalizaMedicalaID { get; set; }
    
    // Parametru
    public string NumeParametru { get; set; } = string.Empty;
    public string? CodParametru { get; set; }
    
    // Valoare
    public string Valoare { get; set; } = string.Empty;
    public string? UnitatiMasura { get; set; }
    public string? TipValoare { get; set; }
    
    // Limite Normale
    public decimal? ValoareNormalaMin { get; set; }
    public decimal? ValoareNormalaMax { get; set; }
    public string? ValoareNormalaText { get; set; }
    
    // Status
    public bool EsteAnormal { get; set; }
    public string? NivelGravitate { get; set; }
    
    // Interpretare
    public string? Observatii { get; set; }
}
