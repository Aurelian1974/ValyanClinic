namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Diagnosticele Secundare ale unei Consultații
/// Relație 1:N cu Consultatie (max 10 diagnostice secundare)
/// 
/// Format afișare în Scrisoare Medicală:
///   1. E11.9 - Diabet zaharat tip 2 fără complicații
///   2. E78.0 - Hipercolesterolemie pură
/// </summary>
public class ConsultatieDiagnosticSecundar
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== ORDERING ====================
    /// <summary>
    /// Ordinea de afișare (1-10)
    /// </summary>
    public int OrdineAfisare { get; set; } = 1;

    // ==================== ICD10 ====================
    /// <summary>
    /// Codul ICD-10 pentru acest diagnostic secundar (ex: "E11.9")
    /// </summary>
    public string? CodICD10 { get; set; }
    
    /// <summary>
    /// Numele diagnosticului din catalogul ICD-10 (ex: "Diabet zaharat tip 2 fără complicații")
    /// </summary>
    public string? NumeDiagnostic { get; set; }

    // ==================== DESCRIERE ====================
    /// <summary>
    /// Descriere detaliată a diagnosticului (HTML din Rich Text Editor)
    /// Conține observații, severitate, evoluție etc.
    /// </summary>
    public string? Descriere { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
}
