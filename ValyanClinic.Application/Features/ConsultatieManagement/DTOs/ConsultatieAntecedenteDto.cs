namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru Antecedentele Medicale ale unei Consultații (SIMPLIFIED)
/// Conține doar două câmpuri text pentru istoric medical
/// </summary>
public class ConsultatieAntecedenteDto
{
    public Guid? Id { get; set; }
    public Guid ConsultatieID { get; set; }
    
    /// <summary>Istoric medical personal - boli anterioare, intervenții, alergii, medicație cronică</summary>
    public string? IstoricMedicalPersonal { get; set; }
    
    /// <summary>Istoric familial - antecedente heredocolaterale</summary>
    public string? IstoricFamilial { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
