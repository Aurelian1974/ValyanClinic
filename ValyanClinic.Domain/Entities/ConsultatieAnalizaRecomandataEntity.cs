namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Analizele Medicale RECOMANDATE în timpul consultației.
/// Tabelă separată de ConsultatieAnalizeMedicale (care este pentru import/rezultate).
/// </summary>
public class ConsultatieAnalizaRecomandataEntity
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid ConsultatieID { get; set; }
    public Guid? AnalizaNomenclatorID { get; set; } // Link către nomenclatorul de analize

    // ==================== DETALII ANALIZA ====================
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }
    public string TipAnaliza { get; set; } = "Laborator"; // Categoria: Biochimie, Hematologie, etc.

    // ==================== RECOMANDARE ====================
    public DateTime DataRecomandare { get; set; }
    public string? Prioritate { get; set; } // 'Normala', 'Urgent', 'Foarte urgent'
    public bool EsteCito { get; set; } = false;
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiMedic { get; set; }

    // ==================== STATUS TRACKING ====================
    public string Status { get; set; } = "Recomandata"; // 'Recomandata', 'Programata', 'Efectuata', 'Anulata'
    public DateTime? DataProgramata { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataModificare { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
