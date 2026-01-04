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

    /// <summary>Tratament urmat anterior (medicație, proceduri, intervenții) - Anexa 43</summary>
    public string? TratamentAnterior { get; set; }

    /// <summary>Factori de risc identificați (HTA, diabet, fumat, etc.) - Anexa 43</summary>
    public string? FactoriDeRisc { get; set; }
    
    // Audit
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }
}
