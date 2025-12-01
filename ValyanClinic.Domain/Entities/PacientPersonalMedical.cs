namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru relatia Many-to-Many intre Pacienti si PersonalMedical
/// Permite unui pacient sa aiba mai multi doctori si unui doctor sa aiba mai multi pacienti
/// </summary>
public class PacientPersonalMedical
{
    /// <summary>
    /// Identificator unic al relatiei
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID-ul pacientului (FK catre Pacienti)
    /// </summary>
    public Guid PacientID { get; set; }

    /// <summary>
    /// ID-ul personalului medical (FK catre PersonalMedical)
    /// </summary>
    public Guid PersonalMedicalID { get; set; }

    /// <summary>
    /// Tipul relatiei: MedicPrimar, Specialist, MedicConsultant, MedicDeGarda, MedicFamilie, AsistentMedical
    /// </summary>
    public string? TipRelatie { get; set; }

    /// <summary>
    /// Data la care a fost stabilita relatia
    /// </summary>
    public DateTime DataAsocierii { get; set; }

    /// <summary>
    /// Data la care relatia a fost dezactivata (null daca este activa)
    /// </summary>
    public DateTime? DataDezactivarii { get; set; }

    /// <summary>
    /// Indica daca relatia este activa in prezent
    /// </summary>
    public bool EsteActiv { get; set; }

    /// <summary>
    /// Observatii despre relatie
    /// </summary>
    public string? Observatii { get; set; }

    /// <summary>
    /// Motivul asocierii (ex: "Pacient cu probleme cardiace")
    /// </summary>
    public string? Motiv { get; set; }

    // Audit fields
    public DateTime Data_Crearii { get; set; }
    public DateTime Data_Ultimei_Modificari { get; set; }
    public string? Creat_De { get; set; }
    public string? Modificat_De { get; set; }

    // Navigation properties
    public Pacient? Pacient { get; set; }
    public PersonalMedical? PersonalMedical { get; set; }

    // Computed property
    public int ZileDeAsociere => (int)(DateTime.Now - DataAsocierii).TotalDays;
}
