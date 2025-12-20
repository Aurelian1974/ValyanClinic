namespace ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

/// <summary>
/// DTO pentru salvarea draft-ului consultatiei (auto-save cu campuri esentiale)
/// Folosit de SaveConsultatieDraftCommand
/// Contine doar campurile frecvent completate pentru a optimiza performance-ul auto-save-ului
/// </summary>
public class SaveConsultatieDraftDto
{
    // ==================== REQUIRED FIELDS ====================
    public Guid? ConsultatieID { get; set; } // Null pentru INSERT, valoare pentru UPDATE
    public Guid ProgramareID { get; set; }
    public Guid PacientID { get; set; }
    public Guid MedicID { get; set; }
    public DateTime DataConsultatie { get; set; } = DateTime.Today;
    public TimeSpan OraConsultatie { get; set; } = DateTime.Now.TimeOfDay;
    public string TipConsultatie { get; set; } = "Prima consultatie";

    // ==================== ESSENTIAL FIELDS (Most frequently filled) ====================
    
    /// <summary>
    /// Motivul prezentarii (campul cel mai important pentru draft)
    /// </summary>
    public string? MotivPrezentare { get; set; }

    /// <summary>
    /// Istoricul bolii actuale
    /// </summary>
    public string? IstoricBoalaActuala { get; set; }

    // ==================== SEMNE VITALE (Essential measurements) ====================
    
    /// <summary>
    /// Greutatea pacientului (kg)
    /// </summary>
    public decimal? Greutate { get; set; }

    /// <summary>
    /// Inaltimea pacientului (cm)
    /// </summary>
    public decimal? Inaltime { get; set; }

    /// <summary>
    /// Indicele de masa corporala (calculat automat)
    /// </summary>
    public decimal? IMC { get; set; }

    /// <summary>
    /// Temperatura corporala (°C)
    /// </summary>
    public decimal? Temperatura { get; set; }

    /// <summary>
    /// Tensiunea arteriala (format: 120/80)
    /// </summary>
    public string? TensiuneArteriala { get; set; }

    /// <summary>
    /// Pulsul (bpm)
    /// </summary>
    public int? Puls { get; set; }

    // ==================== DIAGNOSTIC (Primary fields) ====================
    
    /// <summary>
    /// Diagnosticul pozitiv principal
    /// </summary>
    public string? DiagnosticPozitiv { get; set; }

    /// <summary>
    /// Coduri ICD-10 (separate prin virgula)
    /// </summary>
    public string? CoduriICD10 { get; set; }

    // ==================== TRATAMENT (Primary field) ====================
    
    /// <summary>
    /// Tratamentul medicamentos recomandat
    /// </summary>
    public string? TratamentMedicamentos { get; set; }

    // ==================== OBSERVATII ====================
    
    /// <summary>
    /// Observatiile medicului (note draft)
    /// </summary>
    public string? ObservatiiMedic { get; set; }

    // ==================== AUDIT ====================
    public Guid CreatDeSauModificatDe { get; set; }
}
