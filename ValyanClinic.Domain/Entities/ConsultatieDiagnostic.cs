namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru Diagnosticul Principal al unei Consultații
/// Relație 1:1 cu Consultatie
/// 
/// Format afișare în Scrisoare Medicală:
///   Diagnostic principal: I10 - Hipertensiune arterială esențială (primară)
///   + Descriere detaliată din RTE
/// </summary>
public class ConsultatieDiagnostic
{
    // ==================== PRIMARY KEY ====================
    public Guid Id { get; set; }

    // ==================== FOREIGN KEY ====================
    public Guid ConsultatieID { get; set; }

    // ==================== DIAGNOSTIC PRINCIPAL ====================
    
    /// <summary>
    /// Codul ICD-10 pentru diagnosticul principal (ex: "I10")
    /// </summary>
    public string? CodICD10Principal { get; set; }
    
    /// <summary>
    /// Numele diagnosticului principal din catalogul ICD-10
    /// (ex: "Hipertensiune arterială esențială (primară)")
    /// </summary>
    public string? NumeDiagnosticPrincipal { get; set; }
    
    /// <summary>
    /// Descriere detaliată a diagnosticului principal (HTML din Rich Text Editor)
    /// </summary>
    public string? DescriereDetaliataPrincipal { get; set; }

    // ==================== LEGACY FIELDS (kept for backwards compatibility) ====================
    /// <summary>Legacy: Text diagnostic pozitiv (folosit pentru fallback)</summary>
    public string? DiagnosticPozitiv { get; set; }
    /// <summary>Legacy: Cod ICD-10 simplu (va fi înlocuit de CodICD10Principal)</summary>
    public string? CoduriICD10 { get; set; }

    // ==================== AUDIT ====================
    public DateTime DataCreare { get; set; }
    public Guid CreatDe { get; set; }
    public DateTime? DataUltimeiModificari { get; set; }
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    public virtual Consultatie? Consultatie { get; set; }
    
    /// <summary>
    /// Colecție de diagnostice secundare (1:N, max 10)
    /// </summary>
    public virtual ICollection<ConsultatieDiagnosticSecundar> DiagnosticeSecundare { get; set; } = new List<ConsultatieDiagnosticSecundar>();
}
