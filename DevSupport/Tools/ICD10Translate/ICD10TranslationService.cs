using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DevSupport.ICD10Translate;

/// <summary>
/// Serviciu pentru traducerea textelor medicale din engleză în română
/// Suportă multiple metode: Google Translate gratuit, Azure, DeepL, LibreTranslate
/// </summary>
public class ICD10TranslationService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TranslationProvider _provider;
    private readonly string? _apiKey;
    private readonly string? _region;
    private readonly string? _endpoint;
    private bool _disposed;
    private int _requestCount;
    private DateTime _lastRequestTime = DateTime.MinValue;

    public enum TranslationProvider
    {
        GoogleFree,      // Google Translate gratuit (fără API key)
        AzureTranslator,
        GoogleCloud,
        LibreTranslate,
        DeepL,
        Manual           // Pentru import din CSV/fișiere traduse manual
    }

    public ICD10TranslationService(
        TranslationProvider provider, 
        string? apiKey = null, 
        string? region = null,
        string? endpoint = null)
    {
        _provider = provider;
        _apiKey = apiKey;
        _region = region ?? "westeurope";
        _endpoint = endpoint ?? GetDefaultEndpoint(provider);
        
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Adaugă headers pentru a simula un browser (necesar pentru Google Free)
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,ro;q=0.8");
    }

    private static string GetDefaultEndpoint(TranslationProvider provider) => provider switch
    {
        TranslationProvider.GoogleFree => "https://translate.googleapis.com",
        TranslationProvider.AzureTranslator => "https://api.cognitive.microsofttranslator.com",
        TranslationProvider.LibreTranslate => "https://libretranslate.com",
        TranslationProvider.DeepL => "https://api-free.deepl.com",
        _ => ""
    };

    /// <summary>
    /// Traduce un singur text din engleză în română
    /// </summary>
    public async Task<string?> TranslateAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return _provider switch
        {
            TranslationProvider.GoogleFree => await TranslateWithGoogleFreeAsync(text),
            TranslationProvider.AzureTranslator => await TranslateWithAzureAsync(text),
            TranslationProvider.LibreTranslate => await TranslateWithLibreAsync(text),
            TranslationProvider.DeepL => await TranslateWithDeepLAsync(text),
            _ => null
        };
    }

    /// <summary>
    /// Traduce un batch de texte (mai eficient pentru API-uri)
    /// </summary>
    public async Task<List<string?>> TranslateBatchAsync(List<string> texts)
    {
        if (texts == null || texts.Count == 0)
            return new List<string?>();

        return _provider switch
        {
            TranslationProvider.GoogleFree => await TranslateBatchWithGoogleFreeAsync(texts),
            TranslationProvider.AzureTranslator => await TranslateBatchWithAzureAsync(texts),
            TranslationProvider.LibreTranslate => await TranslateBatchSequentialAsync(texts),
            TranslationProvider.DeepL => await TranslateBatchWithDeepLAsync(texts),
            _ => new List<string?>(new string?[texts.Count])
        };
    }

    #region Google Translate Free (fără API key)

    /// <summary>
    /// Traduce folosind Google Translate gratuit (endpoint public)
    /// Limitare: ~100 request-uri pe minut pentru a evita rate limiting
    /// </summary>
    private async Task<string?> TranslateWithGoogleFreeAsync(string text)
    {
        try
        {
            // Rate limiting - așteaptă între request-uri
            await RateLimitAsync();

            var encodedText = HttpUtility.UrlEncode(text);
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=ro&dt=t&q={encodedText}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine("⚠️ Rate limit atins, aștept 60 secunde...");
                await Task.Delay(60000);
                response = await _httpClient.GetAsync(url);
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Google Translate error: {response.StatusCode}");
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            
            // Parse JSON response - format: [[["traducere","original",null,null,10]],null,"en",...]
            var translation = ParseGoogleFreeResponse(responseBody);
            return translation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Eroare Google Translate: {ex.Message}");
            return null;
        }
    }

    private string? ParseGoogleFreeResponse(string json)
    {
        try
        {
            // Răspunsul este un array complex, extrage textul tradus
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                return null;

            var translations = root[0];
            if (translations.ValueKind != JsonValueKind.Array)
                return null;

            var result = new StringBuilder();
            foreach (var segment in translations.EnumerateArray())
            {
                if (segment.ValueKind == JsonValueKind.Array && segment.GetArrayLength() > 0)
                {
                    var translatedText = segment[0].GetString();
                    if (!string.IsNullOrEmpty(translatedText))
                    {
                        result.Append(translatedText);
                    }
                }
            }

            return result.Length > 0 ? result.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<string?>> TranslateBatchWithGoogleFreeAsync(List<string> texts)
    {
        var results = new List<string?>();
        var totalTexts = texts.Count;
        var currentIndex = 0;

        foreach (var text in texts)
        {
            currentIndex++;
            var translation = await TranslateWithGoogleFreeAsync(text);
            results.Add(translation);

            // Afișează progres la fiecare 10 traduceri
            if (currentIndex % 10 == 0)
            {
                Console.Write($"\r   Traducere: {currentIndex}/{totalTexts}");
            }
        }

        Console.WriteLine(); // Linie nouă după progres
        return results;
    }

    private async Task RateLimitAsync()
    {
        _requestCount++;
        
        // Așteaptă minim 500ms între request-uri pentru a evita rate limiting
        var timeSinceLastRequest = DateTime.Now - _lastRequestTime;
        if (timeSinceLastRequest.TotalMilliseconds < 500)
        {
            await Task.Delay(500 - (int)timeSinceLastRequest.TotalMilliseconds);
        }

        // La fiecare 50 de request-uri, pauză mai lungă
        if (_requestCount % 50 == 0)
        {
            Console.WriteLine($"   ⏳ Pauză scurtă pentru rate limiting ({_requestCount} request-uri)...");
            await Task.Delay(5000);
        }

        _lastRequestTime = DateTime.Now;
    }

    #endregion

    #region Azure Translator

    private async Task<string?> TranslateWithAzureAsync(string text)
    {
        var results = await TranslateBatchWithAzureAsync(new List<string> { text });
        return results.Count > 0 ? results[0] : null;
    }

    private async Task<List<string?>> TranslateBatchWithAzureAsync(List<string> texts)
    {
        var results = new List<string?>();
        
        if (string.IsNullOrEmpty(_apiKey))
        {
            Console.WriteLine("⚠️ Azure API key lipsește!");
            return new List<string?>(new string?[texts.Count]);
        }

        try
        {
            var route = "/translate?api-version=3.0&from=en&to=ro";
            var uri = new Uri(_endpoint + route);

            var body = texts.Select(t => new { Text = t }).ToArray();
            var requestBody = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                foreach (var item in root.EnumerateArray())
                {
                    var translations = item.GetProperty("translations");
                    if (translations.GetArrayLength() > 0)
                    {
                        var translatedText = translations[0].GetProperty("text").GetString();
                        results.Add(translatedText);
                    }
                    else
                    {
                        results.Add(null);
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ Azure Translation error: {response.StatusCode} - {responseBody}");
                results.AddRange(new string?[texts.Count]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Eroare Azure Translation: {ex.Message}");
            results.AddRange(new string?[texts.Count]);
        }

        return results;
    }

    #endregion

    #region LibreTranslate

    private async Task<string?> TranslateWithLibreAsync(string text)
    {
        try
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                q = text,
                source = "en",
                target = "ro",
                format = "text",
                api_key = _apiKey ?? ""
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_endpoint}/translate", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                return doc.RootElement.GetProperty("translatedText").GetString();
            }
            else
            {
                Console.WriteLine($"❌ LibreTranslate error: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Eroare LibreTranslate: {ex.Message}");
            return null;
        }
    }

    private async Task<List<string?>> TranslateBatchSequentialAsync(List<string> texts)
    {
        var results = new List<string?>();
        foreach (var text in texts)
        {
            results.Add(await TranslateWithLibreAsync(text));
            await Task.Delay(100);
        }
        return results;
    }

    #endregion

    #region DeepL

    private async Task<string?> TranslateWithDeepLAsync(string text)
    {
        var results = await TranslateBatchWithDeepLAsync(new List<string> { text });
        return results.Count > 0 ? results[0] : null;
    }

    private async Task<List<string?>> TranslateBatchWithDeepLAsync(List<string> texts)
    {
        var results = new List<string?>();

        if (string.IsNullOrEmpty(_apiKey))
        {
            Console.WriteLine("⚠️ DeepL API key lipsește!");
            return new List<string?>(new string?[texts.Count]);
        }

        try
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("auth_key", _apiKey),
                new("source_lang", "EN"),
                new("target_lang", "RO")
            }.Concat(texts.Select(t => new KeyValuePair<string, string>("text", t))));

            var response = await _httpClient.PostAsync($"{_endpoint}/v2/translate", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var translations = doc.RootElement.GetProperty("translations");

                foreach (var translation in translations.EnumerateArray())
                {
                    results.Add(translation.GetProperty("text").GetString());
                }
            }
            else
            {
                Console.WriteLine($"❌ DeepL error: {response.StatusCode} - {responseBody}");
                results.AddRange(new string?[texts.Count]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Eroare DeepL: {ex.Message}");
            results.AddRange(new string?[texts.Count]);
        }

        return results;
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}
