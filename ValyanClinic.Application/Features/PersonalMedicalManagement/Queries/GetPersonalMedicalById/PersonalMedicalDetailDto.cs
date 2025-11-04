namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

public class PersonalMedicalDetailDto
{
    public Guid PersonalID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string NumeComplet => $"{Nume} {Prenume}";
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }
    public bool? EsteActiv { get; set; }
    public DateTime? DataCreare { get; set; }
    public Guid? CategorieID { get; set; }
    public Guid? PozitieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    public string? CategorieName { get; set; }
    public string? SpecializareName { get; set; }
    public string? SubspecializareName { get; set; }
}
