namespace ValyanClinic.Domain.Entities.Settings;

/// <summary>
/// Entitate pentru istoric parole utilizatori
/// Tabela: PasswordHistory
/// </summary>
public class PasswordHistory
{
    public Guid HistoricID { get; set; }
    public Guid UtilizatorID { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
public string Salt { get; set; } = string.Empty;
    public DateTime DataCrearii { get; set; }
    public string? CreatDe { get; set; }
}
