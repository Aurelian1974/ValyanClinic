using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalManagement.Commands.CreatePersonal;
using ValyanClinic.Application.Features.PersonalManagement.Commands.UpdatePersonal;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalById;
using ValyanClinic.Components.Pages.Administrare.Personal.Models;
using ValyanClinic.Domain.Interfaces.Repositories;
using Syncfusion.Blazor.DropDowns;
using System.Globalization;

namespace ValyanClinic.Components.Pages.Administrare.Personal.Modals;

public partial class PersonalFormModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<PersonalFormModal> Logger { get; set; } = default!;
    [Inject] private ILocationRepository LocationRepository { get; set; } = default!;

    [Parameter] public EventCallback OnPersonalSaved { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsVisible { get; set; }
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string ActiveTab { get; set; } = "personal";
    private PersonalFormModel Model { get; set; } = new();

    // 🔒 Disposed state protection
    private bool _disposed = false;

    // Location dropdown data
    private List<LocationOption> JudeteOptions { get; set; } = new();
    private List<LocationOption> LocalitatiDomiciliuOptions { get; set; } = new();
    private List<LocationOption> LocalitatiResedintaOptions { get; set; } = new();

    // Loading states pentru UX imbunatatit
    private bool IsLoadingLocalitatiDomiciliu { get; set; }
    private bool IsLoadingLocalitatiResedinta { get; set; }

    // CultureInfo pentru sortare cu diacritice romanesti
    private static readonly CultureInfo RomanianCulture = new("ro-RO");

    public class LocationOption
    {
        public int Id { get; set; }
        public string Nume { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        Model = new PersonalFormModel();
        Logger.LogInformation("PersonalFormModal initialized");

        await LoadJudete();
    }

    #region Helper Methods

    private string GetLocalitatiPlaceholder(bool isLoading, int count)
    {
        if (isLoading)
            return "Se incarca...";

        if (count == 0)
            return "Selecteaza localitatea";

        return $"Cauta sau selecteaza ({count} disponibile)";
    }

    private async Task CopyDomiciliuToResedinta()
    {
        try
        {
            Logger.LogInformation("=== START: Copying Domiciliu data to Resedinta ===");

            // Copiem datele text
            Model.Adresa_Resedinta = Model.Adresa_Domiciliu;
            Model.Cod_Postal_Resedinta = Model.Cod_Postal_Domiciliu;

            Logger.LogInformation("Copied Adresa and Cod Postal");

            // IMPORTANT: Load localitati INAINTE de a seta judetul si orasul
            if (!string.IsNullOrEmpty(Model.Judet_Domiciliu))
            {
                var judet = JudeteOptions.FirstOrDefault(j => j.Nume == Model.Judet_Domiciliu);
                if (judet != null)
                {
                    Logger.LogInformation("Found judet for copy: {JudetId} - {JudetNume}", judet.Id, judet.Nume);

                    IsLoadingLocalitatiResedinta = true;
                    await InvokeAsync(StateHasChanged);

                    // Load localitati pentru judetul copiat
                    await LoadLocalitatiResedinta(judet.Id);

                    IsLoadingLocalitatiResedinta = false;

                    // ACUM setam judetul si orasul DUPA ce am incarcat opțiunile
                    Model.Judet_Resedinta = Model.Judet_Domiciliu;
                    Model.Oras_Resedinta = Model.Oras_Domiciliu;

                    Logger.LogInformation("=== SUCCESS: Copied Domiciliu to Resedinta - Judet: {Judet}, Oras: {Oras} ===",
                        Model.Judet_Resedinta, Model.Oras_Resedinta);
                }
                else
                {
                    Logger.LogWarning("=== WARNING: Judet not found for copy: {JudetNume} ===", Model.Judet_Domiciliu);

                    // Fallback: doar copiem string-urile fara localitati
                    Model.Judet_Resedinta = Model.Judet_Domiciliu;
                    Model.Oras_Resedinta = Model.Oras_Domiciliu;
                }
            }
            else
            {
                Logger.LogInformation("No Judet_Domiciliu to copy");
                Model.Judet_Resedinta = string.Empty;
                Model.Oras_Resedinta = string.Empty;
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR: Failed to copy Domiciliu to Resedinta ===");
        }
    }

    #endregion

    private async Task LoadJudete()
    {
        try
        {
            Logger.LogInformation("=== START: Loading judete for dropdown ===");
            var judete = await LocationRepository.GetJudeteAsync();

            // Sortare cu diacritice romanesti
            JudeteOptions = judete
                .Select(j => new LocationOption { Id = j.Id, Nume = j.Nume })
                .OrderBy(j => j.Nume, StringComparer.Create(RomanianCulture, false))
                .ToList();

            Logger.LogInformation("=== SUCCESS: Loaded {Count} judete (sorted with Romanian diacritics) ===", JudeteOptions.Count);

            if (JudeteOptions.Count > 0)
            {
                Logger.LogInformation("First 3 judete: {Judete}",
                    string.Join(", ", JudeteOptions.Take(3).Select(j => $"{j.Id}:{j.Nume}")));
            }
            else
            {
                Logger.LogWarning("=== WARNING: No judete loaded! Check database and SP ===");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR: Failed to load judete ===");
            JudeteOptions = new List<LocationOption>();
        }
    }

    private async Task OnJudetDomiciliuChangedV2(ChangeEventArgs<string, LocationOption> args)
    {
        try
        {
            var judetNume = args.Value;
            Logger.LogInformation("=== Judet Domiciliu changed to: {Judet} ===", judetNume);

            Model.Judet_Domiciliu = judetNume ?? string.Empty;
            Model.Oras_Domiciliu = string.Empty;
            LocalitatiDomiciliuOptions.Clear();

            if (!string.IsNullOrEmpty(judetNume))
            {
                var judet = JudeteOptions.FirstOrDefault(j => j.Nume == judetNume);
                if (judet != null)
                {
                    Logger.LogInformation("Found judet ID: {JudetId} for name: {JudetNume}", judet.Id, judet.Nume);

                    IsLoadingLocalitatiDomiciliu = true;
                    await InvokeAsync(StateHasChanged);

                    await LoadLocalitatiDomiciliu(judet.Id);

                    IsLoadingLocalitatiDomiciliu = false;
                }
                else
                {
                    Logger.LogWarning("=== WARNING: Judet not found in options: {JudetNume} ===", judetNume);
                }
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR: OnJudetDomiciliuChangedV2 ===");
            IsLoadingLocalitatiDomiciliu = false;
        }
    }

    private async Task OnJudetResedintaChangedV2(ChangeEventArgs<string, LocationOption> args)
    {
        try
        {
            var judetNume = args.Value;
            Logger.LogInformation("=== Judet Resedinta changed to: {Judet} ===", judetNume);

            Model.Judet_Resedinta = judetNume;
            Model.Oras_Resedinta = string.Empty;
            LocalitatiResedintaOptions.Clear();

            if (!string.IsNullOrEmpty(judetNume))
            {
                var judet = JudeteOptions.FirstOrDefault(j => j.Nume == judetNume);
                if (judet != null)
                {
                    Logger.LogInformation("Found judet ID: {JudetId} for name: {JudetNume}", judet.Id, judet.Nume);

                    IsLoadingLocalitatiResedinta = true;
                    await InvokeAsync(StateHasChanged);

                    await LoadLocalitatiResedinta(judet.Id);

                    IsLoadingLocalitatiResedinta = false;
                }
                else
                {
                    Logger.LogWarning("=== WARNING: Judet not found in options: {JudetNume} ===", judetNume);
                }
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR: OnJudetResedintaChangedV2 ===");
            IsLoadingLocalitatiResedinta = false;
        }
    }

    private async Task LoadLocalitatiDomiciliu(int judetId)
    {
        try
        {
            Logger.LogInformation("=== START: Loading localitati for judet ID: {JudetId} ===", judetId);
            var localitati = await LocationRepository.GetLocalitatiByJudetIdAsync(judetId);

            // Sortare cu diacritice romanesti
            LocalitatiDomiciliuOptions = localitati
                .Select(l => new LocationOption { Id = l.Id, Nume = l.Nume })
                .OrderBy(l => l.Nume, StringComparer.Create(RomanianCulture, false))
                .ToList();

            Logger.LogInformation("=== SUCCESS: Loaded {Count} localitati for domiciliu (sorted) ===", LocalitatiDomiciliuOptions.Count);

            if (LocalitatiDomiciliuOptions.Count > 0)
            {
                Logger.LogInformation("First 3 localitati: {Localitati}",
                    string.Join(", ", LocalitatiDomiciliuOptions.Take(3).Select(l => $"{l.Id}:{l.Nume}")));
            }
            else
            {
                Logger.LogWarning("=== WARNING: No localitati found for judet ID {JudetId} ===", judetId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR: Failed to load localitati domiciliu for judet {JudetId} ===", judetId);
            LocalitatiDomiciliuOptions = new List<LocationOption>();
        }
    }

    private async Task LoadLocalitatiResedinta(int judetId)
    {
        try
        {
            Logger.LogInformation("=== START: Loading localitati for judet ID: {JudetId} ===", judetId);
            var localitati = await LocationRepository.GetLocalitatiByJudetIdAsync(judetId);

            // Sortare cu diacritice romanesti
            LocalitatiResedintaOptions = localitati
                .Select(l => new LocationOption { Id = l.Id, Nume = l.Nume })
                .OrderBy(l => l.Nume, StringComparer.Create(RomanianCulture, false))
                .ToList();

            Logger.LogInformation("=== SUCCESS: Loaded {Count} localitati for resedinta (sorted) ===", LocalitatiResedintaOptions.Count);

            if (LocalitatiResedintaOptions.Count > 0)
            {
                Logger.LogInformation("First 3 localitati: {Localitati}",
                    string.Join(", ", LocalitatiResedintaOptions.Take(3).Select(l => $"{l.Id}:{l.Nume}")));
            }
            else
            {
                Logger.LogWarning("=== WARNING: No localitati found for judet ID {JudetId} ===", judetId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR: Failed to load localitati resedinta for judet {JudetId} ===", judetId);
            LocalitatiResedintaOptions = new List<LocationOption>();
        }
    }

    public async Task OpenForAdd()
    {
        try
        {
            Logger.LogInformation("Opening modal for ADD");

            Model = new PersonalFormModel
            {
                Status_Angajat = "Activ",
                Nationalitate = "Romana",
                Cetatenie = "Romana",
                Data_Nasterii = DateTime.Today.AddYears(-30)
            };

            LocalitatiDomiciliuOptions.Clear();
            LocalitatiResedintaOptions.Clear();
            IsLoadingLocalitatiDomiciliu = false;
            IsLoadingLocalitatiResedinta = false;

            IsVisible = true;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "personal";

            await InvokeAsync(StateHasChanged);

            Logger.LogInformation("Modal opened for ADD - Judete available: {Count}", JudeteOptions.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening modal for ADD");
            HasError = true;
            ErrorMessage = $"Eroare la deschiderea formularului: {ex.Message}";
        }
    }

    public async Task OpenForEdit(Guid personalId)
    {
        try
        {
            Logger.LogInformation("=== START OpenForEdit: PersonalId={PersonalId} ===", personalId);

            if (personalId == Guid.Empty)
            {
                Logger.LogError("=== CRITICAL: PersonalId is Guid.Empty! Cannot load data ===");
                HasError = true;
                ErrorMessage = "ID invalid pentru editare";
                IsVisible = true;
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            IsVisible = true;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "personal";

            await InvokeAsync(StateHasChanged);

            Logger.LogInformation("Sending GetPersonalByIdQuery for {PersonalId}", personalId);

            var query = new GetPersonalByIdQuery(personalId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var data = result.Value;

                Logger.LogInformation("=== Data loaded from backend: Id_Personal={Id} ===", data.Id_Personal);

                if (data.Id_Personal == Guid.Empty)
                {
                    Logger.LogError("=== CRITICAL: Loaded data has Guid.Empty as Id_Personal! ===");
                    HasError = true;
                    ErrorMessage = "Date invalide primite din backend";
                    IsLoading = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                Model = new PersonalFormModel
                {
                    Id_Personal = data.Id_Personal,
                    Cod_Angajat = data.Cod_Angajat,
                    Nume = data.Nume,
                    Prenume = data.Prenume,
                    Nume_Anterior = data.Nume_Anterior,
                    CNP = data.CNP,
                    Data_Nasterii = data.Data_Nasterii,
                    Locul_Nasterii = data.Locul_Nasterii,
                    Nationalitate = data.Nationalitate,
                    Cetatenie = data.Cetatenie,
                    Telefon_Personal = data.Telefon_Personal,
                    Telefon_Serviciu = data.Telefon_Serviciu,
                    Email_Personal = data.Email_Personal,
                    Email_Serviciu = data.Email_Serviciu,
                    Adresa_Domiciliu = data.Adresa_Domiciliu,
                    Judet_Domiciliu = data.Judet_Domiciliu,
                    Oras_Domiciliu = data.Oras_Domiciliu,
                    Cod_Postal_Domiciliu = data.Cod_Postal_Domiciliu,
                    Adresa_Resedinta = data.Adresa_Resedinta,
                    Judet_Resedinta = data.Judet_Resedinta,
                    Oras_Resedinta = data.Oras_Resedinta,
                    Cod_Postal_Resedinta = data.Cod_Postal_Resedinta,
                    Stare_Civila = data.Stare_Civila,
                    Functia = data.Functia,
                    Departament = data.Departament,
                    Serie_CI = data.Serie_CI,
                    Numar_CI = data.Numar_CI,
                    Eliberat_CI_De = data.Eliberat_CI_De,
                    Data_Eliberare_CI = data.Data_Eliberare_CI,
                    Valabil_CI_Pana = data.Valabil_CI_Pana,
                    Status_Angajat = data.Status_Angajat,
                    Observatii = data.Observatii
                };

                Logger.LogInformation("=== Model created: Id_Personal={Id}, IsEditMode={IsEditMode} ===",
                    Model.Id_Personal, Model.IsEditMode);

                if (!string.IsNullOrEmpty(data.Judet_Domiciliu))
                {
                    var judet = JudeteOptions.FirstOrDefault(j => j.Nume == data.Judet_Domiciliu);
                    if (judet != null)
                    {
                        await LoadLocalitatiDomiciliu(judet.Id);
                    }
                }

                if (!string.IsNullOrEmpty(data.Judet_Resedinta))
                {
                    var judet = JudeteOptions.FirstOrDefault(j => j.Nume == data.Judet_Resedinta);
                    if (judet != null)
                    {
                        await LoadLocalitatiResedinta(judet.Id);
                    }
                }

                Logger.LogInformation("=== SUCCESS: Data loaded for EDIT ===");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Errors?.FirstOrDefault() ?? "Nu s-au putut incarca datele personalului";
                Logger.LogWarning("Failed to load data for EDIT: {Error}", ErrorMessage);
            }

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR in OpenForEdit ===");
            HasError = true;
            ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task Close()
    {
        if (_disposed) return;

        Logger.LogInformation("Closing modal");

        // Start CSS animation
        IsVisible = false;
        await InvokeAsync(StateHasChanged);

        if (OnClosed.HasDelegate)
        {
            await OnClosed.InvokeAsync();
        }

        // Wait for CSS animation and component cleanup
        Logger.LogDebug("⏳ Modal cleanup delay (500ms)...");
        await Task.Delay(500);

        if (!_disposed)
        {
            // Clear all data after delay
            Model = new PersonalFormModel();
            LocalitatiDomiciliuOptions.Clear();
            LocalitatiResedintaOptions.Clear();
            IsLoadingLocalitatiDomiciliu = false;
            IsLoadingLocalitatiResedinta = false;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            ActiveTab = "personal";

            Logger.LogDebug("✅ Modal cleanup complete");
        }
    }

    private void SetActiveTab(string tabName)
    {
        ActiveTab = tabName;
        Logger.LogDebug("Tab changed to: {TabName}", tabName);
    }

    private async Task HandleOverlayClick()
    {
        // ❌ DEZACTIVAT: Nu închide modalul la click pe overlay pentru modalele Form
        // Pentru a evita pierderea datelor introduse/modificate
        // await Close();

        // Pentru modalele View (readonly) se poate păstra închiderea pe overlay
        return;
    }

    private async Task HandleSubmit()
    {
        try
        {
            Logger.LogInformation("=== START HandleSubmit: IsEditMode={IsEditMode}, Id_Personal={Id} ===",
                     Model.IsEditMode, Model.Id_Personal);

            IsSaving = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);

            if (Model.IsEditMode)
            {
                if (!Model.Id_Personal.HasValue || Model.Id_Personal.Value == Guid.Empty)
                {
                    Logger.LogError("=== CRITICAL: IsEditMode=true but Id_Personal is invalid! ===");
                    HasError = true;
                    ErrorMessage = "ID invalid pentru editare";
                    IsSaving = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                Logger.LogInformation("Updating personal with ID: {PersonalId}", Model.Id_Personal.Value);

                var command = new UpdatePersonalCommand
                {
                    Id_Personal = Model.Id_Personal.Value,
                    Cod_Angajat = Model.Cod_Angajat,
                    Nume = Model.Nume,
                    Prenume = Model.Prenume,
                    Nume_Anterior = Model.Nume_Anterior,
                    CNP = Model.CNP,
                    Data_Nasterii = Model.Data_Nasterii,
                    Locul_Nasterii = Model.Locul_Nasterii,
                    Nationalitate = Model.Nationalitate,
                    Cetatenie = Model.Cetatenie,
                    Telefon_Personal = Model.Telefon_Personal,
                    Telefon_Serviciu = Model.Telefon_Serviciu,
                    Email_Personal = Model.Email_Personal,
                    Email_Serviciu = Model.Email_Serviciu,
                    Adresa_Domiciliu = Model.Adresa_Domiciliu,
                    Judet_Domiciliu = Model.Judet_Domiciliu,
                    Oras_Domiciliu = Model.Oras_Domiciliu,
                    Cod_Postal_Domiciliu = Model.Cod_Postal_Domiciliu,
                    Adresa_Resedinta = Model.Adresa_Resedinta,
                    Judet_Resedinta = Model.Judet_Resedinta,
                    Oras_Resedinta = Model.Oras_Resedinta,
                    Cod_Postal_Resedinta = Model.Cod_Postal_Resedinta,
                    Stare_Civila = Model.Stare_Civila,
                    Functia = Model.Functia,
                    Departament = Model.Departament,
                    Serie_CI = Model.Serie_CI,
                    Numar_CI = Model.Numar_CI,
                    Eliberat_CI_De = Model.Eliberat_CI_De,
                    Data_Eliberare_CI = Model.Data_Eliberare_CI,
                    Valabil_CI_Pana = Model.Valabil_CI_Pana,
                    Status_Angajat = Model.Status_Angajat,
                    Observatii = Model.Observatii,
                    ModificatDe = "CurrentUser"
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("=== SUCCESS: Personal updated: {PersonalId} ===", Model.Id_Personal);

                    // Trigger parent event - parent will handle navigation/reload
                    if (OnPersonalSaved.HasDelegate)
                    {
                        await OnPersonalSaved.InvokeAsync();
                    }

                    // Close modal after parent processes the event
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la actualizare" });
                    Logger.LogWarning("Failed to update personal: {Error}", ErrorMessage);
                }
            }
            else
            {
                Logger.LogInformation("Creating new personal");

                var command = new CreatePersonalCommand
                {
                    Cod_Angajat = Model.Cod_Angajat,
                    Nume = Model.Nume,
                    Prenume = Model.Prenume,
                    Nume_Anterior = Model.Nume_Anterior,
                    CNP = Model.CNP,
                    Data_Nasterii = Model.Data_Nasterii,
                    Locul_Nasterii = Model.Locul_Nasterii,
                    Nationalitate = Model.Nationalitate,
                    Cetatenie = Model.Cetatenie,
                    Telefon_Personal = Model.Telefon_Personal,
                    Telefon_Serviciu = Model.Telefon_Serviciu,
                    Email_Personal = Model.Email_Personal,
                    Email_Serviciu = Model.Email_Serviciu,
                    Adresa_Domiciliu = Model.Adresa_Domiciliu,
                    Judet_Domiciliu = Model.Judet_Domiciliu,
                    Oras_Domiciliu = Model.Oras_Domiciliu,
                    Cod_Postal_Domiciliu = Model.Cod_Postal_Domiciliu,
                    Adresa_Resedinta = Model.Adresa_Resedinta,
                    Judet_Resedinta = Model.Judet_Resedinta,
                    Oras_Resedinta = Model.Oras_Resedinta,
                    Cod_Postal_Resedinta = Model.Cod_Postal_Resedinta,
                    Stare_Civila = Model.Stare_Civila,
                    Functia = Model.Functia,
                    Departament = Model.Departament,
                    Serie_CI = Model.Serie_CI,
                    Numar_CI = Model.Numar_CI,
                    Eliberat_CI_De = Model.Eliberat_CI_De,
                    Data_Eliberare_CI = Model.Data_Eliberare_CI,
                    Valabil_CI_Pana = Model.Valabil_CI_Pana,
                    Status_Angajat = Model.Status_Angajat,
                    Observatii = Model.Observatii,
                    CreatDe = "CurrentUser"
                };

                var result = await Mediator.Send(command);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("=== SUCCESS: Personal created: {PersonalId} ===", result.Value);

                    // Trigger parent event - parent will handle navigation/reload
                    if (OnPersonalSaved.HasDelegate)
                    {
                        await OnPersonalSaved.InvokeAsync();
                    }

                    // Close modal after parent processes the event
                    await Close();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare la creare" });
                    Logger.LogWarning("Failed to create personal: {Error}", ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "=== ERROR in HandleSubmit ===");
            HasError = true;
            ErrorMessage = $"Eroare la salvare: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
