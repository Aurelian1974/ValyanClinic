using Syncfusion.Blazor.Notifications;

namespace ValyanClinic.Services;

/// <summary>
/// Service centralizat pentru notificări profesionale în aplicația Blazor
/// Înlocuiește alert() și confirm() cu UI modern
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Afișează notificare de succes
    /// </summary>
    Task ShowSuccessAsync(string message, string? title = null, int timeoutMs = 3000);

    /// <summary>
    /// Afișează notificare de eroare
    /// </summary>
    Task ShowErrorAsync(string message, string? title = null, int timeoutMs = 5000);

    /// <summary>
    /// Afișează notificare de avertizare
    /// </summary>
    Task ShowWarningAsync(string message, string? title = null, int timeoutMs = 4000);

    /// <summary>
    /// Afișează notificare informativă
    /// </summary>
    Task ShowInfoAsync(string message, string? title = null, int timeoutMs = 3000);

    /// <summary>
    /// Înregistrează referința la componenta Toast pentru a putea afișa notificări
    /// </summary>
    void RegisterToast(SfToast toast);
}

public class NotificationService : INotificationService
{
    private SfToast? _toastRef;

    public void RegisterToast(SfToast toast)
    {
        _toastRef = toast;
    }

    public async Task ShowSuccessAsync(string message, string? title = null, int timeoutMs = 3000)
    {
        if (_toastRef == null) return;

        var toastModel = new ToastModel
        {
            Title = title ?? "Succes",
            Content = message,
            CssClass = "e-toast-success",
            Icon = "e-success toast-icons",
            ShowCloseButton = true,
            Timeout = timeoutMs,
            // ✅ ADDED: Poziționare în dreapta-sus
            Position = new ToastPosition { X = "Right", Y = "Top" }
        };

        await _toastRef.ShowAsync(toastModel);
    }

    public async Task ShowErrorAsync(string message, string? title = null, int timeoutMs = 5000)
    {
        if (_toastRef == null) return;

        var toastModel = new ToastModel
        {
            Title = title ?? "Eroare",
            Content = message,
            CssClass = "e-toast-danger",
            Icon = "e-error toast-icons",
            ShowCloseButton = true,
            Timeout = timeoutMs,
            // ✅ ADDED: Poziționare în dreapta-sus
            Position = new ToastPosition { X = "Right", Y = "Top" }
        };

        await _toastRef.ShowAsync(toastModel);
    }

    public async Task ShowWarningAsync(string message, string? title = null, int timeoutMs = 4000)
    {
        if (_toastRef == null) return;

        var toastModel = new ToastModel
        {
            Title = title ?? "Atenție",
            Content = message,
            CssClass = "e-toast-warning",
            Icon = "e-warning toast-icons",
            ShowCloseButton = true,
            Timeout = timeoutMs,
            // ✅ ADDED: Poziționare în dreapta-sus
            Position = new ToastPosition { X = "Right", Y = "Top" }
        };

        await _toastRef.ShowAsync(toastModel);
    }

    public async Task ShowInfoAsync(string message, string? title = null, int timeoutMs = 3000)
    {
        if (_toastRef == null) return;

        var toastModel = new ToastModel
        {
            Title = title ?? "Informație",
            Content = message,
            CssClass = "e-toast-info",
            Icon = "e-info toast-icons",
            ShowCloseButton = true,
            Timeout = timeoutMs,
            // ✅ ADDED: Poziționare în dreapta-sus
            Position = new ToastPosition { X = "Right", Y = "Top" }
        };

        await _toastRef.ShowAsync(toastModel);
    }
}
