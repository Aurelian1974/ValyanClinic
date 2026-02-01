namespace ValyanClinic.Application.Features.Settings.Queries.GetProgramLucru;

/// <summary>
/// DTO pentru programul de lucru al clinicii
/// </summary>
public record ProgramLucruDto
{
    public Guid Id { get; init; }
    public int ZiSaptamana { get; init; }
    public string NumeZi { get; init; } = string.Empty;
    public bool EsteDeschis { get; init; }
    public string? OraInceput { get; init; }
    public string? OraSfarsit { get; init; }
    public string? PauzaInceput { get; init; }
    public string? PauzaSfarsit { get; init; }
    public string? Observatii { get; init; }
    public DateTime DataCrearii { get; init; }
    public DateTime? DataModificarii { get; init; }
    public string? ModificatDe { get; init; }
}
