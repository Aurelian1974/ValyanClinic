using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;
using ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.ActivateRelatie; // ✅ ADDED
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Services;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

// IDisposable pentru cleanup corect
public partial class PacientAddEditModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<PacientAddEditModal> Logger { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private IFieldPermissionService FieldPermissions { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    [Parameter] public Guid? PacientId { get; set; }

    // Guard flag
    private bool _disposed = false;

    // State
    private bool IsEditMode => PacientId.HasValue && PacientId != Guid.Empty;
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    private string ActiveTab { get; set; } = "personal";

    // Doctori Management
    private List<DoctorAsociatDto> DoctoriAsociati { get; set; } = new();
    private List<DoctorAsociatDto> DoctoriActivi => DoctoriAsociati.Where(d => d.EsteActiv).ToList();
    private List<DoctorAsociatDto> DoctoriInactivi => DoctoriAsociati.Where(d => !d.EsteActiv).ToList();
    private bool IsLoadingDoctori { get; set; }
    private bool ShowAddDoctorModal { get; set; }

    // Confirm modal state
    private bool ShowConfirmRemoveDoctor { get; set; }
    private DoctorAsociatDto? DoctorToRemove { get; set; }
    private bool ShowConfirmAddDoctors { get; set; }

    // ✅ ADDED: Confirm modal state pentru reactivare
    private bool ShowConfirmActivateDoctor { get; set; }
    private DoctorAsociatDto? DoctorToActivate { get; set; }

    // ✅ Permisiuni la nivel de câmp - încărcate din DB
    private bool _permissionsLoaded;
    
    // Form Model
    private PacientFormModel FormModel { get; set; } = new();

    #region Field Permission Helpers
    
    /// <summary>
    /// Verifică dacă un câmp poate fi editat (bazat pe permisiuni din DB).
    /// </summary>
    private bool CanEditField(string fieldName) => 
        FieldPermissions.CanEditField("Pacient", fieldName);
    
    /// <summary>
    /// Verifică dacă un câmp poate fi vizualizat.
    /// </summary>
    private bool CanViewField(string fieldName) => 
        FieldPermissions.CanViewField("Pacient", fieldName);
    
    /// <summary>
    /// Returnează starea unui câmp (Hidden, ReadOnly, Editable).
    /// </summary>
    private FieldState GetFieldState(string fieldName) => 
        FieldPermissions.GetFieldState("Pacient", fieldName, IsEditMode);
    
    /// <summary>
    /// Încarcă permisiunile din DB pentru rolul curent.
    /// </summary>
    private async Task LoadFieldPermissionsAsync()
    {
        if (_permissionsLoaded) return;
        
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var roleClaim = authState.User.FindFirst(ClaimTypes.Role);
            
            if (roleClaim != null)
            {
                await FieldPermissions.LoadPermissionsAsync(roleClaim.Value);
                _permissionsLoaded = true;
                Logger.LogDebug("Field permissions loaded for role: {Role}", roleClaim.Value);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load field permissions");
        }
    }
    
    #endregion

    // Dropdown Options
    private List<string> SexOptions { get; set; } = new() { "M", "F" };

    // Judete list
    private List<string> JudeteList { get; set; } = new()
  {
    "Bucuresti", "Alba", "Arad", "Arges", "Bacau", "Bihor", "Bistrita-Nasaud",
        "Botosani", "Brasov", "Braila", "Buzau", "Caras-Severin", "Calarasi",
        "Cluj", "Constanta", "Covasna", "Dambovita", "Dolj", "Galati", "Giurgiu",
        "Gorj", "Harghita", "Hunedoara", "Ialomita", "Iasi", "Ilfov", "Maramures",
        "Mehedinti", "Mures", "Neamt", "Olt", "Prahova", "Satu Mare", "Salaj",
        "Sibiu", "Suceava", "Teleorman", "Timis", "Tulcea", "Vaslui", "Valcea", "Vrancea"
    };

    // Dispose pentru cleanup corect
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            Logger.LogDebug("[PacientAddEditModal] Disposing - Starting cleanup");

            // Setează flag IMEDIAT
            _disposed = true;

            // Clear toate listele pentru a elibera memoria
            DoctoriAsociati?.Clear();
            DoctoriAsociati = new();
            JudeteList?.Clear();
            SexOptions?.Clear();

            // Reset state flags
            ShowAddDoctorModal = false;
            ShowConfirmRemoveDoctor = false;
            ShowConfirmAddDoctors = false;
            DoctorToRemove = null;

            Logger.LogDebug("[PacientAddEditModal] Dispose complete");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PacientAddEditModal] Error during dispose");
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_disposed) return; // ADDED: Guard check

        if (IsVisible)
        {
            // ✅ Încarcă permisiunile la nivel de câmp când modalul devine vizibil
            await LoadFieldPermissionsAsync();
            
            if (IsEditMode)
            {
                await LoadPacientData();
                await LoadDoctoriAsociati();
            }
            else
            {
                ResetForm();
            }
        }
    }

    private async Task LoadPacientData()
    {
        if (_disposed) return; // ADDED: Guard check

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var query = new GetPacientByIdQuery(PacientId!.Value);
            var result = await Mediator.Send(query);

            if (_disposed) return; // ADDED: Check after async

            if (result.IsSuccess && result.Value != null)
            {
                var pacient = result.Value;
                FormModel = new PacientFormModel
                {
                    Nume = pacient.Nume,
                    Prenume = pacient.Prenume,
                    CNP = pacient.CNP,
                    Cod_Pacient = pacient.Cod_Pacient,
                    Data_Nasterii = pacient.Data_Nasterii,
                    Sex = pacient.Sex,
                    Telefon = pacient.Telefon,
                    Telefon_Secundar = pacient.Telefon_Secundar,
                    Email = pacient.Email,
                    Judet = pacient.Judet,
                    Localitate = pacient.Localitate,
                    Adresa = pacient.Adresa,
                    Cod_Postal = pacient.Cod_Postal,
                    Asigurat = pacient.Asigurat,
                    CNP_Asigurat = pacient.CNP_Asigurat,
                    Nr_Card_Sanatate = pacient.Nr_Card_Sanatate,
                    Casa_Asigurari = pacient.Casa_Asigurari,
                    Alergii = pacient.Alergii,
                    Boli_Cronice = pacient.Boli_Cronice,
                    Medic_Familie = pacient.Medic_Familie,
                    Persoana_Contact = pacient.Persoana_Contact,
                    Telefon_Urgenta = pacient.Telefon_Urgenta,
                    Relatie_Contact = pacient.Relatie_Contact,
                    Activ = pacient.Activ,
                    Observatii = pacient.Observatii
                };
            }
            else
            {
                HasError = true;
                ErrorMessage = result.FirstError ?? "Eroare la încărcarea datelor pacientului.";
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed while loading data");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                HasError = true;
                ErrorMessage = $"Eroare: {ex.Message}";
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoading = false;
            }
        }
    }

    private async Task LoadDoctoriAsociati()
    {
        if (_disposed || !IsEditMode) return; // ADDED: Guard check

        IsLoadingDoctori = true;

        try
        {
            Logger.LogInformation("[PacientAddEditModal] Loading doctori for PacientID: {PacientId}", PacientId);

            var query = new GetDoctoriByPacientQuery(PacientId!.Value, ApenumereActivi: false);
            var result = await Mediator.Send(query);

            if (_disposed) return; // ADDED: Check after async

            if (result.IsSuccess && result.Value != null)
            {
                DoctoriAsociati = result.Value;
                Logger.LogInformation("[PacientAddEditModal] Loaded {Count} doctori", DoctoriAsociati.Count);
            }
            else
            {
                DoctoriAsociati = new List<DoctorAsociatDto>();
                Logger.LogWarning("[PacientAddEditModal] Failed to load doctori: {Error}", result.FirstError);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed while loading doctori");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[PacientAddEditModal] Exception loading doctori");
                DoctoriAsociati = new List<DoctorAsociatDto>();
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoadingDoctori = false;
            }
        }
    }

    private async Task OpenAddDoctorModal()
    {
        if (_disposed) return; // ADDED: Guard check

        if (!IsEditMode)
        {
            await NotificationService.ShowWarningAsync(
         "Vă rugăm să salvați mai întâi pacientul înainte de a adăuga doctori.",
               "Atenție");
            return;
        }

        Logger.LogInformation("[PacientAddEditModal] Opening AddDoctorModal");
        ShowAddDoctorModal = true;
    }

    private async Task OnDoctorAdded()
    {
        if (_disposed) return; // ADDED: Guard check

        Logger.LogInformation("[PacientAddEditModal] Doctor added - reloading list");
        ShowAddDoctorModal = false;
        await LoadDoctoriAsociati();
        StateHasChanged();
    }

    private void RemoveDoctor(DoctorAsociatDto doctor)
    {
        if (_disposed) return; // ADDED: Guard check

        DoctorToRemove = doctor;
        ShowConfirmRemoveDoctor = true;
    }

    // ✅ ADDED: Metodă pentru reactivare relație
    private void ActivateDoctor(DoctorAsociatDto doctor)
    {
        if (_disposed) return;

        DoctorToActivate = doctor;
        ShowConfirmActivateDoctor = true;
    }

    private async Task HandleRemoveDoctorConfirmed()
    {
        if (_disposed || DoctorToRemove == null) return; // ADDED: Guard check

        ShowConfirmRemoveDoctor = false;

        try
        {
            Logger.LogInformation("[PacientAddEditModal] Removing doctor: {DoctorName}, RelatieID: {RelatieID}",
              DoctorToRemove.DoctorNumeComplet, DoctorToRemove.RelatieID);

            var command = new RemoveRelatieCommand(RelatieID: DoctorToRemove.RelatieID);
            var result = await Mediator.Send(command);

            if (_disposed) return; // ADDED: Check after async

            if (result.IsSuccess)
            {
                await LoadDoctoriAsociati();
                await NotificationService.ShowSuccessAsync("Relație dezactivată cu succes!");
            }
            else
            {
                await NotificationService.ShowErrorAsync(result.FirstError ?? "Eroare la dezactivare");
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed during remove operation");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[PacientAddEditModal] Error removing doctor");
                await NotificationService.ShowErrorAsync(ex.Message, "Eroare");
            }
        }
        finally
        {
            if (!_disposed)
            {
                DoctorToRemove = null;
            }
        }
    }

    // ✅ ADDED: Handler pentru confirmare reactivare
    private async Task HandleActivateDoctorConfirmed()
    {
        if (_disposed || DoctorToActivate == null) return;

        ShowConfirmActivateDoctor = false;

        try
        {
            Logger.LogInformation("[PacientAddEditModal] Activating doctor: {DoctorName}, RelatieID: {RelatieID}",
    DoctorToActivate.DoctorNumeComplet, DoctorToActivate.RelatieID);

            var command = new ActivateRelatieCommand(
     RelatieID: DoctorToActivate.RelatieID,
       Observatii: "Relație reactivată din interfața pacient",
           Motiv: "Reluarea tratamentului cu acest doctor",
      ModificatDe: "System");

            var result = await Mediator.Send(command);

            if (_disposed) return;

            if (result.IsSuccess)
            {
                await LoadDoctoriAsociati();
                await NotificationService.ShowSuccessAsync(
        result.SuccessMessage ?? "Relație reactivată cu succes!");
            }
            else
            {
                await NotificationService.ShowErrorAsync(
                 result.FirstError ?? "Eroare la reactivare");
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed during activate operation");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[PacientAddEditModal] Error activating doctor");
                await NotificationService.ShowErrorAsync(ex.Message, "Eroare");
            }
        }
        finally
        {
            if (!_disposed)
            {
                DoctorToActivate = null;
            }
        }
    }

    private string GetBadgeClass(string? tipRelatie)
    {
        return tipRelatie switch
        {
            "MedicPrimar" => "badge-primary",
            "Specialist" => "badge-info",
            "MedicConsultant" => "badge-success",
            "MedicDeGarda" => "badge-warning",
            "MedicFamilie" => "badge-secondary",
            _ => "badge-secondary"
        };
    }

    private string FormatZile(int zile)
    {
        if (zile < 30)
            return $"{zile} zile";
        if (zile < 365)
            return $"{zile / 30} luni";
        return $"{zile / 365} ani";
    }

    private void ResetForm()
    {
        if (_disposed) return; // ADDED: Guard check

        FormModel = new PacientFormModel
        {
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Activ = true,
            Asigurat = false
        };
        ActiveTab = "personal";
        HasError = false;
        ErrorMessage = null;
        DoctoriAsociati = new();
        ShowAddDoctorModal = false;
    }

    private void SetActiveTab(string tab)
    {
        if (_disposed) return; // ADDED: Guard check

        ActiveTab = tab;
    }

    private async Task HandleSubmit()
    {
        if (_disposed) return; // ADDED: Guard check

        IsSaving = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            if (IsEditMode)
            {
                await UpdatePacient();
            }
            else
            {
                await CreatePacient();
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsSaving = false;
            }
        }
    }

    private async Task CreatePacient()
    {
        if (_disposed) return; // ADDED: Guard check

        var command = new CreatePacientCommand
        {
            Nume = FormModel.Nume,
            Prenume = FormModel.Prenume,
            CNP = FormModel.CNP,
            Cod_Pacient = FormModel.Cod_Pacient,
            Data_Nasterii = FormModel.Data_Nasterii,
            Sex = FormModel.Sex,
            Telefon = FormModel.Telefon,
            Telefon_Secundar = FormModel.Telefon_Secundar,
            Email = FormModel.Email,
            Judet = FormModel.Judet,
            Localitate = FormModel.Localitate,
            Adresa = FormModel.Adresa,
            Cod_Postal = FormModel.Cod_Postal,
            Asigurat = FormModel.Asigurat,
            CNP_Asigurat = FormModel.CNP_Asigurat,
            Nr_Card_Sanatate = FormModel.Nr_Card_Sanatate,
            Casa_Asigurari = FormModel.Casa_Asigurari,
            Alergii = FormModel.Alergii,
            Boli_Cronice = FormModel.Boli_Cronice,
            Medic_Familie = FormModel.Medic_Familie,
            Persoana_Contact = FormModel.Persoana_Contact,
            Telefon_Urgenta = FormModel.Telefon_Urgenta,
            Relatie_Contact = FormModel.Relatie_Contact,
            Activ = FormModel.Activ,
            Observatii = FormModel.Observatii,
            CreatDe = "System"
        };

        var result = await Mediator.Send(command);

        if (_disposed) return; // ADDED: Check after async

        if (result.IsSuccess)
        {
            Logger.LogInformation("[PacientAddEditModal] Pacient created successfully with ID: {PacientId}", result.Value);

            PacientId = result.Value;
            ShowConfirmAddDoctors = true;
        }
        else
        {
            HasError = true;
            ErrorMessage = string.Join("\n", result.Errors);
            await NotificationService.ShowErrorAsync(ErrorMessage, "Eroare la salvare");
        }
    }

    private async Task HandleAddDoctorsConfirmed()
    {
        if (_disposed) return; // ADDED: Guard check

        ShowConfirmAddDoctors = false;

        await LoadPacientData();
        await LoadDoctoriAsociati();

        ActiveTab = "doctori";
        StateHasChanged();

        await Task.Delay(300);
        OpenAddDoctorModal();
    }

    private async Task HandleAddDoctorsDeclined()
    {
        if (_disposed) return; // ADDED: Guard check

        ShowConfirmAddDoctors = false;

        await NotificationService.ShowSuccessAsync("Pacient creat cu succes!");
        await Close();
        await OnSaved.InvokeAsync();
    }

    private async Task UpdatePacient()
    {
        if (_disposed) return; // ADDED: Guard check

        var command = new UpdatePacientCommand
        {
            Id = PacientId!.Value,
            Nume = FormModel.Nume,
            Prenume = FormModel.Prenume,
            CNP = FormModel.CNP,
            Data_Nasterii = FormModel.Data_Nasterii,
            Sex = FormModel.Sex,
            Telefon = FormModel.Telefon,
            Telefon_Secundar = FormModel.Telefon_Secundar,
            Email = FormModel.Email,
            Judet = FormModel.Judet,
            Localitate = FormModel.Localitate,
            Adresa = FormModel.Adresa,
            Cod_Postal = FormModel.Cod_Postal,
            Asigurat = FormModel.Asigurat,
            CNP_Asigurat = FormModel.CNP_Asigurat,
            Nr_Card_Sanatate = FormModel.Nr_Card_Sanatate,
            Casa_Asigurari = FormModel.Casa_Asigurari,
            Alergii = FormModel.Alergii,
            Boli_Cronice = FormModel.Boli_Cronice,
            Medic_Familie = FormModel.Medic_Familie,
            Persoana_Contact = FormModel.Persoana_Contact,
            Telefon_Urgenta = FormModel.Telefon_Urgenta,
            Relatie_Contact = FormModel.Relatie_Contact,
            Activ = FormModel.Activ,
            Observatii = FormModel.Observatii,
            ModificatDe = "System"
        };

        var result = await Mediator.Send(command);

        if (_disposed) return; // ADDED: Check after async

        if (result.IsSuccess)
        {
            await NotificationService.ShowSuccessAsync(
          result.SuccessMessage ?? "Pacient actualizat cu succes!");
            await Close();
            await OnSaved.InvokeAsync();
        }
        else
        {
            HasError = true;
            ErrorMessage = string.Join("\n", result.Errors);
            await NotificationService.ShowErrorAsync(ErrorMessage, "Eroare la salvare");
        }
    }

    private async Task HandleOverlayClick()
    {
        if (_disposed) return; // ADDED: Guard check

        // Pentru a proteja datele introduse, modalul nu se închide la click pe overlay
        return;
    }

    private async Task Close()
    {
        if (_disposed) return; // ADDED: Guard check

        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        ResetForm();
    }

    // Form Model Class
    public class PacientFormModel
    {
        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele nu poate depăși 100 de caractere")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Prenumele nu poate depăși 100 de caractere")]
        public string Prenume { get; set; } = string.Empty;

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie să conțină exact 13 cifre")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP-ul trebuie să conțină doar cifre")]
        public string? CNP { get; set; }

        public string? Cod_Pacient { get; set; }

        [Required(ErrorMessage = "Data nașterii este obligatorie")]
        public DateTime Data_Nasterii { get; set; }

        [Required(ErrorMessage = "Sexul este obligatoriu")]
        public string Sex { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon { get; set; }

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon_Secundar { get; set; }

        [EmailAddress(ErrorMessage = "Format email invalid")]
        public string? Email { get; set; }

        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Adresa { get; set; }

        [StringLength(6, ErrorMessage = "Codul poștal nu poate depăși 6 caractere")]
        public string? Cod_Postal { get; set; }

        public bool Asigurat { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP Asigurat trebuie să conțină exact 13 cifre")]
        public string? CNP_Asigurat { get; set; }

        public string? Nr_Card_Sanatate { get; set; }
        public string? Casa_Asigurari { get; set; }
        public string? Alergii { get; set; }
        public string? Boli_Cronice { get; set; }
        public string? Medic_Familie { get; set; }
        public string? Persoana_Contact { get; set; }

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon_Urgenta { get; set; }

        public string? Relatie_Contact { get; set; }
        public bool Activ { get; set; }
        public string? Observatii { get; set; }
    }
}
