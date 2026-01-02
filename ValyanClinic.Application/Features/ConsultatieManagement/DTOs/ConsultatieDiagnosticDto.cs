namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Diagnosticul unei Consultații
/// </summary>
public class ConsultatieDiagnosticDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    public string? DiagnosticPozitiv { get; set; }
    public string? DiagnosticDiferential { get; set; }
    public string? DiagnosticEtiologic { get; set; }
    public string? CoduriICD10 { get; set; }
    public string? CoduriICD10Secundare { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
