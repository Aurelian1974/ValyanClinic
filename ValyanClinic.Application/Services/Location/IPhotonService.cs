using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Services.Location;

/// <summary>
/// Serviciu pentru autocomplete de străzi folosind Photon API (OpenStreetMap).
/// </summary>
/// <remarks>
/// <b>API Endpoint:</b> https://photon.komoot.io/api/
/// <b>Rate Limit:</b> Fără limită strictă (recomandat debounce 300ms în UI)
/// <b>Coverage:</b> ~80-90% pentru zone urbane România
/// </remarks>
public interface IPhotonService
{
    /// <summary>
    /// Caută străzi folosind Photon API.
    /// </summary>
    /// <param name="query">Text de căutare (ex: "calea victoriei bucuresti")</param>
    /// <param name="limit">Numărul maxim de rezultate (default 5)</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de sugestii de străzi</returns>
    Task<Result<IReadOnlyList<StreetSuggestion>>> SearchStreetsAsync(
        string query, 
        int limit = 5, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută străzi într-un oraș specific.
    /// </summary>
    /// <param name="streetName">Numele străzii</param>
    /// <param name="city">Numele orașului</param>
    /// <param name="limit">Numărul maxim de rezultate</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de sugestii de străzi</returns>
    Task<Result<IReadOnlyList<StreetSuggestion>>> SearchStreetsInCityAsync(
        string streetName,
        string city,
        int limit = 5,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Sugestie de stradă returnată de Photon API.
/// </summary>
public record StreetSuggestion
{
    /// <summary>Numele străzii (ex: "Calea Victoriei")</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Numele complet pentru afișare (ex: "Calea Victoriei, București, România")</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Orașul în care se află strada</summary>
    public string? City { get; init; }

    /// <summary>Cartierul/Sectorul</summary>
    public string? District { get; init; }

    /// <summary>Județul/Regiunea</summary>
    public string? State { get; init; }

    /// <summary>Codul poștal (dacă este disponibil)</summary>
    public string? PostCode { get; init; }

    /// <summary>Tipul locației (street, house, city, etc.)</summary>
    public string? OsmType { get; init; }

    /// <summary>Latitudine pentru hartă</summary>
    public double? Latitude { get; init; }

    /// <summary>Longitudine pentru hartă</summary>
    public double? Longitude { get; init; }
}
