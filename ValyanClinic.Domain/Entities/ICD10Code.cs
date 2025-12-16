namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru coduri ICD-10 (Clasificarea Internationala a Bolilor)
/// Conform WHO ICD-10-CM 2026 si Ordin MS 1438/2009 (Romania)
/// Tabel: ICD10_Codes
/// </summary>
public class ICD10Code
{
    // ==================== PRIMARY KEY ====================
    public Guid ICD10_ID { get; set; }

    // ==================== FOREIGN KEYS ====================
    /// <summary>
    /// Referință la capitolul ICD-10 (I-XXII)
    /// </summary>
    public Guid ChapterId { get; set; }

    /// <summary>
    /// Referință la secțiunea din capitol (opțional)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Referință la codul părinte pentru ierarhie (self-reference)
    /// </summary>
    public Guid? ParentId { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Codul ICD-10 (ex: I20.0, E11.9, J18) - max 10 caractere
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Codul complet pentru sortare (ex: I20.0) - max 15 caractere
    /// </summary>
    public string FullCode { get; set; } = string.Empty;

    /// <summary>
    /// Codul parinte ca string (ex: I20 pentru I20.0) - max 10 caractere
    /// </summary>
    public string? ParentCode { get; set; }

    // ==================== DESCRIERI ROMÂNĂ ====================
    /// <summary>
    /// Descriere scurtă în română (max 250 caractere)
    /// </summary>
    public string? ShortDescriptionRo { get; set; }

    /// <summary>
    /// Descriere detaliată în română (max 1000 caractere)
    /// </summary>
    public string? LongDescriptionRo { get; set; }

    // ==================== DESCRIERI ENGLEZĂ ====================
    /// <summary>
    /// Descriere scurtă în engleză (max 250 caractere) - REQUIRED
    /// </summary>
    public string ShortDescriptionEn { get; set; } = string.Empty;

    /// <summary>
    /// Descriere detaliată în engleză (max 1000 caractere)
    /// </summary>
    public string? LongDescriptionEn { get; set; }

    // ==================== IERARHIE ====================
    /// <summary>
    /// Nivelul în ierarhia ICD-10 (0=capitol, 1=secțiune, 2=categorie, 3+=subcategorie)
    /// </summary>
    public int HierarchyLevel { get; set; } = 0;

    /// <summary>
    /// True = cod final (se poate folosi în diagnostic)
    /// False = categorie (doar pentru grupare)
    /// </summary>
    public bool IsLeafNode { get; set; } = true;

    /// <summary>
    /// True = codul poate fi facturat/raportat către CNAS
    /// </summary>
    public bool IsBillable { get; set; } = false;

    // ==================== CLASIFICARE ====================
    /// <summary>
    /// Categoria medicală (ex: Cardiovascular, Endocrin, Respirator) - max 50 caractere
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// True = cod frecvent folosit (prioritar în autocomplete)
    /// </summary>
    public bool IsCommon { get; set; } = false;

    /// <summary>
    /// Severitatea bolii: Mild, Moderate, Severe, Critical - max 20 caractere
    /// </summary>
    public string? Severity { get; set; }

    // ==================== CĂUTARE ====================
    /// <summary>
    /// Keywords pentru căutare în română (sinonime, variante)
    /// ex: "hta,hipertensiune,tensiune mare,presiune arteriala"
    /// </summary>
    public string? SearchTermsRo { get; set; }

    /// <summary>
    /// Keywords pentru căutare în engleză
    /// </summary>
    public string? SearchTermsEn { get; set; }

    // ==================== TRADUCERE ====================
    /// <summary>
    /// True = codul a fost tradus în română
    /// </summary>
    public bool IsTranslated { get; set; } = false;

    /// <summary>
    /// Data traducerii
    /// </summary>
    public DateTime? TranslatedAt { get; set; }

    /// <summary>
    /// Cine a tradus (utilizator sau "AI")
    /// </summary>
    public string? TranslatedBy { get; set; }

    // ==================== VERSIUNE ====================
    /// <summary>
    /// Versiunea ICD-10 (ex: "2026", "2025")
    /// </summary>
    public string Version { get; set; } = "2026";

    /// <summary>
    /// Fișierul sursă din care a fost importat
    /// </summary>
    public string? SourceFile { get; set; }

    // ==================== STATUS ====================
    /// <summary>
    /// True = codul este activ și poate fi folosit
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================
    /// <summary>
    /// Capitolul din care face parte codul
    /// </summary>
    public virtual ICD10Chapter? Chapter { get; set; }

    /// <summary>
    /// Secțiunea din care face parte codul (opțional)
    /// </summary>
    public virtual ICD10Section? Section { get; set; }

    /// <summary>
    /// Codul părinte (pentru ierarhie)
    /// </summary>
    public virtual ICD10Code? Parent { get; set; }

    /// <summary>
    /// Codurile copil
    /// </summary>
    public virtual ICollection<ICD10Code> Children { get; set; } = new List<ICD10Code>();

    /// <summary>
    /// Termenii de incluziune pentru acest cod
    /// </summary>
    public virtual ICollection<ICD10InclusionTerm> InclusionTerms { get; set; } = new List<ICD10InclusionTerm>();

    /// <summary>
    /// Excluderile pentru acest cod
    /// </summary>
    public virtual ICollection<ICD10Exclusion> Exclusions { get; set; } = new List<ICD10Exclusion>();

    /// <summary>
    /// Notele pentru acest cod
    /// </summary>
    public virtual ICollection<ICD10Note> Notes { get; set; } = new List<ICD10Note>();

    /// <summary>
    /// Instrucțiunile de codificare pentru acest cod
    /// </summary>
    public virtual ICollection<ICD10CodingInstruction> CodingInstructions { get; set; } = new List<ICD10CodingInstruction>();

    // Alias pentru backward compatibility
    public DateTime DataCreare { get => CreatedAt; set => CreatedAt = value; }
    public DateTime? DataModificare { get => UpdatedAt; set => UpdatedAt = value; }

    // Backward compatibility properties (map to new names)
    public string ShortDescription { get => ShortDescriptionRo ?? ShortDescriptionEn; set => ShortDescriptionRo = value; }
    public string? LongDescription { get => LongDescriptionRo ?? LongDescriptionEn; set => LongDescriptionRo = value; }
    public string? EnglishDescription { get => ShortDescriptionEn; set => ShortDescriptionEn = value ?? string.Empty; }
    public string? SearchTerms { get => SearchTermsRo; set => SearchTermsRo = value; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Returneaza displayul complet: "I20.0 - Angina instabila"
    /// </summary>
    public string DisplayText => $"{Code} - {ShortDescription}";

    /// <summary>
    /// Returneaza categoria cu emoji
    /// </summary>
    public string CategoryDisplay => Category switch
    {
        "Cardiovascular" => "❤️ Cardiovascular",
        "Endocrin" => "🩺 Endocrin",
        "Respirator" => "🫁 Respirator",
        "Digestiv" => "🍽️ Digestiv",
        "Nervos" => "🧠 Nervos",
        "Genito-urinar" => "🔬 Genito-urinar",
        "Musculo-scheletic" => "🦴 Musculo-scheletic",
        "Piele" => "👤 Piele",
        "Ochi" => "👁️ Ochi",
        "Ureche" => "👂 Ureche",
        "Simptome" => "📋 Simptome",
        "Infectioase" => "🦠 Infectioase",
        "Neoplasme" => "🔬 Neoplasme",
        "Mental" => "🧠 Mental",
        "Obstetric" => "🤰 Obstetric",
        "Traumatisme" => "🚑 Traumatisme",
        _ => $"📌 {Category}"
    };

    /// <summary>
    /// Returneaza culoarea badge-ului pentru severitate
    /// </summary>
    public string SeverityColor => Severity?.ToLower() switch
    {
        "mild" => "#10b981",      // Green
        "moderate" => "#f59e0b",  // Orange
        "severe" => "#ef4444",    // Red
        "critical" => "#7c3aed",  // Purple
        _ => "#6b7280"            // Gray
    };

    /// <summary>
    /// Returneaza textul afisat pentru severitate
    /// </summary>
    public string SeverityDisplay => Severity switch
    {
        "Mild" => "Ușoară",
        "Moderate" => "Moderată",
        "Severe" => "Severă",
        "Critical" => "Critică",
        _ => "Nespecificată"
    };

    /// <summary>
    /// Verifica daca codul este un cod parinte (categorie)
    /// </summary>
    public bool IsCategory => !IsLeafNode;

    /// <summary>
    /// Returneaza toate keywords pentru search (lowercase)
    /// </summary>
    public List<string> GetSearchKeywords()
    {
        var keywords = new List<string>
        {
            Code.ToLower(),
            ShortDescription.ToLower()
        };

        if (!string.IsNullOrEmpty(SearchTerms))
        {
            keywords.AddRange(SearchTerms.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLower()));
        }

        return keywords.Distinct().ToList();
    }

    /// <summary>
    /// Calculeaza relevanta pentru un termen de cautare (0-100)
    /// </summary>
    public int CalculateRelevance(string searchTerm)
    {
        searchTerm = searchTerm.ToLower().Trim();

        // Exact match pe cod
        if (Code.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 100;

        // Starts with pe cod
        if (Code.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 90;

        // Exact match pe descriere
        if (ShortDescription.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 85;

        // Starts with pe descriere
        if (ShortDescription.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 80;

        // Contains in descriere
        if (ShortDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 70;

        // Match in search terms
        if (!string.IsNullOrEmpty(SearchTerms) &&
            SearchTerms.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 60;

        return 0;
    }
}
