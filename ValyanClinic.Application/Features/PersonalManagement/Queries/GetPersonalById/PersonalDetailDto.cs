namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalById;

/// <summary>
/// DTO pentru detalii complete personal - ALINIAT CU DB REALA
/// </summary>
public class PersonalDetailDto
{
    public Guid Id_Personal { get; set; }
    public string Cod_Angajat { get; set; } = string.Empty;
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string NumeComplet => $"{Nume} {Prenume}";
    public string? Nume_Anterior { get; set; }
    public string CNP { get; set; } = string.Empty;
    public DateTime Data_Nasterii { get; set; }
    public string? Locul_Nasterii { get; set; }
    public string Nationalitate { get; set; } = "Romana";
    public string Cetatenie { get; set; } = "Romana";
    
    // Contact
    public string? Telefon_Personal { get; set; }
    public string? Telefon_Serviciu { get; set; }
    public string? Email_Personal { get; set; }
    public string? Email_Serviciu { get; set; }
    
    // Adresa Domiciliu
    public string Adresa_Domiciliu { get; set; } = string.Empty;
    public string Judet_Domiciliu { get; set; } = string.Empty;
    public string Oras_Domiciliu { get; set; } = string.Empty;
    public string? Cod_Postal_Domiciliu { get; set; }
    
    // Adresa Resedinta
    public string? Adresa_Resedinta { get; set; }
    public string? Judet_Resedinta { get; set; }
    public string? Oras_Resedinta { get; set; }
    public string? Cod_Postal_Resedinta { get; set; }
    
    // Stare civila si pozitie
    public string? Stare_Civila { get; set; }
    public string Functia { get; set; } = string.Empty;
    public string? Departament { get; set; }
    
    // Carte Identitate
    public string? Serie_CI { get; set; }
    public string? Numar_CI { get; set; }
    public string? Eliberat_CI_De { get; set; }
    public DateTime? Data_Eliberare_CI { get; set; }
    public DateTime? Valabil_CI_Pana { get; set; }
    
    // Status
    public string Status_Angajat { get; set; } = string.Empty;
    public string? Observatii { get; set; }
    
    // Audit
    public DateTime Data_Crearii { get; set; }
    public DateTime? Data_Ultimei_Modificari { get; set; }
    public string? Creat_De { get; set; }
    public string? Modificat_De { get; set; }
    
    // Computed
    public int Varsta => DateTime.Today.Year - Data_Nasterii.Year - 
        (DateTime.Today.DayOfYear < Data_Nasterii.DayOfYear ? 1 : 0);
}
