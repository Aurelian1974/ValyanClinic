namespace ValyanClinic.Application.Features.ProgramareManagement.DTOs;

/// <summary>
/// DTO pentru afișarea detaliilor complete ale unei programări (view/edit).
/// Include toate informațiile disponibile.
/// </summary>
public class ProgramareDetailDto
{
    /// <summary>
    /// ID-ul unic al programării.
    /// </summary>
    public Guid ProgramareID { get; set; }

    /// <summary>
  /// ID-ul pacientului.
    /// </summary>
    public Guid PacientID { get; set; }

    /// <summary>
    /// ID-ul medicului.
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data programării.
    /// </summary>
    public DateTime DataProgramare { get; set; }

    /// <summary>
    /// Ora de început.
    /// </summary>
    public TimeSpan OraInceput { get; set; }

    /// <summary>
    /// Ora de sfârșit.
    /// </summary>
    public TimeSpan OraSfarsit { get; set; }

    /// <summary>
    /// Tipul programării.
    /// </summary>
    public string? TipProgramare { get; set; }

    /// <summary>
    /// Statusul programării.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
 /// Observații complete.
 /// </summary>
    public string? Observatii { get; set; }

    // ==================== INFORMAȚII PACIENT (COMPLETE) ====================

    /// <summary>
    /// Numele complet al pacientului.
    /// </summary>
    public string? PacientNumeComplet { get; set; }

    /// <summary>
    /// Telefonul pacientului.
 /// </summary>
    public string? PacientTelefon { get; set; }

    /// <summary>
    /// Email-ul pacientului.
    /// </summary>
 public string? PacientEmail { get; set; }

    /// <summary>
    /// CNP-ul pacientului.
    /// </summary>
    public string? PacientCNP { get; set; }

    // ==================== INFORMAȚII DOCTOR (COMPLETE) ====================

    /// <summary>
    /// Numele complet al medicului.
    /// </summary>
    public string? DoctorNumeComplet { get; set; }

    /// <summary>
    /// Specializarea medicului.
 /// </summary>
    public string? DoctorSpecializare { get; set; }

    /// <summary>
  /// Telefonul medicului.
    /// </summary>
    public string? DoctorTelefon { get; set; }

    // ==================== AUDIT INFORMATION (COMPLETE) ====================

    /// <summary>
    /// Data și ora creării.
    /// </summary>
    public DateTime DataCreare { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care a creat programarea.
    /// </summary>
    public Guid CreatDe { get; set; }

    /// <summary>
    /// Numele complet al utilizatorului care a creat programarea.
    /// </summary>
    public string? CreatDeNumeComplet { get; set; }

    /// <summary>
    /// Data și ora ultimei modificări.
    /// </summary>
    public DateTime? DataUltimeiModificari { get; set; }

    /// <summary>
 /// ID-ul utilizatorului care a modificat ultima dată programarea.
    /// </summary>
    public Guid? ModificatDe { get; set; }

 // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Durata programării în minute.
    /// </summary>
    public int DurataMinute => (int)(OraSfarsit - OraInceput).TotalMinutes;

    /// <summary>
 /// Data și ora completă de început.
    /// </summary>
    public DateTime DataOraInceput => DataProgramare.Date + OraInceput;

    /// <summary>
  /// Data și ora completă de sfârșit.
    /// </summary>
    public DateTime DataOraSfarsit => DataProgramare.Date + OraSfarsit;

    /// <summary>
    /// Verifică dacă programarea este în viitor.
    /// </summary>
    public bool EsteViitoare => DataOraInceput > DateTime.Now;

    /// <summary>
    /// Verifică dacă programarea este în trecut.
    /// </summary>
    public bool EsteTrecuta => DataOraSfarsit < DateTime.Now;

    /// <summary>
    /// Verifică dacă programarea este în desfășurare.
    /// </summary>
    public bool EsteInDesfasurare
 {
        get
      {
        var now = DateTime.Now;
            return now >= DataOraInceput && now <= DataOraSfarsit;
        }
    }

    /// <summary>
    /// Verifică dacă programarea poate fi editată (doar statusurile Programata și Confirmata).
    /// </summary>
    public bool PoateFiEditata => Status is "Programata" or "Confirmata";

    /// <summary>
    /// Verifică dacă programarea poate fi anulată.
    /// </summary>
    public bool PoateFiAnulata => Status is "Programata" or "Confirmata" or "CheckedIn";

    /// <summary>
    /// String format pentru afișare: "DD.MM.YYYY HH:mm - HH:mm (Durata: XX min)".
    /// </summary>
    public string DataOraFormatata => 
        $"{DataProgramare:dd.MM.yyyy} {OraInceput:hh\\:mm} - {OraSfarsit:hh\\:mm} (Durata: {DurataMinute} min)";
}
