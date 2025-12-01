using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ValyanClinic.Application.Features.UtilizatorManagement.Commands.CreateUtilizator;
using ValyanClinic.Application.Features.UtilizatorManagement.Commands.UpdateUtilizator;
using ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorById;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Security;
using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.AdministrareClinica.Utilizatori.Modals;

public partial class UtilizatorFormModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<UtilizatorFormModal> Logger { get; set; } = default!;
    [Inject] private IPersonalMedicalRepository PersonalMedicalRepository { get; set; } = default!;
    [Inject] private IPasswordHasher PasswordHasher { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public EventCallback OnUtilizatorSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string ActiveTab { get; set; } = "date-utilizator";
    private UtilizatorFormModel Model { get; set; } = new();
    private bool _disposed = false;

    // Password visibility
    private bool ShowPassword { get; set; }
    private string GeneratedPassword { get; set; } = string.Empty;

    // Lookup data
    private bool IsLoadingLookups { get; set; }
    private List<PersonalMedicalOption> PersonalMedicalOptions { get; set; } = new();
    private List<RolOption> RolOptions { get; set; } = new();

    public class PersonalMedicalOption
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class RolOption
    {
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        Model = new UtilizatorFormModel();
        Logger.LogInformation("UtilizatorFormModal initialized");

        // Initialize role options
        InitializeRolOptions();

        // Load personal medical options
        await LoadPersonalMedicalOptions();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            Logger.LogDebug("UtilizatorFormModal disposing");

            PersonalMedicalOptions?.Clear();
            RolOptions?.Clear();
            PersonalMedicalOptions = new();
            RolOptions = new();
            Model = new UtilizatorFormModel();

            Logger.LogDebug("UtilizatorFormModal disposed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in UtilizatorFormModal dispose");
        }
    }

    private void InitializeRolOptions()
    {
        RolOptions = new List<RolOption>
        {
            new() { Text = "Administrator", Value = "Administrator" },
        new() { Text = "Doctor", Value = "Doctor" },
            new() { Text = "Asistent", Value = "Asistent" },
            new() { Text = "Receptioner", Value = "Receptioner" },
            new() { Text = "Manager", Value = "Manager" },
 new() { Text = "Utilizator", Value = "Utilizator" }
        };
    }

    private async Task LoadPersonalMedicalOptions()
    {
        if (_disposed) return;

        try
        {
            IsLoadingLookups = true;
            await InvokeAsync(StateHasChanged);

            var personalMedicalList = await PersonalMedicalRepository.GetAllAsync(
                pageNumber: 1,
           pageSize: 1000,
                esteActiv: true,
     sortColumn: "Nume",
 sortDirection: "ASC");

            if (_disposed) return;

            PersonalMedicalOptions = personalMedicalList
               .Select(pm => new PersonalMedicalOption
               {
                   Id = pm.PersonalID,
                   DisplayName = $"{pm.Nume} {pm.Prenume}" +
               (string.IsNullOrEmpty(pm.Specializare) ? "" : $" - {pm.Specializare}")
               })
                         .OrderBy(pm => pm.DisplayName)
                         .ToList();

            Logger.LogInformation("Loaded {Count} personal medical options", PersonalMedicalOptions.Count);
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Error loading personal medical options");
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoadingLookups = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void SetActiveTab(string tabName)
    {
        if (_disposed) return;
        ActiveTab = tabName;
        Logger.LogDebug("Tab changed to: {TabName}", tabName);
    }

    public async Task OpenForAdd()
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Opening modal for ADD Utilizator");

            Model = new UtilizatorFormModel
            {
                EsteActiv = true,
                Rol = "Utilizator" // Default role
            };

            IsVisible = true;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "date-utilizator";
            ShowPassword = false;
            GeneratedPassword = string.Empty;

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for ADD");
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea formularului: {ex.Message}";
        }
    }

    public async Task OpenForEdit(Guid utilizatorID)
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Opening modal for EDIT Utilizator: {UtilizatorID}", utilizatorID);

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "date-utilizator";
            ShowPassword = false;
            GeneratedPassword = string.Empty;

            await InvokeAsync(StateHasChanged);

            var query = new GetUtilizatorByIdQuery(utilizatorID);
            var result = await Mediator.Send(query);

            if (_disposed) return;

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                Model = new UtilizatorFormModel
                {
                    UtilizatorID = data.UtilizatorID,
                    PersonalMedicalID = data.PersonalMedicalID,
                    Username = data.Username,
                    Email = data.Email,
                    Rol = data.Rol,
                    EsteActiv = data.EsteActiv,
                    Password = string.Empty // Don't load password (security)
                };

                Logger.LogInformation("Data loaded for EDIT mode: {Username}", Model.Username);
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
            if (!_disposed)
            {
                Logger.LogError(ex, "Error opening modal for EDIT");
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

        Logger.LogInformation("Closing modal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        if (!_disposed)
        {
            Model = new UtilizatorFormModel();
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "date-utilizator";
            ShowPassword = false;
            GeneratedPassword = string.Empty;
        }
    }

    private async Task HandleOverlayClick()
    {
        // Don't close on overlay click to prevent accidental data loss
        return;
    }

    private async Task HandleSubmit()
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Submitting form: IsEditMode={IsEditMode}", Model.IsEditMode);

            IsSaving = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);

            if (Model.IsEditMode)
            {
                var command = new UpdateUtilizatorCommand
                {
                    UtilizatorID = Model.UtilizatorID!.Value,
                    Username = Model.Username,
                    Email = Model.Email,
                    Rol = Model.Rol,
                    EsteActiv = Model.EsteActiv,
                    Password = string.IsNullOrWhiteSpace(Model.Password) ? null : Model.Password, // ✅ ADDED: Send password if provided
                    ModificatDe = "CurrentUser" // TODO: Get from auth context
                };

                var result = await Mediator.Send(command);

                if (_disposed) return;

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Utilizator updated successfully: {UtilizatorID}", Model.UtilizatorID);
                    await OnUtilizatorSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la actualizare" });
                    Logger.LogWarning("Failed to update: {Error}", ErrorMessage);
                }
            }
            else
            {
                // ADD MODE - Password required
                if (string.IsNullOrWhiteSpace(Model.Password))
                {
                    HasError = true;
                    ErrorMessage = "Parola este obligatorie pentru utilizator nou";
                    IsSaving = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                var command = new CreateUtilizatorCommand
                {
                    PersonalMedicalID = Model.PersonalMedicalID!.Value,
                    Username = Model.Username,
                    Email = Model.Email,
                    Password = Model.Password, // Will be hashed by handler with BCrypt
                    Rol = Model.Rol,
                    EsteActiv = Model.EsteActiv,
                    CreatDe = "CurrentUser" // TODO: Get from auth context
                };

                var result = await Mediator.Send(command);

                if (_disposed) return;

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Utilizator created successfully: {UtilizatorID}", result.Value);
                    await OnUtilizatorSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la creare" });
                    Logger.LogWarning("Failed to create: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Error submitting form");
                HasError = true;
                ErrorMessage = $"Eroare: {ex.Message}";
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsSaving = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    private void GenerateRandomPassword()
    {
        try
        {
            // Generate password with BCrypt PasswordHasher
            GeneratedPassword = PasswordHasher.GenerateRandomPassword(12, includeSpecialChars: true);
            Model.Password = GeneratedPassword;

            Logger.LogInformation("Random password generated");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating random password");
        }
    }

    private async Task CopyPasswordToClipboard()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", GeneratedPassword);
            Logger.LogInformation("Password copied to clipboard");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error copying password to clipboard");
        }
    }

    public class UtilizatorFormModel
    {
        public Guid? UtilizatorID { get; set; }

        [Required(ErrorMessage = "Personal Medical este obligatoriu")]
        public Guid? PersonalMedicalID { get; set; }

        [Required(ErrorMessage = "Username este obligatoriu")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username trebuie sa aiba intre 3 si 100 caractere")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol este obligatoriu")]
        public string Rol { get; set; } = "Utilizator";

        public bool EsteActiv { get; set; } = true;

        // Password - required only for ADD mode
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie sa aiba minim 8 caractere")]
        public string? Password { get; set; }

        public bool IsEditMode => UtilizatorID.HasValue && UtilizatorID.Value != Guid.Empty;
    }
}
