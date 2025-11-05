using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.SpecializareManagement.Commands.CreateSpecializare;
using ValyanClinic.Application.Features.SpecializareManagement.Commands.UpdateSpecializare;
using ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareById;
using ValyanClinic.Components.Pages.Administrare.Specializari.Models;

namespace ValyanClinic.Components.Pages.Administrare.Specializari.Modals;

public partial class SpecializareFormModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<SpecializareFormModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback OnSpecializareSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private SpecializareFormModel Model { get; set; } = new();

    public async Task OpenForAdd()
    {
        try
        {
            Logger.LogInformation("Opening modal for ADD Specializare");

            Model = new SpecializareFormModel
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

    public async Task OpenForEdit(Guid specializareId)
    {
        try
        {
            Logger.LogInformation("Opening modal for EDIT Specializare: {Id}", specializareId);

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            await InvokeAsync(StateHasChanged);

            var query = new GetSpecializareByIdQuery(specializareId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                Model = new SpecializareFormModel
                {
                    Id = data.Id,
                    Denumire = data.Denumire,
                    Categorie = data.Categorie,
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

        Model = new SpecializareFormModel();
        IsLoading = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private async Task HandleOverlayClick()
    {
        // ❌ DEZACTIVAT: Nu închide modalul la click pe overlay pentru modalele Form
        // Modalele de tip SpecializareForm conțin date care nu trebuie pierdute
        // if (!IsSaving)
        // {
        //     await Close();
        // }

        // 📝 OPȚIONAL: Adaugă feedback visual că modalul nu se poate închide pe overlay
        return;
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
                var command = new UpdateSpecializareCommand
                {
                    Id = Model.Id!.Value,
                    Denumire = Model.Denumire,
                    Categorie = Model.Categorie,
                    Descriere = Model.Descriere,
                    EsteActiv = Model.EsteActiv
                };

                Logger.LogInformation("🔄 Sending UpdateSpecializareCommand for: {Denumire}", command.Denumire);
                var result = await Mediator.Send(command);
                Logger.LogInformation("🔄 Received result - IsSuccess: {IsSuccess}", result.IsSuccess);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Specializare updated successfully");
                    await OnSpecializareSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Actualizare esuata" });
                    Logger.LogWarning("❌ UPDATE FAILED - Errors: {Errors}", string.Join("; ", result.Errors ?? new List<string>()));
                    Logger.LogInformation("🔄 Setting HasError=true, ErrorMessage='{ErrorMessage}'", ErrorMessage);
                }
            }
            else
            {
                var command = new CreateSpecializareCommand
                {
                    Denumire = Model.Denumire,
                    Categorie = Model.Categorie,
                    Descriere = Model.Descriere,
                    EsteActiv = Model.EsteActiv
                };

                Logger.LogInformation("🔄 Sending CreateSpecializareCommand for: {Denumire}", command.Denumire);
                var result = await Mediator.Send(command);
                Logger.LogInformation("🔄 Received result - IsSuccess: {IsSuccess}", result.IsSuccess);

                if (result.Errors != null && result.Errors.Any())
                {
                    Logger.LogInformation("🔄 Result Errors: {Errors}", string.Join("; ", result.Errors));
                }

                if (result.IsSuccess)
                {
                    Logger.LogInformation("✅ CREATE SUCCESS: Specializare created successfully: {Id}", result.Value);
                    await OnSpecializareSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Creare esuata" });
                    Logger.LogWarning("❌ CREATE FAILED - Setting UI Error State");
                    Logger.LogWarning("❌ HasError = {HasError}", HasError);
                    Logger.LogWarning("❌ ErrorMessage = '{ErrorMessage}'", ErrorMessage);
                    Logger.LogWarning("❌ Result.Errors = [{Errors}]", string.Join("; ", result.Errors ?? new List<string>()));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ EXCEPTION in HandleSubmit");
            HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
            Logger.LogError("❌ EXCEPTION - Setting ErrorMessage = '{ErrorMessage}'", ErrorMessage);
        }
        finally
        {
            IsSaving = false;
            Logger.LogInformation("🔄 FINALLY - IsSaving=false, HasError={HasError}, ErrorMessage='{ErrorMessage}'", HasError, ErrorMessage);
            await InvokeAsync(StateHasChanged);
            Logger.LogInformation("🔄 FINALLY - StateHasChanged called");
        }
    }
}
