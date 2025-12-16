namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru termenii de incluziune ICD-10
/// Reprezintă sinonime și variante care se codifică sub un anumit cod
/// Tabel: ICD10_InclusionTerms
/// </summary>
public class ICD10InclusionTerm
{
    // ==================== PRIMARY KEY ====================
    public Guid InclusionId { get; set; }

    // ==================== FOREIGN KEY ====================
    /// <summary>
    /// Referință la codul ICD-10 părinte
    /// </summary>
    public Guid ICD10_ID { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Tipul termenului: "includes", "synonym", "example"
    /// </summary>
    public string TermType { get; set; } = string.Empty;

    // ==================== TEXTE ====================
    /// <summary>
    /// Textul termenului în engleză - REQUIRED
    /// </summary>
    public string TermTextEn { get; set; } = string.Empty;

    /// <summary>
    /// Textul termenului în română
    /// </summary>
    public string? TermTextRo { get; set; }

    // ==================== ORDINE ====================
    /// <summary>
    /// Ordinea de sortare
    /// </summary>
    public int SortOrder { get; set; } = 0;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ==================== NAVIGATION PROPERTIES ====================
    /// <summary>
    /// Codul ICD-10 părinte
    /// </summary>
    public virtual ICD10Code? Code { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    /// <summary>
    /// Textul de afișat (preferă română)
    /// </summary>
    public string DisplayText => TermTextRo ?? TermTextEn;
}
