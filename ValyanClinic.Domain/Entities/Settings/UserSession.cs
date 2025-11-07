namespace ValyanClinic.Domain.Entities.Settings;

/// <summary>
/// Entitate pentru sesiuni active utilizatori
/// Tabela: UserSessions
/// </summary>
public class UserSession
{
    public Guid SessionID { get; set; }
    public Guid UtilizatorID { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string AdresaIP { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? Dispozitiv { get; set; }
    public DateTime DataCreare { get; set; }
    public DateTime DataUltimaActivitate { get; set; }
    public DateTime DataExpirare { get; set; }
    public bool EsteActiva { get; set; } = true;
}
