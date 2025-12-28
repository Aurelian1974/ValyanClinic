using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Services.Location;

/// <summary>
/// Implementare Photon API pentru autocomplete de străzi.
/// </summary>
/// <remarks>
/// Photon este un API gratuit bazat pe OpenStreetMap, optimizat pentru autocomplete.
/// Endpoint: https://photon.komoot.io/api/
/// </remarks>
public class PhotonService : IPhotonService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PhotonService> _logger;
    
    private const string PhotonApiUrl = "https://photon.komoot.io/api/";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PhotonService(HttpClient httpClient, ILogger<PhotonService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<StreetSuggestion>>> SearchStreetsAsync(
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
        {
            return Result<IReadOnlyList<StreetSuggestion>>.Success(Array.Empty<StreetSuggestion>());
        }

        try
        {
            var encodedQuery = HttpUtility.UrlEncode(query);
            var url = $"{PhotonApiUrl}?q={encodedQuery}&limit={limit}";
            
            _logger.LogDebug("Photon API request: {Url}", url);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var photonResponse = await response.Content.ReadFromJsonAsync<PhotonResponse>(JsonOptions, cancellationToken);
            
            if (photonResponse?.Features == null)
            {
                return Result<IReadOnlyList<StreetSuggestion>>.Success(Array.Empty<StreetSuggestion>());
            }

            // Mapam si filtram rezultatele
            var allSuggestions = photonResponse.Features
                .Where(f => f.Properties != null)
                .Select(MapToStreetSuggestion)
                .Where(s => !string.IsNullOrEmpty(s.Name))
                .ToList();
            
            // Deduplica pe baza Nume+Oras+Cartier (pastram primul rezultat pentru fiecare combinatie)
            var suggestions = allSuggestions
                .GroupBy(s => new { 
                    Name = s.Name?.ToLowerInvariant(), 
                    City = s.City?.ToLowerInvariant(),
                    District = s.District?.ToLowerInvariant() 
                })
                .Select(g => g.First())
                .ToList();

            _logger.LogDebug("Photon returned {Count} suggestions ({RawCount} raw) for query '{Query}'", 
                suggestions.Count, allSuggestions.Count, query);
            
            return Result<IReadOnlyList<StreetSuggestion>>.Success(suggestions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Photon API request failed for query '{Query}'", query);
            return Result<IReadOnlyList<StreetSuggestion>>.Failure($"Eroare la conectarea cu serviciul de căutare: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("Photon request cancelled for query '{Query}'", query);
            return Result<IReadOnlyList<StreetSuggestion>>.Success(Array.Empty<StreetSuggestion>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching streets with Photon for query '{Query}'", query);
            return Result<IReadOnlyList<StreetSuggestion>>.Failure($"Eroare neașteptată: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<StreetSuggestion>>> SearchStreetsInCityAsync(
        string streetName,
        string city,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streetName) || streetName.Length < 3)
        {
            return Result<IReadOnlyList<StreetSuggestion>>.Success(Array.Empty<StreetSuggestion>());
        }

        // Combinăm strada cu orașul pentru rezultate mai precise
        var query = string.IsNullOrWhiteSpace(city) 
            ? streetName 
            : $"{streetName} {city}";

        return await SearchStreetsAsync(query, limit, cancellationToken);
    }

    private static StreetSuggestion MapToStreetSuggestion(PhotonFeature feature)
    {
        var props = feature.Properties!;
        
        // Construim DisplayName din componente disponibile
        var displayParts = new List<string>();
        
        if (!string.IsNullOrEmpty(props.Name))
            displayParts.Add(props.Name);
        
        if (!string.IsNullOrEmpty(props.Street) && props.Street != props.Name)
            displayParts.Add(props.Street);
        
        // Adaugam cartierul pentru a diferentia segmentele aceleiasi strazi
        if (!string.IsNullOrEmpty(props.District))
            displayParts.Add($"({props.District})");
            
        if (!string.IsNullOrEmpty(props.City))
            displayParts.Add(props.City);
            
        if (!string.IsNullOrEmpty(props.State))
            displayParts.Add(props.State);
            
        if (!string.IsNullOrEmpty(props.Country))
            displayParts.Add(props.Country);

        var displayName = string.Join(", ", displayParts);

        // Pentru tipul street, folosim Name ca nume de stradă
        // Pentru alte tipuri (house, etc.) folosim Street dacă există
        var streetName = props.OsmType == "N" && !string.IsNullOrEmpty(props.Street) 
            ? props.Street 
            : props.Name ?? string.Empty;

        return new StreetSuggestion
        {
            Name = streetName,
            DisplayName = displayName,
            City = props.City,
            District = props.District,
            State = props.State,
            PostCode = props.Postcode,
            OsmType = props.OsmType,
            Latitude = feature.Geometry?.Coordinates?.Length > 1 ? feature.Geometry.Coordinates[1] : null,
            Longitude = feature.Geometry?.Coordinates?.Length > 0 ? feature.Geometry.Coordinates[0] : null
        };
    }

    #region Photon API Response Models

    private class PhotonResponse
    {
        [JsonPropertyName("features")]
        public List<PhotonFeature>? Features { get; set; }
    }

    private class PhotonFeature
    {
        [JsonPropertyName("properties")]
        public PhotonProperties? Properties { get; set; }
        
        [JsonPropertyName("geometry")]
        public PhotonGeometry? Geometry { get; set; }
    }

    private class PhotonProperties
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("street")]
        public string? Street { get; set; }
        
        [JsonPropertyName("city")]
        public string? City { get; set; }
        
        [JsonPropertyName("district")]
        public string? District { get; set; }
        
        [JsonPropertyName("state")]
        public string? State { get; set; }
        
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        
        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }
        
        [JsonPropertyName("osm_type")]
        public string? OsmType { get; set; }
        
        [JsonPropertyName("osm_key")]
        public string? OsmKey { get; set; }
        
        [JsonPropertyName("osm_value")]
        public string? OsmValue { get; set; }
    }

    private class PhotonGeometry
    {
        [JsonPropertyName("coordinates")]
        public double[]? Coordinates { get; set; }
    }

    #endregion
}
