namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// DTO pentru detalii complete analiză medicală (nomenclator)
/// </summary>
public class AnalizaMedicalaDetailDto
{
    public Guid AnalizaID { get; set; }
    public Guid LaboratorID { get; set; }
    public Guid CategorieID { get; set; }
    
    // Informații analiză
    public string? CodAnaliza { get; set; }
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? NumeScurt { get; set; }
    public string? Acronime { get; set; }
    public string? Descriere { get; set; }
    
    // Instrucțiuni
    public string? PreparareaTestului { get; set; }
    public string? Material { get; set; }
    public string? TermenProcesare { get; set; }
    
    // Prețuri
    public decimal? Pret { get; set; }
    public string? Moneda { get; set; }
    public DateTime? PretActualizatLa { get; set; }
    
    // Categorie
    public string NumeCategorie { get; set; } = string.Empty;
    public string? CategorieIcon { get; set; }
    
    // Laborator
    public string NumeLaborator { get; set; } = string.Empty;
    public string? LaboratorAcronim { get; set; }
    public string? LaboratorWebsite { get; set; }
    
    // Metadata
    public string? URLSursa { get; set; }
    public bool EsteActiv { get; set; }
    public DateTime? DataScraping { get; set; }
}
