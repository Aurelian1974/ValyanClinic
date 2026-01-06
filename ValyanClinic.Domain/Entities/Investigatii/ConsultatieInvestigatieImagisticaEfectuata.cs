namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Investigație Imagistică EFECTUATĂ - rezultatele investigației
/// </summary>
public class ConsultatieInvestigatieImagisticaEfectuata
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid? RecomandareID { get; set; } // Link către recomandarea originală (opțional)
    public Guid? ConsultatieID { get; set; } // Consultația în care s-a înregistrat rezultatul
    public Guid PacientID { get; set; }
    public Guid? InvestigatieNomenclatorID { get; set; }

    // ==================== DETALII ====================
    public string DenumireInvestigatie { get; set; } = string.Empty;
    public string? CodInvestigatie { get; set; }
    public string? RegiuneAnatomica { get; set; }

    // ==================== REZULTATE ====================
    public DateTime DataEfectuare { get; set; }
    public string? CentrulMedical { get; set; } // Unde s-a efectuat
    public string? MedicExecutant { get; set; }
    public string? Rezultat { get; set; } // Descrierea rezultatului
    public string? Concluzii { get; set; }
    public string? CaleFisierRezultat { get; set; } // Path către fișierul scanat/PDF

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataModificare { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION ====================
    public virtual ConsultatieInvestigatieImagisticaRecomandata? Recomandare { get; set; }
    public virtual NomenclatorInvestigatieImagistica? InvestigatiaNomenclator { get; set; }
}
