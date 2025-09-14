using ValyanClinic.Domain.Enums;

namespace ValyanClinic.Domain.Models;

/// <summary>
/// Model pentru personalul clinicii ValyanMed
/// </summary>
public class Personal
{
    // Identificatori Unici
    public Guid Id_Personal { get; set; }
    public string Cod_Angajat { get; set; } = string.Empty;
    public string CNP { get; set; } = string.Empty;
    
    // Date Personale De Baza
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Nume_Anterior { get; set; }
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
    
    // Adresa Resedinta (Daca Difera)
    public string? Adresa_Resedinta { get; set; }
    public string? Judet_Resedinta { get; set; }
    public string? Oras_Resedinta { get; set; }
    public string? Cod_Postal_Resedinta { get; set; }
    
    // Stare Civila Si Familie
    public StareCivila? Stare_Civila { get; set; }
    
    // Date Profesionale
    public string Functia { get; set; } = string.Empty;
    public Departament? Departament { get; set; }
    
    // Date Administrative
    public string? Serie_CI { get; set; }
    public string? Numar_CI { get; set; }
    public string? Eliberat_CI_De { get; set; }
    public DateTime? Data_Eliberare_CI { get; set; }
    public DateTime? Valabil_CI_Pana { get; set; }
    
    // Status Si Metadata
    public StatusAngajat Status_Angajat { get; set; } = StatusAngajat.Activ;
    public string? Observatii { get; set; }
    public DateTime Data_Crearii { get; set; }
    public DateTime Data_Ultimei_Modificari { get; set; }
    public string? Creat_De { get; set; }
    public string? Modificat_De { get; set; }
    
    // Computed Properties pentru UI
    public string NumeComplet => $"{Nume} {Prenume}";
    public string AdresaCompleta => Adresa_Domiciliu + (string.IsNullOrEmpty(Judet_Domiciliu) ? "" : $", {Judet_Domiciliu}");
    public int Varsta => DateTime.Now.Year - Data_Nasterii.Year - (DateTime.Now.DayOfYear < Data_Nasterii.DayOfYear ? 1 : 0);
    public string StatusDisplay => Status_Angajat == StatusAngajat.Activ ? "Activ" : "Inactiv";
    public string DepartamentDisplay => Departament?.ToString() ?? "Nedefinit";
    public string StareCivilaDisplay => Stare_Civila?.ToString() ?? "Nedefinita";
}
