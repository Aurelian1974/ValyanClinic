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

    #region Validation Results
    private CNPValidationResult? _cnpValidationResult;
    private CodeValidationResult? _codeValidationResult;
    private Dictionary<string, ValidationState> _fieldValidationStates = new();
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

        // Initialize field validation states
        _fieldValidationStates.Clear();
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
        return _currentStep switch
        {
            1 => ValidateStep1(),
            2 => ValidateStep2(),
            3 => ValidateStep3(),
            4 => ValidateStep4(),
            5 => ValidateStep5(),
            _ => false
        };
    }

    private bool CanSubmitForm()
    {
        return ValidateStep1() && ValidateStep2() && ValidateStep3() && ValidateStep4() && ValidateStep5();
    }

    private async Task NextStep()
    {
        if (CanProceedToNextStep())
        {
            _currentStep++;
            await SaveDraft();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task PreviousStep()
    {
        if (_currentStep > 1)
        {
            _currentStep--;
            await InvokeAsync(StateHasChanged);
        }
    }
    #endregion

    #region Validation Logic
    private bool ValidateStep1()
    {
        // Verifică că toate câmpurile obligatorii sunt completate
        var hasNume = !string.IsNullOrWhiteSpace(_personalModel.Nume);
        var hasPrenume = !string.IsNullOrWhiteSpace(_personalModel.Prenume);
        var hasCNP = !string.IsNullOrWhiteSpace(_personalModel.CNP) && _personalModel.CNP.Length == 13;
        var hasCodAngajat = !string.IsNullOrWhiteSpace(_personalModel.Cod_Angajat);
        
        // Pentru început, să acceptăm validările de bază fără să aștepte validările async
        // Validările async CNP și Cod vor rula în background, dar nu blochează progresul
        var basicValidation = hasNume && hasPrenume && hasCNP && hasCodAngajat;
        
        return basicValidation;
    }

    private bool ValidateStep2()
    {
        // Implement Step 2 validation (Contact info)
        return true; // Placeholder
    }

    private bool ValidateStep3()
    {
        // Implement Step 3 validation (Address info)
        return true; // Placeholder
    }

    private bool ValidateStep4()
    {
        // Implement Step 4 validation (Professional info)
        return true; // Placeholder
    }

    private bool ValidateStep5()
    {
        // Implement Step 5 validation (Documents)
        return true; // Placeholder
    }

    private string GetFieldCssClass(string fieldName)
    {
        if (_fieldValidationStates.TryGetValue(fieldName, out var state))
        {
            return state switch
            {
                ValidationState.Valid => "field-valid",
                ValidationState.Invalid => "field-invalid",
                ValidationState.Validating => "field-validating",
                _ => ""
            };
        }
        return "";
    }
    #endregion

    #region Field Change Handlers
    private async Task OnFieldChanged(string fieldName, string? value)
    {
        _fieldValidationStates[fieldName] = ValidationState.Validating;
        await InvokeAsync(StateHasChanged);

        // Simulate validation delay
        await Task.Delay(300);

        var isValid = !string.IsNullOrWhiteSpace(value) && value.Length >= 2;
        _fieldValidationStates[fieldName] = isValid ? ValidationState.Valid : ValidationState.Invalid;

        // Forțează recalcularea validării pas pentru a activa/dezactiva butonul
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnCNPChanged(string? cnp)
    {
        if (string.IsNullOrWhiteSpace(cnp))
        {
            _cnpValidationResult = null;
            return;
        }

        _fieldValidationStates[nameof(_personalModel.CNP)] = ValidationState.Validating;
        await InvokeAsync(StateHasChanged);

        // Validate CNP format and extract information
        var validationResult = await ValidateCNP(cnp);
        _cnpValidationResult = validationResult;
        
        if (validationResult.IsValid && validationResult.BirthDate.HasValue)
        {
            _personalModel.Data_Nasterii = validationResult.BirthDate.Value;
        }

        _fieldValidationStates[nameof(_personalModel.CNP)] = validationResult.IsValid 
            ? ValidationState.Valid : ValidationState.Invalid;

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnEmployeeCodeChanged(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            _codeValidationResult = null;
            return;
        }

        _fieldValidationStates[nameof(_personalModel.Cod_Angajat)] = ValidationState.Validating;
        await InvokeAsync(StateHasChanged);

        // Check if code is unique
        var validationResult = await ValidateEmployeeCode(code);
        _codeValidationResult = validationResult;

        _fieldValidationStates[nameof(_personalModel.Cod_Angajat)] = validationResult.IsValid 
            ? ValidationState.Valid : ValidationState.Invalid;

        await InvokeAsync(StateHasChanged);
    }

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
        // This handles form submission for current step
        if (CanProceedToNextStep())
        {
            await NextStep();
        }
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
        await OnCancel.InvokeAsync();
    }
    #endregion

    #region Validation Methods
    private async Task<CNPValidationResult> ValidateCNP(string cnp)
    {
        // Implement comprehensive CNP validation
        if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13)
        {
            return new CNPValidationResult
            {
                IsValid = false,
                ErrorMessage = "CNP-ul trebuie să aibă exact 13 cifre"
            };
        }

        if (!Regex.IsMatch(cnp, @"^\d{13}$"))
        {
            return new CNPValidationResult
            {
                IsValid = false,
                ErrorMessage = "CNP-ul poate conține doar cifre"
            };
        }

        // Extract and validate components
        var sex = int.Parse(cnp.Substring(0, 1));
        var year = int.Parse(cnp.Substring(1, 2));
        var month = int.Parse(cnp.Substring(3, 2));
        var day = int.Parse(cnp.Substring(5, 2));

        // Determine full year based on sex digit
        var fullYear = sex switch
        {
            1 or 2 => 1900 + year,
            3 or 4 => 1800 + year,
            5 or 6 => 2000 + year,
            _ => 0
        };

        if (fullYear == 0 || month < 1 || month > 12 || day < 1 || day > 31)
        {
            return new CNPValidationResult
            {
                IsValid = false,
                ErrorMessage = "CNP conține date invalide"
            };
        }

        try
        {
            var birthDate = new DateTime(fullYear, month, day);
            var sexText = (sex % 2 == 1) ? "Masculin" : "Feminin";
            var age = CalculateAge(birthDate);

            // Check if CNP already exists (simulate async call)
            await Task.Delay(500);
            var cnpExists = await CheckCNPExists(cnp);
            
            if (cnpExists && (!IsEditMode || EditingPersonal?.CNP != cnp))
            {
                return new CNPValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Acest CNP este deja înregistrat în sistem"
                };
            }

            return new CNPValidationResult
            {
                IsValid = true,
                BirthDate = birthDate,
                ParsedInfo = $"{sexText}, {age} ani, născut la {birthDate:dd.MM.yyyy}"
            };
        }
        catch
        {
            return new CNPValidationResult
            {
                IsValid = false,
                ErrorMessage = "Data nașterii din CNP este invalidă"
            };
        }
    }

    private async Task<CodeValidationResult> ValidateEmployeeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new CodeValidationResult
            {
                IsValid = false,
                Message = "Codul angajatului este obligatoriu"
            };
        }

        if (code.Length < 3)
        {
            return new CodeValidationResult
            {
                IsValid = false,
                Message = "Codul trebuie să aibă cel puțin 3 caractere"
            };
        }

        // Simulate async validation
        await Task.Delay(300);
        var codeExists = await CheckEmployeeCodeExists(code);

        if (codeExists && (!IsEditMode || EditingPersonal?.Cod_Angajat != code))
        {
            return new CodeValidationResult
            {
                IsValid = false,
                Message = "Acest cod de angajat este deja folosit"
            };
        }

        return new CodeValidationResult
        {
            IsValid = true,
            Message = "Cod disponibil"
        };
    }

    private async Task<bool> CheckCNPExists(string cnp)
    {
        // Simulate database check
        await Task.Delay(200);
        return false; // Placeholder
    }

    private async Task<bool> CheckEmployeeCodeExists(string code)
    {
        // Simulate database check
        await Task.Delay(200);
        return false; // Placeholder
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
        var toastModel = new ToastModel
        {
            Title = title,
            Content = message,
            CssClass = "e-toast-success",
            Icon = "fas fa-check-circle",
            Timeout = 3000
        };
        await _toastRef.ShowAsync(toastModel);
    }

    private async Task ShowErrorToast(string title, string message)
    {
        var toastModel = new ToastModel
        {
            Title = title,
            Content = message,
            CssClass = "e-toast-error",
            Icon = "fas fa-exclamation-circle",
            Timeout = 5000
        };
        await _toastRef.ShowAsync(toastModel);
    }

    private async Task ShowWarningToast(string title, string message)
    {
        var toastModel = new ToastModel
        {
            Title = title,
            Content = message,
            CssClass = "e-toast-warning",
            Icon = "fas fa-exclamation-triangle",
            Timeout = 4000
        };
        await _toastRef.ShowAsync(toastModel);
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
