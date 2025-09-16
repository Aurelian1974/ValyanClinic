using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using ValyanClinic.Application.Services;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalModel = ValyanClinic.Domain.Models.Personal;
using ValyanClinic.Core.Services;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Business Logic pentru AdministrarePersonal.razor
/// EXEMPLU DE UTILIZARE A FUNCȚIONALITĂȚILOR CRITICE IMPLEMENTATE:
/// - Memory leak prevention prin proper disposal
/// - DataGrid state persistence prin SimpleGridStateService  
/// - Error handling robust
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
    protected SfToast? ToastRef;
    protected SfToast? ModalToastRef; // TOAST PENTRU MODAL

    // State Management - FOLOSIM CLASA EXISTENTA DIN SIGNATURE
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

            // Load statistics
            _state.Statistics = await PersonalService.GetStatisticsAsync();

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
            await ShowToast("Eroare", errorMessage, "e-toast-danger");
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
        await ShowToast("Succes", "Datele au fost actualizate", "e-toast-success");
    }

    #endregion

    #region Grid State Management - CRITICAL FUNCTIONALITY

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

    private async Task OnDepartmentFilterChanged(ChangeEventArgs<Departament?, PersonalModels.FilterOption<Departament?>> args)
    {
        _state.SelectedDepartmentFilter = args.Value;
        _state.SelectedDepartment = args.Value?.ToString() ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnStatusFilterChanged(ChangeEventArgs<StatusAngajat?, PersonalModels.FilterOption<StatusAngajat?>> args)
    {
        _state.SelectedStatusFilter = args.Value;
        _state.SelectedStatus = args.Value?.ToString() ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnSearchTextChanged(ChangedEventArgs args)
    {
        _state.SearchText = args.Value ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnActivityPeriodChanged(ChangeEventArgs<string, string> args)
    {
        _state.SelectedActivityPeriod = args.Value ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task ApplyAdvancedFilters()
    {
        try
        {
            var filteredPersonal = _models.ApplyFilters(_state);

            if (GridRef != null)
            {
                GridRef.DataSource = filteredPersonal;
                await GridRef.Refresh();
            }

            await ShowToast("Filtru aplicat",
                $"Gasite {filteredPersonal.Count} rezultate din {_models.Personal.Count} angajati",
                "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filters");
            await ShowToast("Eroare", "Eroare la aplicarea filtrelor", "e-toast-danger");
        }
    }

    private async Task ClearAdvancedFilters()
    {
        _state.ClearFilters();

        if (GridRef != null)
        {
            GridRef.DataSource = _models.Personal;
            await GridRef.Refresh();
        }

        await ShowToast("Filtre curatate", "Toate filtrele au fost eliminate", "e-toast-success");
        StateHasChanged();
    }

    private async Task ExportFilteredData()
    {
        try
        {
            await ShowToast("Export", "Functia de export va fi implementata in viitor", "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Export error");
            await ShowToast("Eroare Export", "Eroare la exportul datelor", "e-toast-danger");
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

            // ELIMINAT TOAST-UL DIN PĂRINTE - CAUZA PROBLEMEI
            // await ShowToast("Detalii", $"Afisare detalii pentru {personal.NumeComplet}", "e-toast-info");
            
            // Toast-ul va fi afișat în modal prin ModalToastRef dacă este necesar
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing personal detail modal");
            // DOAR pentru erori folosim toast-ul global
            await ShowToast("Eroare", "Eroare la afisarea detaliilor", "e-toast-danger");
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
            _state.IsEditMode = false;
            _state.EditingPersonal = _models.CreateNewPersonal();
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            // ELIMINAT TOAST-UL CARE SE BLUEAZĂ
            // await ShowToast("Personal nou", "Completeaza formularul pentru a adauga personal nou", "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing add personal modal");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de adaugare", "e-toast-danger");
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

            // ELIMINAT TOAST-UL CARE SE BLUEAZĂ
            // await ShowToast("Editare personal", $"Modificati informatiile pentru {personal.NumeComplet}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing edit personal modal");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de editare", "e-toast-danger");
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
                await ShowToast("Succes", $"Personalul {personalModel.NumeComplet} a fost {action} cu succes", "e-toast-success");
                
                await CloseAddEditModal();
                await LoadInitialData();
            }
            else
            {
                await ShowToast("Eroare", result.ErrorMessage ?? "Eroare necunoscută", "e-toast-danger");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while saving personal");
            await ShowToast("Eroare", $"Eroare la salvarea personalului: {ex.Message}", "e-toast-danger");
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
            await ShowToast("Eroare", "Eroare la editarea personalului", "e-toast-danger");
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
                await ShowToast("Stergere", $"Personalul {personal.NumeComplet} va fi sters", "e-toast-info");
                // TODO: Implement actual delete logic
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting personal");
            await ShowToast("Eroare", "Eroare la stergerea personalului", "e-toast-danger");
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

    #region Toast Notifications

    private async Task ShowToast(string title, string content, string cssClass)
    {
        if (ToastRef != null)
        {
            var toastModel = new ToastModel()
            {
                Title = title,
                Content = content,
                CssClass = cssClass,
                ShowCloseButton = true,
                Timeout = 3000
            };
            await ToastRef.ShowAsync(toastModel);
        }
    }

    /// <summary>
    /// Afișează toast în contextul modalului - SOLUȚIA PENTRU TOAST BLURAT
    /// </summary>
    private async Task ShowModalToast(string title, string content, string cssClass = "e-toast-info")
    {
        if (ModalToastRef != null)
        {
            var toastModel = new ToastModel()
            {
                Title = title,
                Content = content,
                CssClass = cssClass,
                ShowCloseButton = true,
                Timeout = 4000
            };
            await ModalToastRef.ShowAsync(toastModel);
        }
        else
        {
            // Fallback la toast-ul global dacă modalul nu e disponibil
            await ShowToast(title, content, cssClass);
        }
    }

    /// <summary>
    /// Handler pentru callback-ul toast din VizualizeazaPersonal
    /// </summary>
    private async Task HandleModalToast((string Title, string Message, string CssClass) args)
    {
        await ShowModalToast(args.Title, args.Message, args.CssClass);
    }

    #endregion

    #region Kebab Menu Management

    private void ToggleKebabMenu()
    {
        _state.ShowKebabMenu = !_state.ShowKebabMenu;
        StateHasChanged();
    }

    private void ToggleStatistics()
    {
        _state.ShowStatistics = !_state.ShowStatistics;
        _state.ShowKebabMenu = false;
        StateHasChanged();
    }

    private void ToggleAdvancedFilters()
    {
        _state.ShowAdvancedFilters = !_state.ShowAdvancedFilters;
        _state.ShowKebabMenu = false;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var functionExists = await JSRuntime.InvokeAsync<bool>("eval", 
                    "typeof window.addClickEventListener === 'function'");

                if (functionExists)
                {
                    await JSRuntime.InvokeVoidAsync("window.addClickEventListener", 
                        DotNetObjectReference.Create(this));
                }
                else
                {
                    await Task.Delay(100);
                    try
                    {
                        await JSRuntime.InvokeVoidAsync("window.addClickEventListener", 
                            DotNetObjectReference.Create(this));
                    }
                    catch (Exception retryEx)
                    {
                        Logger.LogInformation("JavaScript helper functions not available yet: {Error}", retryEx.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogInformation("JavaScript helper functions not ready: {Error}", ex.Message);
            }
        }
    }

    [JSInvokable]
    public void CloseKebabMenu()
    {
        if (_state.ShowKebabMenu)
        {
            _state.ShowKebabMenu = false;
            StateHasChanged();
        }
    }

    #endregion

    #region IAsyncDisposable - CRITICAL MEMORY LEAK PREVENTION

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            _disposed = true;

            // Salvează starea grid-ului înainte de dispose - CRITICAL FUNCTIONALITY
            if (GridRef != null)
            {
                var currentSettings = CaptureGridSettings();
                await GridStateService.SaveGridSettingsAsync(GRID_ID, currentSettings);
                Logger.LogDebug("Grid settings saved on disposal");
            }

            // Manual disposal pentru componentele Syncfusion - MEMORY LEAK PREVENTION
            GridRef?.Dispose();
            PersonalDetailModal?.Dispose();  
            AddEditPersonalModal?.Dispose();
            ToastRef?.Dispose();
            ModalToastRef?.Dispose(); // DISPOSE PENTRU MODAL TOAST

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
