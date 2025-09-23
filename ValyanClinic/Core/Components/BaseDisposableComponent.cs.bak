using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Notifications;
using ValyanClinic.Core.Services;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Core.Components;

/// <summary>
/// Implementare simplificată pentru memory leak prevention în Blazor components
/// Focus pe Syncfusion components disposal și state persistence
/// </summary>
public abstract class BaseDisposableComponent : ComponentBase, IAsyncDisposable
{
    [Inject] protected ILogger<BaseDisposableComponent> Logger { get; set; } = default!;
    
    protected bool _disposed = false;

    /// <summary>
    /// Safe StateHasChanged care verifică disposal
    /// </summary>
    protected void SafeStateHasChanged()
    {
        try
        {
            if (!_disposed)
            {
                InvokeAsync(StateHasChanged);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger?.LogDebug("StateHasChanged called on disposed component");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in SafeStateHasChanged");
        }
    }

    /// <summary>
    /// Cleanup specific pentru componenta curentă
    /// </summary>
    protected virtual ValueTask DisposeComponentAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// IAsyncDisposable implementation
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            _disposed = true;
            await DisposeComponentAsync();
            Logger?.LogDebug("Component disposed: {Component}", GetType().Name);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in DisposeAsync for component: {Component}", GetType().Name);
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Base class simplificată pentru pagini cu DataGrid
/// </summary>
public abstract class BaseDataGridPage<TItem> : BaseDisposableComponent where TItem : class
{
    // Syncfusion Component References
    protected SfGrid<TItem>? GridRef { get; set; }
    protected SfDialog? DetailModal { get; set; }
    protected SfDialog? AddEditModal { get; set; }
    protected SfToast? ToastRef { get; set; }

    protected override async ValueTask DisposeComponentAsync()
    {
        try
        {
            // Manual disposal pentru componentele Syncfusion
            GridRef?.Dispose();
            DetailModal?.Dispose();  
            AddEditModal?.Dispose();
            ToastRef?.Dispose();

            await base.DisposeComponentAsync();
            Logger?.LogDebug("DataGrid page disposed: {Component}", GetType().Name);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in DataGrid page disposal: {Component}", GetType().Name);
        }
    }
}
