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
using PersonalMedicalModel = ValyanClinic.Domain.Models.PersonalMedical;
using ValyanClinic.Core.Services;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Business Logic pentru AdministrarePersonalMedical.razor
/// Adaptat din AdministrarePersonal.razor pentru gestionarea personalului medical
/// DIFERENȚE CHEIE:
/// - PersonalMedical în loc de Personal
/// - Departamente din BD în loc de enum-uri
/// - Poziții medicale specifice
/// - Licența medicală și specializări
/// </summary>
public partial class AdministrarePersonalMedical : ComponentBase, IAsyncDisposable
{
    [Inject] private IPersonalMedicalService PersonalMedicalService { get; set; } = default!;
    [Inject] private IDepartamentMedicalService DepartamentMedicalService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonalMedical> Logger { get; set; } = default!;
    [Inject] private ISimpleGridStateService GridStateService { get; set; } = default!;

    // Component References - ADAPTED FOR MEDICAL STAFF
    protected SfGrid<PersonalMedicalModel>? GridRef;
    protected SfDialog? PersonalMedicalDetailModal;
    protected SfDialog? AddEditPersonalMedicalModal;
    protected SfToast? ToastRef;
    protected SfToast? ModalToastRef;

    // State Management - USING MEDICAL-SPECIFIC CLASSES
    private PersonalMedicalPageState _state = new();
    private PersonalMedicalModels _models = new();
    private bool _disposed = false;

    // Dialog Animation Settings
    private DialogAnimationSettings DialogAnimation = new()
    {
        Effect = DialogEffect.FadeZoom,
        Duration = 300
    };

    // Grid ID pentru persistență - UNIQUE FOR MEDICAL STAFF
    private const string GRID_ID = "personal-medical-management-grid";

    // JavaScript interaction reference
    private DotNetObjectReference<AdministrarePersonalMedical>? _dotNetReference;
    private bool _eventListenersInitialized = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("🏥 Initializing PersonalMedical management page");
            
            await LoadInitialData();
            await LoadDepartamenteMedicale(); // CRUCIAL - încărcare departamente din DB
            InitializeFilterOptions();

            // Load grid settings din persistență
            var savedSettings = await GridStateService.GetGridSettingsAsync(GRID_ID);
            if (savedSettings != null)
            {
                ApplyGridSettings(savedSettings);
                Logger.LogInformation("Grid settings loaded for PersonalMedical management");
            }

            // Initialize JavaScript helpers for kebab menu
            await InitializeJavaScriptHelpers();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error initializing PersonalMedical management page");
            _state.SetError($"Eroare la inițializarea paginii: {ex.Message}");
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_eventListenersInitialized)
        {
            await InitializeJavaScriptHelpers();
        }
    }

    #region JavaScript Integration for Kebab Menu

    private async Task InitializeJavaScriptHelpers()
    {
        try
        {
            if (_disposed) return;

            _dotNetReference ??= DotNetObjectReference.Create(this);

            // Try to initialize with exponential backoff
            var maxRetries = 3;
            var delay = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var jsReady = await JSRuntime.InvokeAsync<bool>("eval", 
                        "typeof window !== 'undefined' && typeof document !== 'undefined'");
                    
                    if (jsReady)
                    {
                        var clickSuccess = await JSRuntime.InvokeAsync<bool>(
                            "window.addClickEventListener", _dotNetReference);
                        var escapeSuccess = await JSRuntime.InvokeAsync<bool>(
                            "window.addEscapeKeyListener", _dotNetReference);
                        
                        if (clickSuccess && escapeSuccess)
                        {
                            _eventListenersInitialized = true;
                            Logger.LogInformation("✅ JavaScript event listeners initialized for PersonalMedical kebab menu");
                            return;
                        }
                    }

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delay);
                        delay *= 2;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "⚠️ JavaScript setup attempt {Attempt} failed for PersonalMedical", attempt);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Critical error initializing JavaScript helpers for PersonalMedical");
        }
    }

    [JSInvokable]
    public async Task CloseKebabMenu()
    {
        if (_disposed) return;
        
        try
        {
            if (_state.ShowKebabMenu)
            {
                Logger.LogInformation("🔘 Closing PersonalMedical kebab menu via JavaScript event");
                _state.ShowKebabMenu = false;
                SafeStateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error closing PersonalMedical kebab menu");
            _state.ShowKebabMenu = false;
            SafeStateHasChanged();
        }
    }

    #endregion

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
            Logger?.LogDebug("StateHasChanged called on disposed PersonalMedical component");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in SafeStateHasChanged for PersonalMedical");
        }
    }

    #region Data Loading - ADAPTED FOR MEDICAL STAFF

    private async Task LoadInitialData()
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            Logger.LogInformation("🔄 Loading PersonalMedical data...");

            // Load personal medical data with proper search request
            var searchRequest = new PersonalMedicalSearchRequest(
                PageNumber: 1,
                PageSize: 1000, // Load all for grid display
                SearchText: null,
                Departament: null,
                Pozitie: null,
                Status: null
            );

            var personalMedicalResult = await PersonalMedicalService.GetPersonalMedicalAsync(searchRequest);
            _models.SetPersonalMedical(personalMedicalResult.Data.ToList());

            Logger.LogInformation("✅ Loaded {Count} PersonalMedical records", personalMedicalResult.Data.Count());
            _state.ClearError();
        }
        catch (Exception ex)
        {
            var errorMessage = "Nu s-au putut incarca datele personalului medical";
            Logger.LogError(ex, "💥 Error loading PersonalMedical data");
            _state.SetError(errorMessage);
            await ShowToast("Eroare", errorMessage, "e-toast-danger");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    /// <summary>
    /// CRUCIAL PENTRU PERSONAL MEDICAL - Încarcă departamentele din baza de date
    /// </summary>
    private async Task LoadDepartamenteMedicale()
    {
        try
        {
            Logger.LogInformation("🏥 Loading departamente medicale from database...");

            var departamente = await DepartamentMedicalService.GetAllDepartamenteMedicaleAsync();
            _state.SetDepartmentOptions(departamente.ToList());
            _models.InitializeFilterOptions(departamente.ToList());

            Logger.LogInformation("✅ Loaded {Count} departamente medicale", departamente.Count());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error loading departamente medicale");
            await ShowToast("Eroare", "Nu s-au putut incarca departamentele medicale", "e-toast-warning");
        }
    }

    private async Task RefreshData()
    {
        try
        {
            await LoadInitialData();
            await LoadDepartamenteMedicale();
            
            if (GridRef != null)
            {
                await GridRef.Refresh();
            }
            
            await ShowToast("Succes", "Datele personalului medical au fost actualizate", "e-toast-success");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error refreshing PersonalMedical data");
            await ShowToast("Eroare", "Eroare la actualizarea datelor", "e-toast-danger");
        }
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
                settings["pageSize"] = GridRef.PageSettings?.PageSize ?? 10;
                settings["currentPage"] = GridRef.PageSettings?.CurrentPage ?? 1;
                settings["lastSaved"] = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "⚠️ Error capturing PersonalMedical grid settings");
        }

        return settings;
    }

    private void ApplyGridSettings(Dictionary<string, object> settings)
    {
        try
        {
            if (settings.TryGetValue("pageSize", out var pageSize))
            {
                Logger.LogDebug("Applied saved page size: {PageSize} for PersonalMedical", pageSize);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "⚠️ Error applying PersonalMedical grid settings");
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
            Logger.LogWarning(ex, "⚠️ Error auto-saving PersonalMedical grid state");
        }
    }

    #endregion

    #region Filter Logic - ADAPTED FOR MEDICAL STAFF

    private void InitializeFilterOptions()
    {
        // Filter options sunt inițializate în _models.InitializeFilterOptions()
        // după încărcarea departamentelor medicale
        Logger.LogInformation("🔍 Filter options initialized for PersonalMedical");
    }

    private async Task OnDepartmentFilterChanged(ChangeEventArgs<string, DepartamentFilterItem> args)
    {
        _state.SelectedDepartment = args.Value ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnPozitieFilterChanged(ChangeEventArgs<PozitiePersonalMedical?, PozitieFilterItem> args)
    {
        _state.SelectedPozitie = args.Value;
        await ApplyAdvancedFilters();
    }

    private async Task OnStatusFilterChanged(ChangeEventArgs<bool?, StatusFilterItem> args)
    {
        _state.SelectedStatus = args.Value;
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
            var filteredPersonalMedical = _models.ApplyFilters(_state);

            if (GridRef != null)
            {
                GridRef.DataSource = filteredPersonalMedical;
                await GridRef.Refresh();
            }

            await ShowToast("Filtru aplicat",
                $"Gasite {filteredPersonalMedical.Count} rezultate din {_models.PersonalMedical.Count} personal medical",
                "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error applying PersonalMedical filters");
            await ShowToast("Eroare", "Eroare la aplicarea filtrelor", "e-toast-danger");
        }
    }

    private async Task ClearAdvancedFilters()
    {
        _state.ClearFilters();

        if (GridRef != null)
        {
            GridRef.DataSource = _models.PersonalMedical;
            await GridRef.Refresh();
        }

        await ShowToast("Filtre curatate", "Toate filtrele au fost eliminate", "e-toast-success");
        StateHasChanged();
    }

    private async Task ExportFilteredData()
    {
        try
        {
            await ShowToast("Export", "Functia de export pentru personal medical va fi implementata in viitor", "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 PersonalMedical export error");
            await ShowToast("Eroare Export", "Eroare la exportul datelor", "e-toast-danger");
        }
    }

    #endregion

    #region Personal Medical Detail Modal

    private async Task ShowPersonalMedicalDetailModal(PersonalMedicalModel personalMedical)
    {
        try
        {
            Logger.LogInformation("🏥 Opening modal for PersonalMedical {PersonalName}", personalMedical.NumeComplet);
            _state.SelectedPersonalMedical = personalMedical;
            _state.IsModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error showing PersonalMedical detail modal");
            await ShowToast("Eroare", "Eroare la afisarea detaliilor", "e-toast-danger");
        }
    }

    private async Task ClosePersonalMedicalDetailModal()
    {
        try
        {
            _state.IsModalVisible = false;
            _state.SelectedPersonalMedical = null;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error closing PersonalMedical modal");
            _state.IsModalVisible = false;
            _state.SelectedPersonalMedical = null;
            StateHasChanged();
        }
    }

    private async Task EditPersonalMedicalFromModal()
    {
        if (_state.SelectedPersonalMedical != null)
        {
            var personalMedicalToEdit = _state.SelectedPersonalMedical;
            await ClosePersonalMedicalDetailModal();
            await Task.Delay(200);
            await ShowEditPersonalMedicalModal(personalMedicalToEdit);
        }
    }

    private void OnModalClosed()
    {
        _state.IsModalVisible = false;
        _state.SelectedPersonalMedical = null;
        StateHasChanged();
    }

    private void OnAddEditModalClosed()
    {
        _state.IsAddEditModalVisible = false;
        _state.EditingPersonalMedical = null;
        _state.IsEditMode = false;
        StateHasChanged();
    }

    #endregion

    #region Add/Edit Personal Medical Modal

    private async Task ShowAddPersonalMedicalModal()
    {
        try
        {
            Logger.LogInformation("🚀 ShowAddPersonalMedicalModal called - Opening add PersonalMedical modal");
            
            _state.IsEditMode = false;
            _state.EditingPersonalMedical = _models.CreateNewPersonalMedical();
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            Logger.LogInformation("✅ Add PersonalMedical modal state set - IsAddEditModalVisible: {IsVisible}", 
                _state.IsAddEditModalVisible);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error showing add PersonalMedical modal");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de adaugare", "e-toast-danger");
        }
    }

    private async Task ShowEditPersonalMedicalModal(PersonalMedicalModel personalMedical)
    {
        try
        {
            Logger.LogInformation("✏️ Starting to show edit modal for PersonalMedical {PersonalName}", personalMedical.NumeComplet);
            _state.IsEditMode = true;
            _state.EditingPersonalMedical = _models.ClonePersonalMedical(personalMedical);
            _state.SelectedPersonalMedicalForEdit = personalMedical;
            _state.IsAddEditModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error showing edit PersonalMedical modal");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de editare", "e-toast-danger");
        }
    }

    private async Task CloseAddEditModal()
    {
        try
        {
            _state.IsAddEditModalVisible = false;
            _state.EditingPersonalMedical = null;
            _state.IsEditMode = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error closing add/edit PersonalMedical modal");
            _state.IsAddEditModalVisible = false;
            _state.EditingPersonalMedical = null;
            _state.IsEditMode = false;
            StateHasChanged();
        }
    }

    // Reference către componenta AdaugaEditezaPersonalMedical pentru a putea apela submit-ul
    // NOTE: These components don't exist yet and will need to be created
    private object? _currentFormComponent; // Using object for now until components are created

    private async Task HandleFormSubmit()
    {
        // TODO: Implement when form component is created
        await ShowToast("Info", "Form component not yet implemented", "e-toast-info");
    }

    private async Task SavePersonalMedical(PersonalMedicalModel personalMedicalModel)
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            Logger.LogInformation("🔄 Starting save process for PersonalMedical {PersonalName}", personalMedicalModel.NumeComplet);

            // TODO: Implement actual save logic when PersonalMedicalService is ready
            // PersonalMedicalResult result;
            // if (_state.IsEditMode)
            // {
            //     result = await PersonalMedicalService.UpdatePersonalMedicalAsync(personalMedicalModel, "current_user");
            // }
            // else
            // {
            //     result = await PersonalMedicalService.CreatePersonalMedicalAsync(personalMedicalModel, "current_user");
            // }

            // Temporary success simulation
            await Task.Delay(1000); // Simulate processing time
            
            var action = _state.IsEditMode ? "actualizat" : "creat";
            await ShowToast("Succes", $"Personalul medical {personalMedicalModel.NumeComplet} a fost {action} cu succes", "e-toast-success");
            
            await CloseAddEditModal();
            await LoadInitialData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Exception occurred while saving PersonalMedical");
            await ShowToast("Eroare", $"Eroare la salvarea personalului medical: {ex.Message}", "e-toast-danger");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    #endregion

    #region Personal Medical Actions

    private async Task EditPersonalMedical(PersonalMedicalModel personalMedical)
    {
        try
        {
            await ShowEditPersonalMedicalModal(personalMedical);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error editing PersonalMedical");
            await ShowToast("Eroare", "Eroare la editarea personalului medical", "e-toast-danger");
        }
    }

    private async Task DeletePersonalMedical(PersonalMedicalModel personalMedical)
    {
        try
        {
            var confirmDelete = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Sigur doriti sa stergeti personalul medical {personalMedical.NumeComplet}?");

            if (confirmDelete)
            {
                await ShowToast("Stergere", $"Personalul medical {personalMedical.NumeComplet} va fi sters", "e-toast-info");
                // TODO: Implement actual delete logic
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error deleting PersonalMedical");
            await ShowToast("Eroare", "Eroare la stergerea personalului medical", "e-toast-danger");
        }
    }

    #endregion

    #region Grid Events

    public void RowSelected(RowSelectEventArgs<PersonalMedicalModel> args) 
    { 
        Logger.LogDebug("PersonalMedical row selected: {PersonalName}", args.Data?.NumeComplet);
    }
    
    public void RowDeselected(RowDeselectEventArgs<PersonalMedicalModel> args) 
    { 
        Logger.LogDebug("PersonalMedical row deselected: {PersonalName}", args.Data?.NumeComplet);
    }

    #endregion

    #region Display Helper Methods - MEDICAL STAFF SPECIFIC

    private string GetPozitieIcon(PozitiePersonalMedical pozitie)
    {
        return pozitie switch
        {
            PozitiePersonalMedical.Doctor => "fas fa-user-md",
            PozitiePersonalMedical.AsistentMedical => "fas fa-user-nurse",
            PozitiePersonalMedical.TehnicianMedical => "fas fa-microscope",
            PozitiePersonalMedical.ReceptionerMedical => "fas fa-clipboard-user",
            PozitiePersonalMedical.Radiolog => "fas fa-x-ray",
            PozitiePersonalMedical.Laborant => "fas fa-flask",
            _ => "fas fa-user"
        };
    }

    #endregion

    #region Toast Notifications

    private async Task ShowToast(string title, string content, string cssClass)
    {
        if (ToastRef != null && !_disposed)
        {
            try
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
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "⚠️ Error showing toast notification");
            }
        }
    }

    /// <summary>
    /// Afișează toast în contextul modalului pentru PersonalMedical
    /// </summary>
    private async Task ShowModalToast(string title, string content, string cssClass = "e-toast-info")
    {
        if (ModalToastRef != null && !_disposed)
        {
            try
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
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "⚠️ Error showing modal toast");
                await ShowToast(title, content, cssClass); // Fallback
            }
        }
        else
        {
            await ShowToast(title, content, cssClass);
        }
    }

    /// <summary>
    /// Handler pentru callback-ul toast din VizualizeazaPersonalMedical
    /// </summary>
    private async Task HandleModalToast((string Title, string Message, string CssClass) args)
    {
        await ShowModalToast(args.Title, args.Message, args.CssClass);
    }

    #endregion

    #region Kebab Menu Management

    private void ToggleKebabMenu()
    {
        try
        {
            var previousState = _state.ShowKebabMenu;
            _state.ShowKebabMenu = !_state.ShowKebabMenu;
            
            Logger.LogInformation("🔘 PersonalMedical kebab menu toggled: {PreviousState} → {NewState}", 
                previousState, _state.ShowKebabMenu);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Critical error toggling PersonalMedical kebab menu");
            _state.ShowKebabMenu = false;
            StateHasChanged();
        }
    }

    private void ToggleStatistics()
    {
        try
        {
            var previousState = _state.ShowStatistics;
            _state.ShowStatistics = !_state.ShowStatistics;
            _state.ShowKebabMenu = false; // Close kebab menu
            
            Logger.LogInformation("📊 PersonalMedical statistics toggled: {PreviousState} → {NewState}", 
                previousState, _state.ShowStatistics);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error toggling PersonalMedical statistics");
            _state.ShowKebabMenu = false;
            StateHasChanged();
        }
    }

    private void ToggleAdvancedFilters()
    {
        try
        {
            var previousState = _state.ShowAdvancedFilters;
            _state.ShowAdvancedFilters = !_state.ShowAdvancedFilters;
            _state.ShowKebabMenu = false; // Close kebab menu
            
            Logger.LogInformation("🔍 PersonalMedical advanced filters toggled: {PreviousState} → {NewState}", 
                previousState, _state.ShowAdvancedFilters);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error toggling PersonalMedical advanced filters");
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

            Logger.LogInformation("🧹 Disposing AdministrarePersonalMedical component");

            // 1. Cleanup JavaScript resources first
            try
            {
                if (_eventListenersInitialized && JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("window.removeEventListeners");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "⚠️ Error cleaning up JavaScript resources for PersonalMedical");
            }

            // 2. Dispose .NET reference
            _dotNetReference?.Dispose();
            _dotNetReference = null;

            // 3. Salvează starea grid-ului înainte de dispose
            if (GridRef != null && GridStateService != null)
            {
                try
                {
                    var currentSettings = CaptureGridSettings();
                    await GridStateService.SaveGridSettingsAsync(GRID_ID, currentSettings);
                    Logger.LogDebug("PersonalMedical grid settings saved on disposal");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "⚠️ Error saving PersonalMedical grid settings on disposal");
                }
            }

            // 4. Manual disposal pentru componentele Syncfusion
            GridRef?.Dispose();
            PersonalMedicalDetailModal?.Dispose();  
            AddEditPersonalMedicalModal?.Dispose();
            ToastRef?.Dispose();
            ModalToastRef?.Dispose();

            Logger.LogInformation("✅ AdministrarePersonalMedical disposed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Critical error during PersonalMedical disposal");
        }

        GC.SuppressFinalize(this);
    }

    #endregion
}
