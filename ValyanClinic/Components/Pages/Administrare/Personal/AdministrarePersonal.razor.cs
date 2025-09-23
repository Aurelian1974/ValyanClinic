using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.SplitButtons;
using ValyanClinic.Application.Services;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalModel = ValyanClinic.Domain.Models.Personal;
using ValyanClinic.Core.Services;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Business Logic pentru AdministrarePersonal.razor
/// SIMPLIFIED VERSION - Removed kebab menu, advanced filtering functionality and all toasts
/// </summary>
public partial class AdministrarePersonal : ComponentBase, IAsyncDisposable
{
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonal> Logger { get; set; } = default!;
    [Inject] private ISimpleGridStateService GridStateService { get; set; } = default!;

    // Component References
    protected SfGrid<PersonalModel>? GridRef;
    protected SfDialog? PersonalDetailModal;
    protected SfDialog? AddEditPersonalModal;

    // State Management
    private PersonalPageState _state = new();
    private PersonalModels _models = new();
    private bool _disposed = false;

    // Dialog Animation Settings
    private DialogAnimationSettings DialogAnimation = new()
    {
        Effect = DialogEffect.FadeZoom,
        Duration = 300
    };

    // Grid ID pentru persistență
    private const string GRID_ID = "personal-management-grid";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadInitialData();
            InitializeFilterOptions();

            // Load grid settings din persistență
            var savedSettings = await GridStateService.GetGridSettingsAsync(GRID_ID);
            if (savedSettings != null)
            {
                ApplyGridSettings(savedSettings);
                Logger.LogInformation("Grid settings loaded for Personal management");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing Personal management page");
            _state.SetError($"Eroare la inițializarea paginii: {ex.Message}");
            StateHasChanged();
        }
    }

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

    #region Data Loading

    private async Task LoadInitialData()
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            // Load personal data
            var searchRequest = new PersonalSearchRequest(
                PageNumber: 1,
                PageSize: 1000,
                SearchText: null,
                Departament: null,
                Status: null
            );

            var personalData = await PersonalService.GetPersonalAsync(searchRequest);
            _models.SetPersonal(personalData.Data.ToList());

            // Load dropdown options
            _state.DropdownOptions = await PersonalService.GetDropdownOptionsAsync();

            Logger.LogInformation("Loaded {RecordCount} personal records", personalData.Data.Count());
            _state.ClearError();
        }
        catch (Exception ex)
        {
            var errorMessage = "Nu s-au putut incarca datele personalului";
            Logger.LogError(ex, "Error loading personal data");
            _state.SetError(errorMessage);
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        await LoadInitialData();
        if (GridRef != null)
        {
            await GridRef.Refresh();
        }
    }

    #endregion

    #region Grid State Management

    private Dictionary<string, object> CaptureGridSettings()
    {
        var settings = new Dictionary<string, object>();

        try
        {
            if (GridRef != null)
            {
                settings["pageSize"] = GridRef.PageSettings?.PageSize ?? 20;
                settings["currentPage"] = GridRef.PageSettings?.CurrentPage ?? 1;
                settings["lastSaved"] = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error capturing grid settings");
        }

        return settings;
    }

    private void ApplyGridSettings(Dictionary<string, object> settings)
    {
        try
        {
            if (settings.TryGetValue("pageSize", out var pageSize))
            {
                Logger.LogDebug("Applied saved page size: {PageSize}", pageSize);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error applying grid settings");
        }
    }

    private async Task OnGridActionComplete()
    {
        try
        {
            var settings = CaptureGridSettings();
            await GridStateService.SaveGridSettingsAsync(GRID_ID, settings);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error auto-saving grid state");
        }
    }

    #endregion

    #region Filter Logic

    private void InitializeFilterOptions()
    {
        _models.InitializeFilterOptions();
    }

    #endregion

    #region Action Dropdown Handler

    /// <summary>
    /// Handler pentru dropdown actions din grid
    /// </summary>
    private async Task OnActionSelected(MenuEventArgs args, PersonalModel personal)
    {
        try
        {
            Logger.LogInformation("Action selected: {ActionId} for personal {PersonalName}", args.Item.Id, personal.NumeComplet);

            switch (args.Item.Id?.ToLower())
            {
                case "view":
                    await ShowPersonalDetailModal(personal);
                    break;
                case "edit":
                    await EditPersonal(personal);
                    break;
                case "delete":
                    await DeletePersonal(personal);
                    break;
                default:
                    Logger.LogWarning("Unknown action selected: {ActionId}", args.Item.Id);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling action selection");
        }
    }

    #endregion

    #region Personal Detail Modal

    private async Task ShowPersonalDetailModal(PersonalModel personal)
    {
        try
        {
            Logger.LogInformation("Opening modal for personal {PersonalName}", personal.NumeComplet);
            _state.SelectedPersonal = personal;
            _state.IsModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing personal detail modal");
        }
    }

    private async Task ClosePersonalDetailModal()
    {
        try
        {
            _state.IsModalVisible = false;
            _state.SelectedPersonal = null;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing modal");
            _state.IsModalVisible = false;
            _state.SelectedPersonal = null;
            StateHasChanged();
        }
    }

    private async Task EditPersonalFromModal()
    {
        if (_state.SelectedPersonal != null)
        {
            var personalToEdit = _state.SelectedPersonal;
            await ClosePersonalDetailModal();
            await Task.Delay(200);
            await ShowEditPersonalModal(personalToEdit);
        }
    }

    private void OnModalClosed()
    {
        _state.IsModalVisible = false;
        _state.SelectedPersonal = null;
        StateHasChanged();
    }

    private void OnAddEditModalClosed()
    {
        _state.IsAddEditModalVisible = false;
        _state.EditingPersonal = null;
        _state.IsEditMode = false;
        StateHasChanged();
    }

    #endregion

    #region Add/Edit Personal Modal

    private async Task ShowAddPersonalModal()
    {
        try
        {
            Logger.LogInformation("Opening add personal modal");
            
            _state.IsEditMode = false;
            _state.EditingPersonal = _models.CreateNewPersonal();
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            Logger.LogInformation("Add personal modal opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing add personal modal");
        }
    }

    private async Task ShowEditPersonalModal(PersonalModel personal)
    {
        try
        {
            Logger.LogInformation("Starting to show edit modal for {PersonalName}", personal.NumeComplet);
            _state.IsEditMode = true;
            _state.EditingPersonal = _models.ClonePersonal(personal);
            _state.SelectedPersonalForEdit = personal;
            _state.IsAddEditModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing edit personal modal");
        }
    }

    private async Task CloseAddEditModal()
    {
        try
        {
            _state.IsAddEditModalVisible = false;
            _state.EditingPersonal = null;
            _state.IsEditMode = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing add/edit modal");
            _state.IsAddEditModalVisible = false;
            _state.EditingPersonal = null;
            _state.IsEditMode = false;
            StateHasChanged();
        }
    }

    // Reference către componenta AdaugaEditezaPersonal pentru a putea apela submit-ul
    private AdaugaEditezaPersonal? _currentFormComponent;

    private async Task HandleFormSubmit()
    {
        if (_currentFormComponent != null)
        {
            await _currentFormComponent.SubmitForm();
        }
    }

    private async Task SavePersonal(PersonalModel personalModel)
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            Logger.LogInformation("Starting save process for {PersonalName}", personalModel.NumeComplet);

            PersonalResult result;

            if (_state.IsEditMode)
            {
                result = await PersonalService.UpdatePersonalAsync(personalModel, "current_user");
            }
            else
            {
                result = await PersonalService.CreatePersonalAsync(personalModel, "current_user");
            }

            if (result.IsSuccess)
            {
                var action = _state.IsEditMode ? "actualizat" : "creat";
                Logger.LogInformation("Personal {PersonalName} successfully {Action}", personalModel.NumeComplet, action);
                
                await CloseAddEditModal();
                await LoadInitialData();
            }
            else
            {
                Logger.LogWarning("Failed to save personal: {ErrorMessage}", result.ErrorMessage);
                _state.SetError(result.ErrorMessage ?? "Eroare necunoscută");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while saving personal");
            _state.SetError($"Eroare la salvarea personalului: {ex.Message}");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    #endregion

    #region Personal Actions

    private async Task EditPersonal(PersonalModel personal)
    {
        try
        {
            await ShowEditPersonalModal(personal);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing personal");
        }
    }

    private async Task DeletePersonal(PersonalModel personal)
    {
        try
        {
            var confirmDelete = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Sigur doriti sa stergeti personalul {personal.NumeComplet}?");

            if (confirmDelete)
            {
                Logger.LogInformation("Personal {PersonalName} will be deleted", personal.NumeComplet);
                // TODO: Implement actual delete logic
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting personal");
        }
    }

    #endregion

    #region Grid Events

    public void RowSelected(RowSelectEventArgs<PersonalModel> args) { }
    public void RowDeselected(RowDeselectEventArgs<PersonalModel> args) { }

    #endregion

    #region Display Helper Methods

    private string GetDepartamentDisplay(Departament? departament)
    {
        return departament switch
        {
            Departament.Administratie => "Administratie",
            Departament.Financiar => "Financiar",
            Departament.IT => "IT",
            Departament.Intretinere => "Intretinere",
            Departament.Logistica => "Logistica",
            Departament.Marketing => "Marketing",
            Departament.Receptie => "Receptie",
            Departament.ResurseUmane => "Resurse Umane",
            Departament.Securitate => "Securitate",
            Departament.Transport => "Transport",
            Departament.Juridic => "Juridic",
            Departament.RelatiiClienti => "Relatii Clienti",
            Departament.Calitate => "Calitate",
            Departament.CallCenter => "Call Center",
            _ => "Nu este specificat"
        };
    }

    private string GetStatusDisplay(StatusAngajat status)
    {
        return status switch
        {
            StatusAngajat.Activ => "Activ",
            StatusAngajat.Inactiv => "Inactiv",
            _ => status.ToString()
        };
    }

    #endregion

    #region IAsyncDisposable - Memory Leak Prevention

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            _disposed = true;

            // 1. Salvează starea grid-ului înainte de dispose
            if (GridRef != null)
            {
                var currentSettings = CaptureGridSettings();
                await GridStateService.SaveGridSettingsAsync(GRID_ID, currentSettings);
                Logger.LogDebug("Grid settings saved on disposal");
            }

            // 2. Manual disposal pentru componentele Syncfusion
            GridRef?.Dispose();
            PersonalDetailModal?.Dispose();  
            AddEditPersonalModal?.Dispose();

            Logger.LogDebug("AdministrarePersonal disposed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during disposal");
        }

        GC.SuppressFinalize(this);
    }

    #endregion
}
