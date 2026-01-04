namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Concluziile și Observațiile unei Consultații
/// Relație 1:1 cu Consultatie
/// </summary>
public class ConsultatieConcluzii
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== PROGNOSTIC ====================
    public string? Prognostic { get; set; } // Favorabil, Rezervat, Sever

    // ==================== CONCLUZIE ====================
    public string? Concluzie { get; set; }

    // ==================== OBSERVATII SUPLIMENTARE ====================
    public string? ObservatiiMedic { get; set; }
    public string? NotePacient { get; set; }

    // ==================== DOCUMENTE ANEXATE ====================
    public string? DocumenteAtatate { get; set; }

    // ==================== SCRISOARE MEDICALĂ - ANEXA 43 ====================
    
    /// <summary>Pacient diagnosticat cu afecțiune oncologică</summary>
    public bool EsteAfectiuneOncologica { get; set; }
    public string? DetaliiAfectiuneOncologica { get; set; }
    
    /// <summary>Indicație de revenire pentru internare</summary>
    public bool AreIndicatieInternare { get; set; }
    public string? TermenInternare { get; set; }
    
    /// <summary>Prescripție medicală</summary>
    public bool? SaEliberatPrescriptie { get; set; }
    public string? SeriePrescriptie { get; set; }
    
    /// <summary>Concediu medical</summary>
    public bool? SaEliberatConcediuMedical { get; set; }
    public string? SerieConcediuMedical { get; set; }
    
    /// <summary>Îngrijiri medicale la domiciliu</summary>
    public bool? SaEliberatIngrijiriDomiciliu { get; set; }
    
    /// <summary>Dispozitive medicale</summary>
    public bool? SaEliberatDispozitiveMedicale { get; set; }
    
    /// <summary>Calea de transmitere</summary>
    public bool TransmiterePrinEmail { get; set; }
    public string? EmailTransmitere { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
