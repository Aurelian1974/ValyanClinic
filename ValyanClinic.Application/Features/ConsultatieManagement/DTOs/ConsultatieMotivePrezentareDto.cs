namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Motivele de Prezentare ale unei Consultații
/// </summary>
public class ConsultatieMotivePrezentareDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
