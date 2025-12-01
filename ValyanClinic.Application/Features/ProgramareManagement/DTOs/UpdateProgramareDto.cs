namespace ValyanClinic.Application.Features.ProgramareManagement.DTOs;

/// <summary>
/// DTO pentru actualizarea unei programări existente.
/// Include ProgramareID și exclude CreatDe/DataCreare (immutable).
/// </summary>
public class UpdateProgramareDto
{
    /// <summary>
    /// ID-ul programării de actualizat (obligatoriu).
    /// </summary>
    public Guid ProgramareID { get; set; }

    /// <summary>
    /// ID-ul pacientului (obligatoriu).
    /// </summary>
    public Guid PacientID { get; set; }

    /// <summary>
    /// ID-ul medicului (obligatoriu).
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data programării (obligatoriu).
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
    /// </summary>
    public string? TipProgramare { get; set; }

    /// <summary>
    /// Statusul programării (obligatoriu).
    /// Poate fi actualizat pentru a reflecta evoluția programării.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Observații (opțional, max 1000 caractere).
    /// </summary>
    public string? Observatii { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care modifică programarea (setat automat din context).
    /// </summary>
    public Guid ModificatDe { get; set; }

    // ==================== HELPER PROPERTIES ====================

    /// <summary>
    /// Verifică dacă toate câmpurile obligatorii sunt completate.
    /// </summary>
    public bool AreToateCampurileObligatorii =>
  ProgramareID != Guid.Empty &&
 PacientID != Guid.Empty &&
        DoctorID != Guid.Empty &&
        DataProgramare != default &&
      OraInceput != default &&
        OraSfarsit != default &&
        !string.IsNullOrEmpty(Status) &&
        ModificatDe != Guid.Empty;

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
