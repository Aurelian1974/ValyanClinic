using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ValyanClinic.Core.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan expiration) where T : class;
}

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = _memoryCache.Get<T>(key);
            _logger.LogDebug("Cache {Action} for key: {Key}", value != null ? "HIT" : "MISS", key);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal
            };

            options.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                _cacheKeys.TryRemove(k.ToString() ?? string.Empty, out _);
                _logger.LogDebug("Cache evicted for key: {Key}, Reason: {Reason}", k, r);
            });

            _memoryCache.Set(key, value, options);
            _cacheKeys.TryAdd(key, true);
            
            _logger.LogDebug("Cache SET for key: {Key}, Expiration: {Expiration}", key, expiration);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            _logger.LogDebug("Cache REMOVE for key: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemovePatternAsync(string pattern)
    {
        try
        {
            var keysToRemove = _cacheKeys.Keys
                .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }

            _logger.LogDebug("Cache REMOVE PATTERN: {Pattern}, Removed {Count} keys", pattern, keysToRemove.Count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
            return Task.CompletedTask;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan expiration) where T : class
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        try
        {
            var value = await getItem();
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", key);
            throw;
        }
    }
}