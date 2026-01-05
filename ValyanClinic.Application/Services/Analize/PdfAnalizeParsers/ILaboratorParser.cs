using ValyanClinic.Application.Features.Analize.Models;

namespace ValyanClinic.Application.Services.Analize.PdfAnalizeParsers;

/// <summary>
/// Interface pentru parsere specifice laboratoarelor
/// </summary>
public interface ILaboratorParser
{
    /// <summary>
    /// Cheia unică a laboratorului
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Numele laboratorului
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Descrierea formatului
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Parsează textul extras din PDF și returnează analizele
    /// </summary>
    ParsePdfResult Parse(string text, string fileName);
}
