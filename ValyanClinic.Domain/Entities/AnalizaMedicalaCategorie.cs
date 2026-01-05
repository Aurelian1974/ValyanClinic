namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru categorii analize medicale
/// Ex: HEMATOLOGIE, BIOCHIMIE, IMUNOLOGIE, etc.
/// </summary>
public class AnalizaMedicalaCategorie
{
    public Guid CategorieID { get; set; }
    public string NumeCategorie { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public string? Icon { get; set; } // FontAwesome icon (ex: "fa-flask")
    public int OrdineAfisare { get; set; } = 0;
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<AnalizaMedicala> Analize { get; set; } = new List<AnalizaMedicala>();
}
