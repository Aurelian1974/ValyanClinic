namespace ValyanClinic.Application.Features.RolManagement.Queries.GetRolById;

/// <summary>
/// DTO detaliat pentru un rol.
/// Include lista de permisiuni asociate.
/// </summary>
public class RolDetailDto
{
    public Guid Id { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public bool EsteActiv { get; set; }
    public bool EsteSistem { get; set; }
    public int OrdineAfisare { get; set; }
    public DateTime DataCrearii { get; set; }
    public DateTime DataUltimeiModificari { get; set; }
    public string? CreatDe { get; set; }
    public string? ModificatDe { get; set; }
    
    /// <summary>
    /// Lista de coduri permisiuni acordate acestui rol.
    /// </summary>
    public List<string> Permisiuni { get; set; } = new();
}
