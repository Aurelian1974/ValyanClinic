namespace ValyanClinic.Application.Features.RolManagement.Queries.GetPermisiuniDefinitii;

/// <summary>
/// DTO pentru o definiție de permisiune.
/// </summary>
public class PermisiuneDefinitieDto
{
    public Guid Id { get; set; }
    public string Cod { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public int OrdineAfisare { get; set; }
}

/// <summary>
/// DTO pentru o categorie de permisiuni cu lista de permisiuni.
/// </summary>
public class CategoriePermisiuniDto
{
    public string Categorie { get; set; } = string.Empty;
    public List<PermisiuneDefinitieDto> Permisiuni { get; set; } = new();
}
