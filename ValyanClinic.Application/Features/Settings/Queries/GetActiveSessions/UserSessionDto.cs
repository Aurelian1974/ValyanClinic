namespace ValyanClinic.Application.Features.Settings.Queries.GetActiveSessions;

public record UserSessionDto
{
    public Guid SessionID { get; init; }
    public Guid UtilizatorID { get; init; }
    public string SessionToken { get; init; } = string.Empty;
    public string AdresaIP { get; init; } = string.Empty;
    public string? UserAgent { get; init; }
    public string? Dispozitiv { get; init; }
    public DateTime DataCreare { get; init; }
    public DateTime DataUltimaActivitate { get; init; }
    public DateTime DataExpirare { get; init; }
    public bool EsteActiva { get; init; }
}
