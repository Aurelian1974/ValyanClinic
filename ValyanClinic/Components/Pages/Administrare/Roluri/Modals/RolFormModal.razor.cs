using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.RolManagement.Commands.CreateRol;
using ValyanClinic.Application.Features.RolManagement.Commands.UpdateRol;
using ValyanClinic.Application.Features.RolManagement.Queries.GetRolById;
using ValyanClinic.Components.Pages.Administrare.Roluri.Models;

namespace ValyanClinic.Components.Pages.Administrare.Roluri.Modals;

public partial class RolFormModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RolFormModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback OnRolSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private RolFormModel Model { get; set; } = new();

    public async Task OpenForAdd()
    {
        try
        {
            Logger.LogInformation("Opening modal for ADD Rol");

            Model = new RolFormModel
            {
                EsteActiv = true,
                OrdineAfisare = 10
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

    public async Task OpenForEdit(Guid rolId)
    {
        try
        {
            Logger.LogInformation("Opening modal for EDIT Rol: {Id}", rolId);

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            await InvokeAsync(StateHasChanged);

            var query = new GetRolByIdQuery(rolId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                Model = new RolFormModel
                {
                    Id = data.Id,
                    Denumire = data.Denumire,
                    Descriere = data.Descriere,
                    EsteActiv = data.EsteActiv,
                    OrdineAfisare = data.OrdineAfisare,
                    Permisiuni = data.Permisiuni
                };

                Logger.LogInformation("Data loaded for EDIT mode");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut încărca datele";
                Logger.LogWarning("Failed to load data: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for EDIT");
            HasError = true;
            ErrorMessage = $"Eroare la încărcare: {ex.Message}";
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

        Model = new RolFormModel();
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private Task HandleOverlayClick()
    {
        // Nu închide la click pe overlay pentru modale cu formulare
        return Task.CompletedTask;
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
                var command = new UpdateRolCommand
                {
                    Id = Model.Id!.Value,
                    Denumire = Model.Denumire,
                    Descriere = Model.Descriere,
                    EsteActiv = Model.EsteActiv,
                    OrdineAfisare = Model.OrdineAfisare,
                    Permisiuni = Model.Permisiuni
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Rol updated successfully");
                    await Close();
                    await OnRolSaved.InvokeAsync();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la actualizare" });
                    Logger.LogWarning("Update failed: {Error}", ErrorMessage);
                }
            }
            else
            {
                var command = new CreateRolCommand
                {
                    Denumire = Model.Denumire,
                    Descriere = Model.Descriere,
                    EsteActiv = Model.EsteActiv,
                    OrdineAfisare = Model.OrdineAfisare,
                    Permisiuni = Model.Permisiuni
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Rol created successfully: {Id}", result.Value);
                    await Close();
                    await OnRolSaved.InvokeAsync();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la creare" });
                    Logger.LogWarning("Create failed: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception during save");
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
