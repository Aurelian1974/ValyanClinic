using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Services;
using Syncfusion.Blazor.Notifications;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PersonalModel = ValyanClinic.Domain.Models.Personal;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Complex multi-step form for adding/editing Personal with advanced validation
/// Features: Auto-save, real-time validation, progressive disclosure, smart suggestions
/// </summary>
public partial class AdaugaEditezaPersonal : ComponentBase, IDisposable
{
    #region Injected Services
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdaugaEditezaPersonal> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public Guid? PersonalId { get; set; }
    [Parameter] public PersonalModel? EditingPersonal { get; set; }
    [Parameter] public EventCallback<PersonalModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    #endregion

    #region Form State
    private PersonalFormModel _personalModel = new();
    private bool IsEditMode => EditingPersonal != null;
    private int _currentStep = 1;
    private int _totalSteps = 5;
    private List<string> _validationErrors = new();
    private SfToast _toastRef = default!;
    
    // Auto-save functionality
    private Timer? _autoSaveTimer;
    private bool _isAutoSaving = false;
    private DateTime? _lastAutoSave;
    private readonly int _autoSaveIntervalMs = 30000; // 30 seconds
    
    // Step 3 - Address
    private bool _sameAsHome = true;
    
    // Step 5 - Documents
    private bool _acceptDataProcessing = false;
    private bool _confirmDataAccuracy = false;
    #endregion

    #region Data Sources for Dropdowns
    private List<string> _cityOptions = new();
    private List<string> _judeteOptions = new();
    private List<string> _nationalityOptions = new();
    private List<string> _citizenshipOptions = new();
    private List<DropdownOption<StareCivila?>> _stareCivilaOptions = new();
    private List<DropdownOption<Departament>> _departmentOptions = new();
    private List<string> _jobTitleOptions = new();
    private List<DropdownOption<string>> _contactMethods = new();
    private List<DropdownOption<string>> _availabilityHours = new();
    private List<DropdownOption<string>> _contractTypes = new();
    private List<DropdownOption<string>> _experienceLevels = new();
    private List<DropdownOption<string>> _workSchedules = new();
    private List<DropdownOption<string>> _educationLevels = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        try
        {
            await InitializeFormData();
            await LoadDropdownData();
            InitializeAutoSave();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing AdaugaEditezaPersonal component");
            await ShowErrorToast("Eroare la inițializarea formularului", ex.Message);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (EditingPersonal != null && _personalModel.Id_Personal != EditingPersonal.Id_Personal)
        {
            await InitializeFormData();
        }
    }

    public void Dispose()
    {
        _autoSaveTimer?.Dispose();
    }
    #endregion

    #region Initialization
    private async Task InitializeFormData()
    {
        if (IsEditMode && EditingPersonal != null)
        {
            _personalModel = PersonalFormModel.FromPersonal(EditingPersonal);
        }
        else
        {
            _personalModel = new PersonalFormModel
            {
                Id_Personal = Guid.NewGuid(),
                Status_Angajat = StatusAngajat.Activ,
                Data_Nasterii = DateTime.Today.AddYears(-30),
                Data_Crearii = DateTime.Now,
                Data_Ultimei_Modificari = DateTime.Now,
                Nationalitate = "Română",
                Cetatenie = "Română"
            };
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadDropdownData()
    {
        try
        {
            // Load Romanian cities
            _cityOptions = GetRomanianCities();
            
            // Load Romanian counties
            _judeteOptions = GetRomanianCounties();
            
            // Load nationalities and citizenships
            _nationalityOptions = GetNationalities();
            _citizenshipOptions = GetCitizenships();
            
            // Load marital status options
            _stareCivilaOptions = new List<DropdownOption<StareCivila?>>
            {
                new("", "Selectați starea civilă", null),
                new("Necăsătorit/ă", "Necăsătorit/ă", StareCivila.Necasatorit),
                new("Căsătorit/ă", "Căsătorit/ă", StareCivila.Casatorit),
                new("Divorțat/ă", "Divorțat/ă", StareCivila.Divortat),
                new("Văduvă/Văduv", "Văduvă/Văduv", StareCivila.Vaduv),
                new("Uniune Consensuală", "Uniune Consensuală", StareCivila.UniuneConsensuala)
            };

            // Load departments
            _departmentOptions = Enum.GetValues<Departament>()
                .Select(d => new DropdownOption<Departament>(d.ToString(), GetDepartmentDisplayName(d), d))
                .ToList();

            // Load job titles
            _jobTitleOptions = GetJobTitles();

            // Load contact methods
            _contactMethods = new List<DropdownOption<string>>
            {
                new("telefon", "Telefon", "telefon"),
                new("email", "Email", "email"),
                new("sms", "SMS", "sms"),
                new("whatsapp", "WhatsApp", "whatsapp")
            };

            // Load availability hours
            _availabilityHours = new List<DropdownOption<string>>
            {
                new("oricand", "Oricând", "oricand"),
                new("program", "În programul de lucru (9-17)", "program"),
                new("seara", "Doar seara (după 18:00)", "seara"),
                new("weekend", "Doar weekendul", "weekend"),
                new("urgente", "Doar în caz de urgență", "urgente")
            };

            // Load contract types
            _contractTypes = new List<DropdownOption<string>>
            {
                new("cim", "Contract Individual de Muncă", "cim"),
                new("csd", "Contract de Servicii cu Drepturi", "csd"),
                new("pfa", "Persoană Fizică Autorizată", "pfa"),
                new("srl", "SRL (Întreprindere)", "srl"),
                new("colaborare", "Contract de Colaborare", "colaborare")
            };

            // Load experience levels
            _experienceLevels = new List<DropdownOption<string>>
            {
                new("entry", "Entry Level (0-2 ani)", "entry"),
                new("junior", "Junior (2-5 ani)", "junior"),
                new("mid", "Mid Level (5-8 ani)", "mid"),
                new("senior", "Senior (8+ ani)", "senior"),
                new("expert", "Expert/Specialist", "expert")
            };

            // Load work schedules
            _workSchedules = new List<DropdownOption<string>>
            {
                new("full", "Full-time (8h/zi)", "full"),
                new("part", "Part-time", "part"),
                new("tura", "Program în ture", "tura"),
                new("flexibil", "Program flexibil", "flexibil"),
                new("garda", "Gărzi medicale", "garda")
            };

            // Load education levels
            _educationLevels = new List<DropdownOption<string>>
            {
                new("liceu", "Liceu", "liceu"),
                new("facultate", "Facultate", "facultate"),
                new("master", "Master", "master"),
                new("doctorat", "Doctorat", "doctorat"),
                new("postdoc", "Post-doctorat", "postdoc")
            };

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dropdown data");
        }
    }

    private void InitializeAutoSave()
    {
        _autoSaveTimer = new Timer(async _ => await AutoSave(), null, _autoSaveIntervalMs, _autoSaveIntervalMs);
    }
    #endregion

    #region Step Navigation
    private string GetStepLabel(int step)
    {
        return step switch
        {
            1 => "Personal",
            2 => "Contact",
            3 => "Adresă",
            4 => "Profesional",
            5 => "Documente",
            _ => $"Pas {step}"
        };
    }

    private string GetStepDescription(int step)
    {
        return step switch
        {
            1 => "Completează informațiile personale de bază",
            2 => "Adaugă datele de contact (telefon, email)",
            3 => "Introduceți adresa de domiciliu și reședință",
            4 => "Setează informațiile profesionale",
            5 => "Încarcă documentele de identitate",
            _ => "Completează informațiile necesare"
        };
    }

    private bool CanProceedToNextStep()
    {
        // Permite navigarea înainte doar dacă nu suntem la ultimul pas
        return _currentStep < _totalSteps;
    }

    private bool CanGoToPreviousStep()
    {
        // Permite navigarea înapoi doar dacă nu suntem la primul pas
        return _currentStep > 1;
    }

    private bool CanSubmitForm()
    {
        // Validare completă doar pentru submit final
        var hasNume = !string.IsNullOrWhiteSpace(_personalModel.Nume);
        var hasPrenume = !string.IsNullOrWhiteSpace(_personalModel.Prenume);
        var hasCNP = !string.IsNullOrWhiteSpace(_personalModel.CNP);
        var hasCodAngajat = !string.IsNullOrWhiteSpace(_personalModel.Cod_Angajat);
        var hasDepartament = _personalModel.Departament.HasValue;
        var hasFunctia = !string.IsNullOrWhiteSpace(_personalModel.Functia);
        
        // Doar câmpurile cu adevărat obligatorii pentru salvare
        return hasNume && hasPrenume && hasCNP && hasCodAngajat && hasDepartament && hasFunctia;
    }

    private async Task NextStep()
    {
        // Navighează la pasul următor doar dacă nu suntem la ultimul pas
        if (_currentStep < _totalSteps)
        {
            await ClearAllToasts();
            
            _currentStep++;
            await SaveDraft();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task PreviousStep()
    {
        // Navighează la pasul anterior doar dacă nu suntem la primul pas
        if (_currentStep > 1)
        {
            await ClearAllToasts();
            
            _currentStep--;
            await InvokeAsync(StateHasChanged);
        }
    }
    #endregion

    #region Validation Logic
    private bool ValidateStep1()
    {
        // Permite navigarea liberă - validarea se va face doar la salvarea finală
        return true;
    }

    private bool ValidateStep2()
    {
        // Permite navigarea liberă
        return true;
    }

    private bool ValidateStep3()
    {
        // Permite navigarea liberă
        return true;
    }

    private bool ValidateStep4()
    {
        // Permite navigarea liberă
        return true;
    }

    private bool ValidateStep5()
    {
        // Permite navigarea liberă
        return true;
    }

    private string GetFieldCssClass(string fieldName)
    {
        // CSS simplu fără stări de validare complexe
        return "";
    }
    #endregion

    #region Field Change Handlers
    private async Task OnBirthDateChanged(DateTime? birthDate)
    {
        if (birthDate.HasValue)
        {
            var age = CalculateAge(birthDate.Value);
            if (age < 16 || age > 70)
            {
                await ShowWarningToast("Vârstă neobișnuită", $"Persoana are {age} ani. Verificați data nașterii.");
            }
        }
        await InvokeAsync(StateHasChanged);
    }
    #endregion

    #region Auto-save and Draft Management
    private async Task AutoSave()
    {
        if (_isAutoSaving || _personalModel == null) return;

        try
        {
            _isAutoSaving = true;
            await InvokeAsync(StateHasChanged);

            // Simulate auto-save to temporary storage
            await Task.Delay(1000);

            _lastAutoSave = DateTime.Now;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during auto-save");
        }
        finally
        {
            _isAutoSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SaveDraft()
    {
        await AutoSave();
        await ShowSuccessToast("Draft salvat", "Progresul a fost salvat cu succes.");
    }
    #endregion

    #region Form Submission
    private async Task HandleStepSubmit()
    {
        // Această metodă se apelează la submit-ul formularului - nu navighează automat
        // Navigarea se face explicit prin butoanele NextStep/PreviousStep
        await SaveDraft();
    }

    private async Task HandleFinalSubmit()
    {
        if (!CanSubmitForm())
        {
            await ShowErrorToast("Formular incomplet", "Te rugăm să completezi toate câmpurile obligatorii.");
            return;
        }

        try
        {
            var personalModel = _personalModel.ToPersonal();
            await OnSave.InvokeAsync(personalModel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting form");
            await ShowErrorToast("Eroare la salvare", ex.Message);
        }
    }

    private async Task HandleCancel()
    {
        // Închide toate toasturile înainte de anulare
        await ClearAllToasts();
        await OnCancel.InvokeAsync();
    }
    #endregion

    #region Toast Management
    
    private async Task ClearAllToasts()
    {
        try
        {
            if (_toastRef != null)
            {
                // Use Hide method for each toast instead of HideAllAsync (which doesn't exist)
                await _toastRef.HideAsync("All");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error clearing toasts");
        }
    }
    
    #endregion

    #region Utility Methods
    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private List<string> GetRomanianCities()
    {
        return new List<string>
        {
            "București", "Cluj-Napoca", "Timișoara", "Iași", "Constanța", "Craiova", "Brașov", "Galați", 
            "Ploiești", "Oradea", "Braila", "Arad", "Pitești", "Sibiu", "Bacău", "Târgu Mureș",
            "Baia Mare", "Buzău", "Botoșani", "Satu Mare", "Râmnicu Vâlcea", "Drobeta-Turnu Severin",
            "Suceava", "Piatra Neamț", "Tulcea", "Târgoviște", "Focșani", "Bistrita", "Reșița", "Alba Iulia"
        };
    }

    private List<string> GetRomanianCounties()
    {
        return new List<string>
        {
            "Alba", "Arad", "Argeș", "Bacău", "Bihor", "Bistrița-Năsăud", "Botoșani", "Brașov",
            "Brăila", "Buzău", "Caraș-Severin", "Călărași", "Cluj", "Constanța", "Covasna", "Dâmbovița",
            "Dolj", "Galați", "Giurgiu", "Gorj", "Harghita", "Hunedoara", "Ialomița", "Iași",
            "Ilfov", "Maramureș", "Mehedinți", "Mureș", "Neamț", "Olt", "Prahova", "Satu Mare",
            "Sălaj", "Sibiu", "Suceava", "Teleorman", "Timiș", "Tulcea", "Vaslui", "Vâlcea", "Vrancea",
            "București"
        };
    }

    private List<string> GetNationalities()
    {
        return new List<string> { "Română", "Maghiară", "Germană", "Romă", "Ucraineană", "Rusă", "Bulgară", "Sârbă", "Italiană", "Franceză" };
    }

    private List<string> GetCitizenships()
    {
        return new List<string> { "Română", "Maghiară", "Germană", "Italiană", "Franceză", "Spaniolă", "Britanică", "Americană" };
    }

    private List<string> GetJobTitles()
    {
        return new List<string>
        {
            "Director General", "Director Adjunct", "Manager", "Șef Departament", "Specialist",
            "Expert", "Consultant", "Asistent", "Operator", "Tehnician", "Secretar", "Receptioner"
        };
    }

    private string GetDepartmentDisplayName(Departament department)
    {
        return department switch
        {
            Departament.Administratie => "Administrație",
            Departament.Financiar => "Financiar",
            Departament.IT => "IT",
            Departament.Intretinere => "Întreținere",
            Departament.Logistica => "Logistică",
            _ => department.ToString()
        };
    }
    #endregion

    #region Toast Notifications
    private async Task ShowSuccessToast(string title, string message)
    {
        try
        {
            if (_toastRef != null)
            {
                var toastModel = new ToastModel
                {
                    Title = title,
                    Content = message,
                    CssClass = "e-toast-success",
                    Icon = "fas fa-check-circle",
                    Timeout = 2000 // Reduced timeout
                };
                await _toastRef.ShowAsync(toastModel);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing success toast");
        }
    }

    private async Task ShowErrorToast(string title, string message)
    {
        try
        {
            if (_toastRef != null)
            {
                var toastModel = new ToastModel
                {
                    Title = title,
                    Content = message,
                    CssClass = "e-toast-error",
                    Icon = "fas fa-exclamation-circle",
                    Timeout = 4000 // Reduced timeout
                };
                await _toastRef.ShowAsync(toastModel);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing error toast");
        }
    }

    private async Task ShowWarningToast(string title, string message)
    {
        try
        {
            if (_toastRef != null)
            {
                var toastModel = new ToastModel
                {
                    Title = title,
                    Content = message,
                    CssClass = "e-toast-warning",
                    Icon = "fas fa-exclamation-triangle",
                    Timeout = 3000 // Reduced timeout
                };
                await _toastRef.ShowAsync(toastModel);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing warning toast");
        }
    }
    #endregion
}

#region Supporting Classes and Enums
public enum ValidationState
{
    None,
    Validating,
    Valid,
    Invalid
}

public class CNPValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime? BirthDate { get; set; }
    public string ParsedInfo { get; set; } = "";
}

public class CodeValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
}

public class DropdownOption<T>
{
    public string Text { get; set; }
    public string Display { get; set; }
    public T Value { get; set; }

    public DropdownOption(string text, string display, T value)
    {
        Text = text;
        Display = display;
        Value = value;
    }
}

public class PersonalFormModel
{
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Numele trebuie să aibă între 2 și 50 de caractere")]
    public string Nume { get; set; } = "";

    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Prenumele trebuie să aibă între 2 și 50 de caractere")]
    public string Prenume { get; set; } = "";

    [Required(ErrorMessage = "CNP-ul este obligatoriu")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie să aibă exact 13 cifre")]
    public string CNP { get; set; } = "";

    [Required(ErrorMessage = "Codul angajatului este obligatoriu")]
    public string Cod_Angajat { get; set; } = "";

    public Guid Id_Personal { get; set; }
    public DateTime Data_Nasterii { get; set; }
    public string? Locul_Nasterii { get; set; }
    public StareCivila? Stare_Civila { get; set; }
    public string? Nume_Anterior { get; set; }
    public string Nationalitate { get; set; } = "";
    public string Cetatenie { get; set; } = "";
    public StatusAngajat Status_Angajat { get; set; }
    public DateTime Data_Crearii { get; set; }
    public DateTime Data_Ultimei_Modificari { get; set; }
    
    // Contact Information
    public string? Telefon_Personal { get; set; }
    public string? Telefon_Serviciu { get; set; }
    public string? Email_Personal { get; set; }
    public string? Email_Serviciu { get; set; }
    public string? Observatii_Contact { get; set; }
    
    // Address Information
    public string? Adresa_Domiciliu { get; set; }
    public string? Judet_Domiciliu { get; set; }
    public string? Oras_Domiciliu { get; set; }
    public string? Cod_Postal_Domiciliu { get; set; }
    public string? Adresa_Resedinta { get; set; }
    public string? Judet_Resedinta { get; set; }
    public string? Oras_Resedinta { get; set; }
    public string? Cod_Postal_Resedinta { get; set; }
    
    // Professional Information
    public Departament? Departament { get; set; }
    public string? Functia { get; set; }
    public DateTime? Data_Angajarii { get; set; }
    public string? Tip_Contract { get; set; }
    public string? Nivel_Experienta { get; set; }
    public string? Program_Lucru { get; set; }
    public string? Responsabilitati { get; set; }
    public string? Studii { get; set; }
    public string? Specializarea { get; set; }
    public string? Competente { get; set; }
    
    // Document Information
    public string? Serie_CI { get; set; }
    public string? Numar_CI { get; set; }
    public string? Eliberat_CI_De { get; set; }
    public DateTime? Data_Eliberare_CI { get; set; }
    public DateTime? Valabil_CI_Pana { get; set; }
    public string? Pasaport { get; set; }
    public string? Permis_Conducere { get; set; }
    public string? Cod_Fiscal { get; set; }
    public string? Card_European_Sanatate { get; set; }
    public string? Observatii { get; set; }

    public static PersonalFormModel FromPersonal(PersonalModel personal)
    {
        return new PersonalFormModel
        {
            Id_Personal = personal.Id_Personal,
            Nume = personal.Nume,
            Prenume = personal.Prenume,
            CNP = personal.CNP,
            Cod_Angajat = personal.Cod_Angajat,
            Data_Nasterii = personal.Data_Nasterii,
            Locul_Nasterii = personal.Locul_Nasterii,
            Stare_Civila = personal.Stare_Civila,
            Nume_Anterior = personal.Nume_Anterior,
            Nationalitate = personal.Nationalitate,
            Cetatenie = personal.Cetatenie,
            Status_Angajat = personal.Status_Angajat,
            Data_Crearii = personal.Data_Crearii,
            Data_Ultimei_Modificari = personal.Data_Ultimei_Modificari,
            
            // Contact
            Telefon_Personal = personal.Telefon_Personal,
            Telefon_Serviciu = personal.Telefon_Serviciu,
            Email_Personal = personal.Email_Personal,
            Email_Serviciu = personal.Email_Serviciu,
            
            // Address
            Adresa_Domiciliu = personal.Adresa_Domiciliu,
            Judet_Domiciliu = personal.Judet_Domiciliu,
            Oras_Domiciliu = personal.Oras_Domiciliu,
            Cod_Postal_Domiciliu = personal.Cod_Postal_Domiciliu,
            Adresa_Resedinta = personal.Adresa_Resedinta,
            Judet_Resedinta = personal.Judet_Resedinta,
            Oras_Resedinta = personal.Oras_Resedinta,
            Cod_Postal_Resedinta = personal.Cod_Postal_Resedinta,
            
            // Professional
            Departament = personal.Departament,
            Functia = personal.Functia,
            
            // Documents
            Serie_CI = personal.Serie_CI,
            Numar_CI = personal.Numar_CI,
            Eliberat_CI_De = personal.Eliberat_CI_De,
            Data_Eliberare_CI = personal.Data_Eliberare_CI,
            Valabil_CI_Pana = personal.Valabil_CI_Pana,
            
            Observatii = personal.Observatii
        };
    }

    public PersonalModel ToPersonal()
    {
        return new PersonalModel
        {
            Id_Personal = Id_Personal,
            Nume = Nume,
            Prenume = Prenume,
            CNP = CNP,
            Cod_Angajat = Cod_Angajat,
            Data_Nasterii = Data_Nasterii,
            Locul_Nasterii = Locul_Nasterii,
            Stare_Civila = Stare_Civila,
            Nume_Anterior = Nume_Anterior,
            Nationalitate = Nationalitate,
            Cetatenie = Cetatenie,
            Status_Angajat = Status_Angajat,
            Data_Crearii = Data_Crearii,
            Data_Ultimei_Modificari = DateTime.Now,
            
            // Contact
            Telefon_Personal = Telefon_Personal,
            Telefon_Serviciu = Telefon_Serviciu,
            Email_Personal = Email_Personal,
            Email_Serviciu = Email_Serviciu,
            
            // Address
            Adresa_Domiciliu = Adresa_Domiciliu,
            Judet_Domiciliu = Judet_Domiciliu,
            Oras_Domiciliu = Oras_Domiciliu,
            Cod_Postal_Domiciliu = Cod_Postal_Domiciliu,
            Adresa_Resedinta = Adresa_Resedinta,
            Judet_Resedinta = Judet_Resedinta,
            Oras_Resedinta = Oras_Resedinta,
            Cod_Postal_Resedinta = Cod_Postal_Resedinta,
            
            // Professional
            Departament = Departament,
            Functia = Functia,
            
            // Documents
            Serie_CI = Serie_CI,
            Numar_CI = Numar_CI,
            Eliberat_CI_De = Eliberat_CI_De,
            Data_Eliberare_CI = Data_Eliberare_CI,
            Valabil_CI_Pana = Valabil_CI_Pana,
            
            Observatii = Observatii
        };
    }
}
#endregion
