namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Tratamentul și Recomandările unei Consultații
/// </summary>
public class ConsultatieTratamentDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    // Tratament Anterior
    /// <summary>Tratament efectuat anterior consultației</summary>
    public string? TratamentAnterior { get; set; }
    
    // Tratament Recomandat
    public string? TratamentMedicamentos { get; set; }
    public string? TratamentNemedicamentos { get; set; }
    public string? RecomandariDietetice { get; set; }
    public string? RecomandariRegimViata { get; set; }
    
    // Recomandari
    public string? InvestigatiiRecomandate { get; set; }
    public string? ConsulturiSpecialitate { get; set; }
    public string? DataUrmatoareiProgramari { get; set; }
    public string? RecomandariSupraveghere { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
