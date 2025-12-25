namespace ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorById;

public class UtilizatorDetailDto
{
    public Guid UtilizatorID { get; set; }
    public Guid? PersonalMedicalID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? RolID { get; set; }
    public string Rol { get; set; } = string.Empty; // RolDenumire mapped here
    public bool EsteActiv { get; set; }
    public DateTime DataCreare { get; set; }
    public DateTime? DataUltimaAutentificare { get; set; }
    public int NumarIncercariEsuate { get; set; }
    public DateTime? DataBlocare { get; set; }
    public string? TokenResetareParola { get; set; }
    public DateTime? DataExpirareToken { get; set; }

    // Audit
    public string? CreatDe { get; set; }
    public DateTime DataCrearii { get; set; }
    public string? ModificatDe { get; set; }
    public DateTime DataUltimeiModificari { get; set; }

    // Date PersonalMedical
    public string NumeCompletPersonalMedical { get; set; } = string.Empty;
    public string? Nume { get; set; }
    public string? Prenume { get; set; }
    public string? Specializare { get; set; }
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }
    public string? Telefon { get; set; }
    public string? EmailPersonalMedical { get; set; }

    // Computed properties
    public bool EsteBlocat => DataBlocare.HasValue;
    public bool TokenEsteValid =>
        !string.IsNullOrEmpty(TokenResetareParola) &&
   DataExpirareToken.HasValue &&
DataExpirareToken.Value > DateTime.Now;
}
