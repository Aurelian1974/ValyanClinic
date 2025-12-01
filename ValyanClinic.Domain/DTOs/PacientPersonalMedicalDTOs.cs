namespace ValyanClinic.Domain.DTOs;

/// <summary>
/// DTO pentru un doctor asociat unui pacient - Domain layer.
/// </summary>
public class DoctorAsociatDto
{
    public Guid RelatieID { get; set; }
    public Guid PersonalMedicalID { get; set; }
    public string DoctorNumeComplet { get; set; } = string.Empty;
    public string? DoctorSpecializare { get; set; }
    public string? DoctorTelefon { get; set; }
    public string? DoctorEmail { get; set; }
    public string? DoctorDepartament { get; set; }
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public DateTime? DataDezactivarii { get; set; }
    public bool EsteActiv { get; set; }
    public int ZileDeAsociere { get; set; }
    public string? Observatii { get; set; }
    public string? Motiv { get; set; }
}

/// <summary>
/// DTO pentru un pacient asociat unui doctor - Domain layer.
/// </summary>
public class PacientAsociatDto
{
    public Guid RelatieID { get; set; }
    public Guid PacientID { get; set; }
    public string PacientCod { get; set; } = string.Empty;
    public string PacientNumeComplet { get; set; } = string.Empty;
    public string? PacientCNP { get; set; }
    public DateTime PacientDataNasterii { get; set; }
    public int PacientVarsta { get; set; }
    public string? PacientTelefon { get; set; }
    public string? PacientEmail { get; set; }
    public string? PacientJudet { get; set; }
    public string? PacientLocalitate { get; set; }
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public DateTime? DataDezactivarii { get; set; }
    public bool EsteActiv { get; set; }
    public int ZileDeAsociere { get; set; }
    public string? Observatii { get; set; }
    public string? Motiv { get; set; }
}
