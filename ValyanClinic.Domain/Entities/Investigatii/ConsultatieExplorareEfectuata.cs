namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Explorare Funcțională EFECTUATĂ - rezultatele explorării
/// </summary>
public class ConsultatieExplorareEfectuata
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid? RecomandareID { get; set; } // Link către recomandarea originală (opțional)
    public Guid? ConsultatieID { get; set; } // Consultația în care s-a înregistrat rezultatul
    public Guid PacientID { get; set; }
    public Guid? ExplorareNomenclatorID { get; set; }

    // ==================== DETALII ====================
    public string DenumireExplorare { get; set; } = string.Empty;
    public string? CodExplorare { get; set; }

    // ==================== REZULTATE ====================
    public DateTime DataEfectuare { get; set; }
    public string? CentrulMedical { get; set; } // Unde s-a efectuat
    public string? MedicExecutant { get; set; }
    public string? Rezultat { get; set; } // Descrierea rezultatului
    public string? Concluzii { get; set; }
    public string? ParametriMasurati { get; set; } // JSON cu valorile măsurate
    public string? CaleFisierRezultat { get; set; } // Path către fișierul scanat/PDF

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataModificare { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION ====================
    public virtual ConsultatieExplorareRecomandata? Recomandare { get; set; }
    public virtual NomenclatorExplorareFunc? ExplorareaaNomenclator { get; set; }
}
