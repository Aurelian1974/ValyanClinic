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

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Business Logic pentru AdministrarePersonal.razor - COMPLET SIMILAR CU UTILIZATORI
/// Separated Business Logic pentru gestionarea personalului
/// </summary>
public partial class AdministrarePersonal : ComponentBase
{
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonal> Logger { get; set; } = default!;

    // Component References
    private SfGrid<PersonalModel>? GridRef;
    private SfToast? ToastRef;
    private SfDialog? PersonalDetailModal;
    private SfDialog? AddEditPersonalModal;

    // State Management
    private PersonalPageState _state = new();
    private PersonalModels _models = new();

    // Dialog Animation Settings
    private DialogAnimationSettings DialogAnimation = new()
    {
        Effect = DialogEffect.FadeZoom,
        Duration = 300
    };

    // Additional state properties for compatibility
    public Departament? SelectedDepartmentFilter { get; set; }
    public StatusAngajat? SelectedStatusFilter { get; set; }
    public string? SelectedActivityPeriod { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Logger.LogDebug("DEBUG: AdministrarePersonal component initializing...");
        try
        {
            await LoadInitialData();
            InitializeFilterOptions();
            Logger.LogDebug("DEBUG: AdministrarePersonal component initialization complete");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ERROR: AdministrarePersonal initialization failed");
            _state.SetError("Eroare la incarcarea datelor initiale");
        }
    }

    #region Data Loading

    private async Task LoadInitialData()
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            // Load personal data - using correct method
            var searchRequest = new ValyanClinic.Application.Services.PersonalSearchRequest(
                PageNumber: 1,
                PageSize: 1000, // Get all records for now
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

            Logger.LogDebug("DEBUG: Loaded {RecordCount} personal records with statistics", personalData.Data.Count());
            _state.SetError(null);
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

    #region Filter Logic

    private void InitializeFilterOptions()
    {
        _models.InitializeFilterOptions();
    }

    private async Task OnDepartmentFilterChanged(ChangeEventArgs<Departament?, PersonalModels.FilterOption<Departament?>> args)
    {
        SelectedDepartmentFilter = args.Value;
        _state.SelectedDepartment = args.Value?.ToString() ?? "";
        await ApplyAdvancedFilters();
    }

    private async Task OnStatusFilterChanged(ChangeEventArgs<StatusAngajat?, PersonalModels.FilterOption<StatusAngajat?>> args)
    {
        SelectedStatusFilter = args.Value;
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
        SelectedActivityPeriod = args.Value ?? "";
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
        SelectedDepartmentFilter = null;
        SelectedStatusFilter = null;
        SelectedActivityPeriod = null;

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
            Logger.LogDebug("DEBUG: Opening modal for personal {PersonalName}", personal.NumeComplet);
            _state.SelectedPersonal = personal;
            _state.IsModalVisible = true;
            StateHasChanged();

            await ShowToast("Detalii", $"Afisare detalii pentru {personal.NumeComplet}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ERROR: Error showing personal detail modal");
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
            Logger.LogDebug("DEBUG ShowAddPersonalModal: Starting to show add personal modal");
            _state.IsEditMode = false;
            _state.EditingPersonal = _models.CreateNewPersonal();
            _state.IsAddEditModalVisible = true;
            
            Logger.LogDebug("DEBUG ShowAddPersonalModal: Modal configuration - IsEditMode: {IsEditMode}, PersonalId: {PersonalId}, IsAddEditModalVisible: {IsModalVisible}", 
                _state.IsEditMode, _state.EditingPersonal.Id_Personal, _state.IsAddEditModalVisible);
            
            StateHasChanged();

            await ShowToast("Personal nou", "Completeaza formularul pentru a adauga personal nou", "e-toast-info");
            Logger.LogDebug("DEBUG ShowAddPersonalModal: Modal opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ERROR ShowAddPersonalModal: Error showing add personal modal");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de adaugare", "e-toast-danger");
        }
    }

    private async Task ShowEditPersonalModal(PersonalModel personal)
    {
        try
        {
            Logger.LogDebug("DEBUG ShowEditPersonalModal: Starting to show edit modal for {PersonalName}", personal.NumeComplet);
            _state.IsEditMode = true;
            _state.EditingPersonal = _models.ClonePersonal(personal);
            _state.SelectedPersonalForEdit = personal; // Păstrăm personalul original pentru referință
            _state.IsAddEditModalVisible = true;
            
            Logger.LogDebug("DEBUG ShowEditPersonalModal: Modal configuration - IsEditMode: {IsEditMode}, EditingPersonalId: {EditingPersonalId}, SelectedPersonalForEditId: {SelectedPersonalId}, IsAddEditModalVisible: {IsModalVisible}", 
                _state.IsEditMode, _state.EditingPersonal.Id_Personal, personal.Id_Personal, _state.IsAddEditModalVisible);
            
            StateHasChanged();

            await ShowToast("Editare personal", $"Modificati informatiile pentru {personal.NumeComplet}", "e-toast-info");
            Logger.LogDebug("DEBUG ShowEditPersonalModal: Edit modal opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ERROR ShowEditPersonalModal: Error showing edit personal modal");
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

    private async Task SavePersonal(PersonalModel personalModel)
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            Logger.LogDebug("DEBUG SavePersonal: Starting save process for {PersonalName}", personalModel.NumeComplet);
            Logger.LogInformation("Starting save process for {PersonalName}", personalModel.NumeComplet);
            Logger.LogDebug("DEBUG SavePersonal: Save details - IsEditMode: {IsEditMode}, PersonalId: {PersonalId}, CNP: {CNP}, Departament: {Departament}, Functia: {Functia}", 
                _state.IsEditMode, personalModel.Id_Personal, personalModel.CNP, personalModel.Departament, personalModel.Functia);

            ValyanClinic.Application.Services.PersonalResult result;
            
            if (_state.IsEditMode && _state.EditingPersonal != null)
            {
                // Update existing personal
                Logger.LogDebug("DEBUG SavePersonal: Updating existing personal {PersonalName}", personalModel.NumeComplet);
                Logger.LogInformation("Updating existing personal {PersonalName} with ID {PersonalId}", 
                    personalModel.NumeComplet, personalModel.Id_Personal);
                Logger.LogDebug("DEBUG SavePersonal: Original personal ID = {OriginalPersonalId}, Calling PersonalService.UpdatePersonalAsync...", 
                    _state.EditingPersonal.Id_Personal);
                
                result = await PersonalService.UpdatePersonalAsync(personalModel, "current_user");
                
                Logger.LogDebug("DEBUG SavePersonal: UpdatePersonalAsync returned - IsSuccess: {IsSuccess}, ErrorMessage: {ErrorMessage}", 
                    result.IsSuccess, result.ErrorMessage ?? "NULL");
                Logger.LogInformation("UpdatePersonalAsync returned. IsSuccess = {IsSuccess}", result.IsSuccess);
                
                if (result.IsSuccess)
                {
                    Logger.LogDebug("DEBUG SavePersonal: Update successful!");
                    Logger.LogInformation("Personal {PersonalName} updated successfully", personalModel.NumeComplet);
                    await ShowToast("Actualizare reușită", $"Personalul {personalModel.NumeComplet} a fost actualizat cu succes", "e-toast-success");
                }
                else
                {
                    Logger.LogError("ERROR SavePersonal: Update failed with error: {Error}", result.ErrorMessage);
                    Logger.LogError("Failed to update personal {PersonalName}: {Error}", 
                        personalModel.NumeComplet, result.ErrorMessage);
                    await ShowToast("Eroare actualizare", result.ErrorMessage ?? "Eroare necunoscută la actualizare", "e-toast-danger");
                    return;
                }
            }
            else
            {
                // Create new personal
                Logger.LogDebug("DEBUG SavePersonal: Creating new personal {PersonalName}", personalModel.NumeComplet);
                Logger.LogInformation("Creating new personal {PersonalName} with ID {PersonalId}", 
                    personalModel.NumeComplet, personalModel.Id_Personal);
                Logger.LogDebug("DEBUG SavePersonal: New personal ID = {NewPersonalId}, Calling PersonalService.CreatePersonalAsync...", 
                    personalModel.Id_Personal);
                
                result = await PersonalService.CreatePersonalAsync(personalModel, "current_user");
                
                Logger.LogDebug("DEBUG SavePersonal: CreatePersonalAsync returned - IsSuccess: {IsSuccess}, ErrorMessage: {ErrorMessage}", 
                    result.IsSuccess, result.ErrorMessage ?? "NULL");
                Logger.LogInformation("CreatePersonalAsync returned. IsSuccess = {IsSuccess}", result.IsSuccess);
                
                if (result.IsSuccess)
                {
                    Logger.LogDebug("DEBUG SavePersonal: Create successful!");
                    Logger.LogInformation("Personal {PersonalName} created successfully", personalModel.NumeComplet);
                    await ShowToast("Creare reușită", $"Personalul {personalModel.NumeComplet} a fost creat cu succes", "e-toast-success");
                }
                else
                {
                    Logger.LogError("ERROR SavePersonal: Create failed with error: {Error}", result.ErrorMessage);
                    Logger.LogError("Failed to create personal {PersonalName}: {Error}", 
                        personalModel.NumeComplet, result.ErrorMessage);
                    await ShowToast("Eroare creare", result.ErrorMessage ?? "Eroare necunoscută la creare", "e-toast-danger");
                    return;
                }
            }

            Logger.LogDebug("DEBUG SavePersonal: Closing modal and refreshing data...");
            Logger.LogInformation("Save process completed successfully for {PersonalName}", personalModel.NumeComplet);
            // Close modal and refresh data
            await CloseAddEditModal();
            await LoadInitialData(); // Refresh the grid with new data
            Logger.LogDebug("DEBUG SavePersonal: Save process completed successfully!");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ERROR SavePersonal: Exception occurred while saving personal {PersonalName}", personalModel?.NumeComplet ?? "Unknown");
            await ShowToast("Eroare", $"Eroare la salvarea personalului: {ex.Message}", "e-toast-danger");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
            Logger.LogDebug("DEBUG SavePersonal: Finally block - Loading set to false");
        }
    }

    // Metoda pentru a fi apelată de butonul submit din footer
    private async Task OnFormSubmit()
    {
        try
        {
            Logger.LogDebug("DEBUG OnFormSubmit: Form submit triggered from footer button");
            Logger.LogDebug("DEBUG OnFormSubmit: _state.EditingPersonal is {IsNotNull}, _state.IsEditMode = {IsEditMode}", 
                _state.EditingPersonal != null ? "NOT NULL" : "NULL", _state.IsEditMode);
            
            if (_state.EditingPersonal != null)
            {
                Logger.LogDebug("DEBUG OnFormSubmit: EditingPersonal details - ID: {Id}, NumeComplet: {NumeComplet}, CNP: {CNP}, Departament: {Departament}, Calling SavePersonal...", 
                    _state.EditingPersonal.Id_Personal, _state.EditingPersonal.NumeComplet, _state.EditingPersonal.CNP, _state.EditingPersonal.Departament);
                
                await SavePersonal(_state.EditingPersonal);
                Logger.LogDebug("DEBUG OnFormSubmit: SavePersonal completed");
            }
            else
            {
                Logger.LogError("ERROR OnFormSubmit: EditingPersonal is null - cannot save!");
                await ShowToast("Eroare", "Nu există date pentru salvare", "e-toast-danger");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ERROR OnFormSubmit: Exception in OnFormSubmit");
            await ShowToast("Eroare", "Eroare la trimiterea formularului", "e-toast-danger");
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

    #endregion

    #region Legacy Support Methods (pentru compatibilitate cu codul existent)

    private async Task ApplyFilters() => await ApplyAdvancedFilters();
    private async Task ClearFilters() => await ClearAdvancedFilters();
    private async Task ExportData() => await ExportFilteredData();

    private async Task OnDepartmentFilterChanged(ChangeEventArgs<string, DropdownItem> args)
    {
        if (Enum.TryParse<Departament>(args.Value, out var dept))
        {
            SelectedDepartmentFilter = dept;
            _state.SelectedDepartment = args.Value ?? "";
        }
        else
        {
            SelectedDepartmentFilter = null;
            _state.SelectedDepartment = "";
        }
        await ApplyAdvancedFilters();
    }

    private async Task OnStatusFilterChanged(ChangeEventArgs<string, StatusItem> args)
    {
        if (Enum.TryParse<StatusAngajat>(args.Value, out var status))
        {
            SelectedStatusFilter = status;
            _state.SelectedStatus = args.Value ?? "";
        }
        else
        {
            SelectedStatusFilter = null;
            _state.SelectedStatus = "";
        }
        await ApplyAdvancedFilters();
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
        _state.ShowKebabMenu = false; // Close menu after selection
        StateHasChanged();
    }

    private void ToggleAdvancedFilters()
    {
        _state.ShowAdvancedFilters = !_state.ShowAdvancedFilters;
        _state.ShowKebabMenu = false; // Close menu after selection
        StateHasChanged();
    }

    // Close kebab menu when clicking outside
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Check if the JavaScript function exists before calling it
                var functionExists = await JSRuntime.InvokeAsync<bool>("eval", "typeof window.addClickEventListener === 'function'");
                
                if (functionExists)
                {
                    await JSRuntime.InvokeVoidAsync("window.addClickEventListener", 
                        DotNetObjectReference.Create(this));
                }
                else
                {
                    // Try again after a short delay to allow scripts to load
                    await Task.Delay(100);
                    try
                    {
                        await JSRuntime.InvokeVoidAsync("window.addClickEventListener", 
                            DotNetObjectReference.Create(this));
                    }
                    catch (Exception retryEx)
                    {
                        Logger.LogDebug("JavaScript helper functions not available yet: {Error}", retryEx.Message);
                        // Kebab menu will still work without outside click functionality
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("JavaScript helper functions not ready: {Error}", ex.Message);
                // Non-critical error - kebab menu will still work, just won't close on outside click
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
}
