using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.Analize.Models;
using ValyanClinic.Application.Services.Analize.PdfAnalizeParsers;

namespace ValyanClinic.Application.Services.Analize;

/// <summary>
/// Serviciu pentru parsarea PDF-urilor cu analize medicale
/// Implementare nativă C# cu PdfPig (fără dependență Python)
/// </summary>
public interface IAnalizePdfParserService
{
    /// <summary>
    /// Obține lista laboratoarelor disponibile
    /// </summary>
    Task<List<LaboratorInfo>> GetLaboratoareAsync();
    
    /// <summary>
    /// Parsează un PDF și returnează analizele
    /// </summary>
    Task<ParsePdfResult> ParsePdfAsync(Stream pdfStream, string fileName, string laboratorKey);
    
    /// <summary>
    /// Parsează un PDF și returnează în format pentru import direct
    /// </summary>
    Task<List<AnalizaImportDto>> ParsePdfForImportAsync(Stream pdfStream, string fileName, string laboratorKey);
}

public class AnalizePdfParserService : IAnalizePdfParserService
{
    private readonly IPdfParserService _pdfParser;
    private readonly ILogger<AnalizePdfParserService> _logger;

    public AnalizePdfParserService(
        IPdfParserService pdfParser,
        ILogger<AnalizePdfParserService> logger)
    {
        _pdfParser = pdfParser;
        _logger = logger;
    }

    public Task<List<LaboratorInfo>> GetLaboratoareAsync()
    {
        // Returnăm lista de laboratoare suportate
        return Task.FromResult(GetDefaultLaboratoare());
    }

    public async Task<ParsePdfResult> ParsePdfAsync(Stream pdfStream, string fileName, string laboratorKey)
    {
        try
        {
            _logger.LogInformation("Parsare PDF: {FileName} (laborator: {Laborator})", fileName, laboratorKey);
            
            var result = await _pdfParser.ParsePdfAsync(pdfStream, fileName);
            
            if (result.Success)
            {
                _logger.LogInformation(
                    "PDF parsat cu succes: {Total} analize ({Anormale} anormale)",
                    result.TotalAnalize, result.AnalizeAnormale);
            }
            else
            {
                _logger.LogWarning("Parsare PDF eșuată: {Error}", result.Error);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la parsarea PDF-ului: {FileName}", fileName);
            return new ParsePdfResult
            {
                Success = false,
                Error = $"Eroare: {ex.Message}"
            };
        }
    }

    public async Task<List<AnalizaImportDto>> ParsePdfForImportAsync(Stream pdfStream, string fileName, string laboratorKey)
    {
        try
        {
            var result = await ParsePdfAsync(pdfStream, fileName, laboratorKey);
            
            if (!result.Success || result.Analize == null)
            {
                _logger.LogWarning("Nu s-au extras analize din {FileName}", fileName);
                return new List<AnalizaImportDto>();
            }

            // Convertește la formatul de import
            return result.Analize.Select(a => new AnalizaImportDto
            {
                NumeAnaliza = a.NumeAnaliza,
                CodAnaliza = a.CodAnaliza,
                TipAnaliza = a.Categorie,
                Valoare = a.Rezultat,
                ValoareNumerica = a.RezultatNumeric,
                UnitatiMasura = a.UnitateMasura,
                ValoareNormalaMin = a.IntervalMin,
                ValoareNormalaMax = a.IntervalMax,
                ValoareNormalaText = a.IntervalText,
                EsteInAfaraLimitelor = a.EsteAnormal,
                DirectieAnormal = a.DirectieAnormal,
                DataRecoltare = result.DataRecoltare,
                Laborator = result.Laborator,
                NumarBuletin = result.NumarBuletin
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la parsarea PDF-ului pentru import: {FileName}", fileName);
            return new List<AnalizaImportDto>();
        }
    }
    
    /// <summary>
    /// Lista de laboratoare suportate
    /// </summary>
    private static List<LaboratorInfo> GetDefaultLaboratoare()
    {
        return new List<LaboratorInfo>
        {
            new() { Key = "universal", Name = "Detectare automată", Description = "Detectează automat laboratorul din PDF" },
            new() { Key = "regina_maria", Name = "Regina Maria", Description = "Format: Denumire (COD) = Valoare UM [min - max]" },
            new() { Key = "synevo", Name = "Synevo", Description = "Format: Denumire | Rezultat | UM | Interval" },
            new() { Key = "medlife", Name = "MedLife", Description = "Format: Test | Rezultat | UM | Interval" },
            new() { Key = "bioclinica", Name = "Bioclinica", Description = "Format: Denumire | Valoare /UM | (min - max)" },
            new() { Key = "clinica_sante", Name = "Clinica Sante", Description = "Format vertical: Nume → [Interval] → UM → Valoare" },
            new() { Key = "smartlabs", Name = "SmartLabs", Description = "Format: (COD) Nume | Valoare UM | Interval UM" },
            new() { Key = "elite_medical", Name = "Elite Medical", Description = "Format: Nume (COD) | = Valoare UM | [min - max] / UM" },
            new() { Key = "sanador", Name = "Sanador", Description = "Format: Denumire | Rezultat | UM | Interval" },
            new() { Key = "gral", Name = "Gral Medical", Description = "Format: Denumire | Rezultat | UM | Interval" },
        };
    }
}
