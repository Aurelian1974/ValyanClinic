using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.CreatePersonalMedical;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.UpdatePersonalMedical;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical.Modals;

public partial class PersonalMedicalFormModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalMedicalFormModal> Logger { get; set; } = default!;

    [Parameter] public EventCallback OnPersonalMedicalSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private PersonalMedicalFormModel Model { get; set; } = new();

    public async Task OpenForAdd()
    {
        try
        {
            Logger.LogInformation("Opening modal for ADD PersonalMedical");

            Model = new PersonalMedicalFormModel
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
            ErrorMessage = $"Error opening form: {ex.Message}";
        }
    }

    public async Task OpenForEdit(Guid personalID)
    {
        try
        {
            Logger.LogInformation("Opening modal for EDIT PersonalMedical: {PersonalID}", personalID);

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            await InvokeAsync(StateHasChanged);

            var query = new GetPersonalMedicalByIdQuery(personalID);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                Model = new PersonalMedicalFormModel
                {
                    PersonalID = data.PersonalID,
                    Nume = data.Nume,
                    Prenume = data.Prenume,
                    Specializare = data.Specializare,
                    NumarLicenta = data.NumarLicenta,
                    Telefon = data.Telefon,
                    Email = data.Email,
                    Departament = data.Departament,
                    Pozitie = data.Pozitie,
                    EsteActiv = data.EsteActiv ?? true,
                    CategorieID = data.CategorieID,
                    SpecializareID = data.SpecializareID,
                    SubspecializareID = data.SubspecializareID
                };

                Logger.LogInformation("Data loaded for EDIT mode");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Could not load personal medical data";
                Logger.LogWarning("Failed to load data: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for EDIT");
            HasError = true;
            ErrorMessage = $"Error loading data: {ex.Message}";
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

        Model = new PersonalMedicalFormModel();
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
                var command = new UpdatePersonalMedicalCommand
                {
                    PersonalID = Model.PersonalID!.Value,
                    Nume = Model.Nume,
                    Prenume = Model.Prenume,
                    Specializare = Model.Specializare,
                    NumarLicenta = Model.NumarLicenta,
                    Telefon = Model.Telefon,
                    Email = Model.Email,
                    Departament = Model.Departament,
                    Pozitie = Model.Pozitie,
                    EsteActiv = Model.EsteActiv,
                    CategorieID = Model.CategorieID,
                    SpecializareID = Model.SpecializareID,
                    SubspecializareID = Model.SubspecializareID
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("PersonalMedical updated successfully");
                    await OnPersonalMedicalSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Update failed" });
                    Logger.LogWarning("Failed to update: {Error}", ErrorMessage);
                }
            }
            else
            {
                var command = new CreatePersonalMedicalCommand
                {
                    Nume = Model.Nume,
                    Prenume = Model.Prenume,
                    Specializare = Model.Specializare,
                    NumarLicenta = Model.NumarLicenta,
                    Telefon = Model.Telefon,
                    Email = Model.Email,
                    Departament = Model.Departament,
                    Pozitie = Model.Pozitie,
                    EsteActiv = Model.EsteActiv,
                    CategorieID = Model.CategorieID,
                    SpecializareID = Model.SpecializareID,
                    SubspecializareID = Model.SubspecializareID
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("PersonalMedical created successfully: {PersonalID}", result.Value);
                    await OnPersonalMedicalSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Create failed" });
                    Logger.LogWarning("Failed to create: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting form");
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public class PersonalMedicalFormModel
    {
        public Guid? PersonalID { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string? Specializare { get; set; }
        public string? NumarLicenta { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Departament { get; set; }
        public string? Pozitie { get; set; }
        public bool EsteActiv { get; set; } = true;
        public Guid? CategorieID { get; set; }
        public Guid? SpecializareID { get; set; }
        public Guid? SubspecializareID { get; set; }

        public bool IsEditMode => PersonalID.HasValue && PersonalID.Value != Guid.Empty;
    }
}
