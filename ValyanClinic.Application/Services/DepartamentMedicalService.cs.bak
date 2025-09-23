using ValyanClinic.Domain.Models;
using ValyanClinic.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Service implementation pentru încărcarea departamentelor medicale din baza de date
/// Folosește caching simplu în memorie pentru performanță optimă și încarcă doar din stored procedures
/// NU folosește enum-uri statice - totul din baza de date pentru flexibilitate maximă
/// </summary>
public class DepartamentMedicalService : IDepartamentMedicalService
{
    private readonly IDepartamentMedicalRepository _repository;
    private readonly ILogger<DepartamentMedicalService> _logger;
    
    // Simple in-memory cache using static dictionary
    private static readonly Dictionary<string, (DateTime Expiry, object Data)> _cache = new();
    
    // Cache keys pentru optimizare
    private const string CACHE_KEY_ALL_DEPARTAMENTE = "departamente_medicale_all";
    private const string CACHE_KEY_CATEGORII = "departamente_medicale_categorii";
    private const string CACHE_KEY_SPECIALIZARI = "departamente_medicale_specializari";
    private const string CACHE_KEY_SUBSPECIALIZARI = "departamente_medicale_subspecializari";
    private const string CACHE_KEY_CONTAINER = "departamente_medicale_container";
    
    // Cache expiration time - departamentele medicale se schimbă rar
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(4);

    public DepartamentMedicalService(
        IDepartamentMedicalRepository repository,
        ILogger<DepartamentMedicalService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<DepartamentMedical>> GetAllDepartamenteMedicaleAsync()
    {
        try
        {
            return await GetFromCacheOrLoadAsync(CACHE_KEY_ALL_DEPARTAMENTE, async () =>
            {
                _logger.LogInformation("Loading all medical departments from database");
                
                var departamente = await _repository.GetAllDepartamenteMedicaleAsync();
                
                _logger.LogInformation("Loaded {Count} medical departments from database", departamente.Count());
                
                return departamente;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all medical departments");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetCategoriiMedicaleAsync()
    {
        try
        {
            return await GetFromCacheOrLoadAsync(CACHE_KEY_CATEGORII, async () =>
            {
                _logger.LogInformation("Loading medical categories from database");
                
                var categorii = await _repository.GetCategoriiMedicaleAsync();
                
                _logger.LogInformation("Loaded {Count} medical categories from database", categorii.Count());
                
                return categorii;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading medical categories");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSpecializariMedicaleAsync()
    {
        try
        {
            return await GetFromCacheOrLoadAsync(CACHE_KEY_SPECIALIZARI, async () =>
            {
                _logger.LogInformation("Loading medical specializations from database");
                
                var specializari = await _repository.GetSpecializariMedicaleAsync();
                
                _logger.LogInformation("Loaded {Count} medical specializations from database", specializari.Count());
                
                return specializari;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading medical specializations");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSubspecializariMedicaleAsync()
    {
        try
        {
            return await GetFromCacheOrLoadAsync(CACHE_KEY_SUBSPECIALIZARI, async () =>
            {
                _logger.LogInformation("Loading medical subspecializations from database");
                
                var subspecializari = await _repository.GetSubspecializariMedicaleAsync();
                
                _logger.LogInformation("Loaded {Count} medical subspecializations from database", subspecializari.Count());
                
                return subspecializari;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading medical subspecializations");
            throw;
        }
    }

    public async Task<DepartamentMedical?> GetDepartamentMedicalByIdAsync(Guid departamentId)
    {
        try
        {
            _logger.LogInformation("Getting medical department by ID: {DepartamentId}", departamentId);
            
            return await _repository.GetDepartamentMedicalByIdAsync(departamentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical department by ID: {DepartamentId}", departamentId);
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSpecializariPentruCategoriaAsync(Guid categorieId)
    {
        try
        {
            _logger.LogInformation("Getting specializations for category: {CategorieId}", categorieId);
            
            return await _repository.GetSpecializariPentruCategoriaAsync(categorieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting specializations for category: {CategorieId}", categorieId);
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> GetSubspecializariPentruSpecializareaAsync(Guid specializareId)
    {
        try
        {
            _logger.LogInformation("Getting subspecializations for specialization: {SpecializareId}", specializareId);
            
            return await _repository.GetSubspecializariPentruSpecializareaAsync(specializareId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subspecializations for specialization: {SpecializareId}", specializareId);
            throw;
        }
    }

    public async Task<bool> DepartamentMedicalExistsAsync(Guid departamentId)
    {
        try
        {
            _logger.LogInformation("Checking if medical department exists: {DepartamentId}", departamentId);
            
            return await _repository.DepartamentMedicalExistsAsync(departamentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking medical department existence: {DepartamentId}", departamentId);
            return false;
        }
    }

    public async Task<DepartamenteMedicaleContainer> GetDepartamenteMedicaleContainerAsync()
    {
        try
        {
            return await GetFromCacheOrLoadAsync(CACHE_KEY_CONTAINER, async () =>
            {
                _logger.LogInformation("Loading medical departments container from database");
                
                // Încarcă toate tipurile de departamente în paralel pentru performanță
                var categoriiTask = GetCategoriiMedicaleAsync();
                var specializariTask = GetSpecializariMedicaleAsync();
                var subspecializariTask = GetSubspecializariMedicaleAsync();
                var toateTask = GetAllDepartamenteMedicaleAsync();
                
                await Task.WhenAll(categoriiTask, specializariTask, subspecializariTask, toateTask);
                
                var container = new DepartamenteMedicaleContainer
                {
                    Categorii = (await categoriiTask).ToList(),
                    Specializari = (await specializariTask).ToList(),
                    Subspecializari = (await subspecializariTask).ToList(),
                    ToateDepartamentele = (await toateTask).ToList()
                };
                
                _logger.LogInformation("Loaded medical departments container: {TotalCategorii} categories, {TotalSpecializari} specializations, {TotalSubspecializari} subspecializations, {TotalDepartamente} total departments",
                    container.TotalCategorii, container.TotalSpecializari, container.TotalSubspecializari, container.TotalDepartamente);
                
                return container;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading medical departments container");
            throw;
        }
    }

    public async Task<IEnumerable<DepartamentMedical>> SearchDepartamenteMedicaleAsync(string searchText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllDepartamenteMedicaleAsync();
            }
            
            _logger.LogInformation("Searching medical departments with text: {SearchText}", searchText);
            
            return await _repository.SearchDepartamenteMedicaleAsync(searchText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching medical departments with text: {SearchText}", searchText);
            throw;
        }
    }

    public async Task RefreshDepartamenteMedicaleAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing medical departments cache");
            
            // Elimină toate cache-urile pentru departamentele medicale
            lock (_cache)
            {
                _cache.Remove(CACHE_KEY_ALL_DEPARTAMENTE);
                _cache.Remove(CACHE_KEY_CATEGORII);
                _cache.Remove(CACHE_KEY_SPECIALIZARI);
                _cache.Remove(CACHE_KEY_SUBSPECIALIZARI);
                _cache.Remove(CACHE_KEY_CONTAINER);
            }
            
            // Pre-încarcă din nou cache-urile principale pentru performanță
            var preloadTasks = new List<Task>
            {
                GetAllDepartamenteMedicaleAsync().ContinueWith(t => { /* ignore result */ }),
                GetCategoriiMedicaleAsync().ContinueWith(t => { /* ignore result */ }),
                GetSpecializariMedicaleAsync().ContinueWith(t => { /* ignore result */ }),
                GetSubspecializariMedicaleAsync().ContinueWith(t => { /* ignore result */ }),
                GetDepartamenteMedicaleContainerAsync().ContinueWith(t => { /* ignore result */ })
            };
            
            await Task.WhenAll(preloadTasks);
            
            _logger.LogInformation("Medical departments cache refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing medical departments cache");
            throw;
        }
    }
    
    #region Simple Cache Implementation
    
    private async Task<T> GetFromCacheOrLoadAsync<T>(string key, Func<Task<T>> loader) where T : class
    {
        lock (_cache)
        {
            if (_cache.TryGetValue(key, out var cached) && cached.Expiry > DateTime.Now)
            {
                return (T)cached.Data;
            }
        }
        
        var data = await loader();
        
        lock (_cache)
        {
            _cache[key] = (DateTime.Now.Add(_cacheExpiration), data);
        }
        
        return data;
    }
    
    #endregion
}
