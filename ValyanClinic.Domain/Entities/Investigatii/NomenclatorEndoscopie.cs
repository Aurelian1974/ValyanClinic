namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Nomenclator pentru Endoscopii (Gastroscopie, Colonoscopie, Bronhoscopie, etc.)
/// </summary>
public class NomenclatorEndoscopie
{
    public Guid Id { get; set; }
    public string Cod { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; } // Digestiv superior, Digestiv inferior, Respirator, etc.
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; } = true;
    public int Ordine { get; set; } = 0;
    public DateTime DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }
}
