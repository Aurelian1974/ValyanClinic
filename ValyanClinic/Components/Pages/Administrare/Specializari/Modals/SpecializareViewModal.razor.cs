using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareById;

namespace ValyanClinic.Components.Pages.Administrare.Specializari.Modals;

public partial class SpecializareViewModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<SpecializareViewModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private SpecializareDetailDto? SpecializareData { get; set; }
    private Guid CurrentSpecializareId { get; set; }

    public async Task Open(Guid specializareId)
    {
        try
        {
            Logger.LogInformation("Opening View modal for Specializare: {Id}", specializareId);

            CurrentSpecializareId = specializareId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            SpecializareData = null;

            await InvokeAsync(StateHasChanged);

            var query = new GetSpecializareByIdQuery(specializareId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                SpecializareData = result.Value;
                Logger.LogInformation("Data loaded successfully for View modal");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele";
                Logger.LogWarning("Failed to load data: {Error}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening View modal");
            HasError = true;
            ErrorMessage = $"Eroare la incarcare: {ex.Message}";
        }
        finally
        {
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

        SpecializareData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentSpecializareId = Guid.Empty;
    }

    private async Task HandleOverlayClick()
    {
        await Close();
    }

    private async Task HandleEdit()
    {
        Logger.LogInformation("Edit requested from View modal: {Id}", CurrentSpecializareId);

        if (OnEditRequested.HasDelegate)
        {
            await OnEditRequested.InvokeAsync(CurrentSpecializareId);
        }
    }

    private async Task HandleDelete()
    {
        Logger.LogInformation("Delete requested from View modal: {Id}", CurrentSpecializareId);

        if (OnDeleteRequested.HasDelegate)
        {
            await OnDeleteRequested.InvokeAsync(CurrentSpecializareId);
        }
    }
}
