namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru coduri ICD-10 (Clasificarea Internationala a Bolilor)
/// Conform WHO ICD-10 si Ordin MS 1438/2009 (Romania)
/// </summary>
public class ICD10Code
{
    // ==================== PRIMARY KEY ====================
    public Guid ICD10_ID { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>
    /// Codul ICD-10 (ex: I20.0, E11.9, J18)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Codul complet pentru sortare (ex: I20.0)
    /// </summary>
    public string FullCode { get; set; } = string.Empty;

    /// <summary>
    /// Categoria medicala (ex: Cardiovascular, Endocrin, Respirator)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    // ==================== DESCRIERI ====================
    /// <summary>
    /// Descriere scurta in romana (max 200 caractere)
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Descriere detaliata in romana
    /// </summary>
    public string? LongDescription { get; set; }

    /// <summary>
    /// Descriere in engleza (pentru referinta)
    /// </summary>
    public string? EnglishDescription { get; set; }

    // ==================== IERARHIE ====================
    /// <summary>
    /// Codul parinte (ex: I20 pentru I20.0)
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// True = cod final (se poate folosi in diagnostic)
    /// False = categorie (doar pentru grupare)
    /// </summary>
    public bool IsLeafNode { get; set; } = true;

    // ==================== CLASIFICARE ====================
    /// <summary>
    /// True = cod frecvent folosit (prioritar in autocomplete)
    /// </summary>
    public bool IsCommon { get; set; } = false;

    /// <summary>
    /// Severitatea bolii: Mild, Moderate, Severe, Critical
    /// </summary>
    public string? Severity { get; set; }

    // ==================== CAUTARE ====================
    /// <summary>
    /// Keywords pentru cautare (romana + sinonime)
    /// ex: "hta,hipertensiune,tensiune mare,presiune arteriala"
    /// </summary>
    public string? SearchTerms { get; set; }

    /// <summary>
    /// Note medicale suplimentare
    /// </summary>
    public string? Notes { get; set; }

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Alias pentru backward compatibility
    public DateTime DataCreare { get => CreatedAt; set => CreatedAt = value; }
    public DateTime? DataModificare { get => UpdatedAt; set => UpdatedAt = value; }

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
