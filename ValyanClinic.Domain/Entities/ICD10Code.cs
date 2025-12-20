namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru coduri ICD-10 (Clasificarea Internationala a Bolilor)
/// Conform structura din ValyanMed database
/// Versiune: ICD-10-CM 2026
/// </summary>
public class ICD10Code
{
    // ==================== PRIMARY KEY ====================
    public Guid ICD10_ID { get; set; }

    // ==================== FOREIGN KEYS ====================
    /// <summary>Capitolul ICD-10 (ex: Chapter IX - Diseases of the circulatory system)</summary>
    public Guid ChapterId { get; set; }
    
    /// <summary>Secțiunea ICD-10 (ex: I10-I15 Hypertensive diseases)</summary>
    public Guid? SectionId { get; set; }
    
    /// <summary>ID-ul codului părinte în ierarhie</summary>
    public Guid? ParentId { get; set; }

    // ==================== IDENTIFICARE ====================
    /// <summary>Codul ICD-10 scurt (ex: I10, E11.9, J18)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Codul complet pentru sortare (ex: I10.0)</summary>
    public string FullCode { get; set; } = string.Empty;

    // ==================== DESCRIERI ====================
    /// <summary>Descriere scurtă în română (max 250 caractere)</summary>
    public string? ShortDescriptionRo { get; set; }

    /// <summary>Descriere detaliată în română (max 1000 caractere)</summary>
    public string? LongDescriptionRo { get; set; }

    /// <summary>Descriere scurtă în engleză (WHO original)</summary>
public string ShortDescriptionEn { get; set; } = string.Empty;

    /// <summary>Descriere detaliată în engleză (WHO original)</summary>
    public string? LongDescriptionEn { get; set; }

    // ==================== IERARHIE ====================
    /// <summary>Codul parinte (ex: I20 pentru I20.0)</summary>
    public string? ParentCode { get; set; }

    /// <summary>Nivelul în ierarhie (0 = rădăcină, 1 = subcategorie, etc.)</summary>
    public int HierarchyLevel { get; set; }

    /// <summary>True = cod final (se poate folosi în diagnostic)</summary>
    public bool IsLeafNode { get; set; } = true;

    /// <summary>True = cod facturabil (poate fi folosit pentru billing)</summary>
  public bool IsBillable { get; set; } = true;

    // ==================== CLASIFICARE ====================
    /// <summary>Categoria medicală (ex: Cardiovascular, Endocrin, Respirator)</summary>
    public string? Category { get; set; }

    /// <summary>True = cod frecvent folosit (prioritar în autocomplete)</summary>
    public bool IsCommon { get; set; } = false;

    /// <summary>Severitatea bolii: Mild, Moderate, Severe, Critical</summary>
    public string? Severity { get; set; }

  // ==================== CAUTARE ====================
    /// <summary>Keywords pentru căutare în română (sinonime, abrevieri)</summary>
    public string? SearchTermsRo { get; set; }

    /// <summary>Keywords pentru căutare în engleză</summary>
    public string? SearchTermsEn { get; set; }

    // ==================== TRADUCERE ====================
    /// <summary>True = traducerea în română a fost completată</summary>
    public bool IsTranslated { get; set; } = false;

    /// <summary>Data traducerii</summary>
    public DateTime? TranslatedAt { get; set; }

    /// <summary>Traducător (ex: AutoTranslate-Google, Manual-Dr.Popescu)</summary>
    public string? TranslatedBy { get; set; }

    // ==================== VERSIUNE ====================
    /// <summary>Versiunea ICD-10 (ex: 2026, 2025)</summary>
    public string Version { get; set; } = "2026";

    /// <summary>Fișier sursă (pentru audit)</summary>
    public string? SourceFile { get; set; }

  // ==================== STATUS ====================
    /// <summary>Cod activ (folosit în sistem)</summary>
    public bool IsActive { get; set; } = true;

    // ==================== AUDIT ====================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Alias pentru backward compatibility
    public DateTime DataCreare { get => CreatedAt; set => CreatedAt = value; }
    public DateTime? DataModificare { get => UpdatedAt; set => UpdatedAt = value; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>Descrierea afișată (prioritate: română dacă există, altfel engleză)</summary>
    public string ShortDescription => ShortDescriptionRo ?? ShortDescriptionEn;

    /// <summary>Descrierea detaliată (prioritate: română dacă există, altfel engleză)</summary>
    public string? LongDescription => LongDescriptionRo ?? LongDescriptionEn;

    /// <summary>Returneaza displayul complet: "I10 - Hipertensiune arterială esențială"</summary>
    public string DisplayText => $"{Code} - {ShortDescription}";

    /// <summary>Returneaza categoria cu emoji</summary>
    public string CategoryDisplay => Category switch
    {
  "Cardiovascular" => "❤️ Cardiovascular",
     "Endocrin" => "🔬 Endocrin",
        "Respirator" => "🫁 Respirator",
        "Digestiv" => "🍽️ Digestiv",
        "Nervos" => "🧠 Nervos",
        "Genito-urinar" => "🩺 Genito-urinar",
        "Musculo-scheletic" => "🦴 Musculo-scheletic",
        "Piele" => "🩹 Piele",
        "Ochi" => "👁️ Ochi",
        "Ureche" => "👂 Ureche",
"Simptome" => "⚕️ Simptome",
        "Infectioase" => "🦠 Infectioase",
        "Neoplasme" => "🔬 Neoplasme",
        "Mental" => "🧠 Mental",
      "Obstetric" => "🤰 Obstetric",
        "Traumatisme" => "🩹 Traumatisme",
     _ => $"📋 {Category}"
    };

    /// <summary>Returneaza culoarea badge-ului pentru severitate</summary>
    public string SeverityColor => Severity?.ToLower() switch
  {
        "mild" => "#10b981", // Green
        "moderate" => "#f59e0b",  // Orange
      "severe" => "#ef4444",    // Red
        "critical" => "#7c3aed",  // Purple
        _ => "#6b7280"      // Gray
    };

    /// <summary>Returneaza textul afisat pentru severitate</summary>
    public string SeverityDisplay => Severity switch
 {
 "Mild" => "Ușoară",
        "Moderate" => "Moderată",
   "Severe" => "Severă",
   "Critical" => "Critică",
     _ => "Nespecificată"
    };

    /// <summary>Verifica daca codul este un cod parinte (categorie)</summary>
    public bool IsCategory => !IsLeafNode;

    /// <summary>Badge pentru traducere incompletă</summary>
    public string TranslationBadge => !IsTranslated ? "⚠️ Netradus" : "✅ Tradus";

    /// <summary>Returneaza toate keywords pentru search (prioritate română, apoi engleză)</summary>
    public List<string> GetSearchKeywords()
    {
        var keywords = new List<string>
     {
   Code.ToLower(),
            ShortDescription.ToLower()
        };

        // Adaugă termeni de căutare în română
        if (!string.IsNullOrEmpty(SearchTermsRo))
        {
      keywords.AddRange(SearchTermsRo.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(k => k.Trim().ToLower()));
        }

  // Adaugă termeni de căutare în engleză (fallback)
     if (!string.IsNullOrEmpty(SearchTermsEn))
 {
    keywords.AddRange(SearchTermsEn.Split(',', StringSplitOptions.RemoveEmptyEntries)
         .Select(k => k.Trim().ToLower()));
        }

        return keywords.Distinct().ToList();
    }

    /// <summary>Calculeaza relevanta pentru un termen de cautare (0-100)</summary>
    public int CalculateRelevance(string searchTerm)
    {
   searchTerm = searchTerm.ToLower().Trim();

        // Exact match pe cod
        if (Code.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
            return 100;

        // Starts with pe cod
        if (Code.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
    return 95;

        // Exact match pe descriere română
        if (ShortDescriptionRo?.Equals(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            return 90;

        // Starts with pe descriere română
        if (ShortDescriptionRo?.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            return 85;

     // Contains în descriere română
        if (ShortDescriptionRo?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
   return 75;

   // Match în search terms română
        if (!string.IsNullOrEmpty(SearchTermsRo) &&
   SearchTermsRo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
      return 70;

        // Fallback: Match în engleză
        if (ShortDescriptionEn.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
          return 60;

        if (!string.IsNullOrEmpty(SearchTermsEn) &&
 SearchTermsEn.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
   return 55;

        return 0;
    }
}
