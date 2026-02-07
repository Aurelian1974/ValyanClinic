namespace ValyanClinic.Domain.Entities.Settings;

/// <summary>
/// Entitate pentru programul de lucru al clinicii pe zile
/// Tabela: ProgramLucru
/// </summary>
public class ProgramLucru
{
    public Guid Id { get; set; }
    public DayOfWeek ZiSaptamana { get; set; }
    public string NumeZi { get; set; } = string.Empty;
    public bool EsteDeschis { get; set; }
    public TimeSpan? OraInceput { get; set; }
    public TimeSpan? OraSfarsit { get; set; }
    public TimeSpan? PauzaInceput { get; set; }
    public TimeSpan? PauzaSfarsit { get; set; }
    public string? Observatii { get; set; }
    public DateTime DataCrearii { get; set; }
    public DateTime? DataModificarii { get; set; }
    public string? ModificatDe { get; set; }
}
