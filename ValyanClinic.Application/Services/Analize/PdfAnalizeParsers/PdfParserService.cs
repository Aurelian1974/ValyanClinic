using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using ValyanClinic.Application.Features.Analize.Models;

namespace ValyanClinic.Application.Services.Analize.PdfAnalizeParsers;

/// <summary>
/// Serviciu principal pentru parsarea PDF-urilor de analize medicale
/// Înlocuiește dependența de Python API
/// </summary>
public interface IPdfParserService
{
    /// <summary>
    /// Parsează un fișier PDF și extrage analizele
    /// </summary>
    Task<ParsePdfResult> ParsePdfAsync(byte[] pdfBytes, string fileName);

    /// <summary>
    /// Parsează un fișier PDF dintr-un stream
    /// </summary>
    Task<ParsePdfResult> ParsePdfAsync(Stream pdfStream, string fileName);

    /// <summary>
    /// Parsează text deja extras (pentru debugging)
    /// </summary>
    ParsePdfResult ParseText(string text, string fileName);
}

/// <summary>
/// Implementare cu PdfPig pentru parsare nativă C#
/// </summary>
public class PdfParserService : IPdfParserService
{
    private readonly UniversalParser _universalParser;

    public PdfParserService()
    {
        _universalParser = new UniversalParser();
    }

    public async Task<ParsePdfResult> ParsePdfAsync(byte[] pdfBytes, string fileName)
    {
        return await Task.Run(() => ParsePdfInternal(pdfBytes, fileName));
    }

    public async Task<ParsePdfResult> ParsePdfAsync(Stream pdfStream, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);
        return await ParsePdfAsync(memoryStream.ToArray(), fileName);
    }

    public ParsePdfResult ParseText(string text, string fileName)
    {
        return _universalParser.Parse(text, fileName);
    }

    private ParsePdfResult ParsePdfInternal(byte[] pdfBytes, string fileName)
    {
        try
        {
            // Extrage text din PDF cu PdfPig
            var text = ExtractTextFromPdf(pdfBytes);

            if (string.IsNullOrWhiteSpace(text))
            {
                return new ParsePdfResult
                {
                    Success = false,
                    Error = "Nu s-a putut extrage text din PDF. Documentul poate fi scanat sau protejat."
                };
            }

            // Parsează textul
            var result = _universalParser.Parse(text, fileName);

            // Adaugă informații suplimentare
            result.TextExtras = text;
            result.ParserUtilizat = _universalParser.Name;

            return result;
        }
        catch (Exception ex)
        {
            return new ParsePdfResult
            {
                Success = false,
                Error = $"Eroare la parsarea PDF: {ex.Message}"
            };
        }
    }

    private string ExtractTextFromPdf(byte[] pdfBytes)
    {
        using var document = PdfDocument.Open(pdfBytes);
        var textBuilder = new System.Text.StringBuilder();

        foreach (var page in document.GetPages())
        {
            // Extrage text păstrând ordinea vizuală pe cât posibil
            var pageText = GetPageTextWithStructure(page);
            textBuilder.AppendLine(pageText);
            textBuilder.AppendLine(); // Separator între pagini
        }

        return textBuilder.ToString();
    }

    private string GetPageTextWithStructure(Page page)
    {
        // Grupăm cuvintele pe linii (pe baza poziției Y)
        var words = page.GetWords().ToList();
        if (!words.Any())
            return page.Text;

        var lines = new List<(double Y, List<Word> Words)>();
        var tolerance = 3.0; // Toleranță pentru gruparea pe aceeași linie

        foreach (var word in words.OrderByDescending(w => w.BoundingBox.Bottom))
        {
            var existingLine = lines.FirstOrDefault(l => Math.Abs(l.Y - word.BoundingBox.Bottom) < tolerance);
            if (existingLine.Words != null)
            {
                existingLine.Words.Add(word);
            }
            else
            {
                lines.Add((word.BoundingBox.Bottom, new List<Word> { word }));
            }
        }

        // Sortăm cuvintele pe fiecare linie de la stânga la dreapta
        var textBuilder = new System.Text.StringBuilder();
        foreach (var line in lines.OrderByDescending(l => l.Y))
        {
            var sortedWords = line.Words.OrderBy(w => w.BoundingBox.Left);
            var lineText = string.Join(" ", sortedWords.Select(w => w.Text));
            textBuilder.AppendLine(lineText);
        }

        return textBuilder.ToString();
    }
}
