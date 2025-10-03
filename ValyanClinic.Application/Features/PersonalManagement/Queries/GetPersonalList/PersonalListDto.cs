namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;

/// <summary>
/// DTO pentru lista de personal - ALINIAT CU DB REALA
/// </summary>
public class PersonalListDto
{
    public Guid Id_Personal { get; set; }
    public string Cod_Angajat { get; set; } = string.Empty;
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string NumeComplet => $"{Nume} {Prenume}";
    public string CNP { get; set; } = string.Empty;
    public string? Telefon_Personal { get; set; }
    public string? Email_Personal { get; set; }
    public DateTime Data_Nasterii { get; set; }
    public string Status_Angajat { get; set; } = string.Empty;
    
    // Informatii despre locatie
    public string Judet_Domiciliu { get; set; } = string.Empty;
    public string Oras_Domiciliu { get; set; } = string.Empty;
    
    // Pozitie
    public string Functia { get; set; } = string.Empty;
    public string? Departament { get; set; }
}
