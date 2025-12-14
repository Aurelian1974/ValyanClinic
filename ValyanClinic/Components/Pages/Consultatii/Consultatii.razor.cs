using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ValyanClinic.Services;
using MediatR;
using FluentValidation;
using System.Security.Claims;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Application.Services.IMC;
using ValyanClinic.Application.Services.Consultatii;

namespace ValyanClinic.Components.Pages.Consultatii;

public partial class Consultatii : ComponentBase, IAsyncDisposable
{
    #region Injected Services

    [Inject] private ILogger<Consultatii> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private INavigationGuardService NavigationGuard { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // ✅ REFACTORED: Folosim servicii în loc de logică locală
    [Inject] private IIMCCalculatorService IMCCalculator { get; set; } = default!;
    [Inject] private IConsultationTimerService TimerService { get; set; } = default!;
    [Inject] private IFormProgressService ProgressService { get; set; } = default!;

    #endregion

    #region Parameters

    [Parameter]
    [SupplyParameterFromQuery(Name = "pacientId")]
    public Guid? PacientId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "programareId")]
    public Guid? ProgramareId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "consultatieId")]
    public Guid? ConsultatieId { get; set; }

    #endregion

    #region User State

    private Guid CurrentUserId { get; set; } = Guid.Empty;
    private Guid CurrentMedicId { get; set; } = Guid.Empty;

    #endregion

    #region Page State

    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }
    private bool IsSaving { get; set; } = false;
    private DateTime? LastSaveTime { get; set; }
    private bool HasUnsavedChanges { get; set; } = false;
    private bool _disposed = false;

    #endregion

    #region JS Interop State

    private DotNetObjectReference<Consultatii>? _dotNetRef;
    private bool _jsInitialized = false;
    private const int AutoSaveIntervalMs = 30000;
    private DateTime? _lastAutoSaveTime;
    private bool _autoSaveEnabled = true;

    #endregion

    #region UI State

    private bool ShowShortcutsModal { get; set; } = false;
    private int ActiveTab { get; set; } = 1;

    #endregion

    #region Computed Properties

    private bool IsEditMode => ConsultatieId.HasValue;
    private bool IsNewConsultation => !ConsultatieId.HasValue;

    // ✅ REFACTORED: Timer properties delegate la serviciu
    private bool IsTimerRunning => TimerService.IsRunning;
    private bool IsTimerPaused => TimerService.IsPaused;
    private string FormattedTime => TimerService.FormattedTime;
    private string TimerWarningClass => TimerService.WarningClass;
    private TimeSpan ElapsedTime => TimerService.ElapsedTime;

    // ✅ REFACTORED: IMC folosește serviciul dedicat
    private IMCResult? IMCResult => (Greutate.HasValue && Inaltime.HasValue && Inaltime.Value > 0)
        ? IMCCalculator.Calculate(Greutate.Value, Inaltime.Value)
        : null;

    private decimal? IMC => IMCResult?.Value;
    private string IMCCategory => IMCResult?.ColorClass ?? string.Empty;
    private string IMCText => IMCResult?.Interpretation ?? string.Empty;

    // ✅ REFACTORED: Progress folosește serviciul dedicat
    private ConsultationProgressResult? _cachedProgress;
    private ConsultationProgressResult ProgressResult
    {
        get
        {
            _cachedProgress = ProgressService.CalculateConsultationProgress(new ConsultationProgressInput
            {
                MotivPrezentare = MotivPrezentare,
                AntecedentePatologice = AntecedentePatologice,
                TratamenteActuale = TratamenteActuale,
                TensiuneSistolica = TensiuneSistolica,
                TensiuneDiastolica = TensiuneDiastolica,
                Puls = Puls,
                Temperatura = Temperatura,
                FreqventaRespiratorie = FreqventaRespiratorie,
                Greutate = Greutate,
                Inaltime = Inaltime,
                ExamenObiectiv = ExamenObiectiv,
                DiagnosticPrincipal = DiagnosticPrincipal,
                PlanTerapeutic = PlanTerapeutic,
                Concluzii = Concluzii,
                DataUrmatoareiVizite = DataUrmatoareiVizite
            });
            return _cachedProgress;
        }
    }

    private int ProgressPercentage => ProgressResult.ProgressPercentage;

    private bool IsTabCompleted(int tabNumber) => tabNumber switch
    {
        1 => ProgressResult.IsTab1Complete,
        2 => ProgressResult.IsTab2Complete,
        3 => ProgressResult.IsTab3Complete,
        4 => ProgressResult.IsTab4Complete,
        _ => false
    };

    #endregion

    #region Patient Data

    private PacientDetailDto? PacientData { get; set; }
    private bool HasAllergies => !string.IsNullOrEmpty(PacientData?.Alergii);
    private string AllergiesText => PacientData?.Alergii ?? string.Empty;
    private bool HasConditions => !string.IsNullOrEmpty(PacientData?.Boli_Cronice);
    private string ConditionsText => PacientData?.Boli_Cronice ?? string.Empty;

    #endregion

    #region Form Fields - Tab 1: Motiv Prezentare & Antecedente

    private string MotivPrezentare { get; set; } = string.Empty;
    private string AntecedentePatologice { get; set; } = string.Empty;
    private string TratamenteActuale { get; set; } = string.Empty;
    private AntecedenteHeredoDto AntecedenteHeredo { get; set; } = new();

    #endregion

    #region Form Fields - Tab 2: Examen Clinic & Investigații

    private int? TensiuneSistolica { get; set; }
    private int? TensiuneDiastolica { get; set; }
    private int? Puls { get; set; }
    private decimal? Temperatura { get; set; }
    private int? FreqventaRespiratorie { get; set; }
    private decimal? Greutate { get; set; }
    private decimal? Inaltime { get; set; }
    private string ExamenObiectiv { get; set; } = string.Empty;
    private string InvestigatiiParaclinice { get; set; } = string.Empty;
    private string StareGenerala { get; set; } = string.Empty;
    private string Tegumente { get; set; } = string.Empty;
    private string Mucoase { get; set; } = string.Empty;
    private int? SpO2 { get; set; }
    private string Edeme { get; set; } = string.Empty;

    #endregion

    #region Form Fields - Tab 3: Diagnostic & Tratament

    private string DiagnosticPrincipal { get; set; } = string.Empty;
    private string DiagnosticSecundar { get; set; } = string.Empty;
    private string PlanTerapeutic { get; set; } = string.Empty;
    private string Recomandari { get; set; } = string.Empty;
    private List<DiagnosisCardDto> DiagnosisList { get; set; } = new();
    private List<MedicationRowDto> MedicationList { get; set; } = new();

    #endregion

    #region Form Fields - Tab 4: Concluzii

    private string Concluzii { get; set; } = string.Empty;
    private DateTime? DataUrmatoareiVizite { get; set; }
    private string NoteUrmatoareaVizita { get; set; } = string.Empty;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("🔵 Initializing Consultatii page - PacientId: {PacientId}", PacientId);

            if (!PacientId.HasValue)
            {
                HasError = true;
                ErrorMessage = "ID pacient lipsește. Vă rugăm selectați un pacient din listă.";
                IsLoading = false;
                return;
            }

            // ✅ Subscribe la timer events pentru UI refresh
            TimerService.OnTick += OnTimerTick;

            await LoadCurrentUserInfo();
            await LoadPacientDataViaMediatr();

            if (IsEditMode && ConsultatieId.HasValue)
            {
                await LoadExistingConsultatieViaMediatr();
            }
            else if (ProgramareId.HasValue)
            {
                await CheckProgramareConsultatieViaMediatr();
            }
            else
            {
                await CheckExistingDraftConsultatieAsync();
            }

            // ✅ REFACTORED: Start timer prin serviciu
            TimerService.Start();
            IsLoading = false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error initializing Consultatii page");
            HasError = true;
            ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            IsLoading = false;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NavigationGuard.EnableGuardAsync(
                hasUnsavedChangesFunc: async () => await Task.FromResult(HasUnsavedChanges),
                customMessage: "Aveți modificări nesalvate în consultație. Sigur doriți să părăsiți pagina?"
            );
            await InitializeKeyboardShortcuts();
        }
    }

    #endregion

    #region Data Loading

    private async Task LoadCurrentUserInfo()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var utilizatorIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(utilizatorIdClaim, out var utilizatorId))
                CurrentUserId = utilizatorId;

            var personalMedicalIdClaim = user.FindFirst("PersonalMedicalID")?.Value;
            if (Guid.TryParse(personalMedicalIdClaim, out var personalMedicalId))
                CurrentMedicId = personalMedicalId;
        }
    }

    private async Task LoadPacientDataViaMediatr()
    {
        var query = new GetPacientByIdQuery(PacientId!.Value);
        var result = await Mediator.Send(query);
        if (result.IsSuccess && result.Value != null)
            PacientData = result.Value;
        else
            throw new Exception("Pacient not found");
    }

    private async Task LoadExistingConsultatieViaMediatr()
    {
        var query = new ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieById.GetConsulatieByIdQuery(ConsultatieId!.Value);
        var result = await Mediator.Send(query);
        if (result.IsSuccess && result.Value != null)
            MapConsultatieToFormFields(result.Value);
    }

    private async Task CheckProgramareConsultatieViaMediatr()
    {
        var query = new ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieByProgramare.GetConsulatieByProgramareQuery(ProgramareId!.Value);
        var result = await Mediator.Send(query);
        if (result.IsSuccess && result.Value != null)
        {
            ConsultatieId = result.Value.ConsultatieID;
            MapConsultatieToFormFields(result.Value);
        }
        else
        {
            await CheckExistingDraftConsultatieAsync();
        }
    }

    private async Task CheckExistingDraftConsultatieAsync()
    {
        var query = new ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetDraftConsulatieByPacient.GetDraftConsulatieByPacientQuery(
            pacientId: PacientId!.Value,
            medicId: CurrentMedicId != Guid.Empty ? CurrentMedicId : null,
            dataConsultatie: DateTime.Today,
            programareId: ProgramareId
        );
        var result = await Mediator.Send(query);
        if (result.IsSuccess && result.Value != null)
        {
            ConsultatieId = result.Value.ConsultatieID;
            MapConsultatieToFormFields(result.Value);
            ToastService.ShowInfo("Informare", "S-a găsit o consultație nefinalizată. Datele au fost încărcate.");
        }
    }

    private void MapConsultatieToFormFields(ConsulatieDetailDto consultatie)
    {
        MotivPrezentare = consultatie.MotivPrezentare ?? string.Empty;
        AntecedentePatologice = consultatie.IstoricBoalaActuala ?? string.Empty;
        Greutate = consultatie.Greutate;
        Inaltime = consultatie.Inaltime;
        Temperatura = consultatie.Temperatura;
        Puls = consultatie.Puls;

        if (!string.IsNullOrEmpty(consultatie.TensiuneArteriala) && consultatie.TensiuneArteriala.Contains('/'))
        {
            var parts = consultatie.TensiuneArteriala.Split('/');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out var sistolica)) TensiuneSistolica = sistolica;
                if (int.TryParse(parts[1], out var diastolica)) TensiuneDiastolica = diastolica;
            }
        }

        DiagnosticPrincipal = consultatie.DiagnosticPozitiv ?? string.Empty;
        PlanTerapeutic = consultatie.TratamentMedicamentos ?? string.Empty;
        Concluzii = consultatie.ObservatiiMedic ?? string.Empty;
    }

    #endregion

    #region Timer Management - Delegated to Service

    private void ToggleTimerPause()
    {
        TimerService.TogglePause();

        if (TimerService.IsPaused)
        {
            _ = JSRuntime.InvokeVoidAsync("ConsultatiiAutoSave.pause");
            ToastService.ShowInfo("Timer", "Consultația este în pauză");
        }
        else
        {
            _ = JSRuntime.InvokeVoidAsync("ConsultatiiAutoSave.resume");
            ToastService.ShowInfo("Timer", "Consultația continuă");
        }
    }

    #endregion

    #region Tab Management

    private void SwitchTab(int tabNumber)
    {
        if (tabNumber < 1 || tabNumber > 4) return;
        ActiveTab = tabNumber;
    }

    #endregion

    #region Actions

    private async Task HandleSaveDraft()
    {
        try
        {
            IsSaving = true;
            var command = new ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft.SaveConsultatieDraftCommand
            {
                ConsultatieID = ConsultatieId,
                ProgramareID = ProgramareId,
                PacientID = PacientId!.Value,
                MedicID = CurrentMedicId,
                DataConsultatie = DateTime.Today,
                OraConsultatie = DateTime.Now.TimeOfDay,
                TipConsultatie = "Prima consultatie",
                MotivPrezentare = string.IsNullOrWhiteSpace(MotivPrezentare) ? null : MotivPrezentare,
                IstoricBoalaActuala = string.IsNullOrWhiteSpace(AntecedentePatologice) ? null : AntecedentePatologice,
                Greutate = Greutate,
                Inaltime = Inaltime,
                IMC = IMC,
                Temperatura = Temperatura,
                TensiuneArteriala = (TensiuneSistolica.HasValue && TensiuneDiastolica.HasValue) ? $"{TensiuneSistolica}/{TensiuneDiastolica}" : null,
                Puls = Puls,
                DiagnosticPozitiv = string.IsNullOrWhiteSpace(DiagnosticPrincipal) ? null : DiagnosticPrincipal,
                CoduriICD10 = DiagnosisList.Any() ? string.Join(", ", DiagnosisList.Select(d => d.Code)) : null,
                TratamentMedicamentos = string.IsNullOrWhiteSpace(PlanTerapeutic) ? null : PlanTerapeutic,
                ObservatiiMedic = string.IsNullOrWhiteSpace(Concluzii) ? null : Concluzii,
                CreatDeSauModificatDe = CurrentUserId
            };

            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                if (!ConsultatieId.HasValue) ConsultatieId = result.Value;
                LastSaveTime = DateTime.Now;
                HasUnsavedChanges = false;
                ToastService.ShowSuccess("Succes", $"Draft salvat la {LastSaveTime:HH:mm:ss}");
            }
            else
            {
                ToastService.ShowError("Eroare", string.Join(", ", result.Errors ?? new List<string>()));
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError("Eroare", ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleFinalize()
    {
        // ✅ REFACTORED: Folosim progress service pentru validare
        var progress = ProgressResult;
        if (progress.MissingRequiredFields.Any() || !ConsultatieId.HasValue)
        {
            var missingFieldsText = progress.MissingRequiredFields.Any()
                ? $"Câmpuri lipsă: {string.Join(", ", progress.MissingRequiredFields)}"
                : "Salvați mai întâi consultația.";
            ToastService.ShowError("Validare", $"Completați câmpurile obligatorii. {missingFieldsText}");
            return;
        }

        try
        {
            IsSaving = true;
            TimerService.Stop();

            var command = new ValyanClinic.Application.Features.ConsultatieManagement.Commands.FinalizeConsultatie.FinalizeConsulatieCommand
            {
                ConsultatieID = ConsultatieId!.Value,
                DurataMinute = TimerService.GetDurationMinutes(),
                ModificatDe = CurrentUserId
            };

            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                ToastService.ShowSuccess("Succes", "Consultație finalizată cu succes!");
                await NavigationGuard.DisableGuardAsync();
                await Task.Delay(1000);
                NavigationManager.NavigateTo("/pacienti/vizualizare");
            }
            else
            {
                ToastService.ShowError("Eroare", string.Join(", ", result.Errors ?? new List<string>()));
                TimerService.Start();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError("Eroare", ex.Message);
            TimerService.Start();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleGenerateLetter()
    {
        await Task.CompletedTask;
        Logger.LogInformation("Generate letter requested");
    }

    private void MarkFormAsDirty()
    {
        if (!HasUnsavedChanges)
        {
            HasUnsavedChanges = true;
        }
    }

    private void ShowKeyboardShortcuts() => ShowShortcutsModal = true;

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (_jsInitialized) return;
        if (e.CtrlKey && e.Key == "s") await HandleSaveDraft();
        else if (e.CtrlKey && e.Key == "Enter") await HandleFinalize();
    }

    #endregion

    #region Keyboard Shortcuts (JS Interop)

    private async Task InitializeKeyboardShortcuts()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("ConsultatiiKeyboard.initialize", _dotNetRef);
            await JSRuntime.InvokeVoidAsync("ConsultatiiAutoSave.initialize", _dotNetRef, AutoSaveIntervalMs);
            _jsInitialized = true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to initialize keyboard shortcuts");
        }
    }

    [JSInvokable] public async Task OnKeyboardSaveDraft() => await HandleSaveDraft();
    [JSInvokable] public async Task OnKeyboardFinalize() => await HandleFinalize();
    [JSInvokable] public void OnKeyboardPreviousTab() { if (ActiveTab > 1) { SwitchTab(ActiveTab - 1); StateHasChanged(); } }
    [JSInvokable] public void OnKeyboardNextTab() { if (ActiveTab < 4) { SwitchTab(ActiveTab + 1); StateHasChanged(); } }
    [JSInvokable] public void OnKeyboardSwitchTab(int tab) { SwitchTab(tab); StateHasChanged(); }
    [JSInvokable] public void OnKeyboardToggleTimer() { ToggleTimerPause(); StateHasChanged(); }

    [JSInvokable]
    public async Task OnAutoSaveTick()
    {
        if (!_autoSaveEnabled || !HasUnsavedChanges || IsSaving || TimerService.IsPaused) return;
        await HandleSaveDraft();
        _lastAutoSaveTime = DateTime.Now;
    }

    [JSInvokable]
    public async Task OnDebouncedSave()
    {
        if (!_autoSaveEnabled || !HasUnsavedChanges || IsSaving) return;
        await HandleSaveDraft();
    }

    #endregion

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // ✅ Unsubscribe from timer events
        TimerService.OnTick -= OnTimerTick;

        // ✅ Dispose timer service
        await TimerService.DisposeAsync();

        if (_jsInitialized)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("ConsultatiiKeyboard.dispose");
                await JSRuntime.InvokeVoidAsync("ConsultatiiAutoSave.dispose");
            }
            catch (JSDisconnectedException) { }
        }
        _dotNetRef?.Dispose();
        try { await NavigationGuard.DisableGuardAsync(); } catch { }
    }

    #endregion
}
