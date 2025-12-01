using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.CreatePersonalMedical;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.UpdatePersonalMedical;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;
using ValyanClinic.Domain.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical.Modals;

public partial class PersonalMedicalFormModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalMedicalFormModal> Logger { get; set; } = default!;
    [Inject] private IDepartamentRepository DepartamentRepository { get; set; } = default!;
    [Inject] private IPozitieRepository PozitieRepository { get; set; } = default!;
    [Inject] private ISpecializareRepository SpecializareRepository { get; set; } = default!;

    [Parameter] public EventCallback OnPersonalMedicalSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string ActiveTab { get; set; } = "date-generale";
    private PersonalMedicalFormModel Model { get; set; } = new();
    private bool _disposed = false;

    // Lookup data
    private bool IsLoadingLookups { get; set; }
    private List<LookupOption> DepartamenteOptions { get; set; } = new();
    private List<LookupOption> PozitiiOptions { get; set; } = new();
    private List<LookupOption> SpecializariOptions { get; set; } = new();

    public class LookupOption
    {
        public Guid Id { get; set; }
        public string Nume { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        Model = new PersonalMedicalFormModel();
        Logger.LogInformation("PersonalMedicalFormModal initialized");

        // Load lookup data
        await LoadLookupData();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            Logger.LogDebug("PersonalMedicalFormModal disposing - clearing dropdown data");

            // Clear dropdown options immediately
            DepartamenteOptions?.Clear();
            PozitiiOptions?.Clear();
            SpecializariOptions?.Clear();
            DepartamenteOptions = new();
            PozitiiOptions = new();
            SpecializariOptions = new();

            // Clear model
            Model = new PersonalMedicalFormModel();

            Logger.LogDebug("PersonalMedicalFormModal disposed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in PersonalMedicalFormModal dispose");
        }
    }

    private async Task LoadLookupData()
    {
        if (_disposed) return;

        try
        {
            IsLoadingLookups = true;
            await InvokeAsync(StateHasChanged);

            // Load Departamente
            var departamente = await DepartamentRepository.GetAllAsync();
            if (_disposed) return;

            DepartamenteOptions = departamente
                .Select(d => new LookupOption { Id = d.IdDepartament, Nume = d.DenumireDepartament })
                .OrderBy(d => d.Nume)
                .ToList();

            // Load Pozitii
            var pozitii = await PozitieRepository.GetAllAsync();
            if (_disposed) return;

            PozitiiOptions = pozitii
                .Where(p => p.EsteActiv)
                .Select(p => new LookupOption { Id = p.Id, Nume = p.Denumire })
                .OrderBy(p => p.Nume)
                .ToList();

            // Load Specializari - FOLOSEȘTE NOUA METODĂ PENTRU DROPDOWN
            var specializariDropdown = await SpecializareRepository.GetDropdownOptionsAsync(esteActiv: true);
            if (_disposed) return;

            SpecializariOptions = specializariDropdown
                 .Select(s => new LookupOption { Id = s.Id, Nume = s.Denumire })
           .OrderBy(s => s.Nume)
                .ToList();

            Logger.LogInformation("Loaded {DeptCount} departamente, {PozCount} pozitii and {SpecCount} specializari",
                DepartamenteOptions.Count, PozitiiOptions.Count, SpecializariOptions.Count);
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Error loading lookup data");
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoadingLookups = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void SetActiveTab(string tabName)
    {
        if (_disposed) return;
        ActiveTab = tabName;
        Logger.LogDebug("Tab changed to: {TabName}", tabName);
    }

    public async Task OpenForAdd()
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Opening modal for ADD PersonalMedical");

            Model = new PersonalMedicalFormModel
            {
                EsteActiv = true
            };

            IsVisible = true;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "date-generale";

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for ADD");
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea formularului: {ex.Message}";
        }
    }

    public async Task OpenForEdit(Guid personalID)
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Opening modal for EDIT PersonalMedical: {PersonalID}", personalID);

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "date-generale";

            await InvokeAsync(StateHasChanged);

            var query = new GetPersonalMedicalByIdQuery(personalID);
            var result = await Mediator.Send(query);

            if (_disposed) return;

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                // ✅ FIX: Log valorile primite din DB pentru debugging
                Logger.LogInformation("DEBUG OpenForEdit - Date primite din DB:");
                Logger.LogInformation("PersonalID: {PersonalID}", data.PersonalID);
                Logger.LogInformation("  Nume: {Nume} {Prenume}", data.Nume, data.Prenume);
                Logger.LogInformation("  CategorieID: {CategorieID}", data.CategorieID);
                Logger.LogInformation("  PozitieID: {PozitieID}", data.PozitieID);
                Logger.LogInformation("  SpecializareID: {SpecializareID}", data.SpecializareID);
                Logger.LogInformation("  Text fields - Pozitie: '{Pozitie}', Departament: '{Departament}', Specializare: '{Specializare}'",
                    data.Pozitie, data.Departament, data.Specializare);

                Model = new PersonalMedicalFormModel
                {
                    PersonalID = data.PersonalID,
                    Nume = data.Nume,
                    Prenume = data.Prenume,
                    NumarLicenta = data.NumarLicenta,
                    Telefon = data.Telefon,
                    Email = data.Email,
                    EsteActiv = data.EsteActiv ?? true,
                    DepartamentID = data.CategorieID,  // CategorieID mapped to DepartamentID
                    PozitieID = data.PozitieID,  // ✅ FIX: ACUM VA FI POPULAT DIN DB!
                    SpecializareID = data.SpecializareID
                };

                // ✅ FIX: Log valorile mapate în model pentru debugging
                Logger.LogInformation("DEBUG OpenForEdit - Valori mapate în Model:");
                Logger.LogInformation("  Model.DepartamentID: {DepartamentID}", Model.DepartamentID);
                Logger.LogInformation("  Model.PozitieID: {PozitieID}", Model.PozitieID);
                Logger.LogInformation("  Model.SpecializareID: {SpecializareID}", Model.SpecializareID);

                // ✅ FIX: Verifică dacă PozitieID există în dropdown options
                if (Model.PozitieID.HasValue)
                {
                    var foundPozitie = PozitiiOptions.FirstOrDefault(p => p.Id == Model.PozitieID.Value);
                    if (foundPozitie != null)
                    {
                        Logger.LogInformation("DEBUG OpenForEdit - PozitieID găsit în dropdown: {Id} - '{Nume}'",
                            foundPozitie.Id, foundPozitie.Nume);
                    }
                    else
                    {
                        Logger.LogWarning("DEBUG OpenForEdit - PozitieID NU GĂSIT în dropdown options! PozitieID: {PozitieID}", Model.PozitieID);
                        Logger.LogInformation("DEBUG OpenForEdit - Options disponibile ({Count}):", PozitiiOptions.Count);
                        foreach (var opt in PozitiiOptions.Take(5))
                        {
                            Logger.LogInformation("  Option: {Id} - '{Nume}'", opt.Id, opt.Nume);
                        }
                    }
                }
                else
                {
                    Logger.LogWarning("DEBUG OpenForEdit - PozitieID este NULL în data din DB!");
                }

                Logger.LogInformation("Data loaded for EDIT mode: {Nume} {Prenume}",
  Model.Nume, Model.Prenume);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele";
                Logger.LogWarning("Failed to load data: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Error opening modal for EDIT");
                HasError = true;
                ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    public async Task Close()
    {
        if (_disposed) return;

        Logger.LogInformation("Closing modal");
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        await Task.Delay(300);

        if (!_disposed)
        {
            Model = new PersonalMedicalFormModel();
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "date-generale";
        }
    }

    private async Task HandleOverlayClick()
    {
        // ❌ DEZACTIVAT: Nu închide modalul la click pe overlay
        // Acest modal conține date importante care nu trebuie pierdute
        // await Close();

        // 📝 OPȚIONAL: Poți adăuga un warning visual că trebuie să folosească butoanele
        // Pentru moment, nu facem nimic - modalul rămâne deschis
        return;
    }

    private async Task HandleSubmit()
    {
        if (_disposed) return;

        try
        {
            Logger.LogInformation("Submitting form: IsEditMode={IsEditMode}", Model.IsEditMode);
            Logger.LogInformation("Selected PozitieID: {PozitieID}", Model.PozitieID);

            IsSaving = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);

            // Get selected names for text fields (backwards compatibility)
            var departamentName = DepartamenteOptions.FirstOrDefault(d => d.Id == Model.DepartamentID)?.Nume;

            // FIX: Nu folosi default value pentru pozitie - permite NULL dacă nu e găsită
            var pozitieDisplayName = PozitiiOptions.FirstOrDefault(p => p.Id == Model.PozitieID)?.Nume;

            var specializareName = SpecializariOptions.FirstOrDefault(s => s.Id == Model.SpecializareID)?.Nume;

            // Log pentru debugging
            Logger.LogInformation("Mapping values: Departament={Departament}, Pozitie={Pozitie}, Specializare={Specializare}",
                departamentName, pozitieDisplayName, specializareName);
            Logger.LogInformation("Available Pozitii count: {Count}", PozitiiOptions.Count);
            if (PozitiiOptions.Count > 0)
            {
                Logger.LogInformation("First Pozitie: Id={Id}, Nume={Nume}", PozitiiOptions.First().Id, PozitiiOptions.First().Nume);
            }

            if (Model.IsEditMode)
            {
                var command = new UpdatePersonalMedicalCommand
                {
                    PersonalID = Model.PersonalID!.Value,
                    Nume = Model.Nume,
                    Prenume = Model.Prenume,
                    Specializare = specializareName,  // Text field for display
                    NumarLicenta = Model.NumarLicenta,
                    Telefon = Model.Telefon,
                    Email = Model.Email,
                    Departament = departamentName,  // Text field for display
                    Pozitie = pozitieDisplayName,  // Text field for display - NOW NULL if not found
                    EsteActiv = Model.EsteActiv,
                    CategorieID = Model.DepartamentID,  // FK to Departamente
                    PozitieID = Model.PozitieID,  // FK to Pozitii
                    SpecializareID = Model.SpecializareID,  // FK to Specializari
                    SubspecializareID = null  // Not used
                };

                var result = await Mediator.Send(command);

                if (_disposed) return;

                if (result.IsSuccess)
                {
                    Logger.LogInformation("PersonalMedical updated successfully: {PersonalID}", Model.PersonalID);
                    await OnPersonalMedicalSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la actualizare" });
                    Logger.LogWarning("Failed to update: {Error}", ErrorMessage);
                }
            }
            else
            {
                var command = new CreatePersonalMedicalCommand
                {
                    Nume = Model.Nume,
                    Prenume = Model.Prenume,
                    Specializare = specializareName,
                    NumarLicenta = Model.NumarLicenta,
                    Telefon = Model.Telefon,
                    Email = Model.Email,
                    Departament = departamentName,
                    Pozitie = pozitieDisplayName,  // NOW NULL if not found
                    EsteActiv = Model.EsteActiv,
                    CategorieID = Model.DepartamentID,
                    PozitieID = Model.PozitieID,
                    SpecializareID = Model.SpecializareID,
                    SubspecializareID = null
                };

                var result = await Mediator.Send(command);

                if (_disposed) return;

                if (result.IsSuccess)
                {
                    Logger.LogInformation("PersonalMedical created successfully: {PersonalID}", result.Value);
                    await OnPersonalMedicalSaved.InvokeAsync();
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la creare" });
                    Logger.LogWarning("Failed to create: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Error submitting form");
                HasError = true;
                ErrorMessage = $"Eroare: {ex.Message}";
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsSaving = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    public class PersonalMedicalFormModel
    {
        public Guid? PersonalID { get; set; }

        [Required(ErrorMessage = "Nume este obligatoriu")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prenume este obligatoriu")]
        public string Prenume { get; set; } = string.Empty;

        public string? NumarLicenta { get; set; }
        public string? Telefon { get; set; }

        [EmailAddress(ErrorMessage = "Email invalid")]
        public string? Email { get; set; }

        public bool EsteActiv { get; set; } = true;

        // Dropdown selections (FK IDs)
        public Guid? DepartamentID { get; set; }

        [Required(ErrorMessage = "Pozitie este obligatorie")]
        public Guid? PozitieID { get; set; }

        public Guid? SpecializareID { get; set; }

        public bool IsEditMode => PersonalID.HasValue && PersonalID.Value != Guid.Empty;
    }
}
