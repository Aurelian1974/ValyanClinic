namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Diagnosticul unei Consultații
/// </summary>
public class ConsultatieDiagnosticDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    // Normalized fields
    public string? CodICD10Principal { get; set; }
    public string? NumeDiagnosticPrincipal { get; set; }
    public string? DescriereDetaliataPrincipal { get; set; }
    
    // Legacy fields (kept for backwards compatibility)
    public string? DiagnosticPozitiv { get; set; }
    public string? CoduriICD10 { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
