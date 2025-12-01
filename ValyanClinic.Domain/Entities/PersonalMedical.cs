namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru PersonalMedical - INDEPENDENT
/// PersonalMedical este o tabelă STANDALONE pentru personal medical
/// NU are relație cu tabela Personal
/// </summary>
public class PersonalMedical
{
    // Primary Key - INDEPENDENT GUID (NOT FK)
    public Guid PersonalID { get; set; }

    // Date de baza
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;

    // Informatii medicale
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }

    // Contact
    public string? Telefon { get; set; }
    public string? Email { get; set; }

    // Pozitie (text fields for display - backwards compatibility)
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }

    // Status
    public bool? EsteActiv { get; set; }

    // Foreign Keys to lookup tables
    public Guid? CategorieID { get; set; }       // FK to Departamente (old name for DepartamentID)
    public Guid? PozitieID { get; set; }         // FK to Pozitii
    public Guid? SpecializareID { get; set; }    // FK to Specializari
    public Guid? SubspecializareID { get; set; } // FK to Specializari (not used currently)

    // Audit
    public DateTime? DataCreare { get; set; }

    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
}
