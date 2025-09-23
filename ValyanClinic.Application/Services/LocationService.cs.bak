using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Interfaces;
using ValyanClinic.Domain.Models;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Service simplificat pentru gestionarea județelor și localităților
/// </summary>
public class LocationService : ILocationService
{
    private readonly IJudetRepository _judetRepository;
    private readonly ILocalitateRepository _localitateRepository;
    private readonly ILogger<LocationService> _logger;

    public LocationService(
        IJudetRepository judetRepository,
        ILocalitateRepository localitateRepository,
        ILogger<LocationService> logger)
    {
        _judetRepository = judetRepository;
        _localitateRepository = localitateRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Judet>> GetAllJudeteAsync()
    {
        try
        {
            _logger.LogInformation("🚀 LocationService.GetAllJudeteAsync() called");
            _logger.LogInformation("📞 Calling _judetRepository.GetOrderedByNameAsync()...");
            
            var result = await _judetRepository.GetOrderedByNameAsync();
            var judeteList = result.ToList();
            
            _logger.LogInformation("✅ LocationService retrieved {Count} judete from repository", judeteList.Count);
            
            if (judeteList.Count > 0)
            {
                _logger.LogInformation("📋 Sample judete: {Sample}", 
                    string.Join(", ", judeteList.Take(3).Select(j => $"{j.IdJudet}-{j.Nume}")));
            }
            else
            {
                _logger.LogError("💥 CRITICAL: LocationService received 0 judete from repository!");
            }
            
            return judeteList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 FATAL ERROR in LocationService.GetAllJudeteAsync()");
            throw;
        }
    }

    public async Task<IEnumerable<Localitate>> GetLocalitatiByJudetIdAsync(int judetId)
    {
        try
        {
            _logger.LogInformation("Retrieving localitati for judet ID: {JudetId}", judetId);
            
            if (judetId <= 0)
            {
                return Enumerable.Empty<Localitate>();
            }

            return await _localitateRepository.GetByJudetIdOrderedAsync(judetId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localitati for judet ID: {JudetId}", judetId);
            throw;
        }
    }

    public async Task<Judet?> GetJudetByNameAsync(string nume)
    {
        try
        {
            var judete = await _judetRepository.GetAllAsync();
            return judete.FirstOrDefault(j => j.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving judet by name: {Nume}", nume);
            throw;
        }
    }

    public async Task<Localitate?> GetLocalitateByNameAndJudetAsync(string nume, int judetId)
    {
        try
        {
            var localitati = await _localitateRepository.GetByJudetIdOrderedAsync(judetId);
            return localitati.FirstOrDefault(l => l.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localitate by name: {Nume} for judet: {JudetId}", nume, judetId);
            throw;
        }
    }
}
