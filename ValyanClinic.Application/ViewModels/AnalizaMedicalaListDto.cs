namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// DTO pentru afișare analiză medicală în liste (nomenclator)
/// </summary>
public class AnalizaMedicalaListDto
{
    public Guid AnalizaID { get; set; }
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? NumeScurt { get; set; }
    public string? Acronime { get; set; }
    public string NumeCategorie { get; set; } = string.Empty;
    public string? CategorieIcon { get; set; }
    public string NumeLaborator { get; set; } = string.Empty;
    public string? LaboratorAcronim { get; set; }
    public decimal? Pret { get; set; }
    public string? Moneda { get; set; }
}
