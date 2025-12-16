namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru capitolele ICD-10 (I-XXII)
/// Tabel: ICD10_Chapters
/// </summary>
public class ICD10Chapter
{
    // ==================== PRIMARY KEY ====================
    public Guid ChapterId { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Numărul capitolului (1-22, corespunde I-XXII în notație romană)
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// Codul de început al intervalului (ex: A00)
    /// </summary>
    public string CodeRangeStart { get; set; } = string.Empty;

    /// <summary>
    /// Codul de sfârșit al intervalului (ex: B99)
    /// </summary>
    public string CodeRangeEnd { get; set; } = string.Empty;

    // ==================== DESCRIERI ====================
    /// <summary>
    /// Descrierea capitolului în română
    /// </summary>
    public string? DescriptionRo { get; set; }

    /// <summary>
    /// Descrierea capitolului în engleză
    /// </summary>
    public string DescriptionEn { get; set; } = string.Empty;

    // ==================== VERSIUNE ====================
    /// <summary>
    /// Versiunea ICD-10 (ex: "2026")
    /// </summary>
    public string Version { get; set; } = "2026";

    // ==================== STATUS ====================
    /// <summary>
    /// True = capitolul este activ
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    /// <summary>
    /// Secțiunile din acest capitol
    /// </summary>
    public virtual ICollection<ICD10Section> Sections { get; set; } = new List<ICD10Section>();

    /// <summary>
    /// Codurile din acest capitol
    /// </summary>
    public virtual ICollection<ICD10Code> Codes { get; set; } = new List<ICD10Code>();

    // ==================== COMPUTED PROPERTIES ====================
    /// <summary>
    /// Numărul capitolului în notație romană
    /// </summary>
    public string ChapterRoman => ChapterNumber switch
    {
        1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V",
        6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
        11 => "XI", 12 => "XII", 13 => "XIII", 14 => "XIV", 15 => "XV",
        16 => "XVI", 17 => "XVII", 18 => "XVIII", 19 => "XIX", 20 => "XX",
        21 => "XXI", 22 => "XXII",
        _ => ChapterNumber.ToString()
    };

    /// <summary>
    /// Text de afișare: "I (A00-B99) - Boli infecțioase"
    /// </summary>
    public string DisplayText => $"{ChapterRoman} ({CodeRangeStart}-{CodeRangeEnd}) - {DescriptionRo ?? DescriptionEn}";

    /// <summary>
    /// Intervalul de coduri: "A00-B99"
    /// </summary>
    public string CodeRange => $"{CodeRangeStart}-{CodeRangeEnd}";
}
