using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieById;

namespace ValyanClinic.Components.Pages.Administrare.Pozitii.Modals;

public partial class PozitieViewModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PozitieViewModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback<Guid> OnEditRequested { get; set; }
    [Parameter] public EventCallback<Guid> OnDeleteRequested { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private PozitieDetailDto? PozitieData { get; set; }
    private Guid CurrentPozitieId { get; set; }

    public async Task Open(Guid pozitieId)
    {
        try
        {
            Logger.LogInformation("Opening View modal for Pozitie: {Id}", pozitieId);

            CurrentPozitieId = pozitieId;
            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            PozitieData = null;

            await InvokeAsync(StateHasChanged);

            var query = new GetPozitieByIdQuery(pozitieId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PozitieData = result.Value;
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

        PozitieData = null;
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
        CurrentPozitieId = Guid.Empty;
    }

    private async Task HandleOverlayClick()
    {
        await Close();
    }

    private async Task HandleEdit()
    {
        Logger.LogInformation("Edit requested from View modal: {Id}", CurrentPozitieId);

        if (OnEditRequested.HasDelegate)
        {
            await OnEditRequested.InvokeAsync(CurrentPozitieId);
        }
    }

    private async Task HandleDelete()
    {
        Logger.LogInformation("Delete requested from View modal: {Id}", CurrentPozitieId);

        if (OnDeleteRequested.HasDelegate)
        {
            await OnDeleteRequested.InvokeAsync(CurrentPozitieId);
        }
    }
}
