using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace ValyanClinic.Services;

/// <summary>
/// Service pentru protejarea împotriva pierderii datelor nesalvate
/// Combină Blazor Navigation Guard + JavaScript beforeunload
/// </summary>
public interface INavigationGuardService
{
    /// <summary>
    /// Activează protecția pentru o pagină specifică
    /// </summary>
    Task EnableGuardAsync(Func<Task<bool>> hasUnsavedChangesFunc, string? customMessage = null);

    /// <summary>
    /// Dezactivează protecția (când utilizatorul salvează sau confirmă pierderea datelor)
    /// </summary>
    Task DisableGuardAsync();

    /// <summary>
    /// Verifică dacă există modificări nesalvate
    /// </summary>
    bool HasUnsavedChanges { get; }
}

public class NavigationGuardService : INavigationGuardService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;
    private IDisposable? _locationChangingRegistration;
    private Func<Task<bool>>? _hasUnsavedChangesFunc;
    private string _customMessage = "Aveți modificări nesalvate. Sigur doriți să părăsiți pagina?";
    private bool _isEnabled;
    private IJSObjectReference? _jsModule;

    public NavigationGuardService(IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
    }

    public bool HasUnsavedChanges
    {
        get
        {
            if (!_isEnabled || _hasUnsavedChangesFunc == null)
                return false;

            // Synchronous check (for quick access)
            // Note: This may not always be accurate for async checks
            return _hasUnsavedChangesFunc().GetAwaiter().GetResult();
        }
    }

    public async Task EnableGuardAsync(Func<Task<bool>> hasUnsavedChangesFunc, string? customMessage = null)
    {
        _hasUnsavedChangesFunc = hasUnsavedChangesFunc ?? throw new ArgumentNullException(nameof(hasUnsavedChangesFunc));
        _customMessage = customMessage ?? _customMessage;
        _isEnabled = true;

        // Register Blazor internal navigation guard
        _locationChangingRegistration = _navigationManager.RegisterLocationChangingHandler(OnLocationChanging);

        // Enable JavaScript beforeunload event
        try
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/navigationGuard.js");

            await _jsModule.InvokeVoidAsync("enableBeforeUnload", _customMessage);
        }
        catch (Exception)
        {
            // Fallback: use inline JavaScript if module not available
            await _jsRuntime.InvokeVoidAsync("eval", $@"
                window.onbeforeunload = function(e) {{
                    return '{_customMessage.Replace("'", "\\'")}';
                }};
            ");
        }
    }

    public async Task DisableGuardAsync()
    {
        _isEnabled = false;
        _hasUnsavedChangesFunc = null;

        // Unregister Blazor navigation guard
        _locationChangingRegistration?.Dispose();
        _locationChangingRegistration = null;

        // Disable JavaScript beforeunload
        try
        {
            if (_jsModule != null)
            {
                await _jsModule.InvokeVoidAsync("disableBeforeUnload");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval", "window.onbeforeunload = null;");
            }
        }
        catch (JSDisconnectedException)
        {
            // ✅ FIXED: Circuit disconnected (page refresh/close) - ignore
            // JavaScript cleanup is not needed when circuit is already disconnected
        }
        catch (Exception)
        {
            // Silent fail - guard already disabled or other JS error
        }
    }

    private async ValueTask OnLocationChanging(LocationChangingContext context)
    {
        if (!_isEnabled || _hasUnsavedChangesFunc == null)
            return;

        // Check if there are unsaved changes
        var hasChanges = await _hasUnsavedChangesFunc();
        if (!hasChanges)
            return;

        // Prevent navigation and show confirmation
        // Note: In Blazor Server, we can't show a custom modal directly from here
        // We need to use JavaScript confirm (or implement a custom modal in the component)
        
        try
        {
            var confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", _customMessage);
            if (!confirmed)
            {
                context.PreventNavigation();
            }
            else
            {
                // User confirmed - disable guard to allow navigation
                await DisableGuardAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // ✅ FIXED: Circuit disconnected - prevent navigation by default for safety
            context.PreventNavigation();
        }
        catch (Exception)
        {
            // Fallback: prevent navigation
            context.PreventNavigation();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await DisableGuardAsync();

            if (_jsModule != null)
            {
                await _jsModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // ✅ FIXED: Circuit disconnected (page refresh/close) - safe to ignore
            // The browser will handle beforeunload cleanup automatically
        }
        catch (Exception)
        {
            // Silent fail - already disposed or other error
        }

        GC.SuppressFinalize(this);
    }
}
