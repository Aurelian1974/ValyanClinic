namespace ValyanClinic.Application.Features.ProgramareManagement.DTOs;

/// <summary>
/// DTO pentru afișarea programărilor în listă/grid (view optimizat pentru performanță).
/// Conține doar câmpurile esențiale pentru afișare.
/// </summary>
public class ProgramareListDto
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
    /// Data programării (format: yyyy-MM-dd).
    /// </summary>
    public DateTime DataProgramare { get; set; }

    /// <summary>
    /// Ora de început (format: HH:mm).
    /// </summary>
    public TimeSpan OraInceput { get; set; }

    /// <summary>
    /// Ora de sfârșit (format: HH:mm).
    /// </summary>
    public TimeSpan OraSfarsit { get; set; }

    /// <summary>
    /// Durata programării în minute (calculată).
    /// </summary>
    public int DurataMinute => (int)(OraSfarsit - OraInceput).TotalMinutes;

    /// <summary>
    /// Tipul programării (ConsultatieInitiala, ControlPeriodic, etc.).
    /// </summary>
    public string? TipProgramare { get; set; }

    /// <summary>
    /// Statusul programării (Programata, Confirmata, CheckedIn, Finalizata, Anulata, NoShow).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Observații (scurte, max 200 caractere pentru listă).
    /// </summary>
    public string? ObservatiiScurte => Observatii?.Length > 200
        ? Observatii.Substring(0, 197) + "..."
        : Observatii;

    /// <summary>
    /// Observații complete.
    /// </summary>
    public string? Observatii { get; set; }

    // ==================== INFORMAȚII PACIENT ====================

    /// <summary>
    /// Numele complet al pacientului (Nume Prenume).
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

    /// <summary>
    /// ✅ NEW - Vârsta pacientului calculată.
    /// </summary>
    public int PacientVarsta { get; set; }

    /// <summary>
    /// ✅ NEW - Motiv programare (ex: Control periodic, Dureri abdominale).
    /// </summary>
    public string? Motiv { get; set; }

    /// <summary>
    /// ✅ NEW - Diagnostic (dacă există din consultații anterioare).
    /// </summary>
    public string? Diagnostic { get; set; }

    /// <summary>
    /// ✅ NEW - Tratament actual pacient (pentru context medical).
    /// </summary>
    public string? TratamentActual { get; set; }

    // ==================== INFORMAȚII DOCTOR ====================

    /// <summary>
    /// Numele complet al medicului (Nume Prenume).
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

    /// <summary>
    /// ✅ NEW - Email-ul medicului (pentru trimitere notificări).
    /// </summary>
    public string? DoctorEmail { get; set; }

    // ==================== AUDIT INFO ====================

    /// <summary>
    /// Data și ora creării programării.
    /// </summary>
    public DateTime DataCreare { get; set; }

    /// <summary>
    /// Numele complet al utilizatorului care a creat programarea.
    /// </summary>
    public string? CreatDeNumeComplet { get; set; }

    /// <summary>
    /// Data și ora ultimei modificări.
    /// </summary>
    public DateTime? DataUltimeiModificari { get; set; }

    // ==================== COMPUTED PROPERTIES (pentru UI) ====================

    /// <summary>
    /// Data și ora completă de început (pentru sortare și comparare).
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
    /// Verifică dacă programarea este în desfășurare (NOW între început și sfârșit).
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
    /// String format pentru afișare în UI: "DD.MM.YYYY HH:mm - HH:mm".
    /// </summary>
    public string DataOraFormatata =>
        $"{DataProgramare:dd.MM.yyyy} {OraInceput:hh\\:mm} - {OraSfarsit:hh\\:mm}";

    /// <summary>
    /// Culoare pentru badge status (Bootstrap colors).
    /// </summary>
    public string StatusColor => Status?.ToLower() switch
    {
        "programata" => "secondary",
        "confirmata" => "info",
        "checkedin" => "primary",
        "inconsultatie" => "warning",
        "finalizata" => "success",
        "anulata" => "danger",
        "noshow" => "dark",
        _ => "secondary"
    };

    /// <summary>
    /// Culoare pentru badge tip programare (Bootstrap colors).
    /// </summary>
    public string TipProgramareColor => TipProgramare?.ToLower() switch
    {
        "consultatieinitial" => "primary",
        "controlperiodic" => "info",
        "consultatie" => "secondary",
        "investigatie" => "warning",
        "procedura" => "success",
        "urgenta" => "danger",
        "telemedicina" => "dark",
        "ladomiciliu" => "purple",
        _ => "secondary"
    };
}
