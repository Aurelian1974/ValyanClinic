namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entity pentru specializarile medicale
/// </summary>
public class Specializare
{
    public Guid Id { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCrearii { get; set; } = DateTime.Now;
    public DateTime DataUltimeiModificari { get; set; } = DateTime.Now;
    public string? CreatDe { get; set; }
    public string? ModificatDe { get; set; }
}
