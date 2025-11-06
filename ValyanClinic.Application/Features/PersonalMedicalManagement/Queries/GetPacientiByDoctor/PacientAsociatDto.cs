namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPacientiByDoctor;

/// <summary>
/// DTO pentru pacienții asociați cu un doctor
/// </summary>
public class PacientAsociatDto
{
    public Guid RelatieID { get; set; }
    public Guid PacientID { get; set; }
    public string PacientNumeComplet { get; set; } = string.Empty;
    public string? PacientCNP { get; set; }
    public string? PacientTelefon { get; set; }
    public string? PacientEmail { get; set; }
    public DateTime PacientDataNasterii { get; set; }
    public int PacientVarsta { get; set; }
    public string PacientSex { get; set; } = string.Empty;
 
    // Informații despre relație
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public DateTime? DataDezactivarii { get; set; }
    public bool EsteActiv { get; set; }
    public string? Motiv { get; set; }
    public string? Observatii { get; set; }
    
    // Computed properties
    public int ZileDeAsociere => EsteActiv 
        ? (DateTime.Now - DataAsocierii).Days 
        : (DataDezactivarii!.Value - DataAsocierii).Days;
    
    public string StatusText => EsteActiv ? "Activ" : "Inactiv";
}
