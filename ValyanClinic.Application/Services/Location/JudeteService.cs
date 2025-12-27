using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Services.Location;

/// <summary>
/// Implementare a IJudeteService care încarcă județele și localitățile din baza de date
/// și le cache-uiește pentru performanță.
/// </summary>
public class JudeteService : IJudeteService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<JudeteService> _logger;

    // Cache keys
    private const string JudeteCacheKey = "JudeteService_AllJudete";
    private const string LocalitatiCacheKeyPrefix = "JudeteService_Localitati_";
    
    // Cache durations
    private static readonly TimeSpan JudeteCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan LocalitatiCacheDuration = TimeSpan.FromMinutes(30);

    // Romanian culture for proper sorting
    private static readonly System.Globalization.CultureInfo RomanianCulture = 
        new System.Globalization.CultureInfo("ro-RO");

    public JudeteService(
        ILocationRepository locationRepository, 
        IMemoryCache cache, 
        ILogger<JudeteService> logger)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<JudetDto>>> GetJudeteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try cache first
            if (_cache.TryGetValue(JudeteCacheKey, out IReadOnlyList<JudetDto>? cachedJudete) && cachedJudete != null)
            {
                _logger.LogDebug("[JudeteService] Returning {Count} judete from CACHE", cachedJudete.Count);
                return Result<IReadOnlyList<JudetDto>>.Success(cachedJudete);
            }

            _logger.LogInformation("[JudeteService] Loading judete from DATABASE (cache miss)");

            // Load from database
            var judeteFromDb = await _locationRepository.GetJudeteAsync(cancellationToken);

            // Map and sort with Romanian culture
            var judete = judeteFromDb
                .Select(j => new JudetDto { Id = j.Id, Nume = j.Nume })
                .OrderBy(j => j.Nume, StringComparer.Create(RomanianCulture, ignoreCase: true))
                .ToList()
                .AsReadOnly();

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(JudeteCacheDuration)
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(JudeteCacheKey, judete, cacheOptions);

            _logger.LogInformation("[JudeteService] Loaded and cached {Count} judete for {Duration}h", 
                judete.Count, JudeteCacheDuration.TotalHours);

            return Result<IReadOnlyList<JudetDto>>.Success(judete);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[JudeteService] GetJudete operation cancelled");
            return Result<IReadOnlyList<JudetDto>>.Failure("Operație anulată");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JudeteService] Error loading judete from database");
            return Result<IReadOnlyList<JudetDto>>.Failure($"Eroare la încărcarea județelor: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<LocalitateDto>>> GetLocalitatiByJudetAsync(
        int judetId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{LocalitatiCacheKeyPrefix}{judetId}";

            // Try cache first
            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<LocalitateDto>? cachedLocalitati) && cachedLocalitati != null)
            {
                _logger.LogDebug("[JudeteService] Returning {Count} localitati for judet {JudetId} from CACHE", 
                    cachedLocalitati.Count, judetId);
                return Result<IReadOnlyList<LocalitateDto>>.Success(cachedLocalitati);
            }

            _logger.LogInformation("[JudeteService] Loading localitati for judet {JudetId} from DATABASE", judetId);

            // Load from database
            var localitatiFromDb = await _locationRepository.GetLocalitatiByJudetIdAsync(judetId, cancellationToken);

            // Map and sort
            var localitati = localitatiFromDb
                .Select(l => new LocalitateDto { Id = l.Id, Nume = l.Nume, JudetId = judetId })
                .OrderBy(l => l.Nume, StringComparer.Create(RomanianCulture, ignoreCase: true))
                .ToList()
                .AsReadOnly();

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(LocalitatiCacheDuration)
                .SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(cacheKey, localitati, cacheOptions);

            _logger.LogInformation("[JudeteService] Loaded and cached {Count} localitati for judet {JudetId}", 
                localitati.Count, judetId);

            return Result<IReadOnlyList<LocalitateDto>>.Success(localitati);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[JudeteService] GetLocalitati operation cancelled");
            return Result<IReadOnlyList<LocalitateDto>>.Failure("Operație anulată");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JudeteService] Error loading localitati for judet {JudetId}", judetId);
            return Result<IReadOnlyList<LocalitateDto>>.Failure($"Eroare la încărcarea localităților: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<LocalitateDto>>> GetLocalitatiByJudetNameAsync(
        string judetNume, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(judetNume))
            {
                return Result<IReadOnlyList<LocalitateDto>>.Success(Array.Empty<LocalitateDto>().AsReadOnly());
            }

            // First get all judete to find the ID
            var judeteResult = await GetJudeteAsync(cancellationToken);
            if (!judeteResult.IsSuccess)
            {
                return Result<IReadOnlyList<LocalitateDto>>.Failure(judeteResult.Errors);
            }

            var judet = judeteResult.Value.FirstOrDefault(j => 
                j.Nume.Equals(judetNume, StringComparison.OrdinalIgnoreCase));

            if (judet == null)
            {
                _logger.LogWarning("[JudeteService] Judet '{JudetNume}' not found", judetNume);
                return Result<IReadOnlyList<LocalitateDto>>.Success(Array.Empty<LocalitateDto>().AsReadOnly());
            }

            return await GetLocalitatiByJudetAsync(judet.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JudeteService] Error getting localitati for judet name '{JudetNume}'", judetNume);
            return Result<IReadOnlyList<LocalitateDto>>.Failure($"Eroare: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public void InvalidateCache()
    {
        _cache.Remove(JudeteCacheKey);
        // Note: Localitati cache entries will expire naturally
        _logger.LogInformation("[JudeteService] Cache invalidated");
    }
}
