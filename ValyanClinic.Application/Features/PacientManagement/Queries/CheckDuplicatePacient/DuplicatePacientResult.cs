namespace ValyanClinic.Application.Features.PacientManagement.Queries.CheckDuplicatePacient;

/// <summary>
/// Rezultatul verificării duplicatelor pentru un pacient
/// </summary>
public class DuplicatePacientResult
{
    /// <summary>
    /// Indică dacă există un duplicat
    /// </summary>
    public bool HasDuplicate { get; set; }

    /// <summary>
    /// Tipul de duplicat găsit
    /// </summary>
    public DuplicateType Type { get; set; } = DuplicateType.None;

    /// <summary>
    /// Mesajul de avertizare pentru utilizator
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// ID-ul pacientului duplicat (dacă există)
    /// </summary>
    public Guid? DuplicatePacientId { get; set; }

    /// <summary>
    /// Numele complet al pacientului duplicat
    /// </summary>
    public string? DuplicatePacientName { get; set; }

    /// <summary>
    /// Codul pacientului duplicat
    /// </summary>
    public string? DuplicatePacientCod { get; set; }
}

/// <summary>
/// Tipul de duplicat detectat
/// </summary>
public enum DuplicateType
{
    /// <summary>
    /// Nu există duplicat
    /// </summary>
    None,

    /// <summary>
    /// CNP identic găsit
    /// </summary>
    ExactCNP,

    /// <summary>
    /// Potrivire exactă pe Nume + Prenume + Data Nașterii
    /// </summary>
    ExactNameAndBirthDate,

    /// <summary>
    /// Potrivire parțială (nume similar)
    /// </summary>
    SimilarName
}
