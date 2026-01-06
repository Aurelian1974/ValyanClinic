namespace ValyanClinic.Domain.Entities.Investigatii;

/// <summary>
/// Nomenclator pentru Investigații Imagistice (Radiografie, CT, RMN, Ecografie, etc.)
/// </summary>
public class NomenclatorInvestigatieImagistica
{
    public Guid Id { get; set; }
    public string Cod { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; } // Radiologie, CT, RMN, Ecografie, etc.
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; } = true;
    public int Ordine { get; set; } = 0;
    public DateTime DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }
}
