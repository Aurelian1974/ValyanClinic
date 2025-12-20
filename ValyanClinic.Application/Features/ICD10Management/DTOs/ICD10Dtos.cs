namespace ValyanClinic.Application.Features.ICD10Management.DTOs;

/// <summary>
/// DTO pentru rezultatele cautarii ICD-10
/// Mapează rezultatul din sp_ICD10_Search
/// </summary>
public class ICD10SearchResultDto
{
    public Guid ICD10_ID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullCode { get; set; } = string.Empty;
    
    /// <summary>Descriere scurtă (prioritate română, fallback engleză)</summary>
    public string ShortDescription { get; set; } = string.Empty;
    
    /// <summary>Descriere lungă (opțional)</summary>
    public string? LongDescription { get; set; }
    
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public bool IsCommon { get; set; }
    public bool IsLeafNode { get; set; }
    public bool IsBillable { get; set; }
    public bool IsTranslated { get; set; }
    
    /// <summary>Număr capitol (ex: 9 pentru Diseases of the circulatory system)</summary>
    public int? ChapterNumber { get; set; }
    
    /// <summary>Descriere capitol (engleză)</summary>
    public string? ChapterDescription { get; set; }
    
    /// <summary>Scor de relevanță calculat de sp_ICD10_Search (0-100)</summary>
    public int RelevanceScore { get; set; }

    /// <summary>✅ NOU: Flag pentru favorite (populat runtime din ICD10_Favorites)</summary>
    public bool IsFavorite { get; set; }

    // Display properties
public string DisplayText => $"{Code} - {ShortDescription}";
    
    public string CategoryIcon => Category switch
    {
        "Cardiovascular" => "❤️",
        "Endocrin" => "🔬",
  "Respirator" => "🫁",
        "Digestiv" => "🍽️",
        "Nervos" => "🧠",
        "Simptome" => "⚕️",
        _ => "📋"
    };

    /// <summary>Badge pentru traducere</summary>
    public string TranslationBadge => !IsTranslated ? "⚠️ EN" : "🇷🇴 RO";
    
    /// <summary>Indicator cod frecvent</summary>
 public string CommonBadge => IsCommon ? "⭐ Frecvent" : "";
}

/// <summary>
/// DTO pentru detalii complete cod ICD-10
/// Folosit pentru afișare completă într-un modal/dialog
/// </summary>
public class ICD10DetailDto
{
    public Guid ICD10_ID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullCode { get; set; } = string.Empty;
    
    // Descrieri în ambele limbi
    public string? ShortDescriptionRo { get; set; }
    public string? LongDescriptionRo { get; set; }
    public string ShortDescriptionEn { get; set; } = string.Empty;
    public string? LongDescriptionEn { get; set; }
 
    // Ierarhie
public string? ParentCode { get; set; }
    public int HierarchyLevel { get; set; }
    public bool IsLeafNode { get; set; }
    public bool IsBillable { get; set; }
    
    // Clasificare
    public string? Category { get; set; }
    public bool IsCommon { get; set; }
    public string? Severity { get; set; }
    
    // Cautare
    public string? SearchTermsRo { get; set; }
    public string? SearchTermsEn { get; set; }
    
    // Traducere
    public bool IsTranslated { get; set; }
    public DateTime? TranslatedAt { get; set; }
    public string? TranslatedBy { get; set; }
    
    // Versiune
    public string Version { get; set; } = "2026";
    public string? SourceFile { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

    /// <summary>Descrierea preferată (română dacă există)</summary>
    public string PreferredShortDescription => ShortDescriptionRo ?? ShortDescriptionEn;
    
    public string PreferredLongDescription => LongDescriptionRo ?? LongDescriptionEn ?? "";
}

/// <summary>
/// DTO pentru statistici ICD-10 (dashboard)
/// </summary>
public class ICD10StatisticsDto
{
    public int TotalCodes { get; set; }
    public int TranslatedCodes { get; set; }
    public int UntranslatedCodes { get; set; }
    public int CommonCodes { get; set; }
    public int LeafNodeCodes { get; set; }
    public int TotalCategories { get; set; }
    public Dictionary<string, int> CodesByCategory { get; set; } = new();
 public Dictionary<string, int> CodesBySeverity { get; set; } = new();
    
    /// <summary>Procentaj traducere</summary>
 public decimal TranslationPercentage => TotalCodes > 0 
 ? Math.Round((decimal)TranslatedCodes / TotalCodes * 100, 2) 
        : 0;
}

/// <summary>
/// DTO pentru adăugare diagnostic la consultație
/// </summary>
public class DiagnosticDto
{
    public Guid DiagnosticID { get; set; }
    public Guid ConsultatieID { get; set; }
    
    /// <summary>Cod ICD-10 (ex: I10, E11.9)</summary>
    public string CodICD { get; set; } = string.Empty;
    
    /// <summary>Descrierea diagnosticului (preluată din ICD10 sau customizată)</summary>
    public string DescriereaDiagnosticului { get; set; } = string.Empty;
    
    /// <summary>Tip: Principal, Secundar, Complicație</summary>
    public string TipDiagnostic { get; set; } = "Secundar";
    
    /// <summary>Severitate: Mild, Moderate, Severe, Critical</summary>
    public string? Severitate { get; set; }
  
    /// <summary>Status: Activ, Rezolvat, Cronic</summary>
    public string? Status { get; set; }
    
  public DateTime? DataDiagnostic { get; set; }
    
    // Computed
    public bool IsPrincipal => TipDiagnostic == "Principal";
}
