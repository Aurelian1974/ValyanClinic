namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO Complet pentru o Consultație cu toate detaliile (normalized structure)
/// Folosit pentru GET operations și UI rendering
/// </summary>
public class ConsultatieCompleteDto
{
    // Master Data
    public Guid ConsultatieID { get; set; }
    public Guid? ProgramareID { get; set; }
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public DateTime DataConsultatie { get; set; }
    public TimeSpan OraConsultatie { get; set; }
    public string TipConsultatie { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DataFinalizare { get; set; }
    public int DurataMinute { get; set; }
    
    // Audit Master
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
    
    // Detail Sections (1:1 relationships)
    public ConsultatieMotivePrezentareDto? MotivePrezentare { get; set; }
    public ConsultatieAntecedenteDto? Antecedente { get; set; }
    public ConsultatieExamenObiectivDto? ExamenObiectiv { get; set; }
    public ConsultatieInvestigatiiDto? Investigatii { get; set; }
    public ConsultatieDiagnosticDto? Diagnostic { get; set; }
    public ConsultatieTratamentDto? Tratament { get; set; }
    public ConsultatieConcluziiDto? Concluzii { get; set; }
    
    // Detail Collections (1:N relationships)
    public List<ConsultatieAnalizaMedicalaDto>? AnalizeMedicale { get; set; }
    
    // Computed Property
    public DateTime DataOraConsultatie => DataConsultatie.Date + OraConsultatie;
}
