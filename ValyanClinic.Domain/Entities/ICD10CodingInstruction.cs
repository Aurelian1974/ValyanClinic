namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru instrucțiunile de codificare ICD-10
/// "Code first", "Use additional code", "Code also"
/// Tabel: ICD10_CodingInstructions
/// </summary>
public class ICD10CodingInstruction
{
    // ==================== PRIMARY KEY ====================
    public Guid InstructionId { get; set; }

    // ==================== FOREIGN KEY ====================
    /// <summary>
    /// Referință la codul ICD-10 asociat
    /// </summary>
    public Guid ICD10_ID { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Tipul instrucțiunii: "codefirst", "useadditional", "codealso", "sequencefirst"
    /// </summary>
    public string InstructionType { get; set; } = string.Empty;

    // ==================== TEXTE ====================
    /// <summary>
    /// Textul instrucțiunii în engleză - REQUIRED (max 1000 caractere)
    /// </summary>
    public string InstructionTextEn { get; set; } = string.Empty;

    /// <summary>
    /// Textul instrucțiunii în română (max 1000 caractere)
    /// </summary>
    public string? InstructionTextRo { get; set; }

    /// <summary>
    /// Codul ICD-10 referențiat (dacă există) - max 20 caractere
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
    /// Codul ICD-10 asociat
    /// </summary>
    public virtual ICD10Code? Code { get; set; }

    // ==================== COMPUTED PROPERTIES ====================
    /// <summary>
    /// Textul de afișat (preferă română)
    /// </summary>
    public string DisplayText => InstructionTextRo ?? InstructionTextEn;

    /// <summary>
    /// Tipul instrucțiunii formatat
    /// </summary>
    public string InstructionTypeDisplay => InstructionType.ToLower() switch
    {
        "codefirst" => "⚡ Codifică primul",
        "useadditional" => "➕ Folosește cod adițional",
        "codealso" => "🔗 Codifică și",
        "sequencefirst" => "1️⃣ Secvență principală",
        _ => $"📋 {InstructionType}"
    };

    /// <summary>
    /// True dacă este o instrucțiune "Code first" (codul curent trebuie să fie principal)
    /// </summary>
    public bool IsCodeFirst => InstructionType.Equals("codefirst", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// True dacă necesită cod adițional
    /// </summary>
    public bool RequiresAdditionalCode => InstructionType.Equals("useadditional", StringComparison.OrdinalIgnoreCase);
}
