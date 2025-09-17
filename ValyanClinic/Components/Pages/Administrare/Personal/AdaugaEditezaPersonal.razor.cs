using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Services;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Application.Validators;
using ValyanClinic.Components.Shared.Validation;
using ValyanClinic.Components.Shared;
using PersonalModel = ValyanClinic.Domain.Models.Personal;
using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdaugaEditezaPersonal : ComponentBase
{
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    [Inject] private ILocationService LocationService { get; set; } = default!;
    [Inject] private IValidationService ValidationService { get; set; } = default!;
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
            showResedintaSection = true;
            Logger.LogInformation("🏠 Residence section set to shown for new personal");
        }
        
        Logger.LogInformation("✅ AdaugaEditezaPersonal OnInitializedAsync completed - LocationDependentGridDropdowns should initialize now");
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
    /// Logica inversată: bifat = adrese identice (card ascuns), nebifat = adrese diferite (card vizibil)
    /// </summary>
    private void OnResedintaCheckboxChanged(ChangeEventArgs args)
    {
        if (args.Value is bool isChecked)
        {
            showResedintaSection = isChecked;
            
            // Dacă checkbox-ul este BIFAT (adrese identice), curăță datele de reședință
            if (showResedintaSection)
            {
                personalFormModel.Adresa_Resedinta = null;
                personalFormModel.Judet_Resedinta = null;
                personalFormModel.Oras_Resedinta = null;
                personalFormModel.Cod_Postal_Resedinta = null;
                
                // Reset și dropdown-urile
                selectedJudetResedintaId = null;
                selectedLocalitateResedintaId = null;
            }
            
            StateHasChanged();
        }
    }

    /// <summary>
    /// Valideaza un camp specific in timp real
    /// </summary>
    private async Task ValidateFieldAsync(string propertyName)
    {
        try
        {
            var personalModel = personalFormModel.ToPersonal();
            var result = await ValidationService.ValidateAsync(personalModel);
            
            // Elimina erorile vechi pentru aceasta proprietate
            validationErrors.RemoveAll(e => e.PropertyName == propertyName);
            
            // Adauga erorile noi pentru aceasta proprietate
            var propertyErrors = result.GetErrorsForProperty(propertyName);
            validationErrors.AddRange(propertyErrors);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating field: {FieldName}", propertyName);
        }
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

    // Display name helpers
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

    private void LoadDropdownOptions()
    {
        try
        {
            // Load department options
            departmentOptions = Enum.GetValues<Departament>()
                .Select(d => new DropdownOption<Departament> 
                { 
                    Text = GetDepartmentDisplayName(d), 
                    Value = d 
                })
                .ToList();

            // Load stare civila options
            stareCivilaOptions = Enum.GetValues<StareCivila>()
                .Select(s => new DropdownOption<StareCivila>
                {
                    Text = GetStareCivilaDisplayName(s),
                    Value = s
                })
                .ToList();

            // Load status angajat options
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
}

public class DropdownOption<T>
{
    public string Text { get; set; } = "";
    public T Value { get; set; } = default!;
}

public class PersonalFormModel
{
    // Identificatori Unici
    public Guid Id_Personal { get; set; }
    public string Cod_Angajat { get; set; } = "";
    public string CNP { get; set; } = "";
    
    // Date Personale De Baza
    public string Nume { get; set; } = "";
    public string Prenume { get; set; } = "";
    public string? Nume_Anterior { get; set; }
    public DateTime Data_Nasterii { get; set; }
    public string? Locul_Nasterii { get; set; }
    public string Nationalitate { get; set; } = "Romana";
    public string Cetatenie { get; set; } = "Romana";
    
    // Contact
    public string? Telefon_Personal { get; set; }
    public string? Telefon_Serviciu { get; set; }
    public string? Email_Personal { get; set; }
    public string? Email_Serviciu { get; set; }
    
    // Adresa Domiciliu
    public string Adresa_Domiciliu { get; set; } = "";
    public string Judet_Domiciliu { get; set; } = "";
    public string Oras_Domiciliu { get; set; } = "";
    public string? Cod_Postal_Domiciliu { get; set; }
    
    // Adresa Resedinta (Daca Difera)
    public string? Adresa_Resedinta { get; set; }
    public string? Judet_Resedinta { get; set; }
    public string? Oras_Resedinta { get; set; }
    public string? Cod_Postal_Resedinta { get; set; }
    
    // Stare Civila Si Familie
    public StareCivila? Stare_Civila { get; set; }
    
    // Date Profesionale
    public string Functia { get; set; } = "";
    public Departament? Departament { get; set; }
    
    // Date Administrative
    public string? Serie_CI { get; set; }
    public string? Numar_CI { get; set; }
    public string? Eliberat_CI_De { get; set; }
    public DateTime? Data_Eliberare_CI { get; set; }
    public DateTime? Valabil_CI_Pana { get; set; }
    
    // Status Si Metadata
    public StatusAngajat Status_Angajat { get; set; } = StatusAngajat.Activ;
    public string? Observatii { get; set; }
    
    // Audit fields - pentru pastrarea valorilor la editare
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
            // Pastreaza valorile originale pentru audit
            Creat_De = personal.Creat_De,
            Modificat_De = personal.Modificat_De,
            Data_Crearii = personal.Data_Crearii,
            Data_Ultimei_Modificari = personal.Data_Ultimei_Modificari
        };
    }

    public PersonalModel ToPersonal()
    {
        var now = DateTime.Now; // CORECTAT: foloseste ora locala in loc de UTC
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
            
            // Audit fields - tratare diferita pentru creare vs editare, cu ora locala
            Data_Crearii = isNewRecord ? now : Data_Crearii,
            Data_Ultimei_Modificari = now,
            Creat_De = isNewRecord ? "SYSTEM" : (Creat_De ?? "SYSTEM"),
            Modificat_De = "SYSTEM" // TODO: Inlocuiti cu utilizatorul autentificat
        };
    }
}
