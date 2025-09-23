using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// State management pentru lookup-urile dependente Județ-Localitate
/// ZERO cod de business în markup - doar state pure
/// </summary>
public class LocationDependentState
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationDependentState> _logger;

    // State properties
    public List<Judet> Judete { get; private set; } = new();
    public List<Localitate> Localitati { get; private set; } = new();
    public int? SelectedJudetId { get; set; }
    public int? SelectedLocalitateId { get; set; }
    public bool IsLoadingJudete { get; private set; }
    public bool IsLoadingLocalitati { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Events pentru comunicarea cu UI-ul
    public event Action? StateChanged;
    public event Action<string?>? JudetNameChanged;
    public event Action<string?>? LocalitateNameChanged;

    public LocationDependentState(ILocationService locationService, ILogger<LocationDependentState> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Încarcă județele la inițializarea componentei
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("🚀 LocationDependentState initialization started");
        
        await LoadJudeteAsync();
        
        _logger.LogInformation("📊 After LoadJudeteAsync - Judete count: {Count}", Judete.Count);
        
        // Dacă avem un județ pre-selectat, încarcă localitățile
        if (SelectedJudetId.HasValue && SelectedJudetId > 0)
        {
            _logger.LogInformation("🔄 Pre-selected judet found: {JudetId}, loading localitati...", SelectedJudetId);
            await LoadLocalitatiAsync(SelectedJudetId.Value);
        }
        else
        {
            _logger.LogInformation("ℹ️ No pre-selected judet, skipping localitati loading");
        }
        
        _logger.LogInformation("✅ LocationDependentState initialization completed successfully");
    }

    /// <summary>
    /// Încarcă județele din baza de date
    /// </summary>
    public async Task LoadJudeteAsync()
    {
        try
        {
            _logger.LogInformation("🔄 Starting to load judete from LocationService...");
            
            IsLoadingJudete = true;
            ErrorMessage = null;
            NotifyStateChanged();

            _logger.LogInformation("📞 Calling LocationService.GetAllJudeteAsync()...");
            var judete = await _locationService.GetAllJudeteAsync();
            
            Judete = judete.ToList();

            _logger.LogInformation("✅ Successfully loaded {Count} judete from database", Judete.Count);
            
            if (Judete.Count > 0)
            {
                _logger.LogInformation("📋 First 3 judete: {Judete}", 
                    string.Join(", ", Judete.Take(3).Select(j => $"{j.IdJudet}-{j.Nume}")));
            }
            else
            {
                _logger.LogWarning("⚠️ No judete loaded from database - this might be a problem");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 CRITICAL ERROR loading judete from LocationService");
            ErrorMessage = "Eroare la încărcarea județelor";
            Judete = new List<Judet>();
        }
        finally
        {
            IsLoadingJudete = false;
            NotifyStateChanged();
            _logger.LogInformation("🏁 LoadJudeteAsync completed - IsLoadingJudete: {IsLoading}, Count: {Count}", 
                IsLoadingJudete, Judete.Count);
        }
    }

    /// <summary>
    /// Încarcă localitățile pentru un județ specificat
    /// </summary>
    public async Task LoadLocalitatiAsync(int judetId)
    {
        try
        {
            IsLoadingLocalitati = true;
            ErrorMessage = null;
            NotifyStateChanged();

            var localitati = await _locationService.GetLocalitatiByJudetIdAsync(judetId);
            Localitati = localitati.ToList();

            _logger.LogInformation("Loaded {Count} localitati for judet {JudetId}", Localitati.Count, judetId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading localitati for judet {JudetId}", judetId);
            ErrorMessage = "Eroare la încărcarea localităților";
            Localitati = new List<Localitate>();
        }
        finally
        {
            IsLoadingLocalitati = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Schimbă județul selectat și resetează localitatea
    /// </summary>
    public async Task ChangeJudetAsync(int? judetId, string? judetName)
    {
        try
        {
            SelectedJudetId = judetId;
            SelectedLocalitateId = null; // Reset localitate
            Localitati.Clear(); // Clear localitati pentru vizualizare

            // Notifică schimbarea numelui de județ
            JudetNameChanged?.Invoke(judetName);
            LocalitateNameChanged?.Invoke(null); // Reset numele localității

            // Încarcă localitățile pentru noul județ
            if (judetId.HasValue && judetId > 0)
            {
                await LoadLocalitatiAsync(judetId.Value);
            }

            _logger.LogInformation("Judet changed to: {JudetId} - {JudetName}", judetId, judetName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing judet to {JudetId}", judetId);
            ErrorMessage = "Eroare la schimbarea județului";
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Schimbă localitatea selectată
    /// </summary>
    public void ChangeLocalitate(int? localitateId, string? localitateName)
    {
        try
        {
            SelectedLocalitateId = localitateId;
            LocalitateNameChanged?.Invoke(localitateName);

            _logger.LogInformation("Localitate changed to: {LocalitateId} - {LocalitateName}", 
                localitateId, localitateName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing localitate to {LocalitateId}", localitateId);
        }
    }

    /// <summary>
    /// Setează județul pe baza numelui (pentru editare)
    /// </summary>
    public async Task SetJudetByNameAsync(string? judetName)
    {
        if (string.IsNullOrEmpty(judetName)) return;

        var judet = Judete.FirstOrDefault(j => j.Nume.Equals(judetName, StringComparison.OrdinalIgnoreCase));
        if (judet != null)
        {
            await ChangeJudetAsync(judet.IdJudet, judet.Nume);
        }
    }

    /// <summary>
    /// Setează localitatea pe baza numelui (pentru editare)
    /// </summary>
    public void SetLocalitateByName(string? localitateName)
    {
        if (string.IsNullOrEmpty(localitateName) || !SelectedJudetId.HasValue) return;

        var localitate = Localitati.FirstOrDefault(l => l.Nume.Equals(localitateName, StringComparison.OrdinalIgnoreCase));
        if (localitate != null)
        {
            ChangeLocalitate(localitate.IdOras, localitate.Nume);
        }
    }

    /// <summary>
    /// Resetează starea componentei
    /// </summary>
    public void Reset()
    {
        SelectedJudetId = null;
        SelectedLocalitateId = null;
        Localitati.Clear();
        ErrorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifică UI-ul despre schimbările de state
    /// </summary>
    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
