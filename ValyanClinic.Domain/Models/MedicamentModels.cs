namespace ValyanClinic.Domain.Models;

/// <summary>
/// Medicament din Nomenclatorul ANM.
/// Coloanele respecta exact structura fisierului Excel ANM.
/// </summary>
public class MedicamentNomenclator
{
    public Guid Id { get; set; }

    /// <summary>Col 1: Cod CIM (codul unic ANM)</summary>
    public string CodCIM { get; set; } = string.Empty;

    /// <summary>Col 2: Denumire comerciala</summary>
    public string DenumireComerciala { get; set; } = string.Empty;

    /// <summary>Col 3: DCI (Denumire Comuna Internationala)</summary>
    public string? DCI { get; set; }

    /// <summary>Col 4: Forma farmaceutica</summary>
    public string? FormaFarmaceutica { get; set; }

    /// <summary>Col 5: Concentratie</summary>
    public string? Concentratie { get; set; }

    /// <summary>Col 6: Firma / tara producatoare APP</summary>
    public string? FirmaTaraProducatoareAPP { get; set; }

    /// <summary>Col 7: Firma / tara detinatoare APP</summary>
    public string? FirmaTaraDetinatoareAPP { get; set; }

    /// <summary>Col 8: Cod ATC</summary>
    public string? CodATC { get; set; }

    /// <summary>Col 9: Actiune terapeutica</summary>
    public string? ActiuneTerapeutica { get; set; }

    /// <summary>Col 10: Prescriptie (P, PRF, OTC, etc.)</summary>
    public string? Prescriptie { get; set; }

    /// <summary>Col 11: Nr / data ambalaj APP</summary>
    public string? NrDataAmbalajAPP { get; set; }

    /// <summary>Col 12: Ambalaj</summary>
    public string? Ambalaj { get; set; }

    /// <summary>Col 13: Volum ambalaj</summary>
    public string? VolumAmbalaj { get; set; }

    /// <summary>Col 14: Valabilitate ambalaj</summary>
    public string? ValabilitateAmbalaj { get; set; }

    /// <summary>Col 15: Bulina (simbol)</summary>
    public string? Bulina { get; set; }

    /// <summary>Col 16: Diez (simbol #)</summary>
    public string? Diez { get; set; }

    /// <summary>Col 17: Stea (simbol)</summary>
    public string? Stea { get; set; }

    /// <summary>Col 18: Triunghi (simbol)</summary>
    public string? Triunghi { get; set; }

    /// <summary>Col 19: Dreptunghi (simbol)</summary>
    public string? Dreptunghi { get; set; }

    /// <summary>Col 20: Data actualizare</summary>
    public string? DataActualizare { get; set; }

    // Coloane de audit
    public DateTime DataImport { get; set; }
    public DateTime DataUltimaActualizare { get; set; }
    public bool Activ { get; set; } = true;
}

/// <summary>
/// Rezultat sincronizare nomenclator.
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int RecordsAdded { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsDeactivated { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime SyncDate { get; set; } = DateTime.Now;
}

/// <summary>
/// Statistici nomenclator.
/// </summary>
public class NomenclatorStats
{
    public int TotalActive { get; set; }
    public int TotalInactive { get; set; }
    public int TotalDCI { get; set; }
    public int TotalProducatori { get; set; }
    public DateTime? UltimaActualizare { get; set; }
    public DateTime? UltimaSincronizareReusita { get; set; }
}
