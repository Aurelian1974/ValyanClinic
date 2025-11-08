namespace ValyanClinic.Application.Features.ProgramareManagement.DTOs;

/// <summary>
/// DTO pentru crearea unei programări noi.
/// Nu include ProgramareID (generat de DB) și audit fields (populate automat).
/// </summary>
public class CreateProgramareDto
{
    /// <summary>
    /// ID-ul pacientului (obligatoriu).
    /// </summary>
    public Guid PacientID { get; set; }

    /// <summary>
    /// ID-ul medicului (obligatoriu).
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data programării (obligatoriu, nu poate fi în trecut).
    /// </summary>
    public DateTime DataProgramare { get; set; }

    /// <summary>
    /// Ora de început (obligatoriu).
    /// </summary>
public TimeSpan OraInceput { get; set; }

    /// <summary>
    /// Ora de sfârșit (obligatoriu, trebuie să fie > OraInceput).
    /// </summary>
    public TimeSpan OraSfarsit { get; set; }

    /// <summary>
    /// Tipul programării (opțional).
    /// Valori posibile: ConsultatieInitiala, ControlPeriodic, Consultatie, Investigatie, Procedura, Urgenta, Telemedicina, LaDomiciliu.
    /// </summary>
    public string? TipProgramare { get; set; }

    /// <summary>
    /// Statusul programării (opțional, default: "Programata").
    /// </summary>
    public string Status { get; set; } = "Programata";

    /// <summary>
    /// Observații (opțional, max 1000 caractere).
    /// </summary>
    public string? Observatii { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care creează programarea (setat automat din context).
    /// </summary>
    public Guid CreatDe { get; set; }

    // ==================== HELPER PROPERTIES (pentru validare în UI) ====================

    /// <summary>
    /// Verifică dacă datele minime necesare sunt completate.
    /// </summary>
    public bool AreToateCampurileObligatorii =>
        PacientID != Guid.Empty &&
        DoctorID != Guid.Empty &&
        DataProgramare != default &&
        OraInceput != default &&
        OraSfarsit != default &&
        CreatDe != Guid.Empty;

/// <summary>
    /// Durata programării în minute (pentru validare).
    /// </summary>
    public int DurataMinute => (int)(OraSfarsit - OraInceput).TotalMinutes;

  /// <summary>
/// Data și ora completă de început (pentru conflict checking).
    /// </summary>
    public DateTime DataOraInceput => DataProgramare.Date + OraInceput;

    /// <summary>
    /// Data și ora completă de sfârșit (pentru conflict checking).
    /// </summary>
    public DateTime DataOraSfarsit => DataProgramare.Date + OraSfarsit;
}
