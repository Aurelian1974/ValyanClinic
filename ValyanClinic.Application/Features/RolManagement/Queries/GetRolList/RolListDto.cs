namespace ValyanClinic.Application.Features.RolManagement.Queries.GetRolList;

/// <summary>
/// DTO pentru lista de roluri.
/// Include informații suplimentare pentru afișare în grid.
/// </summary>
public class RolListDto
{
    public Guid Id { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; }
    public bool EsteSistem { get; set; }
    public int OrdineAfisare { get; set; }
    public int NumarPermisiuni { get; set; }
    public int NumarUtilizatori { get; set; }
    public DateTime DataCrearii { get; set; }
    public DateTime DataUltimeiModificari { get; set; }
    public string? CreatDe { get; set; }
    public string? ModificatDe { get; set; }
}
