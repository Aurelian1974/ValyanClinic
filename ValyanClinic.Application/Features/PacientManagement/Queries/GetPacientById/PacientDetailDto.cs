namespace ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;

/// <summary>
/// DTO pentru detaliile pacientului
/// </summary>
public class PacientDetailDto
{
    // Identificare
    public Guid Id { get; set; }
    public string Cod_Pacient { get; set; } = string.Empty;
    public string? CNP { get; set; }

    // Date personale
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string NumeComplet { get; set; } = string.Empty;
    public DateTime Data_Nasterii { get; set; }
    public int Varsta { get; set; }
    public string Sex { get; set; } = string.Empty;

    // Date de contact
    public string? Telefon { get; set; }
    public string? Telefon_Secundar { get; set; }
    public string? Email { get; set; }

    // Adresă
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Adresa { get; set; }
    public string AdresaCompleta { get; set; } = string.Empty;
    public string? Cod_Postal { get; set; }

    // Informații asigurare
    public bool Asigurat { get; set; }
    public string? CNP_Asigurat { get; set; }
    public string? Nr_Card_Sanatate { get; set; }
    public string? Casa_Asigurari { get; set; }

    // Date medicale de bază
    public string? Alergii { get; set; }
    public string? Boli_Cronice { get; set; }
    public string? Medic_Familie { get; set; }

    // Contact urgență
    public string? Persoana_Contact { get; set; }
    public string? Telefon_Urgenta { get; set; }
    public string? Relatie_Contact { get; set; }

    // Informații administrative
    public DateTime Data_Inregistrare { get; set; }
    public DateTime? Ultima_Vizita { get; set; }
    public int Nr_Total_Vizite { get; set; }
    public bool Activ { get; set; }
    public string? Observatii { get; set; }

    // Audit
    public DateTime Data_Crearii { get; set; }
    public DateTime Data_Ultimei_Modificari { get; set; }
    public string? Creat_De { get; set; }
    public string? Modificat_De { get; set; }
}
