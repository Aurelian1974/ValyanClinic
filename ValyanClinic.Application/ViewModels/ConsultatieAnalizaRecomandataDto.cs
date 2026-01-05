namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// DTO pentru analize medicale recomandate în timpul consultației
/// </summary>
public class ConsultatieAnalizaRecomandataDto
{
    public Guid Id { get; set; }
    public Guid ConsultatieID { get; set; }
    public Guid? AnalizaNomenclatorID { get; set; }
    
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }
    public string TipAnaliza { get; set; } = "Laborator"; // Categoria
    
    public DateTime DataRecomandare { get; set; }
    public string? Prioritate { get; set; }
    public bool EsteCito { get; set; }
    public string? IndicatiiClinice { get; set; }
    public string? ObservatiiMedic { get; set; }
    
    public string Status { get; set; } = "Recomandata";
    public DateTime? DataProgramata { get; set; }
}
