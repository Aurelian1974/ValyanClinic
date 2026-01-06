namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Investigație Imagistică RECOMANDATĂ în cadrul unei consultații
/// </summary>
public class ConsultatieInvestigatieImagisticaRecomandata
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid ConsultatieID { get; set; }
    public Guid? InvestigatieNomenclatorID { get; set; }

    // ==================== DETALII ====================
    public string DenumireInvestigatie { get; set; } = string.Empty;
    public string? CodInvestigatie { get; set; }
    public string? RegiuneAnatomica { get; set; }

    // ==================== RECOMANDARE ====================
    public DateTime DataRecomandare { get; set; }
    public string? Prioritate { get; set; } // Normala, Urgent, Foarte urgent
    public bool EsteCito { get; set; } = false;
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiMedic { get; set; }

    // ==================== STATUS ====================
    public string Status { get; set; } = "Recomandata"; // Recomandata, Programata, Efectuata, Anulata

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataModificare { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION ====================
    public virtual Consultatie? Consultatie { get; set; }
    public virtual NomenclatorInvestigatieImagistica? InvestigatiaNomenclator { get; set; }
}
