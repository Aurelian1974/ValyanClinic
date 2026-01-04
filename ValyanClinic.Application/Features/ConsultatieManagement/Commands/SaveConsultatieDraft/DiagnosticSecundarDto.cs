namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft;

/// <summary>
/// DTO pentru un diagnostic secundar în SaveConsultatieDraftCommand
/// Format afișare în Scrisoare Medicală:
///   1. E11.9 - Diabet zaharat tip 2 fără complicații
/// </summary>
public class DiagnosticSecundarDto
{
    /// <summary>Ordinea de afișare (1-10)</summary>
    public int OrdineAfisare { get; set; }
    
    /// <summary>Codul ICD-10 (ex: "E11.9")</summary>
    public string? CodICD10 { get; set; }
    
    /// <summary>Numele diagnosticului din catalog ICD-10</summary>
    public string? NumeDiagnostic { get; set; }
    
    /// <summary>Descriere detaliată (HTML din RTE)</summary>
    public string? Descriere { get; set; }
}
