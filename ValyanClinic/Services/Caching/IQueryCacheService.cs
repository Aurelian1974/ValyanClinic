namespace ValyanClinic.Services.Caching;

/// <summary>
/// Service pentru caching query results pentru optimizare performanță
/// </summary>
public interface IQueryCacheService
{
    /// <summary>
    /// Obține valoare din cache sau execută funcția și cache-uiește rezultatul
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(
        string cacheKey, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null);

    /// <summary>
    /// Invalidează cache pentru un pattern specific
    /// </summary>
    void InvalidateByPattern(string pattern);

    /// <summary>
    /// Invalidează cache complet pentru un entity type
    /// </summary>
    void InvalidateEntityCache(string entityName);

    /// <summary>
    /// Clear all cache
    /// </summary>
    void ClearAll();
}
