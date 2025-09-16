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
        try
        {
            await LoadInitialData();
            InitializeFilterOptions();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "AdministrarePersonal initialization failed");
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

            Logger.LogInformation("Loaded {RecordCount} personal records", personalData.Data.Count());
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
            Logger.LogInformation("Opening modal for personal {PersonalName}", personal.NumeComplet);
            _state.SelectedPersonal = personal;
            _state.IsModalVisible = true;
            StateHasChanged();

            await ShowToast("Detalii", $"Afisare detalii pentru {personal.NumeComplet}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing personal detail modal");
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

            await ShowToast("Personal nou", "Completeaza formularul pentru a adauga personal nou", "e-toast-info");
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
            _state.SelectedPersonalForEdit = personal; // Păstrăm personalul original pentru referință
            _state.IsAddEditModalVisible = true;
            
            StateHasChanged();

            await ShowToast("Editare personal", $"Modificati informatiile pentru {personal.NumeComplet}", "e-toast-info");
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

    private async Task SavePersonal(PersonalModel personalModel)
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            Logger.LogInformation("Starting save process for {PersonalName}", personalModel.NumeComplet);

            ValyanClinic.Application.Services.PersonalResult result;
            
            if (_state.IsEditMode && _state.EditingPersonal != null)
            {
                // Update existing personal
                Logger.LogInformation("Updating existing personal {PersonalName} with ID {PersonalId}", 
                    personalModel.NumeComplet, personalModel.Id_Personal);
                
                result = await PersonalService.UpdatePersonalAsync(personalModel, "current_user");
                
                Logger.LogInformation("UpdatePersonalAsync returned. IsSuccess = {IsSuccess}", result.IsSuccess);
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Personal {PersonalName} updated successfully", personalModel.NumeComplet);
                    await ShowToast("Actualizare reușită", $"Personalul {personalModel.NumeComplet} a fost actualizat cu succes", "e-toast-success");
                }
                else
                {
                    Logger.LogError("Failed to update personal {PersonalName}: {Error}", 
                        personalModel.NumeComplet, result.ErrorMessage);
                    await ShowToast("Eroare actualizare", result.ErrorMessage ?? "Eroare necunoscută la actualizare", "e-toast-danger");
                    return;
                }
            }
            else
            {
                // Create new personal
                Logger.LogInformation("Creating new personal {PersonalName} with ID {PersonalId}", 
                    personalModel.NumeComplet, personalModel.Id_Personal);
                
                result = await PersonalService.CreatePersonalAsync(personalModel, "current_user");
                
                Logger.LogInformation("CreatePersonalAsync returned. IsSuccess = {IsSuccess}", result.IsSuccess);
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Personal {PersonalName} created successfully", personalModel.NumeComplet);
                    await ShowToast("Creare reușită", $"Personalul {personalModel.NumeComplet} a fost creat cu succes", "e-toast-success");
                }
                else
                {
                    Logger.LogError("Failed to create personal {PersonalName}: {Error}", 
                        personalModel.NumeComplet, result.ErrorMessage);
                    await ShowToast("Eroare creare", result.ErrorMessage ?? "Eroare necunoscută la creare", "e-toast-danger");
                    return;
                }
            }

            Logger.LogInformation("Save process completed successfully for {PersonalName}", personalModel.NumeComplet);
            // Close modal and refresh data
            await CloseAddEditModal();
            await LoadInitialData(); // Refresh the grid with new data
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while saving personal {PersonalName}", personalModel?.NumeComplet ?? "Unknown");
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
                        Logger.LogInformation("JavaScript helper functions not available yet: {Error}", retryEx.Message);
                        // Kebab menu will still work without outside click functionality
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogInformation("JavaScript helper functions not ready: {Error}", ex.Message);
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
