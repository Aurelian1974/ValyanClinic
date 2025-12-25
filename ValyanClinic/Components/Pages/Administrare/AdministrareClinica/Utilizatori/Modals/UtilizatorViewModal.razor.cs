using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorById;

namespace ValyanClinic.Components.Pages.Administrare.AdministrareClinica.Utilizatori.Modals;

public partial class UtilizatorViewModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<UtilizatorViewModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string ActiveTab { get; set; } = "general";
    private UtilizatorDetailDto? UtilizatorData { get; set; }
    private Guid CurrentUtilizatorId { get; set; }
    private bool _disposed = false;

    protected override bool ShouldRender() => !_disposed;

    public async Task Open(Guid utilizatorId)
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Opening UtilizatorViewModal for ID: {UtilizatorID}", utilizatorId);

            CurrentUtilizatorId = utilizatorId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "general";
            UtilizatorData = null;

            await InvokeAsync(StateHasChanged);

            await LoadUtilizatorData(utilizatorId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening UtilizatorViewModal");
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea modalului: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadUtilizatorData(Guid utilizatorId)
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Loading data for Utilizator: {UtilizatorID}", utilizatorId);

            var query = new GetUtilizatorByIdQuery(utilizatorId);
            var result = await Mediator.Send(query);

            if (_disposed) return;

            if (result.IsSuccess && result.Value != null)
            {
                UtilizatorData = result.Value;
                Logger.LogInformation("Data loaded successfully for: {Username}", UtilizatorData.Username);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele utilizatorului";
                Logger.LogWarning("Failed to load data: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Error loading Utilizator data");
                HasError = true;
                ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    public async Task Close()
    {
        if (_disposed) return;

        Logger.LogInformation("Closing UtilizatorViewModal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        if (!_disposed)
        {
            UtilizatorData = null;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "general";
        }
    }

    private void SetActiveTab(string tabName)
    {
        if (_disposed) return;
        ActiveTab = tabName;
        Logger.LogDebug("Tab changed to: {TabName}", tabName);
    }

    private async Task HandleOverlayClick()
    {
        // Don't close on overlay click
        return;
    }

    private async Task HandleEdit()
    {
        if (_disposed || UtilizatorData == null) return;

        Logger.LogInformation("Edit requested for Utilizator: {UtilizatorID}", CurrentUtilizatorId);

        if (OnEditRequested.HasDelegate)
        {
            await OnEditRequested.InvokeAsync(CurrentUtilizatorId);
        }

        await Close();
    }

    private async Task HandleDelete()
    {
        if (_disposed || UtilizatorData == null) return;

        Logger.LogInformation("Delete requested for Utilizator: {UtilizatorID}", CurrentUtilizatorId);

        if (OnDeleteRequested.HasDelegate)
        {
            await OnDeleteRequested.InvokeAsync(CurrentUtilizatorId);
        }

        await Close();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            Logger.LogDebug("UtilizatorViewModal disposing");

            UtilizatorData = null;
            IsVisible = false;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;

            Logger.LogDebug("UtilizatorViewModal disposed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in UtilizatorViewModal dispose");
        }
    }
}
