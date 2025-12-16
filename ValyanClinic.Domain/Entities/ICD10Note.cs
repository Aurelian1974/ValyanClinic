namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru notele ICD-10
/// Note generale, instrucțiuni de codificare, definiții
/// Tabel: ICD10_Notes
/// </summary>
public class ICD10Note
{
    // ==================== PRIMARY KEY ====================
    public Guid NoteId { get; set; }

    // ==================== FOREIGN KEY ====================
    /// <summary>
    /// Referință la codul ICD-10 asociat
    /// </summary>
    public Guid ICD10_ID { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Tipul notei: "general", "definition", "instruction", "seealso"
    /// </summary>
    public string NoteType { get; set; } = "general";

    // ==================== TEXTE ====================
    /// <summary>
    /// Textul notei în engleză - REQUIRED (max 2000 caractere)
    /// </summary>
    public string NoteTextEn { get; set; } = string.Empty;

    /// <summary>
    /// Textul notei în română (max 2000 caractere)
    /// </summary>
    public string? NoteTextRo { get; set; }

    // ==================== ORDINE ====================
    /// <summary>
    /// Ordinea de sortare
    /// </summary>
    public int SortOrder { get; set; } = 0;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ==================== NAVIGATION PROPERTIES ====================
    /// <summary>
    /// Codul ICD-10 asociat
    /// </summary>
    public virtual ICD10Code? Code { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    /// <summary>
    /// Textul de afișat (preferă română)
    /// </summary>
    public string DisplayText => NoteTextRo ?? NoteTextEn;

    /// <summary>
    /// Iconul pentru tipul de notă
    /// </summary>
    public string NoteIcon => NoteType.ToLower() switch
    {
        "general" => "📝",
        "definition" => "📖",
        "instruction" => "⚠️",
        "seealso" => "🔗",
        _ => "📋"
    };
}
