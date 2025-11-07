using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Queries.GetSystemSettings;

public record SystemSettingDto
{
    public Guid SetareID { get; init; }
    public string Categorie { get; init; } = string.Empty;
    public string Cheie { get; init; } = string.Empty;
 public string Valoare { get; init; } = string.Empty;
    public string? Descriere { get; init; }
    public string? ValoareDefault { get; init; }
    public string TipDate { get; init; } = "String";
    public bool EsteEditabil { get; init; }
    public DateTime DataCrearii { get; init; }
    public DateTime? DataModificarii { get; init; }
    public string? ModificatDe { get; init; }
}
