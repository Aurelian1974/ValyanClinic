namespace ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieById;

public class PozitieDetailDto
{
    public Guid Id { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; }
    public DateTime DataCrearii { get; set; }
    public DateTime DataUltimeiModificari { get; set; }
    public string? CreatDe { get; set; }
    public string? ModificatDe { get; set; }
}
