namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Endoscopie EFECTUATĂ - rezultatele endoscopiei
/// </summary>
public class ConsultatieEndoscopieEfectuata
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEYS ====================
    public Guid? RecomandareID { get; set; } // Link către recomandarea originală (opțional)
    public Guid? ConsultatieID { get; set; } // Consultația în care s-a înregistrat rezultatul
    public Guid PacientID { get; set; }
    public Guid? EndoscopieNomenclatorID { get; set; }

    // ==================== DETALII ====================
    public string DenumireEndoscopie { get; set; } = string.Empty;
    public string? CodEndoscopie { get; set; }

    // ==================== REZULTATE ====================
    public DateTime DataEfectuare { get; set; }
    public string? CentrulMedical { get; set; } // Unde s-a efectuat
    public string? MedicExecutant { get; set; }
    public string? Rezultat { get; set; } // Descrierea macroscopică
    public string? Concluzii { get; set; }
    public string? BiopsiiPrelevate { get; set; } // Dacă s-au prelevat biopsii
    public string? RezultatHistopatologic { get; set; }
    public string? CaleFisierRezultat { get; set; } // Path către fișierul/imaginile

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataModificare { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION ====================
    public virtual ConsultatieEndoscopieRecomandata? Recomandare { get; set; }
    public virtual NomenclatorEndoscopie? EndoscopiaNomenclator { get; set; }
}
