namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Diagnosticul unei Consultații
/// Relație 1:1 cu Consultatie
/// </summary>
public class ConsultatieDiagnostic
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== DIAGNOSTIC ====================
    public string? DiagnosticPozitiv { get; set; }
    public string? DiagnosticDiferential { get; set; }
    public string? DiagnosticEtiologic { get; set; }
    public string? CoduriICD10 { get; set; }
    public string? CoduriICD10Secundare { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
