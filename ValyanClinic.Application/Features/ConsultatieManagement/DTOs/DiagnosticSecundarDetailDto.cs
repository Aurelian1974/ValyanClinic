namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru un diagnostic secundar în ConsulatieDetailDto
/// Folosit la încărcarea datelor pentru afișare/editare
/// 
/// Format afișare în Scrisoare Medicală:
///   1. E11.9 - Diabet zaharat tip 2 fără complicații
/// </summary>
public class DiagnosticSecundarDetailDto
{
    /// <summary>ID-ul diagnosticului secundar</summary>
    public Guid Id { get; set; }
    
    /// <summary>Ordinea de afișare (1-10)</summary>
    public int OrdineAfisare { get; set; }
    
    /// <summary>Codul ICD-10 (ex: "E11.9")</summary>
    public string? CodICD10 { get; set; }
    
    /// <summary>Numele diagnosticului din catalog ICD-10</summary>
    public string? NumeDiagnostic { get; set; }
    
    /// <summary>Descriere detaliată (HTML din RTE)</summary>
    public string? Descriere { get; set; }
}
