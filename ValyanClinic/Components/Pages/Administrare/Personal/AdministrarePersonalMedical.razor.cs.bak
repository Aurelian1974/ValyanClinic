using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using ValyanClinic.Application.Services;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalMedicalModel = ValyanClinic.Domain.Models.PersonalMedical;
using ValyanClinic.Core.Services;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Business Logic pentru AdministrarePersonalMedical.razor
/// CLEAN VERSION - Removed kebab menu and advanced filtering
/// </summary>
public partial class AdministrarePersonalMedical : ComponentBase, IAsyncDisposable
{
    [Inject] private IPersonalMedicalService PersonalMedicalService { get; set; } = default!;
    [Inject] private IDepartamentMedicalService DepartamentMedicalService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonalMedical> Logger { get; set; } = default!;
    [Inject] private ISimpleGridStateService GridStateService { get; set; } = default!;

    // Component References
    protected SfGrid<PersonalMedicalModel>? GridRef;
    protected SfDialog? PersonalMedicalDetailModal;
    protected SfDialog? AddEditPersonalMedicalModal;
    protected SfToast? ToastRef;
    protected SfToast? ModalToastRef;

    // State Management
    private PersonalMedicalPageState _state = new();
    private PersonalMedicalModels _models = new();
    private bool _disposed = false;

    // Dialog Animation Settings
    private DialogAnimationSettings DialogAnimation = new()
    {
        Effect = DialogEffect.FadeZoom,
        Duration = 300
    };

    private const string GRID_ID = "personal-medical-management-grid";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("🏥 Initializing PersonalMedical management page");
            
            await LoadInitialData();
            await LoadDepartamenteMedicale();

            var savedSettings = await GridStateService.GetGridSettingsAsync(GRID_ID);
            if (savedSettings != null)
            {
                ApplyGridSettings(savedSettings);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error initializing PersonalMedical management page");
            _state.SetError($"Eroare la inițializarea paginii: {ex.Message}");
            StateHasChanged();
        }
    }

    #region Data Loading

    private async Task LoadInitialData()
    {
        try
        {
            _state.SetLoading(true);
            StateHasChanged();

            try
            {
                var searchRequest = new PersonalMedicalSearchRequest(
                    PageNumber: 1,
                    PageSize: 1000,
                    SearchText: null,
                    Departament: null,
                    Pozitie: null,
                    Status: null
                );

                var personalMedicalResult = await PersonalMedicalService.GetPersonalMedicalAsync(searchRequest);
                _models.SetPersonalMedical(personalMedicalResult.Data.ToList());
                Logger.LogInformation("✅ Loaded {Count} PersonalMedical records", personalMedicalResult.Data.Count());
            }
            catch (Exception serviceEx)
            {
                Logger.LogWarning(serviceEx, "⚠️ Service failed - using demo data");
                
                var demoPersonalMedical = CreateDemoPersonalMedical();
                _models.SetPersonalMedical(demoPersonalMedical);
                
                await ShowToast("Demo Mode", "Folosind date demo", "e-toast-info");
            }

            _state.ClearError();
        }
        catch (Exception ex)
        {
            var errorMessage = "Nu s-au putut incarca datele personalului medical";
            Logger.LogError(ex, "💥 Error loading data");
            _state.SetError(errorMessage);
            await ShowToast("Eroare", errorMessage, "e-toast-danger");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    private async Task LoadDepartamenteMedicale()
    {
        try
        {
            var departamente = await DepartamentMedicalService.GetAllDepartamenteMedicaleAsync();
            _state.SetDepartmentOptions(departamente.ToList());
            _models.InitializeFilterOptions(departamente.ToList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error loading departamente - using fallback");
            
            var fallbackDepartments = new List<DepartamentMedical>
            {
                new() { DepartamentID = Guid.NewGuid(), Nume = "Cardiologie" },
                new() { DepartamentID = Guid.NewGuid(), Nume = "Pneumologie" },
                new() { DepartamentID = Guid.NewGuid(), Nume = "Neurologie" },
                new() { DepartamentID = Guid.NewGuid(), Nume = "Oftalmologie" },
                new() { DepartamentID = Guid.NewGuid(), Nume = "Dermatologie" },
            };
            
            _state.SetDepartmentOptions(fallbackDepartments);
            _models.InitializeFilterOptions(fallbackDepartments);
            
            await ShowToast("Avertisment", "Date demo pentru departamente", "e-toast-warning");
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
            
            await ShowToast("Succes", "Date actualizate", "e-toast-success");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error refreshing data");
            await ShowToast("Eroare", "Eroare la actualizare", "e-toast-danger");
        }
    }

    #endregion

    #region Grid Management

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
            Logger.LogWarning(ex, "⚠️ Error capturing grid settings");
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
            Logger.LogWarning(ex, "⚠️ Error applying grid settings");
        }
    }

    public void RowSelected(RowSelectEventArgs<PersonalMedicalModel> args) 
    { 
        Logger.LogDebug("Row selected: {PersonalName}", args.Data?.NumeComplet);
    }
    
    public void RowDeselected(RowDeselectEventArgs<PersonalMedicalModel> args) 
    { 
        Logger.LogDebug("Row deselected: {PersonalName}", args.Data?.NumeComplet);
    }

    #endregion

    #region Personal Medical Actions

    private async Task ShowAddPersonalMedicalModal()
    {
        try
        {
            Logger.LogInformation("🚀 Opening add PersonalMedical modal");
            
            _state.IsEditMode = false;
            _state.EditingPersonalMedical = _models.CreateNewPersonalMedical();
            _state.IsAddEditModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error showing add modal");
            await ShowToast("Eroare", "Eroare la deschiderea formularului", "e-toast-danger");
        }
    }

    private async Task ShowPersonalMedicalDetailModal(PersonalMedicalModel personalMedical)
    {
        try
        {
            _state.SelectedPersonalMedical = personalMedical;
            _state.IsModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error showing detail modal");
            await ShowToast("Eroare", "Eroare la afișarea detaliilor", "e-toast-danger");
        }
    }

    private async Task ClosePersonalMedicalDetailModal()
    {
        _state.IsModalVisible = false;
        _state.SelectedPersonalMedical = null;
        StateHasChanged();
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

    private async Task ShowEditPersonalMedicalModal(PersonalMedicalModel personalMedical)
    {
        try
        {
            _state.IsEditMode = true;
            _state.EditingPersonalMedical = _models.ClonePersonalMedical(personalMedical);
            _state.SelectedPersonalMedicalForEdit = personalMedical;
            _state.IsAddEditModalVisible = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error showing edit modal");
            await ShowToast("Eroare", "Eroare la editare", "e-toast-danger");
        }
    }

    private async Task CloseAddEditModal()
    {
        _state.IsAddEditModalVisible = false;
        _state.EditingPersonalMedical = null;
        _state.IsEditMode = false;
        StateHasChanged();
    }

    private async Task HandleFormSubmit()
    {
        await ShowToast("Info", "Form component not yet implemented", "e-toast-info");
    }

    private async Task EditPersonalMedical(PersonalMedicalModel personalMedical)
    {
        await ShowEditPersonalMedicalModal(personalMedical);
    }

    private async Task DeletePersonalMedical(PersonalMedicalModel personalMedical)
    {
        try
        {
            var confirmDelete = await JSRuntime.InvokeAsync<bool>("confirm",
                $"Sigur doriti sa stergeti personalul medical {personalMedical.NumeComplet}?");

            if (confirmDelete)
            {
                await ShowToast("Ștergere", $"Personal {personalMedical.NumeComplet} va fi șters", "e-toast-info");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error deleting");
            await ShowToast("Eroare", "Eroare la ștergere", "e-toast-danger");
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

    #region Helper Methods

    private string GetPozitieIcon(PozitiePersonalMedical pozitie)
    {
        return pozitie.GetFontAwesomeIcon();
    }

    private string GetPozitieDisplayName(PozitiePersonalMedical? pozitie)
    {
        if (!pozitie.HasValue) return "Nespecificat";
        return pozitie.Value.GetDisplayName();
    }

    private List<PersonalMedicalModel> CreateDemoPersonalMedical()
    {
        return new List<PersonalMedicalModel>
        {
            new()
            {
                PersonalID = Guid.NewGuid(),
                Nume = "Popescu",
                Prenume = "Dr. Maria",
                Email = "maria.popescu@valyanmed.ro",
                Telefon = "0744123456",
                Pozitie = PozitiePersonalMedical.Doctor,
                Specializare = "Cardiologie",
                NumarLicenta = "MD12345",
                Departament = "Cardiologie",
                EsteActiv = true,
                DataCreare = DateTime.Now.AddMonths(-6),
                CategorieName = "Medicina Interna",
                SpecializareName = "Cardiologie"
            },
            new()
            {
                PersonalID = Guid.NewGuid(),
                Nume = "Ionescu",
                Prenume = "Ana",
                Email = "ana.ionescu@valyanmed.ro",
                Telefon = "0744123457",
                Pozitie = PozitiePersonalMedical.AsistentMedical,
                Specializare = "Cardiologie",
                Departament = "Cardiologie",
                EsteActiv = true,
                DataCreare = DateTime.Now.AddMonths(-3),
                CategorieName = "Asistenta Medicala",
                SpecializareName = "Cardiologie"
            },
            new()
            {
                PersonalID = Guid.NewGuid(),
                Nume = "Vasile",
                Prenume = "Dr. Alexandru",
                Email = "alex.vasile@valyanmed.ro",
                Telefon = "0744123458",
                Pozitie = PozitiePersonalMedical.Doctor,
                Specializare = "Pneumologie",
                NumarLicenta = "MD12346",
                Departament = "Pneumologie",
                EsteActiv = true,
                DataCreare = DateTime.Now.AddMonths(-12),
                CategorieName = "Medicina Interna",
                SpecializareName = "Pneumologie"
            }
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
                Logger.LogWarning(ex, "⚠️ Error showing toast");
            }
        }
    }

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
                await ShowToast(title, content, cssClass);
            }
        }
        else
        {
            await ShowToast(title, content, cssClass);
        }
    }

    private async Task HandleModalToast((string Title, string Message, string CssClass) args)
    {
        await ShowModalToast(args.Title, args.Message, args.CssClass);
    }

    #endregion

    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            _disposed = true;

            if (GridRef != null && GridStateService != null)
            {
                try
                {
                    var currentSettings = CaptureGridSettings();
                    await GridStateService.SaveGridSettingsAsync(GRID_ID, currentSettings);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "⚠️ Error saving grid settings on disposal");
                }
            }

            try
            {
                GridRef?.Dispose();
                PersonalMedicalDetailModal?.Dispose();  
                AddEditPersonalMedicalModal?.Dispose();
                ToastRef?.Dispose();
                ModalToastRef?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "⚠️ Error disposing components");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Critical error during disposal");
        }

        GC.SuppressFinalize(this);
    }

    #endregion
}
