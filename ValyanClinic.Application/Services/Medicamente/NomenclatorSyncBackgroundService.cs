using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ValyanClinic.Application.Services.Medicamente;

/// <summary>
/// Configurare pentru sincronizarea automată a nomenclatorului.
/// </summary>
public class NomenclatorSyncOptions
{
    public const string SectionName = "NomenclatorSync";
    
    /// <summary>
    /// Activează sincronizarea automată.
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Interval de sincronizare în ore (default: 168 = 7 zile).
    /// </summary>
    public int IntervalHours { get; set; } = 168;
    
    /// <summary>
    /// Ora la care să ruleze sincronizarea (0-23, default: 3 AM).
    /// </summary>
    public int PreferredHour { get; set; } = 3;
    
    /// <summary>
    /// Sincronizare la pornirea aplicației dacă este necesară.
    /// </summary>
    public bool SyncOnStartup { get; set; } = true;
}

/// <summary>
/// Background service care sincronizează periodic nomenclatorul ANM.
/// Rulează săptămânal sau la pornirea aplicației dacă este necesar.
/// </summary>
public class NomenclatorSyncBackgroundService : BackgroundService
{
    private readonly ILogger<NomenclatorSyncBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NomenclatorSyncOptions _options;

    public NomenclatorSyncBackgroundService(
        ILogger<NomenclatorSyncBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<NomenclatorSyncOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Sincronizarea automată a nomenclatorului este dezactivată.");
            return;
        }

        _logger.LogInformation(
            "Serviciul de sincronizare nomenclator pornit. Interval: {Hours}h, Ora preferată: {Hour}:00",
            _options.IntervalHours, _options.PreferredHour);

        // Sincronizare la startup dacă e activată
        if (_options.SyncOnStartup)
        {
            // Așteptăm puțin pentru a lăsa aplicația să pornească
            _logger.LogInformation("Aștept 5 secunde înainte de verificarea sincronizării...");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            _logger.LogInformation("Încep verificarea sincronizării...");
            await TrySyncIfNeededAsync(stoppingToken);
        }

        // Loop periodic
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculăm următoarea sincronizare
                var nextSync = CalculateNextSyncTime();
                var delay = nextSync - DateTime.Now;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation(
                        "Următoarea sincronizare nomenclator: {NextSync} (în {Hours}h {Minutes}m)",
                        nextSync, (int)delay.TotalHours, delay.Minutes);

                    await Task.Delay(delay, stoppingToken);
                }

                await TrySyncIfNeededAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutdown graceful
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare în background service nomenclator. Se reîncearcă în 1h.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Serviciul de sincronizare nomenclator s-a oprit.");
    }

    private async Task TrySyncIfNeededAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<INomenclatorMedicamenteService>();

            // Verificăm dacă e nevoie de sincronizare
            var needsSync = await service.NeedsSyncAsync(cancellationToken);
            
            if (!needsSync)
            {
                _logger.LogInformation("Nomenclatorul este actualizat, nu este necesară sincronizarea.");
                return;
            }

            _logger.LogInformation("Se începe sincronizarea nomenclatorului ANM...");
            
            var result = await service.SyncFromANMAsync(cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                _logger.LogInformation(
                    "Sincronizare reușită: {Added} adăugate, {Updated} actualizate, {Deactivated} dezactivate în {Duration}",
                    result.Value.RecordsAdded,
                    result.Value.RecordsUpdated,
                    result.Value.RecordsDeactivated,
                    result.Value.Duration);
            }
            else
            {
                _logger.LogWarning("Sincronizarea a eșuat: {Error}", result.FirstError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la sincronizarea nomenclatorului");
        }
    }

    private DateTime CalculateNextSyncTime()
    {
        var now = DateTime.Now;
        var preferredTime = now.Date.AddHours(_options.PreferredHour);
        
        // Dacă ora preferată e în trecut, mergem la ziua următoare
        if (preferredTime <= now)
        {
            preferredTime = preferredTime.AddDays(1);
        }

        // Apoi adăugăm intervalul
        var intervalDays = _options.IntervalHours / 24.0;
        
        // Găsim următoarea zi care se potrivește cu intervalul
        while ((preferredTime - now).TotalHours < _options.IntervalHours * 0.9)
        {
            preferredTime = preferredTime.AddDays(1);
        }

        return preferredTime;
    }
}
