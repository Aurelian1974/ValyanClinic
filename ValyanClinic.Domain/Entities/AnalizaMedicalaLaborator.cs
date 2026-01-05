namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru nomenclator laboratoare medicale
/// </summary>
public class AnalizaMedicalaLaborator
{
    public Guid LaboratorID { get; set; }
    public string NumeLaborator { get; set; } = string.Empty;
    public string? Acronim { get; set; }
    public string? Adresa { get; set; }
    public string? Localitate { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; } = DateTime.Now;
    public DateTime DataUltimaActualizare { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<AnalizaMedicala> Analize { get; set; } = new List<AnalizaMedicala>();
}
