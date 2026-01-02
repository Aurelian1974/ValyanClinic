namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Investigațiile efectuate în cadrul unei Consultații
/// </summary>
public class ConsultatieInvestigatiiDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    public string? InvestigatiiLaborator { get; set; }
    public string? InvestigatiiImagistice { get; set; }
    public string? InvestigatiiEKG { get; set; }
    public string? AlteInvestigatii { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
