using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PozitieManagement.Commands.CreatePozitie;
using ValyanClinic.Application.Features.PozitieManagement.Commands.UpdatePozitie;
using ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieById;
using ValyanClinic.Components.Pages.Administrare.Pozitii.Models;

namespace ValyanClinic.Components.Pages.Administrare.Pozitii.Modals;

public partial class PozitieFormModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PozitieFormModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback OnPozitieSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private PozitieFormModel Model { get; set; } = new();

    public async Task OpenForAdd()
    {
        try
        {
            Logger.LogInformation("Opening modal for ADD Pozitie");

            Model = new PozitieFormModel
            {
                EsteActiv = true
            };

            IsVisible = true;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for ADD");
            HasError = true;
            ErrorMessage = $"Eroare la deschidere: {ex.Message}";
        }
    }

    public async Task OpenForEdit(Guid pozitieId)
    {
        try
        {
            Logger.LogInformation("Opening modal for EDIT Pozitie: {Id}", pozitieId);

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            await InvokeAsync(StateHasChanged);

            var query = new GetPozitieByIdQuery(pozitieId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                Model = new PozitieFormModel
                {
                    Id = data.Id,
                    Denumire = data.Denumire,
                    Descriere = data.Descriere,
                    EsteActiv = data.EsteActiv
                };

                Logger.LogInformation("Data loaded for EDIT mode");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele";
                Logger.LogWarning("Failed to load data: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for EDIT");
            HasError = true;
            ErrorMessage = $"Eroare la incarcare: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task Close()
    {
        Logger.LogInformation("Closing modal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        Model = new PozitieFormModel();
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private async Task HandleOverlayClick()
    {
        if (!IsSaving)
        {
            await Close();
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            Logger.LogInformation("Submitting form: IsEditMode={IsEditMode}", Model.IsEditMode);

            IsSaving = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);

            if (Model.IsEditMode)
            {
                var command = new UpdatePozitieCommand
                {
                    Id = Model.Id!.Value,
                    Denumire = Model.Denumire,
                    Descriere = Model.Descriere,
                    EsteActiv = Model.EsteActiv
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Pozitie updated successfully");
                    await OnPozitieSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Actualizare esuata" });
                    Logger.LogWarning("Failed to update: {Error}", ErrorMessage);
                }
            }
            else
            {
                var command = new CreatePozitieCommand
                {
                    Denumire = Model.Denumire,
                    Descriere = Model.Descriere,
                    EsteActiv = Model.EsteActiv
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Pozitie created successfully: {Id}", result.Value);
                    await OnPozitieSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Creare esuata" });
                    Logger.LogWarning("Failed to create: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting form");
            HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
