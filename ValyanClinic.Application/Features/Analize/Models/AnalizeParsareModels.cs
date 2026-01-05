namespace ValyanClinic.Application.Features.Analize.Models;

/// <summary>
/// Informații despre un laborator disponibil pentru parsare
/// </summary>
public class LaboratorInfo
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Analiză parsată din PDF
/// </summary>
public class AnalizaParsataDto
{
    public string Categorie { get; set; } = string.Empty;
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }
    public string Rezultat { get; set; } = string.Empty;
    public decimal? RezultatNumeric { get; set; }
    public string UnitateMasura { get; set; } = string.Empty;
    public decimal? IntervalMin { get; set; }
    public decimal? IntervalMax { get; set; }
    public string IntervalText { get; set; } = string.Empty;
    public bool EsteAnormal { get; set; }
    public string? DirectieAnormal { get; set; }
    public string? Comentariu { get; set; }
    public string? Metoda { get; set; }
}

/// <summary>
/// Rezultatul parsării unui PDF
/// </summary>
public class ParsePdfResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string Laborator { get; set; } = string.Empty;
    public string NumarBuletin { get; set; } = string.Empty;
    public string DataRecoltare { get; set; } = string.Empty;
    public string PacientNume { get; set; } = string.Empty;
    public string PacientCnp { get; set; } = string.Empty;
    public List<AnalizaParsataDto> Analize { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int TotalAnalize { get; set; }
    public int AnalizeAnormale { get; set; }
    
    // Debugging
    public string? TextExtras { get; set; }
    public string? ParserUtilizat { get; set; }
}

/// <summary>
/// Format pentru import în baza de date
/// </summary>
public class AnalizaImportDto
{
    public string NumeAnaliza { get; set; } = string.Empty;
    public string? CodAnaliza { get; set; }
    public string TipAnaliza { get; set; } = string.Empty;
    public string Valoare { get; set; } = string.Empty;
    public decimal? ValoareNumerica { get; set; }
    public string UnitatiMasura { get; set; } = string.Empty;
    public decimal? ValoareNormalaMin { get; set; }
    public decimal? ValoareNormalaMax { get; set; }
    public string ValoareNormalaText { get; set; } = string.Empty;
    public bool EsteInAfaraLimitelor { get; set; }
    public string? DirectieAnormal { get; set; }
    public string? DataRecoltare { get; set; }
    public string? Laborator { get; set; }
    public string? NumarBuletin { get; set; }
    
    // Alias properties pentru compatibilitate
    public string Denumire => NumeAnaliza;
    public string Categorie => TipAnaliza;
}
