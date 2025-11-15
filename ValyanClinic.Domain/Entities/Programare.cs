namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entity pentru programări medicale.
/// Reprezintă o programare între un pacient și un medic.
/// </summary>
public class Programare
{
    /// <summary>
    /// Identificator unic al programării (Primary Key).
    /// </summary>
    public Guid ProgramareID { get; set; }

    /// <summary>
    /// ID-ul pacientului programat (Foreign Key către Pacienti.Id).
    /// </summary>
    public Guid PacientID { get; set; }

    /// <summary>
    /// ID-ul medicului care va efectua consultația (Foreign Key către PersonalMedical.PersonalID).
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data programării (fără componentă de timp).
    /// </summary>
    public DateTime DataProgramare { get; set; }

    /// <summary>
    /// Ora de început a programării.
    /// </summary>
    public TimeSpan OraInceput { get; set; }

    /// <summary>
    /// Ora de sfârșit a programării.
    /// </summary>
    public TimeSpan OraSfarsit { get; set; }

    /// <summary>
    /// Tipul programării (ex: ConsultatieInitiala, ControlPeriodic, Investigatie, etc.).
    /// </summary>
    public string? TipProgramare { get; set; }

    /// <summary>
    /// Statusul programării (ex: Programata, Confirmata, CheckedIn, Finalizata, Anulata, NoShow).
    /// Valoare implicită: "Programata".
    /// </summary>
  public string Status { get; set; } = "Programata";

    /// <summary>
    /// Observații sau note suplimentare despre programare.
    /// </summary>
    public string? Observatii { get; set; }

    // ==================== AUDIT FIELDS ====================

    /// <summary>
    /// Data și ora creării programării.
    /// </summary>
    public DateTime DataCreare { get; set; }

    /// <summary>
    /// ID-ul utilizatorului (PersonalMedical) care a creat programarea.
    /// </summary>
    public Guid CreatDe { get; set; }

    /// <summary>
    /// Data și ora ultimei modificări a programării.
    /// </summary>
    public DateTime? DataUltimeiModificari { get; set; }

    /// <summary>
    /// ID-ul utilizatorului (PersonalMedical) care a modificat ultima dată programarea.
/// </summary>
    public Guid? ModificatDe { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    // Aceste proprietăți sunt populate din JOIN-uri ale stored procedures
    // și nu sunt mapate direct în baza de date (sunt read-only pentru afișare).

    /// <summary>
    /// Numele complet al pacientului (Nume + Prenume).
    /// Populat din JOIN cu tabelul Pacienti.
    /// </summary>
    public string? PacientNumeComplet { get; set; }

    /// <summary>
    /// Numărul de telefon al pacientului.
/// Populat din JOIN cu tabelul Pacienti.
    /// </summary>
    public string? PacientTelefon { get; set; }

    /// <summary>
    /// Adresa de email a pacientului.
    /// Populat din JOIN cu tabelul Pacienti.
    /// </summary>
    public string? PacientEmail { get; set; }

    /// <summary>
    /// CNP-ul pacientului.
    /// Populat din JOIN cu tabelul Pacienti.
    /// </summary>
    public string? PacientCNP { get; set; }

    /// <summary>
    /// Numele complet al medicului (Nume + Prenume).
    /// Populat din JOIN cu tabelul PersonalMedical.
    /// </summary>
    public string? DoctorNumeComplet { get; set; }

 /// <summary>
    /// Specializarea medicului.
    /// Populat din JOIN cu tabelul PersonalMedical.
    /// </summary>
    public string? DoctorSpecializare { get; set; }

    /// <summary>
    /// Numărul de telefon al medicului.
 /// Populat din JOIN cu tabelul PersonalMedical.
    /// </summary>
    public string? DoctorTelefon { get; set; }

    /// <summary>
    /// ✅ NEW - Adresa de email a medicului.
    /// Populat din JOIN cu tabelul PersonalMedical.
    /// Utilizat pentru trimiterea notificărilor automate.
    /// </summary>
    public string? DoctorEmail { get; set; }

    /// <summary>
 /// Numele complet al utilizatorului care a creat programarea.
    /// Populat din JOIN cu tabelul PersonalMedical.
    /// </summary>
    public string? CreatDeNumeComplet { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Durata programării în minute.
    /// Calculată ca diferență între OraSfarsit și OraInceput.
    /// </summary>
    public int DurataMinute => (int)(OraSfarsit - OraInceput).TotalMinutes;

    /// <summary>
    /// Data și ora completă de început a programării.
    /// Combină DataProgramare cu OraInceput.
/// </summary>
    public DateTime DataOraInceput => DataProgramare.Date + OraInceput;

    /// <summary>
    /// Data și ora completă de sfârșit a programării.
    /// Combină DataProgramare cu OraSfarsit.
    /// </summary>
    public DateTime DataOraSfarsit => DataProgramare.Date + OraSfarsit;

    /// <summary>
    /// Verifică dacă programarea este în viitor (după data curentă).
    /// </summary>
    public bool EsteViitoare => DataOraInceput > DateTime.Now;

    /// <summary>
    /// Verifică dacă programarea este în trecut (înainte de data curentă).
    /// </summary>
    public bool EsteTrecuta => DataOraSfarsit < DateTime.Now;

    /// <summary>
    /// Verifică dacă programarea este în desfășurare (între DataOraInceput și DataOraSfarsit).
    /// </summary>
    public bool EsteInDesfasurare
    {
        get
    {
            var now = DateTime.Now;
 return now >= DataOraInceput && now <= DataOraSfarsit;
        }
    }
}
