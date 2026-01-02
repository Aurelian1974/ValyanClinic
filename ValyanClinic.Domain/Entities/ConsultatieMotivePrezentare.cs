namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Motivele de Prezentare ale unei Consultații
/// Relație 1:1 cu Consultatie
/// </summary>
public class ConsultatieMotivePrezentare
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== MOTIVE PREZENTARE ====================
    public string? MotivPrezentare { get; set; }
    public string? IstoricBoalaActuala { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
