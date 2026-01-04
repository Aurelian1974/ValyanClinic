namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Antecedentele Medicale ale unei Consultații (simplificat)
/// Relație 1:1 cu Consultatie
/// Conține doar Istoric Medical Personal și Istoric Familial
/// </summary>
public class ConsultatieAntecedente
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== ANTECEDENTE SIMPLIFICATE ====================
    
    /// <summary>
    /// Istoric medical personal (boli anterioare, intervenții chirurgicale, 
    /// alergii, tratamente cronice, etc.)
    /// </summary>
    public string? IstoricMedicalPersonal { get; set; }
    
    /// <summary>
    /// Istoric familial (boli ereditare, antecedente în familie)
    /// </summary>
    public string? IstoricFamilial { get; set; }

    /// <summary>
    /// Tratament urmat anterior (medicație, proceduri, intervenții)
    /// Conform Scrisoare Medicală Anexa 43
    /// </summary>
    public string? TratamentAnterior { get; set; }

    /// <summary>
    /// Factori de risc identificați (HTA, diabet, fumat, sedentarism, obezitate, etc.)
    /// Conform Scrisoare Medicală Anexa 43
    /// </summary>
    public string? FactoriDeRisc { get; set; }

    /// <summary>
    /// Alergii cunoscute (medicamente, alimente, substanțe, etc.)
    /// Conform Scrisoare Medicală Anexa 43
    /// </summary>
    public string? Alergii { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
