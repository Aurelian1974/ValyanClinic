namespace ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareList;

public class SpecializareListDto
{
    public Guid Id { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; }
    public DateTime DataCrearii { get; set; }
    public DateTime DataUltimeiModificari { get; set; }
    public string? CreatDe { get; set; }
    public string? ModificatDe { get; set; }
}
