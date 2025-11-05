namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;

/// <summary>
/// DTO pentru afisarea unui doctor asociat cu un pacient
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
