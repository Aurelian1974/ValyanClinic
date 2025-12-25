namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru Utilizator
/// Asociat cu PersonalMedical (relatie 1:1)
/// </summary>
public class Utilizator
{
    // Primary Key
    public Guid UtilizatorID { get; set; }

    // Foreign Key către PersonalMedical (relație 1:1) - nullable pentru superadmin
    public Guid? PersonalMedicalID { get; set; }

    // Credentiale
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    // Rol (FK către Roluri)
    public Guid? RolID { get; set; }
    
    // Denumirea rolului (mapată din JOIN)
    public string? RolDenumire { get; set; }
    
    // Navigation property pentru Rol
    public Rol? Rol { get; set; }

    // Status
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; }
    public DateTime? DataUltimaAutentificare { get; set; }

    // Securitate
    public int NumarIncercariEsuate { get; set; }
    public DateTime? DataBlocare { get; set; }
    public string? TokenResetareParola { get; set; }
    public DateTime? DataExpirareToken { get; set; }

    // Audit
    public string? CreatDe { get; set; }
    public DateTime DataCrearii { get; set; }
    public string? ModificatDe { get; set; }
    public DateTime DataUltimeiModificari { get; set; }

    // Navigation property
    public PersonalMedical? PersonalMedical { get; set; }

    // Computed properties
    public bool EsteBlocat => DataBlocare.HasValue;
    public bool TokenEsteValid =>
        !string.IsNullOrEmpty(TokenResetareParola) &&
        DataExpirareToken.HasValue &&
        DataExpirareToken.Value > DateTime.Now;
    public string NumeCompletPersonalMedical => PersonalMedical?.NumeComplet ?? string.Empty;
}
