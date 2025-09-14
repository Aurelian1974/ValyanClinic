using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
        Console.WriteLine("DEBUG: AdministrarePersonal component initializing...");
        try
        {
            await LoadInitialData();
            InitializeFilterOptions();
            Console.WriteLine("DEBUG: AdministrarePersonal component initialization complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: AdministrarePersonal initialization failed: {ex.Message}");
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

            Console.WriteLine($"DEBUG: Loaded {personalData.Data.Count()} personal records with statistics");
            _state.SetError(null);
        }
        catch (Exception ex)
        {
            var errorMessage = "Nu s-au putut incarca datele personalului";
            Console.WriteLine($"Error loading personal data: {ex.Message}");
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
            Console.WriteLine($"Error applying filters: {ex.Message}");
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
            Console.WriteLine($"Export error: {ex.Message}");
            await ShowToast("Eroare Export", "Eroare la exportul datelor", "e-toast-danger");
        }
    }

    #endregion

    #region Personal Detail Modal

    private async Task ShowPersonalDetailModal(PersonalModel personal)
    {
        try
        {
            Console.WriteLine($"DEBUG: Opening modal for personal {personal.NumeComplet}");
            _state.SelectedPersonal = personal;
            _state.IsModalVisible = true;
            StateHasChanged();

            await ShowToast("Detalii", $"Afisare detalii pentru {personal.NumeComplet}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Error showing personal detail modal: {ex.Message}");
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
            Console.WriteLine($"Error closing modal: {ex.Message}");
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

    #endregion

    #region Add/Edit Personal Modal

    private async Task ShowAddPersonalModal()
    {
        try
        {
            Console.WriteLine("DEBUG: Opening Add Personal Modal");
            _state.IsEditMode = false;
            _state.EditingPersonal = _models.CreateNewPersonal();
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            await ShowToast("Personal nou", "Completeaza formularul pentru a adauga personal nou", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Error showing add personal modal: {ex.Message}");
            await ShowToast("Eroare", "Eroare la deschiderea formularului de adaugare", "e-toast-danger");
        }
    }

    private async Task ShowEditPersonalModal(PersonalModel personal)
    {
        try
        {
            _state.IsEditMode = true;
            _state.EditingPersonal = _models.ClonePersonal(personal);
            _state.SelectedPersonalForEdit = personal; // P?str?m personalul original pentru referin??
            _state.IsAddEditModalVisible = true;
            StateHasChanged();

            await ShowToast("Editare personal", $"Modificati informatiile pentru {personal.NumeComplet}", "e-toast-info");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing edit personal modal: {ex.Message}");
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
            Console.WriteLine($"Error closing add/edit modal: {ex.Message}");
            _state.IsAddEditModalVisible = false;
            _state.EditingPersonal = null;
            _state.IsEditMode = false;
            StateHasChanged();
        }
    }

    private void OnAddEditModalClosed()
    {
        _state.IsAddEditModalVisible = false;
        _state.EditingPersonal = null;
        _state.IsEditMode = false;
        StateHasChanged();
    }

    private async Task SavePersonal()
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            if (_state.IsEditMode && _state.EditingPersonal != null)
            {
                // TODO: Implement actual update logic
                await ShowToast("Actualizare", $"Personalul {_state.EditingPersonal.NumeComplet} a fost actualizat cu succes", "e-toast-success");
            }
            else if (_state.EditingPersonal != null)
            {
                // TODO: Implement actual create logic  
                await ShowToast("Creare", $"Personalul {_state.EditingPersonal.NumeComplet} a fost creat cu succes", "e-toast-success");
            }

            await Task.Delay(1000); // Simulate processing time
            await CloseAddEditModal();
            await LoadInitialData(); // Refresh the grid
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving personal: {ex.Message}");
            await ShowToast("Eroare", $"Eroare la salvarea personalului: {ex.Message}", "e-toast-danger");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    // Metod? pentru a fi apelat? de butonul submit din footer
    private async Task OnFormSubmit()
    {
        try
        {
            // În loc să căutăm un form specific, să deleghum salvarea direct la componenta AdaugaEditezaPersonal
            // Componenta copil va gestiona propria validare și submit
            await SavePersonal();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnFormSubmit: {ex.Message}");
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
            Console.WriteLine($"Error editing personal: {ex.Message}");
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
            Console.WriteLine($"Error deleting personal: {ex.Message}");
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
                // Enhanced JavaScript call with proper error handling
                await JSRuntime.InvokeVoidAsync("window.addClickEventListener", 
                    DotNetObjectReference.Create(this));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not add click event listener: {ex.Message}");
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
