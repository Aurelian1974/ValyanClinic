using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ValyanClinic.Core.Services;

/// <summary>
/// SIMPLE GRID STATE PERSISTENCE SERVICE
/// Implementare simplificată pentru persistența stării grid-ului Syncfusion
/// Folosește ICacheService pentru a păstra setările temporar in memoria aplicatiei
/// </summary>
public interface ISimpleGridStateService
{
    /// <summary>
    /// Salvează setările unui grid (page size, sortare, filtre)
    /// </summary>
    Task SaveGridSettingsAsync(string gridId, Dictionary<string, object> settings);
    
    /// <summary>
    /// Recuperează setările salvate pentru un grid
    /// </summary>
    Task<Dictionary<string, object>?> GetGridSettingsAsync(string gridId);
    
    /// <summary>
    /// Șterge setările pentru un grid specific
    /// </summary>
    Task ClearGridSettingsAsync(string gridId);
}

public class SimpleGridStateService : ISimpleGridStateService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SimpleGridStateService> _logger;
    
    private const string GRID_SETTINGS_PREFIX = "grid_settings_";
    private const int CACHE_EXPIRY_HOURS = 24;

    public SimpleGridStateService(IMemoryCache memoryCache, ILogger<SimpleGridStateService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task SaveGridSettingsAsync(string gridId, Dictionary<string, object> settings)
    {
        try
        {
            var cacheKey = $"{GRID_SETTINGS_PREFIX}{gridId}";
            var expiration = TimeSpan.FromHours(CACHE_EXPIRY_HOURS);
            
            _memoryCache.Set(cacheKey, settings, expiration);
            _logger.LogDebug("Grid settings saved for: {GridId}", gridId);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save grid settings for: {GridId}", gridId);
            return Task.CompletedTask;
        }
    }

    public Task<Dictionary<string, object>?> GetGridSettingsAsync(string gridId)
    {
        try
        {
            var cacheKey = $"{GRID_SETTINGS_PREFIX}{gridId}";
            
            if (_memoryCache.TryGetValue(cacheKey, out var settings) && settings is Dictionary<string, object> gridSettings)
            {
                _logger.LogDebug("Grid settings retrieved for: {GridId}", gridId);
                return Task.FromResult<Dictionary<string, object>?>(gridSettings);
            }
            
            return Task.FromResult<Dictionary<string, object>?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve grid settings for: {GridId}", gridId);
            return Task.FromResult<Dictionary<string, object>?>(null);
        }
    }

    public Task ClearGridSettingsAsync(string gridId)
    {
        try
        {
            var cacheKey = $"{GRID_SETTINGS_PREFIX}{gridId}";
            _memoryCache.Remove(cacheKey);
            _logger.LogDebug("Grid settings cleared for: {GridId}", gridId);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear grid settings for: {GridId}", gridId);
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Simple grid state model pentru persistență
/// </summary>
public class SimpleGridState
{
    public Dictionary<string, object> Settings { get; set; } = new();
    public int PageSize { get; set; } = 20;
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = "ASC";
    public List<SimpleFilter> Filters { get; set; } = new();
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
}

public class SimpleFilter
{
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Operator { get; set; } = "contains";
}

/// <summary>
/// Extension method pentru DI registration
/// </summary>
public static class SimpleGridStateExtensions
{
    public static IServiceCollection AddSimpleGridStateService(this IServiceCollection services)
    {
        services.AddScoped<ISimpleGridStateService, SimpleGridStateService>();
        return services;
    }
}
