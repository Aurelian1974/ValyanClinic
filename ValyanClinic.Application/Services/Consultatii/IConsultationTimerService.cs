using System;
using System.Threading.Tasks;

namespace ValyanClinic.Application.Services.Consultatii;

/// <summary>
/// Serviciu pentru gestionarea timer-ului de consultații.
/// Poate fi reutilizat în orice formular care necesită măsurarea timpului.
/// </summary>
public interface IConsultationTimerService : IAsyncDisposable
{
    /// <summary>
    /// Timpul scurs de la pornirea timer-ului
    /// </summary>
    TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Indică dacă timer-ul rulează
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Indică dacă timer-ul este în pauză
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// Timpul formatat pentru afișare (MM:SS)
    /// </summary>
    string FormattedTime { get; }

    /// <summary>
    /// Clasa CSS pentru warning bazată pe durata consultației
    /// </summary>
    string WarningClass { get; }

    /// <summary>
    /// Event fired când timer-ul se actualizează (o dată pe secundă)
    /// </summary>
    event EventHandler? OnTick;

    /// <summary>
    /// Pornește timer-ul
    /// </summary>
    void Start();

    /// <summary>
    /// Oprește timer-ul complet
    /// </summary>
    void Stop();

    /// <summary>
    /// Pune timer-ul în pauză (păstrează timpul scurs)
    /// </summary>
    void Pause();

    /// <summary>
    /// Reia timer-ul din pauză
    /// </summary>
    void Resume();

    /// <summary>
    /// Toggle între pauză și rezumare
    /// </summary>
    void TogglePause();

    /// <summary>
    /// Resetează timer-ul la zero
    /// </summary>
    void Reset();

    /// <summary>
    /// Obține durata în minute (pentru salvare în DB)
    /// </summary>
    int GetDurationMinutes();
}
