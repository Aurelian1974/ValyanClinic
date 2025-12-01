using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services.Draft;

/// <summary>
/// Helper pentru auto-save cu timer în componente Blazor
/// Encapsulează timer logic și Blazor lifecycle management
/// Parte din Hybrid Approach pentru Draft Management
/// </summary>
/// <typeparam name="T">Tipul datelor pentru draft (ex: CreateConsultatieCommand)</typeparam>
public class DraftAutoSaveHelper<T> : IDisposable where T : class
{
    private readonly ILogger<DraftAutoSaveHelper<T>> _logger;
    private readonly int _intervalSeconds;
    private System.Threading.Timer? _timer;
    private Func<Task<bool>>? _shouldSaveCallback;
    private Func<Task>? _saveCallback;
    private bool _isDisposed;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">Logger pentru diagnosticare</param>
    /// <param name="intervalSeconds">Interval în secunde între auto-save-uri (default: 60s)</param>
    public DraftAutoSaveHelper(
        ILogger<DraftAutoSaveHelper<T>> logger,
        int intervalSeconds = 60)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _intervalSeconds = intervalSeconds > 0 ? intervalSeconds : 60;

        _logger.LogDebug("[DraftAutoSave] Helper creat cu interval {Seconds}s", _intervalSeconds);
    }

    /// <summary>
    /// Pornește timer-ul de auto-save
    /// </summary>
    /// <param name="shouldSaveCallback">Callback care returnează true dacă există modificări nesalvate</param>
    /// <param name="saveCallback">Callback care execută salvarea draft-ului</param>
    public void Start(Func<Task<bool>> shouldSaveCallback, Func<Task> saveCallback)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(DraftAutoSaveHelper<T>));
        }

        _shouldSaveCallback = shouldSaveCallback ?? throw new ArgumentNullException(nameof(shouldSaveCallback));
        _saveCallback = saveCallback ?? throw new ArgumentNullException(nameof(saveCallback));

        // Stop orice timer existent
        Stop();

        // Creează noul timer
        _timer = new System.Threading.Timer(
            async _ => await TryAutoSave(),
            null,
            TimeSpan.FromSeconds(_intervalSeconds),
            TimeSpan.FromSeconds(_intervalSeconds)
        );

        _logger.LogInformation("[DraftAutoSave] Timer pornit (interval: {Seconds}s)", _intervalSeconds);
    }

    /// <summary>
    /// Oprește timer-ul de auto-save
    /// </summary>
    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Dispose();
            _timer = null;
            _logger.LogDebug("[DraftAutoSave] Timer oprit");
        }
    }

    /// <summary>
    /// Resetează timer-ul (repornește countdown-ul)
    /// Util când user face o acțiune care ar trebui să reseteze timer-ul
    /// </summary>
    public void Reset()
    {
        if (_timer != null && _shouldSaveCallback != null && _saveCallback != null)
        {
            _logger.LogDebug("[DraftAutoSave] Timer resetat");
            Start(_shouldSaveCallback, _saveCallback);
        }
    }

    /// <summary>
    /// Încearcă să salveze draft-ul automat (apelat de timer)
    /// </summary>
    private async Task TryAutoSave()
    {
        if (_isDisposed || _shouldSaveCallback == null || _saveCallback == null)
        {
            return;
        }

        try
        {
            // Verifică dacă există modificări nesalvate
            var shouldSave = await _shouldSaveCallback();

            if (shouldSave)
            {
                _logger.LogInformation("[DraftAutoSave] Auto-save declanșat (modificări nesalvate detectate)");
                await _saveCallback();
                _logger.LogInformation("[DraftAutoSave] Auto-save executat cu succes");
            }
            else
            {
                _logger.LogDebug("[DraftAutoSave] Skip auto-save (nu există modificări nesalvate)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DraftAutoSave] Eroare la auto-save");
            // Nu propagăm excepția pentru a nu crăpa timer-ul
        }
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _logger.LogDebug("[DraftAutoSave] Disposing helper");
        Stop();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
