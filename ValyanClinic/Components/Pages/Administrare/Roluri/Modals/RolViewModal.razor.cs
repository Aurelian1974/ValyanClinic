using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.RolManagement.Queries.GetRolById;

namespace ValyanClinic.Components.Pages.Administrare.Roluri.Modals;

public partial class RolViewModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RolViewModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }
    [Parameter] public EventCallback<Guid> OnManagePermissionsRequested { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private RolDetailDto? RolData { get; set; }
    private Guid CurrentRolId { get; set; }

    public async Task Open(Guid rolId)
    {
        try
        {
            Logger.LogInformation("Opening View modal for Rol: {Id}", rolId);

            CurrentRolId = rolId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            RolData = null;

            await InvokeAsync(StateHasChanged);
            await LoadRolData(rolId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening View modal for {Id}", rolId);
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea modalului: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task Close()
    {
        Logger.LogInformation("Closing View modal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        RolData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentRolId = Guid.Empty;
    }

    private async Task LoadRolData(Guid rolId)
    {
        try
        {
            Logger.LogInformation("Loading Rol data: {Id}", rolId);

            var query = new GetRolByIdQuery(rolId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                RolData = result.Value;
                HasError = false;
                Logger.LogInformation("Data loaded successfully for {Id}", rolId);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele";
                Logger.LogWarning("Failed to load data for {Id}: {Error}", rolId, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            Logger.LogError(ex, "Exception loading data for {Id}", rolId);
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleOverlayClick()
    {
        await Close();
    }

    private async Task HandleEdit()
    {
        Logger.LogInformation("Edit requested from View modal: {Id}", CurrentRolId);

        if (OnEditRequested.HasDelegate)
        {
            await OnEditRequested.InvokeAsync(CurrentRolId);
        }
    }

    private async Task HandleDelete()
    {
        Logger.LogInformation("Delete requested from View modal: {Id}", CurrentRolId);

        if (OnDeleteRequested.HasDelegate)
        {
            await OnDeleteRequested.InvokeAsync(CurrentRolId);
        }
    }

    private async Task HandleManagePermissions()
    {
        Logger.LogInformation("Manage permissions requested from View modal: {Id}", CurrentRolId);

        if (OnManagePermissionsRequested.HasDelegate)
        {
            await OnManagePermissionsRequested.InvokeAsync(CurrentRolId);
        }
    }
}
