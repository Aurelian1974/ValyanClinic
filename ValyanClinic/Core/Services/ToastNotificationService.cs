using Syncfusion.Blazor.Notifications;

namespace ValyanClinic.Core.Services;

/// <summary>
/// SERVICE pentru gestionarea centralizata a toast-urilor in aplicatia Blazor
/// Rezolva problema cu toast-urile blurate in modale
/// </summary>
public interface IToastNotificationService
{
    /// <summary>
    /// inregistreaza instanta de toast pentru utilizare globala
    /// </summary>
    void RegisterToast(SfToast toast);
    
    /// <summary>
    /// Afiseaza toast de succes
    /// </summary>
    Task ShowSuccessAsync(string title, string message);
    
    /// <summary>
    /// Afiseaza toast de eroare
    /// </summary>
    Task ShowErrorAsync(string title, string message);
    
    /// <summary>
    /// Afiseaza toast de avertizare
    /// </summary>
    Task ShowWarningAsync(string title, string message);
    
    /// <summary>
    /// Afiseaza toast informativ
    /// </summary>
    Task ShowInfoAsync(string title, string message);
}

public class ToastNotificationService : IToastNotificationService
{
    private SfToast? _globalToast;
    private SfToast? _modalToast;
    private readonly ILogger<ToastNotificationService> _logger;

    public ToastNotificationService(ILogger<ToastNotificationService> logger)
    {
        _logger = logger;
    }

    public void RegisterToast(SfToast toast)
    {
        _globalToast = toast;
        _logger.LogDebug("Global toast registered");
    }

    public void RegisterModalToast(SfToast toast)
    {
        _modalToast = toast;
        _logger.LogDebug("Modal toast registered");
    }

    public async Task ShowSuccessAsync(string title, string message)
    {
        await ShowToastAsync(title, message, "success");
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        await ShowToastAsync(title, message, "error");
    }

    public async Task ShowWarningAsync(string title, string message)
    {
        await ShowToastAsync(title, message, "warning");
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        await ShowToastAsync(title, message, "info");
    }

    private async Task ShowToastAsync(string title, string message, string type)
    {
        try
        {
            // Foloseste toast-ul modal daca exista, altfel pe cel global
            var activeToast = _modalToast ?? _globalToast;
            
            if (activeToast != null)
            {
                var toastModel = new ToastModel
                {
                    Title = title,
                    Content = message,
                    CssClass = $"e-toast-{type}",
                    Icon = type switch
                    {
                        "success" => "fas fa-check-circle",
                        "error" => "fas fa-exclamation-circle", 
                        "warning" => "fas fa-exclamation-triangle",
                        "info" => "fas fa-info-circle",
                        _ => ""
                    },
                    Timeout = type == "error" ? 6000 : 4000
                };

                await activeToast.ShowAsync(toastModel);
                _logger.LogDebug("Toast shown: {Type} - {Title}", type, title);
            }
            else
            {
                _logger.LogWarning("No toast component registered");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing toast: {Title}", title);
        }
    }
}

/// <summary>
/// Extension methods pentru inregistrarea serviciului
/// </summary>
public static class ToastServiceExtensions
{
    public static IServiceCollection AddToastNotificationService(this IServiceCollection services)
    {
        services.AddScoped<IToastNotificationService, ToastNotificationService>();
        return services;
    }
}
