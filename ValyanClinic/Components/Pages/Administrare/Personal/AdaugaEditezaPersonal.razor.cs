using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Services;
using ValyanClinic.Application.Validators;
using ValyanClinic.Components.Shared.Validation;
using PersonalModel = ValyanClinic.Domain.Models.Personal;
using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdaugaEditezaPersonal : ComponentBase
{
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    [Inject] private IValidationService ValidationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdaugaEditezaPersonal> Logger { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    [Parameter] public PersonalModel? EditingPersonal { get; set; }
    [Parameter] public EventCallback<PersonalModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private PersonalFormModel personalFormModel = new();
    private List<ValidationError> validationErrors = new();
    
    // Dropdown options
    private List<DropdownOption<Departament>> departmentOptions = new();
    private List<DropdownOption<StareCivila>> stareCivilaOptions = new();
    private List<DropdownOption<StatusAngajat>> statusAngajatOptions = new();
    private List<DropdownOption<string>> judeteOptions = new();
    
    private bool isSubmitting = false;
    private FluentValidationHelper<PersonalModel>? validationHelper;

    private bool IsEditMode => EditingPersonal != null;

    protected override async Task OnInitializedAsync()
    {
        await LoadDropdownOptions();
        
        if (IsEditMode && EditingPersonal != null)
        {
            Logger.LogInformation("Editare personal - CNP: {CNP}", EditingPersonal.CNP);
            personalFormModel = PersonalFormModel.FromPersonal(EditingPersonal);
        }
        else
        {
            Logger.LogInformation("Adăugare personal nou");
            personalFormModel = new PersonalFormModel
            {
                Id_Personal = Guid.NewGuid(),
                Data_Nasterii = DateTime.Today.AddYears(-30),
                Status_Angajat = StatusAngajat.Activ,
                Nationalitate = "Română",
                Cetatenie = "Română"
            };
        }
    }

    private async Task LoadDropdownOptions()
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

            // Load judete options (simplified list - in production this would come from a service)
            judeteOptions = GetJudeteRomania();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dropdown options");
        }
    }

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
            validationErrors = [new ValidationError { ErrorMessage = "A apărut o eroare la salvarea datelor" }];
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
    /// Validează un câmp specific în timp real
    /// </summary>
    private async Task ValidateFieldAsync(string propertyName)
    {
        try
        {
            var personalModel = personalFormModel.ToPersonal();
            var result = await ValidationService.ValidateAsync(personalModel);
            
            // Elimină erorile vechi pentru această proprietate
            validationErrors.RemoveAll(e => e.PropertyName == propertyName);
            
            // Adaugă erorile noi pentru această proprietate
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
    /// Obține erorile pentru un câmp specific
    /// </summary>
    private List<string> GetFieldErrors(string propertyName)
    {
        return validationErrors
            .Where(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.ErrorMessage)
            .ToList();
    }

    /// <summary>
    /// Verifică dacă un câmp are erori
    /// </summary>
    private bool HasFieldErrors(string propertyName)
    {
        return validationErrors.Any(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obține clasa CSS pentru un câmp în funcție de starea de validare
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

    private List<DropdownOption<string>> GetJudeteRomania()
    {
        return new List<DropdownOption<string>>
        {
            new() { Text = "Alba", Value = "Alba" },
            new() { Text = "Arad", Value = "Arad" },
            new() { Text = "Argeș", Value = "Argeș" },
            new() { Text = "Bacău", Value = "Bacău" },
            new() { Text = "Bihor", Value = "Bihor" },
            new() { Text = "Bistrița-Năsăud", Value = "Bistrița-Năsăud" },
            new() { Text = "Botoșani", Value = "Botoșani" },
            new() { Text = "Brașov", Value = "Brașov" },
            new() { Text = "Brăila", Value = "Brăila" },
            new() { Text = "București", Value = "București" },
            new() { Text = "Buzău", Value = "Buzău" },
            new() { Text = "Caraș-Severin", Value = "Caraș-Severin" },
            new() { Text = "Călărași", Value = "Călărași" },
            new() { Text = "Cluj", Value = "Cluj" },
            new() { Text = "Constanța", Value = "Constanța" },
            new() { Text = "Covasna", Value = "Covasna" },
            new() { Text = "Dâmbovița", Value = "Dâmbovița" },
            new() { Text = "Dolj", Value = "Dolj" },
            new() { Text = "Galați", Value = "Galați" },
            new() { Text = "Giurgiu", Value = "Giurgiu" },
            new() { Text = "Gorj", Value = "Gorj" },
            new() { Text = "Harghita", Value = "Harghita" },
            new() { Text = "Hunedoara", Value = "Hunedoara" },
            new() { Text = "Ialomița", Value = "Ialomița" },
            new() { Text = "Iași", Value = "Iași" },
            new() { Text = "Ilfov", Value = "Ilfov" },
            new() { Text = "Maramureș", Value = "Maramureș" },
            new() { Text = "Mehedinți", Value = "Mehedinți" },
            new() { Text = "Mureș", Value = "Mureș" },
            new() { Text = "Neamț", Value = "Neamț" },
            new() { Text = "Olt", Value = "Olt" },
            new() { Text = "Prahova", Value = "Prahova" },
            new() { Text = "Sălaj", Value = "Sălaj" },
            new() { Text = "Satu Mare", Value = "Satu Mare" },
            new() { Text = "Sibiu", Value = "Sibiu" },
            new() { Text = "Suceava", Value = "Suceava" },
            new() { Text = "Teleorman", Value = "Teleorman" },
            new() { Text = "Timiș", Value = "Timiș" },
            new() { Text = "Tulcea", Value = "Tulcea" },
            new() { Text = "Vaslui", Value = "Vaslui" },
            new() { Text = "Vâlcea", Value = "Vâlcea" },
            new() { Text = "Vrancea", Value = "Vrancea" }
        };
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
    
    // Date Personale De Bază
    public string Nume { get; set; } = "";
    public string Prenume { get; set; } = "";
    public string? Nume_Anterior { get; set; }
    public DateTime Data_Nasterii { get; set; }
    public string? Locul_Nasterii { get; set; }  // CORECTAT numele proprietății
    public string Nationalitate { get; set; } = "Română";
    public string Cetatenie { get; set; } = "Română";
    
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
    
    // Audit fields - pentru păstrarea valorilor la editare
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
            Locul_Nasterii = personal.Locul_Nasterii, // CORECTAT - folosește Locul_Nasterii nu Locul_Nasterei
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
            // Păstrează valorile originale pentru audit
            Creat_De = personal.Creat_De,
            Modificat_De = personal.Modificat_De,
            Data_Crearii = personal.Data_Crearii,
            Data_Ultimei_Modificari = personal.Data_Ultimei_Modificari
        };
    }

    public PersonalModel ToPersonal()
    {
        var now = DateTime.Now; // CORECTAT: folosește ora locală în loc de UTC
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
            
            // Audit fields - tratare diferită pentru creare vs editare, cu ora locală
            Data_Crearii = isNewRecord ? now : Data_Crearii,
            Data_Ultimei_Modificari = now,
            Creat_De = isNewRecord ? "SYSTEM" : (Creat_De ?? "SYSTEM"),
            Modificat_De = "SYSTEM" // TODO: Înlocuiți cu utilizatorul autentificat
        };
    }
}
