using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Services.Caching;

/// <summary>
/// Implementation pentru query caching cu Memory Cache
/// </summary>
public class QueryCacheService : IQueryCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<QueryCacheService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    // Default cache settings
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan FilterOptionsExpiration = TimeSpan.FromMinutes(30);

    public QueryCacheService(
        IMemoryCache memoryCache,
        ILogger<QueryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        // Try get from cache
        if (_memoryCache.TryGetValue(cacheKey, out T? cachedValue))
        {
            _logger.LogDebug("Cache HIT for key: {CacheKey}", cacheKey);
            return cachedValue;
        }

        _logger.LogDebug("Cache MISS for key: {CacheKey}", cacheKey);

        // Execute factory and cache result
        var value = await factory();

        if (value != null)
        {
            var cacheExpiration = expiration ?? DefaultExpiration;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(cacheExpiration)
                .SetSlidingExpiration(cacheExpiration / 2)
                .RegisterPostEvictionCallback((key, val, reason, state) =>
                {
                    _logger.LogDebug("Cache evicted: {Key}, Reason: {Reason}", key, reason);
                    lock (_lock)
                    {
                        _cacheKeys.Remove(key.ToString()!);
                    }
                });

            _memoryCache.Set(cacheKey, value, cacheEntryOptions);

            lock (_lock)
            {
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogInformation("Cached result for key: {CacheKey}, Expiration: {Expiration}",
                cacheKey, cacheExpiration);
        }

        return value;
    }

    public void InvalidateByPattern(string pattern)
    {
        _logger.LogInformation("Invalidating cache by pattern: {Pattern}", pattern);

        lock (_lock)
        {
            var keysToRemove = _cacheKeys
                .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
                _logger.LogDebug("Removed cache key: {Key}", key);
            }

            _logger.LogInformation("Invalidated {Count} cache entries for pattern: {Pattern}",
                keysToRemove.Count, pattern);
        }
    }

    public void InvalidateEntityCache(string entityName)
    {
        _logger.LogInformation("Invalidating all cache for entity: {EntityName}", entityName);
        InvalidateByPattern($":{entityName}:");
    }

    public void ClearAll()
    {
        _logger.LogWarning("Clearing ALL cache");

        lock (_lock)
        {
            foreach (var key in _cacheKeys.ToList())
            {
                _memoryCache.Remove(key);
            }

            _cacheKeys.Clear();
        }

        _logger.LogInformation("All cache cleared");
    }
}

/// <summary>
/// Helper pentru generare cache keys consistente
/// </summary>
public static class CacheKeyHelper
{
    public static string GenerateKey(string entityName, params object[] parameters)
    {
        var paramString = string.Join(":", parameters.Select(p => p?.ToString() ?? "null"));
        return $"Query:{entityName}:{paramString}";
    }

    public static string GeneratePagedQueryKey(
        string entityName,
        int pageNumber,
        int pageSize,
        string? search = null,
        params (string name, string? value)[] filters)
    {
        var parts = new List<string>
        {
            "Query",
            entityName,
            $"Page{pageNumber}",
            $"Size{pageSize}"
        };

        if (!string.IsNullOrEmpty(search))
        {
            parts.Add($"Search:{search}");
        }

        foreach (var (name, value) in filters.Where(f => !string.IsNullOrEmpty(f.value)))
        {
            parts.Add($"{name}:{value}");
        }

        return string.Join(":", parts);
    }

    public static string GenerateFilterOptionsKey(string entityName)
    {
        return $"FilterOptions:{entityName}";
    }
}
