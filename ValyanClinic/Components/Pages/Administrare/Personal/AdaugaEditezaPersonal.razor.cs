using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Services;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Application.Validators;
using ValyanClinic.Components.Shared.Validation;
using ValyanClinic.Components.Shared;
using ValyanClinic.Core.Services;
using PersonalModel = ValyanClinic.Domain.Models.Personal;
using System.ComponentModel.DataAnnotations;
using Syncfusion.Blazor.Inputs;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdaugaEditezaPersonal : ComponentBase
{
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    [Inject] private ILocationService LocationService { get; set; } = default!;
    [Inject] private IValidationService ValidationService { get; set; } = default!;
    [Inject] private IToastNotificationService ToastService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdaugaEditezaPersonal> Logger { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    [Parameter] public PersonalModel? EditingPersonal { get; set; }
    [Parameter] public EventCallback<PersonalModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private PersonalFormModel personalFormModel = new();
    private List<ValidationError> validationErrors = new();
    
    // Dropdown options pentru enums
    private List<DropdownOption<Departament>> departmentOptions = new();
    private List<DropdownOption<StareCivila>> stareCivilaOptions = new();
    private List<DropdownOption<StatusAngajat>> statusAngajatOptions = new();
    
    // Selected values pentru lookup-uri dependente - folosind componentele
    private int? _selectedJudetDomiciliuId = null;
    private int? _selectedLocalitateDomiciliuId = null;
    private int? _selectedJudetResedintaId = null;
    private int? _selectedLocalitateResedintaId = null;

    // Properties cu logging pentru a urmări schimbările
    private int? selectedJudetDomiciliuId 
    { 
        get => _selectedJudetDomiciliuId;
        set 
        {
            if (_selectedJudetDomiciliuId != value)
            {
                Logger.LogInformation("🔥 Parent selectedJudetDomiciliuId changed: {OldValue} → {NewValue}", 
                    _selectedJudetDomiciliuId, value);
                _selectedJudetDomiciliuId = value;
            }
        }
    }

    private int? selectedLocalitateDomiciliuId 
    { 
        get => _selectedLocalitateDomiciliuId;
        set 
        {
            if (_selectedLocalitateDomiciliuId != value)
            {
                Logger.LogInformation("🔥 Parent selectedLocalitateDomiciliuId changed: {OldValue} → {NewValue}", 
                    _selectedLocalitateDomiciliuId, value);
                _selectedLocalitateDomiciliuId = value;
            }
        }
    }

    private int? selectedJudetResedintaId 
    { 
        get => _selectedJudetResedintaId;
        set 
        {
            if (_selectedJudetResedintaId != value)
            {
                Logger.LogInformation("🔥 Parent selectedJudetResedintaId changed: {OldValue} → {NewValue}", 
                    _selectedJudetResedintaId, value);
                _selectedJudetResedintaId = value;
            }
        }
    }

    private int? selectedLocalitateResedintaId 
    { 
        get => _selectedLocalitateResedintaId;
        set 
        {
            if (_selectedLocalitateResedintaId != value)
            {
                Logger.LogInformation("🔥 Parent selectedLocalitateResedintaId changed: {OldValue} → {NewValue}", 
                    _selectedLocalitateResedintaId, value);
                _selectedLocalitateResedintaId = value;
            }
        }
    }
    
    private bool isSubmitting = false;
    private FluentValidationHelper<PersonalModel>? validationHelper;

    // Checkbox state pentru adresa de resedinta
    private bool showResedintaSection = false;

    private bool IsEditMode => EditingPersonal != null;

    // CNP validation state pentru feedback vizual
    private bool isCNPValid = false;
    private bool isCNPBeingValidated = false;
    private string cnpFeedbackMessage = "";
    private string cnpFeedbackCssClass = "";

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("🚀 AdaugaEditezaPersonal OnInitializedAsync started");
        
        LoadDropdownOptions();
        
        if (IsEditMode && EditingPersonal != null)
        {
            Logger.LogInformation("📝 Edit mode - Personal CNP: {CNP}", EditingPersonal.CNP);
            personalFormModel = PersonalFormModel.FromPersonal(EditingPersonal);
            
            // Verifică dacă există date de reședință pentru a determina starea checkbox-ului
            var hasResedintaData = !string.IsNullOrEmpty(EditingPersonal.Adresa_Resedinta) ||
                                  !string.IsNullOrEmpty(EditingPersonal.Judet_Resedinta) ||
                                  !string.IsNullOrEmpty(EditingPersonal.Oras_Resedinta) ||
                                  !string.IsNullOrEmpty(EditingPersonal.Cod_Postal_Resedinta);
            
            showResedintaSection = !hasResedintaData;
            Logger.LogInformation("📍 Residence section will be {ShowState} (has residence data: {HasData})", 
                showResedintaSection ? "shown" : "hidden", hasResedintaData);
        }
        else
        {
            Logger.LogInformation("➕ Add mode - Creating new personal");
            personalFormModel = new PersonalFormModel
            {
                Id_Personal = Guid.NewGuid(),
                Data_Nasterii = DateTime.Today.AddYears(-30),
                Status_Angajat = StatusAngajat.Activ,
                Nationalitate = "Romana",
                Cetatenie = "Romana"
            };
            
            // 🔥 AUTO-GENERARE COD ANGAJAT PENTRU ADĂUGARE NOU
            await GenerateNextCodAngajatAsync();
            
            showResedintaSection = true;
            Logger.LogInformation("🏠 Residence section set to shown for new personal");
        }
        
        Logger.LogInformation("✅ AdaugaEditezaPersonal OnInitializedAsync completed - LocationDependentGridDropdowns should initialize now");
    }

    /// <summary>
    /// Generează și setează următorul cod de angajat disponibil pentru personal nou
    /// </summary>
    private async Task GenerateNextCodAngajatAsync()
    {
        try
        {
            Logger.LogInformation("🔢 Generating next employee code for new personal");
            
            // Folosește serviciul PersonalService pentru generarea codului
            var nextCode = await PersonalService.GetNextCodAngajatAsync();
            
            personalFormModel.Cod_Angajat = nextCode;
            
            Logger.LogInformation("✅ Generated next employee code: {Code}", nextCode);
            
            // Notificare vizuală pentru utilizator
            await ToastService.ShowInfoAsync(
                "Cod Angajat Generat", 
                $"Următorul cod disponibil: {nextCode}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error generating next employee code");
            
            // Fallback la un cod cu timestamp
            var timestamp = DateTime.Now.ToString("HHmmss");
            personalFormModel.Cod_Angajat = $"EMP{timestamp}";
            
            await ToastService.ShowWarningAsync(
                "Avertisment", 
                "Nu s-a putut genera automat codul. Utilizați codul generat temporar și verificați manual.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("🎨 AdaugaEditezaPersonal first render completed - DOM should be ready");
            Logger.LogInformation("📊 Current state: ShowResedintaSection={ShowResedinta}, IsEditMode={IsEdit}", 
                showResedintaSection, IsEditMode);
            
            // LocationDependentGridDropdowns componentele ar trebui să se inițializeze acum
        }
        
        await Task.CompletedTask;
    }

    #region Event Handlers simplificati pentru Lookup-uri

    private async Task OnJudetDomiciliuNameChanged(string? judetName)
    {
        Logger.LogInformation("🔥 Parent OnJudetDomiciliuNameChanged: {Name} - selectedJudetDomiciliuId={JudetId}", 
            judetName, selectedJudetDomiciliuId);
        personalFormModel.Judet_Domiciliu = judetName ?? "";
        // Nu apela StateHasChanged() aici - componenta copil se va actualiza singură
    }

    private async Task OnLocalitateDomiciliuNameChanged(string? localitateName)
    {
        Logger.LogInformation("🔥 Parent OnLocalitateDomiciliuNameChanged: {Name} - selectedLocalitateDomiciliuId={LocalitateId}", 
            localitateName, selectedLocalitateDomiciliuId);
        personalFormModel.Oras_Domiciliu = localitateName ?? "";
        // Nu apela StateHasChanged() aici - componenta copil se va actualiza singură
    }

    private async Task OnJudetResedintaNameChanged(string? judetName)
    {
        Logger.LogInformation("🔥 Parent OnJudetResedintaNameChanged: {Name} - selectedJudetResedintaId={JudetId}", 
            judetName, selectedJudetResedintaId);
        personalFormModel.Judet_Resedinta = judetName;
        // Nu apela StateHasChanged() aici - componenta copil se va actualiza singură
    }

    private async Task OnLocalitateResedintaNameChanged(string? localitateName)
    {
        Logger.LogInformation("🔥 Parent OnLocalitateResedintaNameChanged: {Name} - selectedLocalitateResedintaId={LocalitateId}", 
            localitateName, selectedLocalitateResedintaId);
        personalFormModel.Oras_Resedinta = localitateName;
        // Nu apela StateHasChanged() aici - componenta copil se va actualiza singură
    }

    #endregion

    private async Task HandleSubmit()
    {
        validationErrors.Clear();
        isSubmitting = true;
        
        try
        {
            Logger.LogInformation("Submitting personal form with CNP: {CNP}", personalFormModel.CNP);
            
            var personalModel = personalFormModel.ToPersonal();
            
            // Validare FluentValidation
            var validationResult = IsEditMode 
                ? await ValidationService.ValidateForUpdateAsync(personalModel)
                : await ValidationService.ValidateForCreateAsync(personalModel);

            if (!validationResult.IsValid)
            {
                validationErrors = validationResult.Errors;
                Logger.LogWarning("Validation failed with {ErrorCount} errors", validationErrors.Count);
                StateHasChanged();
                return;
            }

            Logger.LogInformation("Validation passed, proceeding to save personal: {PersonalName}", 
                personalModel.NumeComplet);

            await OnSave.InvokeAsync(personalModel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting personal form");
            validationErrors = [new ValidationError { ErrorMessage = "A aparut o eroare la salvarea datelor" }];
        }
        finally
        {
            isSubmitting = false;
            StateHasChanged();
        }
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }

    /// <summary>
    /// Metodă publică pentru a declanșa submit-ul din exterior (ex: din FooterTemplate)
    /// </summary>
    public async Task SubmitForm()
    {
        await HandleSubmit();
    }

    /// <summary>
    /// Gestionează schimbarea checkbox-ului pentru adresa de resedinta
    /// </summary>
    private void OnResedintaCheckboxChanged(ChangeEventArgs args)
    {
        if (args.Value is bool isChecked)
        {
            showResedintaSection = isChecked;
            
            if (showResedintaSection)
            {
                personalFormModel.Adresa_Resedinta = null;
                personalFormModel.Judet_Resedinta = null;
                personalFormModel.Oras_Resedinta = null;
                personalFormModel.Cod_Postal_Resedinta = null;
                
                selectedJudetResedintaId = null;
                selectedLocalitateResedintaId = null;
            }
            
            StateHasChanged();
        }
    }

    private void LoadDropdownOptions()
    {
        try
        {
            departmentOptions = Enum.GetValues<Departament>()
                .Select(d => new DropdownOption<Departament> 
                { 
                    Text = GetDepartmentDisplayName(d), 
                    Value = d 
                })
                .ToList();

            stareCivilaOptions = Enum.GetValues<StareCivila>()
                .Select(s => new DropdownOption<StareCivila>
                {
                    Text = GetStareCivilaDisplayName(s),
                    Value = s
                })
                .ToList();

            statusAngajatOptions = Enum.GetValues<StatusAngajat>()
                .Select(s => new DropdownOption<StatusAngajat>
                {
                    Text = GetStatusAngajatDisplayName(s),
                    Value = s
                })
                .ToList();

            Logger.LogInformation("Loaded dropdown options for enums");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dropdown options");
        }
    }

    private string GetDepartmentDisplayName(Departament department)
    {
        var displayAttribute = typeof(Departament)
            .GetField(department.ToString())?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return displayAttribute?.Name ?? department.ToString();
    }

    private string GetStareCivilaDisplayName(StareCivila stare)
    {
        var displayAttribute = typeof(StareCivila)
            .GetField(stare.ToString())?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return displayAttribute?.Name ?? stare.ToString();
    }

    private string GetStatusAngajatDisplayName(StatusAngajat status)
    {
        var displayAttribute = typeof(StatusAngajat)
            .GetField(status.ToString())?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return displayAttribute?.Name ?? status.ToString();
    }

    /// <summary>
    /// Obtine erorile pentru un camp specific
    /// </summary>
    private List<string> GetFieldErrors(string propertyName)
    {
        return validationErrors
            .Where(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.ErrorMessage)
            .ToList();
    }

    /// <summary>
    /// Verifica daca un camp are erori
    /// </summary>
    private bool HasFieldErrors(string propertyName)
    {
        return validationErrors.Any(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtine clasa CSS pentru un camp in functie de starea de validare
    /// </summary>
    private string GetFieldCssClass(string propertyName)
    {
        var baseClass = "form-control";
        if (HasFieldErrors(propertyName))
        {
            return $"{baseClass} is-invalid";
        }
        return baseClass;
    }

    /// <summary>
    /// Obține clasa CSS pentru câmpul CNP în funcție de starea validării
    /// </summary>
    private string GetCNPFieldCssClass()
    {
        if (isCNPBeingValidated)
        {
            return "cnp-validating";
        }
        else if (!string.IsNullOrWhiteSpace(personalFormModel.CNP))
        {
            if (isCNPValid)
            {
                return "cnp-valid";
            }
            else if (personalFormModel.CNP.Length == 13)
            {
                return "cnp-invalid";
            }
            else
            {
                return "cnp-incomplete";
            }
        }
        return "";
    }

    /// <summary>
    /// Handler pentru input CNP cu evenimente HTML native
    /// </summary>
    private async Task OnCNPInputSimple(ChangeEventArgs args)
    {
        var cnpValue = args.Value?.ToString() ?? "";
        personalFormModel.CNP = cnpValue;

        // Reset feedback-ul imediat ce utilizatorul schimbă valoarea
        ResetCNPValidationFeedback();

        // Validare în timp real doar pentru CNP-uri complete
        if (!string.IsNullOrWhiteSpace(cnpValue) && cnpValue.Length == 13)
        {
            await ValidateCNPRealTime(cnpValue);
        }
        else if (!string.IsNullOrWhiteSpace(cnpValue))
        {
            // Feedback pentru CNP incomplet
            SetCNPValidationFeedback(false, 
                $"CNP incomplet - {cnpValue.Length}/13 caractere", 
                "cnp-feedback-warning");
        }

        StateHasChanged();
    }

    /// <summary>
    /// Handler pentru blur CNP cu evenimente HTML native
    /// </summary>
    private async Task OnCNPBlurSimple(Microsoft.AspNetCore.Components.Web.FocusEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(personalFormModel.CNP))
        {
            ResetCNPValidationFeedback();
            return;
        }

        // Validare completă la blur
        await ValidateCNPRealTime(personalFormModel.CNP);

        // Dacă CNP-ul este valid, calculează data nașterii
        if (isCNPValid)
        {
            await CalculateAndUpdateBirthDate();
        }
    }

    /// <summary>
    /// Validează CNP-ul în timp real cu feedback vizual
    /// </summary>
    private async Task ValidateCNPRealTime(string cnp)
    {
        try
        {
            isCNPBeingValidated = true;
            StateHasChanged();

            // Validare preliminară rapidă
            if (cnp.Length != 13)
            {
                SetCNPValidationFeedback(false, 
                    "CNP-ul trebuie să aibă exact 13 cifre", 
                    "cnp-feedback-error");
                return;
            }

            if (!cnp.All(char.IsDigit))
            {
                SetCNPValidationFeedback(false, 
                    "CNP-ul trebuie să conțină doar cifre", 
                    "cnp-feedback-error");
                return;
            }

            // Validare cu algoritm complet CNP
            var validationResult = ValidateCNPComplete(cnp);
            
            if (validationResult.IsValid)
            {
                var calculatedAge = CalculateAgeFromCNP(cnp);
                SetCNPValidationFeedback(true, 
                    $"CNP valid - Vârsta: {calculatedAge} ani", 
                    "cnp-feedback-success");
            }
            else
            {
                var userFriendlyMessage = GetUserFriendlyCNPError(validationResult.ErrorMessage);
                SetCNPValidationFeedback(false, userFriendlyMessage, "cnp-feedback-error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la validarea CNP {CNP}", cnp);
            SetCNPValidationFeedback(false, 
                "Eroare la validarea CNP-ului", 
                "cnp-feedback-error");
        }
        finally
        {
            isCNPBeingValidated = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Calculează și actualizează data nașterii dacă CNP-ul este valid
    /// </summary>
    private async Task CalculateAndUpdateBirthDate()
    {
        try
        {
            var calculatedBirthDate = ParseBirthDateFromCNP(personalFormModel.CNP);
            if (calculatedBirthDate.HasValue)
            {
                var currentBirthDate = personalFormModel.Data_Nasterii;
                var newBirthDate = calculatedBirthDate.Value;

                // Verifică dacă data calculată diferă semnificativ de cea existentă
                bool shouldUpdate = false;
                string updateReason = "";

                if (currentBirthDate.Date == DateTime.Today.AddYears(-30))
                {
                    // Data implicită - actualizează automat
                    shouldUpdate = true;
                    updateReason = "automat (dată implicită)";
                }
                else if (Math.Abs((currentBirthDate.Date - newBirthDate.Date).TotalDays) > 30)
                {
                    // Diferență semnificativă - actualizează cu notificare
                    shouldUpdate = true;
                    updateReason = "calculat din CNP (diferența mare față de data existentă)";
                }
                else if (currentBirthDate.Date != newBirthDate.Date)
                {
                    // Diferență mică - actualizează silențios
                    shouldUpdate = true;
                    updateReason = "corecție minore din CNP";
                }

                if (shouldUpdate)
                {
                    var oldDate = personalFormModel.Data_Nasterii.ToString("dd.MM.yyyy");
                    personalFormModel.Data_Nasterii = newBirthDate;
                    
                    Logger.LogInformation("✅ Data nașterii actualizată {Reason}: {OldDate} → {NewDate} (CNP: {CNP})", 
                        updateReason, oldDate, newBirthDate.ToString("dd.MM.yyyy"), personalFormModel.CNP);
                    
                    // Notificare vizuală pentru utilizator (numai pentru diferențe mari)
                    if (updateReason.Contains("diferența mare"))
                    {
                        await ToastService.ShowSuccessAsync(
                            "Data nașterii actualizată", 
                            $"Calculată din CNP: {oldDate} → {newBirthDate.ToString("dd.MM.yyyy")}");
                    }
                    else if (!updateReason.Contains("implicită"))
                    {
                        await ToastService.ShowInfoAsync(
                            "Data nașterii ajustată", 
                            $"Corecție bazată pe CNP: {newBirthDate.ToString("dd.MM.yyyy")}");
                    }
                    
                    StateHasChanged();
                }
            }
        }
        catch (ArgumentException ex)
        {
            // Erori de validare CNP - afișează în interfață
            Logger.LogWarning("⚠️ CNP invalid - {Error}: {CNP}", ex.Message, personalFormModel.CNP);
            SetCNPValidationFeedback(false, GetUserFriendlyCNPError(ex.Message), "cnp-feedback-error");
            await ShowCNPValidationError(ex.Message);
        }
        catch (Exception ex)
        {
            // Alte erori - log pentru debug, nu deranja utilizatorul
            Logger.LogError(ex, "🔥 Eroare neașteptată la parsarea CNP {CNP}", personalFormModel.CNP);
        }
    }

    /// <summary>
    /// Validare completă CNP cu algoritm de control
    /// </summary>
    private (bool IsValid, string ErrorMessage) ValidateCNPComplete(string cnp)
    {
        if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return (false, "CNP invalid: format incorect");

        try
        {
            // Validare cifra de control (ultima cifră)
            var controlDigit = CalculateCNPControlDigit(cnp.Substring(0, 12));
            var actualControlDigit = int.Parse(cnp[12].ToString());
            
            if (controlDigit != actualControlDigit)
            {
                return (false, $"Cifra de control incorectă: așteptată {controlDigit}, găsită {actualControlDigit}");
            }

            // Prima cifră determină sexul și secolul
            int firstDigit = int.Parse(cnp[0].ToString());
            
            // Extragere an, lună, zi din CNP (poziții 1-2, 3-4, 5-6)
            int cnpYear = int.Parse(cnp.Substring(1, 2));
            int cnpMonth = int.Parse(cnp.Substring(3, 2));
            int cnpDay = int.Parse(cnp.Substring(5, 2));

            // Determinare secol complet bazat pe prima cifră
            int fullYear = firstDigit switch
            {
                1 or 2 => 1900 + cnpYear,        // Persoane născute între 1900-1999
                3 or 4 => 1800 + cnpYear,        // Persoane născute între 1800-1899 (rar)
                5 or 6 => 2000 + cnpYear,        // Persoane născute între 2000-2099
                7 or 8 => 2000 + cnpYear,        // Rezidenți străini născuți între 2000-2099
                _ => throw new ArgumentException($"Prima cifră CNP invalidă: {firstDigit}")
            };

            // Validare interval an rezonabil pentru personal medical
            var currentYear = DateTime.Now.Year;
            if (fullYear < 1940 || fullYear > currentYear)
                return (false, $"Anul calculat din CNP nu este în intervalul acceptat (1940-{currentYear}): {fullYear}");

            // Validare lună
            if (cnpMonth < 1 || cnpMonth > 12)
                return (false, $"Luna din CNP nu este validă: {cnpMonth}");

            // Validare zi și creare dată
            try
            {
                var birthDate = new DateTime(fullYear, cnpMonth, cnpDay);
                
                // Verificare că data nu este în viitor
                if (birthDate.Date > DateTime.Today)
                    return (false, "Data nașterii calculată este în viitor");

                // Verificare vârstă rezonabilă pentru angajați (între 16-80 ani)
                var age = DateTime.Today.Year - birthDate.Year;
                if (birthDate.Date > DateTime.Today.AddYears(-age))
                    age--;
                
                if (age < 16)
                    return (false, $"Vârsta calculată este prea mică pentru un angajat: {age} ani");
                
                if (age > 80)
                    return (false, $"Vârsta calculată nu este rezonabilă: {age} ani");

                return (true, "CNP valid");
            }
            catch (ArgumentOutOfRangeException)
            {
                return (false, $"Ziua {cnpDay} nu este validă pentru luna {cnpMonth}/{fullYear}");
            }
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            return (false, $"CNP-ul nu poate fi procesat: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculează cifra de control pentru CNP conform algoritmului oficial
    /// </summary>
    private int CalculateCNPControlDigit(string cnp12Digits)
    {
        var weights = new int[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        int sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cnp12Digits[i].ToString()) * weights[i];
        }

        int remainder = sum % 11;
        return remainder < 10 ? remainder : 1;
    }

    /// <summary>
    /// Calculează vârsta pe baza CNP-ului
    /// </summary>
    private int CalculateAgeFromCNP(string cnp)
    {
        var birthDate = ParseBirthDateFromCNP(cnp);
        if (!birthDate.HasValue) return 0;

        var age = DateTime.Today.Year - birthDate.Value.Year;
        if (birthDate.Value.Date > DateTime.Today.AddYears(-age))
            age--;
        
        return age;
    }

    /// <summary>
    /// Setează feedback-ul vizual pentru validarea CNP
    /// </summary>
    private void SetCNPValidationFeedback(bool isValid, string message, string cssClass)
    {
        isCNPValid = isValid;
        cnpFeedbackMessage = message;
        cnpFeedbackCssClass = cssClass;
    }

    /// <summary>
    /// Resetează feedback-ul vizual pentru CNP
    /// </summary>
    private void ResetCNPValidationFeedback()
    {
        isCNPValid = false;
        cnpFeedbackMessage = "";
        cnpFeedbackCssClass = "";
    }

    /// <summary>
    /// Afișează eroare de validare CNP
    /// </summary>
    private async Task ShowCNPValidationError(string errorMessage)
    {
        try
        {
            var userFriendlyMessage = GetUserFriendlyCNPError(errorMessage);
            await ToastService.ShowWarningAsync("CNP invalid", userFriendlyMessage);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la afișarea notificării de eroare CNP");
        }
    }

    /// <summary>
    /// Convertește mesajele tehnice de eroare în mesaje prietenoase pentru utilizator
    /// </summary>
    private string GetUserFriendlyCNPError(string technicalError)
    {
        return technicalError.ToLowerInvariant() switch
        {
            var error when error.Contains("prima cifră cnp invalidă") => 
                "Prima cifră din CNP nu este validă (trebuie să fie între 1-8)",
            var error when error.Contains("anul calculat") => 
                "Anul calculat din CNP nu este realist",
            var error when error.Contains("luna din cnp") => 
                "Luna din CNP nu este validă (trebuie să fie între 01-12)",
            var error when error.Contains("data nașterii calculată este în viitor") => 
                "CNP-ul indică o dată de naștere din viitor",
            var error when error.Contains("vârsta calculată") => 
                "CNP-ul indică o vârstă nepotrivită pentru un angajat",
            var error when error.Contains("ziua") && error.Contains("nu este validă") => 
                "Ziua din CNP nu este validă pentru luna respectivă",
            var error when error.Contains("cnp-ul nu poate fi procesat") => 
                "CNP-ul conține o dată invalidă",
            var error when error.Contains("cifra de control") => 
                "CNP-ul nu respectă algoritmul de control românesc",
            _ => "CNP-ul introdus nu respectă formatul românesc standard"
        };
    }

    /// <summary>
    /// Parsează data nașterii din CNP-ul românesc
    /// Algoritm conform standardului românesc pentru CNP cu validare extinsă
    /// </summary>
    private DateTime? ParseBirthDateFromCNP(string cnp)
    {
        // Validare preliminară
        if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return null;

        try
        {
            // Prima cifră determină sexul și secolul
            int firstDigit = int.Parse(cnp[0].ToString());
            
            // Extragere an, lună, zi din CNP (poziții 1-2, 3-4, 5-6)
            int cnpYear = int.Parse(cnp.Substring(1, 2));
            int cnpMonth = int.Parse(cnp.Substring(3, 2));
            int cnpDay = int.Parse(cnp.Substring(5, 2));

            // Determinare secol complet bazat pe prima cifră
            int fullYear = firstDigit switch
            {
                1 or 2 => 1900 + cnpYear,        // Persoane născute între 1900-1999
                3 or 4 => 1800 + cnpYear,        // Persoane născute între 1800-1899 (rar)
                5 or 6 => 2000 + cnpYear,        // Persoane născute între 2000-2099
                7 or 8 => 2000 + cnpYear,        // Rezidenți străini născuți între 2000-2099
                _ => throw new ArgumentException($"Prima cifră CNP invalidă: {firstDigit}")
            };

            // Validare interval an rezonabil pentru personal medical
            var currentYear = DateTime.Now.Year;
            if (fullYear < 1940 || fullYear > currentYear)
                throw new ArgumentException($"Anul calculat din CNP nu este în intervalul acceptat (1940-{currentYear}): {fullYear}");

            // Validare lună
            if (cnpMonth < 1 || cnpMonth > 12)
                throw new ArgumentException($"Luna din CNP nu este validă: {cnpMonth}");

            // Validare zi și creare dată (DateTime va valida automat zile invalide pentru lună)
            DateTime birthDate;
            try
            {
                birthDate = new DateTime(fullYear, cnpMonth, cnpDay);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentException($"Ziua {cnpDay} nu este valabilă pentru luna {cnpMonth}/{fullYear}");
            }

            // Verificare că data nu este în viitor
            if (birthDate.Date > DateTime.Today)
                throw new ArgumentException("Data nașterii calculată este în viitor");

            // Verificare vârstă rezonabilă pentru angajați (între 16-80 ani)
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age))
                age--;
            
            if (age < 16)
                throw new ArgumentException($"Vârsta calculată este prea mică pentru un angajat: {age} ani");
            
            if (age > 80)
                throw new ArgumentException($"Vârsta calculată nu este rezonabilă: {age} ani");

            return birthDate;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            // Pentru orice altă excepție (FormatException, etc.)
            throw new ArgumentException($"CNP-ul nu poate fi procesat: {ex.Message}", ex);
        }
    }
}

public class DropdownOption<T>
{
    public string Text { get; set; } = "";
    public T Value { get; set; } = default!;
}

public class PersonalFormModel
{
    public Guid Id_Personal { get; set; }
    public string Cod_Angajat { get; set; } = "";
    public string CNP { get; set; } = "";
    public string Nume { get; set; } = "";
    public string Prenume { get; set; } = "";
    public string? Nume_Anterior { get; set; }
    public DateTime Data_Nasterii { get; set; }
    public string? Locul_Nasterii { get; set; }
    public string Nationalitate { get; set; } = "Romana";
    public string Cetatenie { get; set; } = "Romana";
    public string? Telefon_Personal { get; set; }
    public string? Telefon_Serviciu { get; set; }
    public string? Email_Personal { get; set; }
    public string? Email_Serviciu { get; set; }
    public string Adresa_Domiciliu { get; set; } = "";
    public string Judet_Domiciliu { get; set; } = "";
    public string Oras_Domiciliu { get; set; } = "";
    public string? Cod_Postal_Domiciliu { get; set; }
    public string? Adresa_Resedinta { get; set; }
    public string? Judet_Resedinta { get; set; }
    public string? Oras_Resedinta { get; set; }
    public string? Cod_Postal_Resedinta { get; set; }
    public StareCivila? Stare_Civila { get; set; }
    public string Functia { get; set; } = "";
    public Departament? Departament { get; set; }
    public string? Serie_CI { get; set; }
    public string? Numar_CI { get; set; }
    public string? Eliberat_CI_De { get; set; }
    public DateTime? Data_Eliberare_CI { get; set; }
    public DateTime? Valabil_CI_Pana { get; set; }
    public StatusAngajat Status_Angajat { get; set; } = StatusAngajat.Activ;
    public string? Observatii { get; set; }
    public DateTime Data_Crearii { get; set; }
    public DateTime Data_Ultimei_Modificari { get; set; }
    public string? Creat_De { get; set; }
    public string? Modificat_De { get; set; }

    public static PersonalFormModel FromPersonal(PersonalModel personal)
    {
        return new PersonalFormModel
        {
            Id_Personal = personal.Id_Personal,
            Cod_Angajat = personal.Cod_Angajat,
            CNP = personal.CNP,
            Nume = personal.Nume,
            Prenume = personal.Prenume,
            Nume_Anterior = personal.Nume_Anterior,
            Data_Nasterii = personal.Data_Nasterii,
            Locul_Nasterii = personal.Locul_Nasterii,
            Nationalitate = personal.Nationalitate,
            Cetatenie = personal.Cetatenie,
            Telefon_Personal = personal.Telefon_Personal,
            Telefon_Serviciu = personal.Telefon_Serviciu,
            Email_Personal = personal.Email_Personal,
            Email_Serviciu = personal.Email_Serviciu,
            Adresa_Domiciliu = personal.Adresa_Domiciliu,
            Judet_Domiciliu = personal.Judet_Domiciliu,
            Oras_Domiciliu = personal.Oras_Domiciliu,
            Cod_Postal_Domiciliu = personal.Cod_Postal_Domiciliu,
            Adresa_Resedinta = personal.Adresa_Resedinta,
            Judet_Resedinta = personal.Judet_Resedinta,
            Oras_Resedinta = personal.Oras_Resedinta,
            Cod_Postal_Resedinta = personal.Cod_Postal_Resedinta,
            Stare_Civila = personal.Stare_Civila,
            Functia = personal.Functia,
            Departament = personal.Departament,
            Serie_CI = personal.Serie_CI,
            Numar_CI = personal.Numar_CI,
            Eliberat_CI_De = personal.Eliberat_CI_De,
            Data_Eliberare_CI = personal.Data_Eliberare_CI,
            Valabil_CI_Pana = personal.Valabil_CI_Pana,
            Status_Angajat = personal.Status_Angajat,
            Observatii = personal.Observatii,
            Creat_De = personal.Creat_De,
            Modificat_De = personal.Modificat_De,
            Data_Crearii = personal.Data_Crearii,
            Data_Ultimei_Modificari = personal.Data_Ultimei_Modificari
        };
    }

    public PersonalModel ToPersonal()
    {
        var now = DateTime.Now;
        var isNewRecord = Id_Personal == Guid.Empty || Data_Crearii == default;
        
        return new PersonalModel
        {
            Id_Personal = Id_Personal,
            Cod_Angajat = Cod_Angajat,
            CNP = CNP,
            Nume = Nume,
            Prenume = Prenume,
            Nume_Anterior = Nume_Anterior,
            Data_Nasterii = Data_Nasterii,
            Locul_Nasterii = Locul_Nasterii,
            Nationalitate = Nationalitate,
            Cetatenie = Cetatenie,
            Telefon_Personal = Telefon_Personal,
            Telefon_Serviciu = Telefon_Serviciu,
            Email_Personal = Email_Personal,
            Email_Serviciu = Email_Serviciu,
            Adresa_Domiciliu = Adresa_Domiciliu,
            Judet_Domiciliu = Judet_Domiciliu,
            Oras_Domiciliu = Oras_Domiciliu,
            Cod_Postal_Domiciliu = Cod_Postal_Domiciliu,
            Adresa_Resedinta = Adresa_Resedinta,
            Judet_Resedinta = Judet_Resedinta,
            Oras_Resedinta = Oras_Resedinta,
            Cod_Postal_Resedinta = Cod_Postal_Resedinta,
            Stare_Civila = Stare_Civila,
            Functia = Functia,
            Departament = Departament,
            Serie_CI = Serie_CI,
            Numar_CI = Numar_CI,
            Eliberat_CI_De = Eliberat_CI_De,
            Data_Eliberare_CI = Data_Eliberare_CI,
            Valabil_CI_Pana = Valabil_CI_Pana,
            Status_Angajat = Status_Angajat,
            Observatii = Observatii,
            Data_Crearii = isNewRecord ? now : Data_Crearii,
            Data_Ultimei_Modificari = now,
            Creat_De = isNewRecord ? "SYSTEM" : (Creat_De ?? "SYSTEM"),
            Modificat_De = "SYSTEM"
        };
    }
}
