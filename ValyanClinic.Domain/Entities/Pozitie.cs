namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entity pentru pozitiile/functiile personalului medical si non-medical
/// </summary>
public class Pozitie
{
    public Guid Id { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCrearii { get; set; } = DateTime.UtcNow;
    public DateTime DataUltimeiModificari { get; set; } = DateTime.UtcNow;
    public string? CreatDe { get; set; }
    public string? ModificatDe { get; set; }
}
