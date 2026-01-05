namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entitate pentru nomenclator analize medicale
/// Reprezintă analize disponibile în laboratoare (ex: Hemoleucogramă, Glicemie, etc.)
/// NOMENCLATOR - nu date tranzacționale
/// </summary>
public class AnalizaMedicala
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
    public string? Material { get; set; } // Ex: "Sânge venos", "Urină"
    public string? TermenProcesare { get; set; } // Ex: "1-2 zile lucrătoare"
    
    // Prețuri
    public decimal? Pret { get; set; }
    public string? Moneda { get; set; } = "RON";
    public DateTime? PretActualizatLa { get; set; }
    
    // Metadata
    public string? URLSursa { get; set; }
    public bool EsteActiv { get; set; } = true;
    public DateTime? DataScraping { get; set; }
    public DateTime DataCreare { get; set; } = DateTime.Now;
    public DateTime DataUltimaActualizare { get; set; } = DateTime.Now;

    // Navigation properties
    public AnalizaMedicalaLaborator? Laborator { get; set; }
    public AnalizaMedicalaCategorie? Categorie { get; set; }
}
