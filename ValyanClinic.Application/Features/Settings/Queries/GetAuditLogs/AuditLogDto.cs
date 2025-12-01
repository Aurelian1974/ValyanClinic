namespace ValyanClinic.Application.Features.Settings.Queries.GetAuditLogs;

public record AuditLogDto
{
    public Guid AuditID { get; init; }
    public Guid? UtilizatorID { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Actiune { get; init; } = string.Empty;
    public DateTime DataActiune { get; init; }
    public string? Entitate { get; init; }
    public string? EntitateID { get; init; }
    public string? ValoareVeche { get; init; }
    public string? ValoareNoua { get; init; }
    public string? AdresaIP { get; init; }
    public string? UserAgent { get; init; }
    public string? Dispozitiv { get; init; }
    public string StatusActiune { get; init; } = "Success";
    public string? DetaliiEroare { get; init; }
}
