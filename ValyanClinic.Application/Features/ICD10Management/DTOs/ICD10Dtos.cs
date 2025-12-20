namespace ValyanClinic.Application.Features.ICD10Management.DTOs;

/// <summary>
/// DTO pentru rezultatele cautarii ICD-10
/// </summary>
public class ICD10SearchResultDto
{
    public Guid ICD10_ID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public bool IsCommon { get; set; }
    public string? Severity { get; set; }
    public int RelevanceScore { get; set; }

    // Display properties
    public string DisplayText => $"{Code} - {ShortDescription}";
    public string CategoryIcon => Category switch
    {
        "Cardiovascular" => "??",
        "Endocrin" => "??",
        "Respirator" => "??",
        "Digestiv" => "???",
        "Nervos" => "??",
        _ => "??"
    };
}

/// <summary>
/// DTO pentru detalii complete cod ICD-10
/// </summary>
public class ICD10DetailDto
{
    public Guid ICD10_ID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public string? EnglishDescription { get; set; }
    public string? ParentCode { get; set; }
    public bool IsLeafNode { get; set; }
    public bool IsCommon { get; set; }
    public string? Severity { get; set; }
    public string? SearchTerms { get; set; }
    public string? Notes { get; set; }
    public DateTime DataCreare { get; set; }
}

/// <summary>
/// DTO pentru statistici ICD-10
/// </summary>
public class ICD10StatisticsDto
{
    public int TotalCodes { get; set; }
    public int CommonCodes { get; set; }
    public int TotalCategories { get; set; }
    public Dictionary<string, int> CodesByCategory { get; set; } = new();
    public Dictionary<string, int> CodesBySeverity { get; set; } = new();
}
