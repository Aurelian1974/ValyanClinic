namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// DTO pentru categorie analize medicale (pentru dropdown-uri)
/// </summary>
public class AnalizaMedicalaCategorieDto
{
    public Guid CategorieID { get; set; }
    public string NumeCategorie { get; set; } = string.Empty;
    public string? Descriere { get; set; }
    public string? Icon { get; set; }
    public int NumarAnalize { get; set; }
    public bool EsteActiv { get; set; }
}
