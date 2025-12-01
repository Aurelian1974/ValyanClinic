namespace ValyanClinic.Domain.Entities.Settings;

/// <summary>
/// Entitate pentru audit trail
/// Tabela: Audit_Log
/// </summary>
public class AuditLog
{
    public Guid AuditID { get; set; }
    public Guid? UtilizatorID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Actiune { get; set; } = string.Empty;
    public DateTime DataActiune { get; set; }
    public string? Entitate { get; set; }
    public string? EntitateID { get; set; }
    public string? ValoareVeche { get; set; }
    public string? ValoareNoua { get; set; }
    public string? AdresaIP { get; set; }
    public string? UserAgent { get; set; }
    public string? Dispozitiv { get; set; }
    public string StatusActiune { get; set; } = "Success";
    public string? DetaliiEroare { get; set; }
}
