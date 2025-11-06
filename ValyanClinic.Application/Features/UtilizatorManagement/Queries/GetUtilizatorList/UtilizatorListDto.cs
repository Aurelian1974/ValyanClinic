namespace ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorList;

public class UtilizatorListDto
{
    public Guid UtilizatorID { get; set; }
    public Guid PersonalMedicalID { get; set; }
    public string Username { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
 public string Rol { get; set; } = string.Empty;
    public bool EsteActiv { get; set; }
    public DateTime DataCreare { get; set; }
    public DateTime? DataUltimaAutentificare { get; set; }
    public int NumarIncercariEsuate { get; set; }
    public DateTime? DataBlocare { get; set; }
    
    // Date PersonalMedical
    public string NumeCompletPersonalMedical { get; set; } = string.Empty;
    public string? Specializare { get; set; }
    public string? Departament { get; set; }
    public string? Telefon { get; set; }
    
    // Computed properties
    public bool EsteBlocat => DataBlocare.HasValue;
    public string StatusText => EsteBlocat ? "Blocat" : (EsteActiv ? "Activ" : "Inactiv");
}
