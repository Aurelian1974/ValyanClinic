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

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
