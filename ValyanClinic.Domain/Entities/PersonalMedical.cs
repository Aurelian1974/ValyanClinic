namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate de domeniu pentru PersonalMedical - ALINIAT CU DB REALA
/// Nota: PersonalID este PK, nu are ID separat
/// </summary>
public class PersonalMedical
{
    // Primary Key (FK la Personal)
    public Guid PersonalID { get; set; }
    
    // Date de baza (duplicate din Personal pentru query optimization?)
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    
    // Informatii medicale
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    
    // Contact (duplicate din Personal?)
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    
    // Pozitie
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }
    
    // Status
    public bool? EsteActiv { get; set; }
    
    // Categorii (FK la alte tabele de nomenclator)
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    
    // Audit
    public DateTime? DataCreare { get; set; }
    
    // Navigation properties
    public Personal? Personal { get; set; }
    
    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
}
