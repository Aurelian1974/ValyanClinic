using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Services.Security;

/// <summary>
/// Service pentru sanitizare HTML si prevenire XSS attacks
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>
    /// Sanitizeaza text pentru afisare sigura in HTML
    /// </summary>
    string Sanitize(string? input);
    
    /// <summary>
    /// Sanitizeaza text si converteste la MarkupString pentru Blazor
    /// </summary>
    MarkupString SanitizeMarkup(string? input);
    
    /// <summary>
    /// Strip toate tag-urile HTML
    /// </summary>
    string StripHtmlTags(string? input);
    
    /// <summary>
    /// Encode pentru JavaScript context
    /// </summary>
    string EncodeForJavaScript(string? input);
}

public class HtmlSanitizerService : IHtmlSanitizerService
{
    // Whitelist pentru tag-uri HTML permise (doar pentru rich text daca e necesar)
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "b", "i", "u", "em", "strong", "br", "p"
    };
    
    // Pattern pentru detectare tag-uri HTML
    private static readonly Regex HtmlTagPattern = new(@"<[^>]*>", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    // Pattern pentru detectare script-uri
    private static readonly Regex ScriptPattern = new(@"<script[^>]*>.*?</script>", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    
    // Pattern pentru detectare event handlers
    private static readonly Regex EventHandlerPattern = new(@"\bon\w+\s*=", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        // 1. Remove script tags
        input = ScriptPattern.Replace(input, string.Empty);
        
        // 2. Remove event handlers (onclick, onerror, etc.)
        input = EventHandlerPattern.Replace(input, string.Empty);
        
        // 3. Strip ALL HTML tags (safest approach)
        input = StripHtmlTags(input);
        
        // 4. Encode special characters
        input = System.Net.WebUtility.HtmlEncode(input);
        
        return input;
    }

    public MarkupString SanitizeMarkup(string? input)
    {
        return new MarkupString(Sanitize(input));
    }

    public string StripHtmlTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        // Remove all HTML tags
        return HtmlTagPattern.Replace(input, string.Empty);
    }

    public string EncodeForJavaScript(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        // JavaScript-safe encoding
        return input
            .Replace("\\", "\\\\")  // Backslash
            .Replace("'", "\\'")    // Single quote
            .Replace("\"", "\\\"")  // Double quote
            .Replace("\n", "\\n")   // Newline
            .Replace("\r", "\\r")   // Carriage return
            .Replace("\t", "\\t")   // Tab
            .Replace("<", "\\x3C")  // Less than
            .Replace(">", "\\x3E")  // Greater than
            .Replace("&", "\\x26"); // Ampersand
    }
}
