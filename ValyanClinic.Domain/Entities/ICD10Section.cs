namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru secțiunile ICD-10 (grupări în cadrul capitolelor)
/// Tabel: ICD10_Sections
/// </summary>
public class ICD10Section
{
    // ==================== PRIMARY KEY ====================
    public Guid SectionId { get; set; }

    // ==================== FOREIGN KEY ====================
    /// <summary>
    /// Referință la capitolul părinte
    /// </summary>
    public Guid ChapterId { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Codul secțiunii (ex: "A00-A09") - max 20 caractere
    /// </summary>
    public string SectionCode { get; set; } = string.Empty;

    /// <summary>
    /// Codul de început al intervalului (ex: A00)
    /// </summary>
    public string CodeRangeStart { get; set; } = string.Empty;

    /// <summary>
    /// Codul de sfârșit al intervalului (ex: A09)
    /// </summary>
    public string CodeRangeEnd { get; set; } = string.Empty;

    // ==================== DESCRIERI ====================
    /// <summary>
    /// Descrierea secțiunii în română
    /// </summary>
    public string? DescriptionRo { get; set; }

    /// <summary>
    /// Descrierea secțiunii în engleză
    /// </summary>
    public string DescriptionEn { get; set; } = string.Empty;

    // ==================== ORDINE ====================
    /// <summary>
    /// Ordinea de sortare în cadrul capitolului
    /// </summary>
    public int SortOrder { get; set; } = 0;

    // ==================== STATUS ====================
    /// <summary>
    /// True = secțiunea este activă
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    /// <summary>
    /// Capitolul părinte
    /// </summary>
    public virtual ICD10Chapter? Chapter { get; set; }

    /// <summary>
    /// Codurile din această secțiune
    /// </summary>
    public virtual ICollection<ICD10Code> Codes { get; set; } = new List<ICD10Code>();

    // ==================== COMPUTED PROPERTIES ====================
    /// <summary>
    /// Intervalul de coduri: "A00-A09"
    /// </summary>
    public string CodeRange => $"{CodeRangeStart}-{CodeRangeEnd}";

    /// <summary>
    /// Text de afișare: "(A00-A09) - Boli infecțioase intestinale"
    /// </summary>
    public string DisplayText => $"({CodeRange}) - {DescriptionRo ?? DescriptionEn}";
}
