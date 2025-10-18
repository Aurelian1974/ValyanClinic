namespace ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

/// <summary>
/// DTO pentru lista de pacienti
/// </summary>
public class PacientListDto
{
    public Guid Id { get; set; }
    public string Cod_Pacient { get; set; } = string.Empty;
    public string? CNP { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string NumeComplet { get; set; } = string.Empty;
    public DateTime Data_Nasterii { get; set; }
    public int Varsta { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string AdresaCompleta { get; set; } = string.Empty;
    public bool Asigurat { get; set; }
    public string? Casa_Asigurari { get; set; }
    public DateTime? Ultima_Vizita { get; set; }
    public int Nr_Total_Vizite { get; set; }
    public bool Activ { get; set; }
}
