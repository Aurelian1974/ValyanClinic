using System.Collections.ObjectModel;

namespace ValyanClinic.Services;

/// <summary>
/// Service for displaying toast notifications throughout the application
/// </summary>
public class ToastService
{
    private readonly List<ToastMessage> _messages = new();
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Event raised when a new toast message is added
    /// </summary>
    public event Action? OnToastChanged;

    /// <summary>
    /// Get all active toast messages
    /// </summary>
    public ReadOnlyCollection<ToastMessage> Messages
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _messages.AsReadOnly();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Show a success toast notification
    /// </summary>
    public void ShowSuccess(string title, string message, int durationMs = 3000)
    {
        ShowToast(ToastType.Success, title, message, durationMs);
    }

    /// <summary>
    /// Show a warning toast notification
    /// </summary>
    public void ShowWarning(string title, string message, int durationMs = 4000)
    {
        ShowToast(ToastType.Warning, title, message, durationMs);
    }

    /// <summary>
    /// Show an error toast notification
    /// </summary>
    public void ShowError(string title, string message, int durationMs = 5000)
    {
        ShowToast(ToastType.Error, title, message, durationMs);
    }

    /// <summary>
    /// Show an info toast notification
    /// </summary>
    public void ShowInfo(string title, string message, int durationMs = 3000)
    {
        ShowToast(ToastType.Info, title, message, durationMs);
    }

    /// <summary>
    /// Show a toast notification with custom type
    /// </summary>
    private void ShowToast(ToastType type, string title, string message, int durationMs)
    {
        var toast = new ToastMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Title = title,
            Message = message,
            DurationMs = durationMs,
            Timestamp = DateTime.Now
        };

        _lock.EnterWriteLock();
        try
        {
            _messages.Add(toast);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        OnToastChanged?.Invoke();

        // Auto-remove after duration
        if (durationMs > 0)
        {
            Task.Delay(durationMs).ContinueWith(_ => RemoveToast(toast.Id));
        }
    }

    /// <summary>
    /// Remove a specific toast message
    /// </summary>
    public void RemoveToast(Guid id)
    {
        _lock.EnterWriteLock();
        try
        {
            var toast = _messages.FirstOrDefault(m => m.Id == id);
            if (toast != null)
            {
                _messages.Remove(toast);
                OnToastChanged?.Invoke();
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Clear all toast messages
    /// </summary>
    public void ClearAll()
    {
        _lock.EnterWriteLock();
        try
        {
            _messages.Clear();
            OnToastChanged?.Invoke();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}

/// <summary>
/// Represents a single toast notification message
/// </summary>
public class ToastMessage
{
    public Guid Id { get; set; }
    public ToastType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Types of toast notifications
/// </summary>
public enum ToastType
{
    Success,
    Warning,
    Error,
    Info
}
