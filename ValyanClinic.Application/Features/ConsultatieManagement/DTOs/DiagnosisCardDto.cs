namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru card-ul de diagnostic în pagina de consultații
/// Folosit pentru afișarea și editarea diagnosticelor ICD-10
/// </summary>
public class DiagnosisCardDto
{
    /// <summary>
    /// ID unic pentru tracking în UI (nu persistat)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tipul diagnosticului: "Principal" sau "Secundar"
    /// </summary>
    public string Type { get; set; } = "Secundar";

    /// <summary>
    /// Codul ICD-10 (ex: J06.9, I10, E11.9)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Denumirea diagnosticului
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detalii suplimentare despre diagnostic
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Verifică dacă acest card este diagnosticul principal
    /// </summary>
    public bool IsPrincipal => Type == "Principal";

    /// <summary>
    /// Verifică dacă card-ul are date valide
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Code) || !string.IsNullOrWhiteSpace(Name);
}
