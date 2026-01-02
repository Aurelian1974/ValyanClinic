namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Concluziile și Observațiile unei Consultații
/// </summary>
public class ConsultatieConcluziiDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    public string? Prognostic { get; set; }
    public string? Concluzie { get; set; }
    public string? ObservatiiMedic { get; set; }
    public string? NotePacient { get; set; }
    public string? DocumenteAtatate { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
