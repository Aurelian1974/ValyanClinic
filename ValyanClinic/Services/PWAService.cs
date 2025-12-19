using Microsoft.JSInterop;

namespace ValyanClinic.Services;

/// <summary>
/// Service pentru interacțiune cu PWA și funcționalități offline
/// </summary>
public class PWAService
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<PWAService>? _dotNetReference;

    public event EventHandler<string>? OnOfflineEvent;
    public event EventHandler? OnInstallPromptReady;
    public event EventHandler? OnAppInstalled;
    public event EventHandler? OnUpdateAvailable;
    public event EventHandler<SyncCompleteEventArgs>? OnSyncComplete;

    public PWAService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Inițializează interop-ul PWA
    /// </summary>
    public async Task InitializeAsync()
    {
        _dotNetReference = DotNetObjectReference.Create(this);
        await _jsRuntime.InvokeVoidAsync("eval",
            $"window.pwaInterop = {System.Text.Json.JsonSerializer.Serialize(_dotNetReference)};");
        await _jsRuntime.InvokeVoidAsync("eval",
            $"window.offlineSyncInterop = {System.Text.Json.JsonSerializer.Serialize(_dotNetReference)};");
    }

    /// <summary>
    /// Instalează PWA
    /// </summary>
    public async Task<bool> InstallAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("pwaInstaller.install");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obține status-ul PWA
    /// </summary>
    public async Task<PWAStatus?> GetStatusAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<PWAStatus>("pwaInstaller.getStatus");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Cere permisiune pentru notificări
    /// </summary>
    public async Task<bool> RequestNotificationPermissionAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("pwaInstaller.requestNotifications");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Arată o notificare
    /// </summary>
    public async Task ShowNotificationAsync(string title, NotificationOptions? options = null)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("pwaInstaller.showNotification", title, options ?? new NotificationOptions());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PWAService] Notification error: {ex.Message}");
        }
    }

    /// <summary>
    /// Forțează sincronizarea datelor offline
    /// </summary>
    public async Task<bool> ForceSyncAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("pwaInstaller.forceSync");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obține status-ul queue-ului offline
    /// </summary>
    public async Task<QueueStatus?> GetQueueStatusAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<QueueStatus>("offlineSync.getQueueStatus");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sincronizează datele offline acum
    /// </summary>
    public async Task SyncNowAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("offlineSync.syncNow");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PWAService] Sync error: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifică dacă aplicația e online
    /// </summary>
    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("offlineSync.isOnline");
        }
        catch
        {
            return true; // Default to online
        }
    }

    /// <summary>
    /// Stochează date în IndexedDB
    /// </summary>
    public async Task<bool> StoreDataAsync<T>(string storeName, T data)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("offlineSync.storeData", storeName, data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obține date din IndexedDB
    /// </summary>
    public async Task<T?> GetDataAsync<T>(string storeName, string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<T>("offlineSync.getData", storeName, key);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Obține toate datele dintr-un store
    /// </summary>
    public async Task<List<T>?> GetAllDataAsync<T>(string storeName)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<List<T>>("offlineSync.getAllData", storeName);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Șterge date din IndexedDB
    /// </summary>
    public async Task<bool> DeleteDataAsync(string storeName, string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("offlineSync.deleteData", storeName, key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Queue-uiește un request pentru sincronizare offline
    /// </summary>
    public async Task<int?> QueueRequestAsync(string type, string endpoint, string method, object data)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<int>("offlineSync.queueRequest", type, endpoint, method, data);
        }
        catch
        {
            return null;
        }
    }

    // Callbacks invocate din JavaScript
    [JSInvokable]
    public void OnOfflineEvent(string eventType, object data)
    {
        OnOfflineEvent?.Invoke(this, eventType);

        if (eventType == "sync_complete" && data != null)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var syncData = System.Text.Json.JsonSerializer.Deserialize<SyncCompleteEventArgs>(json);
                if (syncData != null)
                {
                    OnSyncComplete?.Invoke(this, syncData);
                }
            }
            catch { }
        }
    }

    [JSInvokable]
    public void OnInstallPromptReady()
    {
        OnInstallPromptReady?.Invoke(this, EventArgs.Empty);
    }

    [JSInvokable]
    public void OnAppInstalled()
    {
        OnAppInstalled?.Invoke(this, EventArgs.Empty);
    }

    [JSInvokable]
    public void OnUpdateAvailable()
    {
        OnUpdateAvailable?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _dotNetReference?.Dispose();
    }
}

// Models
public class PWAStatus
{
    public bool IsInstalled { get; set; }
    public bool CanInstall { get; set; }
    public bool HasServiceWorker { get; set; }
    public bool IsOnline { get; set; }
}

public class NotificationOptions
{
    public string? Body { get; set; }
    public string? Icon { get; set; } = "/icon-192.png";
    public string? Badge { get; set; } = "/icon-192.png";
    public int[]? Vibrate { get; set; } = new[] { 200, 100, 200 };
    public object? Data { get; set; }
}

public class QueueStatus
{
    public int Count { get; set; }
    public List<QueuedRequest>? Items { get; set; }
}

public class QueuedRequest
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public object? Data { get; set; }
    public long Timestamp { get; set; }
    public int Retries { get; set; }
}

public class SyncCompleteEventArgs : EventArgs
{
    public int Success { get; set; }
    public int Failed { get; set; }
    public int Total { get; set; }
}
