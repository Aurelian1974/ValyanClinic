namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Medicamentele prescrise într-o Consultație
/// Relație 1:N cu Consultatie (o consultație poate avea multiple medicamente)
/// </summary>
public class ConsultatieMedicament
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== ORDERING ====================
    /// <summary>Ordinea afișării în lista de medicamente</summary>
    public int OrdineAfisare { get; set; }

    // ==================== MEDICATION DETAILS ====================
    /// <summary>Numele medicamentului</summary>
    public string NumeMedicament { get; set; } = string.Empty;

    /// <summary>Doza prescrisă (ex: 500mg, 10ml)</summary>
    public string? Doza { get; set; }

    /// <summary>Frecvența administrării (ex: 1x3/zi, la 8 ore)</summary>
    public string? Frecventa { get; set; }

    /// <summary>Durata tratamentului (ex: 7 zile, 2 săptămâni)</summary>
    public string? Durata { get; set; }

    /// <summary>Cantitatea prescrisă (ex: 1 cutie, 30 comprimate)</summary>
    public string? Cantitate { get; set; }

    /// <summary>Observații suplimentare pentru medicament</summary>
    public string? Observatii { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
