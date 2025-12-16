namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru excluderile ICD-10
/// Indică ce coduri NU se folosesc împreună sau ce se codifică altfel
/// Tabel: ICD10_Exclusions
/// </summary>
public class ICD10Exclusion
{
    // ==================== PRIMARY KEY ====================
    public Guid ExclusionId { get; set; }

    // ==================== FOREIGN KEY ====================
    /// <summary>
    /// Referință la codul ICD-10 sursă
    /// </summary>
    public Guid ICD10_ID { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Tipul excluderii: "excludes1" (nu se folosesc împreună), "excludes2" (codificare diferită)
    /// </summary>
    public string ExclusionType { get; set; } = string.Empty;

    // ==================== TEXTE ====================
    /// <summary>
    /// Textul notei în engleză - REQUIRED
    /// </summary>
    public string NoteTextEn { get; set; } = string.Empty;

    /// <summary>
    /// Textul notei în română
    /// </summary>
    public string? NoteTextRo { get; set; }

    /// <summary>
    /// Codul ICD-10 referențiat (unde trebuie codificat) - max 20 caractere
    /// </summary>
    public string? ReferencedCode { get; set; }

    // ==================== ORDINE ====================
    /// <summary>
    /// Ordinea de sortare
    /// </summary>
    public int SortOrder { get; set; } = 0;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ==================== NAVIGATION PROPERTIES ====================
    /// <summary>
    /// Codul ICD-10 sursă
    /// </summary>
    public virtual ICD10Code? Code { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    /// <summary>
    /// Textul de afișat (preferă română)
    /// </summary>
    public string DisplayText => NoteTextRo ?? NoteTextEn;

    /// <summary>
    /// True dacă este Excludes1 (nu se folosesc niciodată împreună)
    /// </summary>
    public bool IsExcludes1 => ExclusionType.Equals("excludes1", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// True dacă este Excludes2 (se pot folosi împreună dacă este documentat)
    /// </summary>
    public bool IsExcludes2 => ExclusionType.Equals("excludes2", StringComparison.OrdinalIgnoreCase);
}
