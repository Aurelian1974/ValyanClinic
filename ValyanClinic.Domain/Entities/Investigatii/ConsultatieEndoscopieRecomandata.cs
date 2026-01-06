namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Endoscopie RECOMANDATĂ în cadrul unei consultații
/// </summary>
public class ConsultatieEndoscopieRecomandata
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid ConsultatieID { get; set; }
    public Guid? EndoscopieNomenclatorID { get; set; }

    // ==================== DETALII ====================
    public string DenumireEndoscopie { get; set; } = string.Empty;
    public string? CodEndoscopie { get; set; }

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
    public virtual NomenclatorEndoscopie? EndoscopiaNomenclator { get; set; }
}
