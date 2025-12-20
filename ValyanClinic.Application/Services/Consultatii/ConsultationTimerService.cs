using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Services.Consultatii;

/// <summary>
/// Implementare serviciu timer pentru consultații.
/// Gestionează pornire, oprire, pauză și resume cu event-uri pentru UI.
/// </summary>
public class ConsultationTimerService : IConsultationTimerService
{
    private readonly ILogger<ConsultationTimerService> _logger;
    private Timer? _timer;
    private DateTime _startTime;
    private TimeSpan _pausedElapsedTime = TimeSpan.Zero;
    private bool _disposed = false;

    // Praguri pentru warning-uri (minute)
    private const int WARNING_THRESHOLD_MINUTES = 15;
    private const int DANGER_THRESHOLD_MINUTES = 20;

    public ConsultationTimerService(ILogger<ConsultationTimerService> logger)
    {
        _logger = logger;
    }

    public TimeSpan ElapsedTime { get; private set; } = TimeSpan.Zero;
    public bool IsRunning { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;

    public string FormattedTime => $"{(int)ElapsedTime.TotalMinutes:D2}:{ElapsedTime.Seconds:D2}";

    public string WarningClass
    {
        get
        {
            var minutes = (int)ElapsedTime.TotalMinutes;
            if (minutes >= DANGER_THRESHOLD_MINUTES) return "danger";
            if (minutes >= WARNING_THRESHOLD_MINUTES) return "warning";
            return string.Empty;
        }
    }

    public event EventHandler? OnTick;

    public void Start()
    {
        if (IsRunning) return;

        _startTime = DateTime.Now;
        IsRunning = true;
        IsPaused = false;
        _pausedElapsedTime = TimeSpan.Zero;

        _timer = new Timer(
            callback: _ => UpdateElapsedTime(),
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1)
        );

        _logger.LogDebug("Consultation timer started at {StartTime}", _startTime);
    }

    public void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        IsPaused = false;

        _timer?.Dispose();
        _timer = null;

        _logger.LogInformation(
            "Consultation timer stopped. Duration: {Duration} minutes",
            GetDurationMinutes()
        );
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused) return;

        IsPaused = true;
        _pausedElapsedTime = ElapsedTime;

        _timer?.Dispose();
        _timer = null;

        _logger.LogDebug("Consultation timer paused at {ElapsedTime}", ElapsedTime);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        _startTime = DateTime.Now - _pausedElapsedTime;

        _timer = new Timer(
            callback: _ => UpdateElapsedTime(),
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1)
        );

        _logger.LogDebug("Consultation timer resumed at {ElapsedTime}", ElapsedTime);
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Reset()
    {
        Stop();
        ElapsedTime = TimeSpan.Zero;
        _pausedElapsedTime = TimeSpan.Zero;
        _logger.LogDebug("Consultation timer reset");
    }

    public int GetDurationMinutes()
    {
        return (int)ElapsedTime.TotalMinutes;
    }

    private void UpdateElapsedTime()
    {
        if (_disposed || IsPaused) return;

        ElapsedTime = DateTime.Now - _startTime;
        OnTick?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _timer?.Dispose();
        _timer = null;

        await ValueTask.CompletedTask;
        _logger.LogDebug("ConsultationTimerService disposed");
    }
}
