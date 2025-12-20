using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Services.IMC;
using ValyanClinic.Infrastructure.Services.DraftStorage;

namespace ValyanClinic.Components.Pages.Consultatii;

public partial class Consultatii : ComponentBase, IDisposable
{
    #region Dependency Injection
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IIMCCalculatorService IMCCalculator { get; set; } = default!;
    [Inject] private IDraftStorageService<CreateConsultatieCommand> DraftStorage { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Consultatii> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public Guid ProgramareIdParam { get; set; }
    [Parameter] public Guid PacientIdParam { get; set; }
    #endregion

    #region State Properties
    private CreateConsultatieCommand Model { get; set; } = new();
    private PacientDetailDto? Pacient { get; set; }
    
    // Tab Management
    private int ActiveTabIndex { get; set; } = 0;
    private int ProgressPercent => (int)(((ActiveTabIndex + 1) / 4.0) * 100);

    // Expandable Sections
    private bool AHCExpanded { get; set; } = false;
    private bool APPExpanded { get; set; } = false;
    private bool ExamenCardioExpanded { get; set; } = false;

    // IMC Calculator
    private decimal? GreutateKg { get; set; }
    private decimal? InaltimeCm { get; set; }
    private IMCResult? IMCResult { get; set; }

    // Character Counters
    private int MotivCharCount { get; set; } = 0;

    // Date helper pentru input type="date"
    private DateTime? DataUrmatoareiProgramariDate
    {
        get => DateTime.TryParse(Model.DataUrmatoareiProgramari, out var date) ? date : null;
        set => Model.DataUrmatoareiProgramari = value?.ToString("yyyy-MM-dd");
    }

    // Diagnostic Cards
    private List<DiagnosticCard> DiagnosticCards { get; set; } = new() { new DiagnosticCard { IsPrincipal = true } };

    // Medication Rows
    private List<MedicationRow> MedicationRows { get; set; } = new() { new MedicationRow() };

    // Allergy Alert
    private bool ShowAllergyAlert { get; set; } = false;
    private string AllergyMessage { get; set; } = string.Empty;

    // Timer
    private System.Timers.Timer? ConsultationTimer { get; set; }
    private int TimerSeconds { get; set; } = 0;
    private string TimerClass => TimerSeconds switch { < 1800 => "", < 3600 => "warning", _ => "danger" };

    // Saving State
    private bool IsSaving { get; set; } = false;
    private DateTime? DraftLastSaved { get; set; }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        Model.ProgramareID = ProgramareIdParam;
        Model.PacientID = PacientIdParam;

        await LoadPacientDataAsync();
        await LoadDraftAsync();
        StartConsultationTimer();
        await RegisterKeyboardShortcuts();
        await RegisterBeforeUnloadHandler();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Show allergy reminder when entering Tab 3 (medications)
            if (ActiveTabIndex == 2 && !string.IsNullOrEmpty(Pacient?.Alergii))
            {
                await Task.Delay(500);
                await ShowToastAsync("warning", "Reminder", $"Pacientul are alergie la {Pacient.Alergii}!");
            }
        }
    }

    public void Dispose()
    {
        ConsultationTimer?.Stop();
        ConsultationTimer?.Dispose();
        _ = UnregisterKeyboardShortcuts();
        _ = UnregisterBeforeUnloadHandler();
        _dotNetRef?.Dispose();
    }
    #endregion

    #region Data Loading
    private async Task LoadPacientDataAsync()
    {
        try
        {
            var query = new GetPacientByIdQuery(PacientIdParam);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                Pacient = result.Value;
                if (!string.IsNullOrEmpty(Pacient.Alergii)) Model.APP_Alergii = Pacient.Alergii;
            }
            else
            {
                await ShowToastAsync("error", "Eroare", "Datele pacientului nu au putut fi încărcate.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading patient data");
            await ShowToastAsync("error", "Eroare", $"Eroare: {ex.Message}");
        }
    }

    private async Task LoadDraftAsync()
    {
        try
        {
            var draftResult = await DraftStorage.LoadDraftAsync(ProgramareIdParam);

            if (draftResult.IsSuccess && draftResult.Data != null)
            {
                Model = draftResult.Data;
                DraftLastSaved = draftResult.SavedAt;

                if (Model.Greutate.HasValue) GreutateKg = Model.Greutate.Value;
                if (Model.Inaltime.HasValue) InaltimeCm = Model.Inaltime.Value;

                await ShowToastAsync("success", "Draft Încărcat", "Draft-ul anterior a fost restaurat.");
            }
        }
        catch (Exception) { /* Silent fail */ }
    }
    #endregion

    #region Tab Management
    private async Task OnTabClick(int index)
    {
        if (index < 0 || index >= 4) return;

        ActiveTabIndex = index;
        
        // Show allergy reminder when entering Tab 3 (medications)
        if (index == 2 && !string.IsNullOrEmpty(Pacient?.Alergii))
        {
            await Task.Delay(500);
            await ShowToastAsync("warning", "Reminder", $"Pacientul are alergie la {Pacient.Alergii}!");
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task GoToPreviousTab()
    {
        if (ActiveTabIndex > 0)
        {
            await OnTabClick(ActiveTabIndex - 1);
        }
    }

    private async Task GoToNextTab()
    {
        if (ActiveTabIndex < 3)
        {
            await OnTabClick(ActiveTabIndex + 1);
        }
        else
        {
            // Last tab - finalize consultation
            await SaveConsultation();
        }
    }
    #endregion

    #region IMC Calculator
    private async Task OnGreutateChanged(ChangeEventArgs e)
    {
        if (decimal.TryParse(e.Value?.ToString(), out var greutate))
        {
            GreutateKg = greutate;
            Model.Greutate = greutate;
        }
        else
        {
            GreutateKg = null;
            Model.Greutate = null;
        }

        CalculateIMC();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnInaltimeChanged(ChangeEventArgs e)
    {
        if (decimal.TryParse(e.Value?.ToString(), out var inaltime))
        {
            InaltimeCm = inaltime;
            Model.Inaltime = inaltime;
        }
        else
        {
            InaltimeCm = null;
            Model.Inaltime = null;
        }

        CalculateIMC();
        await InvokeAsync(StateHasChanged);
    }

    private void CalculateIMC()
    {
        if (GreutateKg.HasValue && InaltimeCm.HasValue && IMCCalculator.AreValuesValid(GreutateKg.Value, InaltimeCm.Value))
        {
            IMCResult = IMCCalculator.Calculate(GreutateKg.Value, InaltimeCm.Value);
        }
        else
        {
            IMCResult = null;
        }
    }
    #endregion

    #region Character Counter
    private void UpdateCharCount(string field, string? value)
    {
        if (field == "motiv") MotivCharCount = value?.Length ?? 0;
        StateHasChanged();
    }

    private string GetCharCounterClass(int count, int maxLength)
    {
        var percentage = (double)count / maxLength * 100;
        return percentage switch { >= 100 => "danger", >= 90 => "warning", _ => "" };
    }
    #endregion

    #region Diagnostic Management
    private async Task AddDiagnostic()
    {
        DiagnosticCards.Add(new DiagnosticCard { IsPrincipal = false });
        await ShowToastAsync("success", "Adăugat", "Diagnostic nou adăugat.");
        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveDiagnostic(int index)
    {
        // Check if it's the principal diagnosis
        if (DiagnosticCards[index].IsPrincipal)
        {
            await ShowToastAsync("warning", "Atenție", "Nu puteți șterge diagnosticul principal.");
            return;
        }

        if (DiagnosticCards.Count > 1)
        {
            DiagnosticCards.RemoveAt(index);
            if (!DiagnosticCards.Any(d => d.IsPrincipal) && DiagnosticCards.Count > 0)
            {
                DiagnosticCards[0].IsPrincipal = true;
            }
            await ShowToastAsync("success", "Șters", "Diagnostic eliminat.");
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnDiagnosticPrincipalChanged(int index)
    {
        for (int i = 0; i < DiagnosticCards.Count; i++)
        {
            if (i != index) DiagnosticCards[i].IsPrincipal = false;
        }
        StateHasChanged();
    }

    private string GetPrincipalDiagnostic()
    {
        var principal = DiagnosticCards.FirstOrDefault(d => d.IsPrincipal);
        return !string.IsNullOrEmpty(principal?.Denumire) ? principal.Denumire : "—";
    }
    #endregion

    #region Medication Management
    private async Task AddMedication()
    {
        MedicationRows.Add(new MedicationRow());
        await ShowToastAsync("success", "Adăugat", "Medicament nou adăugat.");
        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveMedication(int index)
    {
        if (MedicationRows.Count > 1)
        {
            MedicationRows.RemoveAt(index);
            await ShowToastAsync("success", "Șters", "Medicament eliminat.");
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CheckAllergyMatch(int index, string? medicationName)
    {
        if (string.IsNullOrWhiteSpace(medicationName) || string.IsNullOrWhiteSpace(Pacient?.Alergii))
        {
            ShowAllergyAlert = false;
            return;
        }

        var alergii = Pacient.Alergii.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(a => a.Trim().ToLowerInvariant());

        var medicationLower = medicationName.ToLowerInvariant();
        var hasMatch = alergii.Any(alergie => medicationLower.Contains(alergie) || alergie.Contains(medicationLower));

        if (hasMatch)
        {
            ShowAllergyAlert = true;
            AllergyMessage = $"Medicamentul '{medicationName}' poate conține substanțe la care pacientul este alergic!";
            _ = ShowToastAsync("warning", "ALERTĂ ALERGIE", AllergyMessage);
        }

        StateHasChanged();
    }

    private string GetAllergyMatchClass(string? medicationName)
    {
        if (string.IsNullOrWhiteSpace(medicationName) || string.IsNullOrWhiteSpace(Pacient?.Alergii)) return "";

        var alergii = Pacient.Alergii.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(a => a.Trim().ToLowerInvariant());
        var medicationLower = medicationName.ToLowerInvariant();

        return alergii.Any(alergie => medicationLower.Contains(alergie) || alergie.Contains(medicationLower)) 
            ? "allergy-match" 
            : "";
    }
    #endregion

    #region Timer
    private void StartConsultationTimer()
    {
        ConsultationTimer = new System.Timers.Timer(1000);
        ConsultationTimer.Elapsed += OnTimerElapsed;
        ConsultationTimer.AutoReset = true;
        ConsultationTimer.Start();
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        TimerSeconds++;
        await InvokeAsync(StateHasChanged);
    }

    private string FormatTimer(int seconds)
    {
        var hours = seconds / 3600;
        var minutes = (seconds % 3600) / 60;
        var secs = seconds % 60;
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }
    #endregion

    #region Save Operations
    private async Task SaveDraft()
    {
        try
        {
            IsSaving = true;
            await InvokeAsync(StateHasChanged);

            BuildModelFromUI();
            await DraftStorage.SaveDraftAsync(ProgramareIdParam, Model, "current-user-id");
            DraftLastSaved = DateTime.Now;

            await ShowToastAsync("success", "Salvat", "Draft salvat cu succes.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving draft");
            await ShowToastAsync("error", "Eroare", $"Eroare la salvarea draft-ului: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task AutoSaveOnChange()
    {
        // Trigger autosave indicator update
        DraftLastSaved = DateTime.Now;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SaveConsultation()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            BuildModelFromUI();
            var result = await Mediator.Send(Model);

            if (result.IsSuccess)
            {
                await DraftStorage.ClearDraftAsync(ProgramareIdParam);
                await ShowToastAsync("success", "Consultație Salvată", "Consultația a fost finalizată cu succes!");
                await Task.Delay(2000);
                Navigation.NavigateTo("/consultatii");
            }
            else
            {
                await ShowToastAsync("error", "Eroare", result.FirstError ?? "Eroare la salvarea consultației.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving consultation");
            await ShowToastAsync("error", "Eroare", $"Eroare: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void BuildModelFromUI()
    {
        var principal = DiagnosticCards.FirstOrDefault(d => d.IsPrincipal);
        if (principal != null)
        {
            Model.CoduriICD10 = principal.CodICD10;
            Model.DiagnosticPozitiv = principal.Denumire;
        }

        var secundare = DiagnosticCards.Where(d => !d.IsPrincipal && !string.IsNullOrEmpty(d.CodICD10)).ToList();
        if (secundare.Any()) Model.CoduriICD10Secundare = string.Join(",", secundare.Select(d => d.CodICD10));

        var medications = MedicationRows.Where(m => !string.IsNullOrEmpty(m.Nume)).ToList();
        if (medications.Any())
        {
            Model.TratamentMedicamentos = string.Join("\n", medications.Select(m => 
                $"{m.Nume} - {m.Doza} - {m.Frecventa} - {m.Durata}"));
        }
    }
    #endregion

    #region Keyboard Shortcuts & Event Handlers
    private DotNetObjectReference<Consultatii>? _dotNetRef;

    private async Task RegisterKeyboardShortcuts()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            
            // Use global registerKeyboardShortcuts function from notifications.js
            await JSRuntime.InvokeVoidAsync("eval", @"
                window.consultatiiKeyboardHandler = function(e) {
                    if (e.ctrlKey || e.metaKey) {
                        if (e.key === 's' || e.key === 'S') {
                            e.preventDefault();
                            const saveDraftBtn = document.querySelector('.btn-outline:has(i.fa-save)');
                            if (saveDraftBtn && !saveDraftBtn.disabled) {
                                saveDraftBtn.click();
                            }
                        } else if (e.key === 'ArrowRight') {
                            e.preventDefault();
                            const nextBtn = document.querySelector('.btn-success:last-of-type, .btn:has(i.fa-arrow-right)');
                            if (nextBtn && !nextBtn.disabled) {
                                nextBtn.click();
                            }
                        } else if (e.key === 'ArrowLeft') {
                            e.preventDefault();
                            const prevBtn = document.querySelector('.btn:has(i.fa-arrow-left)');
                            if (prevBtn && !prevBtn.disabled) {
                                prevBtn.click();
                            }
                        }
                    }
                };
                document.addEventListener('keydown', window.consultatiiKeyboardHandler);
            ");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error registering keyboard shortcuts");
        }
    }

    private async Task UnregisterKeyboardShortcuts()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", @"
                if (window.consultatiiKeyboardHandler) {
                    document.removeEventListener('keydown', window.consultatiiKeyboardHandler);
                    delete window.consultatiiKeyboardHandler;
                }
            ");
        }
        catch { /* Silent fail on dispose */ }
    }

    private async Task RegisterBeforeUnloadHandler()
    {
        try
        {
            // Register beforeunload warning for unsaved changes
            await JSRuntime.InvokeVoidAsync("eval", @"
                window.consultatiiBeforeUnloadHandler = function(e) {
                    const hasContent = document.querySelector('textarea')?.value || 
                                      document.querySelector('input[type=""text""]')?.value;
                    if (hasContent) {
                        e.preventDefault();
                        e.returnValue = 'Aveți modificări nesalvate. Sigur doriți să părăsiți pagina?';
                        return e.returnValue;
                    }
                };
                window.addEventListener('beforeunload', window.consultatiiBeforeUnloadHandler);
            ");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error registering beforeunload handler");
        }
    }

    private async Task UnregisterBeforeUnloadHandler()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("eval", @"
                if (window.consultatiiBeforeUnloadHandler) {
                    window.removeEventListener('beforeunload', window.consultatiiBeforeUnloadHandler);
                    delete window.consultatiiBeforeUnloadHandler;
                }
            ");
        }
        catch { /* Silent fail on dispose */ }
    }
    #endregion

    #region Toast Notifications
    private async Task ShowToastAsync(string type, string title, string message)
    {
        try
        {
            // Call global JavaScript function showToast(message, type, duration)
            var fullMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
            await JSRuntime.InvokeVoidAsync("showToast", fullMessage, type, 3000);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to show toast notification: {Type} - {Message}", type, message);
        }
    }
    #endregion

    #region Helper Classes
    private class DiagnosticCard
    {
        public string CodICD10 { get; set; } = string.Empty;
        public string Denumire { get; set; } = string.Empty;
        public bool IsPrincipal { get; set; } = false;
    }

    private class MedicationRow
    {
        public string Nume { get; set; } = string.Empty;
        public string Doza { get; set; } = string.Empty;
        public string Frecventa { get; set; } = string.Empty;
        public string Durata { get; set; } = string.Empty;
    }
    #endregion
}
