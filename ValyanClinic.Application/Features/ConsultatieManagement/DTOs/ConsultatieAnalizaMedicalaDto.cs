namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru o Analiză Medicală din cadrul unei Consultații
/// </summary>
public class ConsultatieAnalizaMedicalaDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    // Tipul Analizei
    public string TipAnaliza { get; set; } = string.Empty;
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }
    
    // Status
    public string StatusAnaliza { get; set; } = "Recomandata";
    
    // Date
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
    public long? DimensiuneFisier { get; set; }
    
    // Costuri
    public decimal? Pret { get; set; }
    public bool Decontat { get; set; }
    
    // Links
    public Guid? LaboratorID { get; set; }
    public Guid? MedicInterpretareID { get; set; }
    
    // Detalii (parametri individuali)
    public List<ConsultatieAnalizaDetaliuDto>? Detalii { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}

/// <summary>
/// DTO pentru un parametru individual dintr-o analiză medicală
/// </summary>
public class ConsultatieAnalizaDetaliuDto
{
    public Guid? Id { get; set; }
    public Guid AnalizaMedicalaID { get; set; }
    
    public string NumeParametru { get; set; } = string.Empty;
    public string? CodParametru { get; set; }
    
    public string Valoare { get; set; } = string.Empty;
    public string? UnitatiMasura { get; set; }
    public string? TipValoare { get; set; }
    
    public decimal? ValoareNormalaMin { get; set; }
    public decimal? ValoareNormalaMax { get; set; }
    public string? ValoareNormalaText { get; set; }
    
    public bool EsteAnormal { get; set; }
    public string? NivelGravitate { get; set; }
    
    public string? Observatii { get; set; }
    
    public DateTime DataCreare { get; set; }
}
