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
using ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;
using ValyanClinic.Application.Features.ICD10Management.Queries.GetFavorites;
using ValyanClinic.Application.Features.ICD10Management.Commands.AddFavorite;
using ValyanClinic.Application.Features.ICD10Management.Commands.RemoveFavorite;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

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
    private string CoduriICD10Principal { get; set; } = string.Empty;
    private string CoduriICD10Secundare { get; set; } = string.Empty;
    private string PlanTerapeutic { get; set; } = string.Empty;
    private string Recomandari { get; set; } = string.Empty;
    private List<DiagnosisCardDto> DiagnosisList { get; set; } = new();
    private List<MedicationRowDto> MedicationList { get; set; } = new();
    
    // Secțiuni Diagnostic Secundar (max 10)
    private List<DiagnosticSecundarSection> DiagnosticeSecundare { get; set; } = new();
    private int? ActiveSecundarSearchIndex { get; set; } = null;

    // ICD-10 Inline Search State
    private string SearchTermPrincipal { get; set; } = string.Empty;
    private string SearchTermSecundar { get; set; } = string.Empty;
    private bool IsSearchingPrincipal { get; set; } = false;
    private bool IsSearchingSecundar { get; set; } = false;
    private List<ICD10SearchResultDto> SearchResultsPrincipal { get; set; } = new();
    private List<ICD10SearchResultDto> SearchResultsSecundar { get; set; } = new();
    private System.Timers.Timer? _searchTimerPrincipal;
    private System.Timers.Timer? _searchTimerSecundar;
    
    // ICD-10 Favorites State
    private bool ShowFavoritesPrincipal { get; set; } = false;
    private bool ShowFavoritesSecundar { get; set; } = false;
    private List<ICD10SearchResultDto> FavoritesList { get; set; } = new();
    private bool IsLoadingFavorites { get; set; } = false;
    private HashSet<string> FavoriteCodesSet { get; set; } = new();

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

    #region ICD-10 Inline Search Methods

    private async Task OnSearchPrincipalAsync()
    {
        _searchTimerPrincipal?.Stop();
        _searchTimerPrincipal?.Dispose();

        if (string.IsNullOrWhiteSpace(SearchTermPrincipal) || SearchTermPrincipal.Length < 2)
        {
            SearchResultsPrincipal.Clear();
            StateHasChanged();
            return;
        }

        _searchTimerPrincipal = new System.Timers.Timer(300);
        _searchTimerPrincipal.Elapsed += async (s, e) =>
        {
            _searchTimerPrincipal?.Stop();
            await ExecuteSearchPrincipalAsync();
        };
        _searchTimerPrincipal.AutoReset = false;
        _searchTimerPrincipal.Start();
    }

    private async Task ExecuteSearchPrincipalAsync()
    {
        await InvokeAsync(async () =>
        {
            try
            {
                IsSearchingPrincipal = true;
                StateHasChanged();

                var query = new SearchICD10Query(SearchTermPrincipal, null, false, true, 10);
                var result = await Mediator.Send(query);

                if (result.IsSuccess)
                {
                    SearchResultsPrincipal = result.Value?.ToList() ?? new();
                }
                else
                {
                    SearchResultsPrincipal.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Consultatii] Error searching ICD-10 Principal");
                SearchResultsPrincipal.Clear();
            }
            finally
            {
                IsSearchingPrincipal = false;
                StateHasChanged();
            }
        });
    }

    private async Task OnSearchSecundarAsync()
    {
        _searchTimerSecundar?.Stop();
        _searchTimerSecundar?.Dispose();

        if (string.IsNullOrWhiteSpace(SearchTermSecundar) || SearchTermSecundar.Length < 2)
        {
            SearchResultsSecundar.Clear();
            StateHasChanged();
            return;
        }

        _searchTimerSecundar = new System.Timers.Timer(300);
        _searchTimerSecundar.Elapsed += async (s, e) =>
        {
            _searchTimerSecundar?.Stop();
            await ExecuteSearchSecundarAsync();
        };
        _searchTimerSecundar.AutoReset = false;
        _searchTimerSecundar.Start();
    }

    private async Task ExecuteSearchSecundarAsync()
    {
        await InvokeAsync(async () =>
        {
            try
            {
                IsSearchingSecundar = true;
                StateHasChanged();

                var query = new SearchICD10Query(SearchTermSecundar, null, false, true, 10);
                var result = await Mediator.Send(query);

                if (result.IsSuccess)
                {
                    SearchResultsSecundar = result.Value?.ToList() ?? new();
                }
                else
                {
                    SearchResultsSecundar.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Consultatii] Error searching ICD-10 Secundar");
                SearchResultsSecundar.Clear();
            }
            finally
            {
                IsSearchingSecundar = false;
                StateHasChanged();
            }
        });
    }

    private void SelectICD10Principal(ICD10SearchResultDto item)
    {
        CoduriICD10Principal = $"{item.Code}|{item.ShortDescription}";
        SearchTermPrincipal = string.Empty;
        SearchResultsPrincipal.Clear();
        MarkFormAsDirty();
        StateHasChanged();
        Logger.LogInformation("[Consultatii] Selected ICD-10 Principal: {Code}", item.Code);
    }

    private void ClearICD10Principal()
    {
        CoduriICD10Principal = string.Empty;
        MarkFormAsDirty();
        StateHasChanged();
    }

    private void AddICD10Secundar(ICD10SearchResultDto item)
    {
        // Limită maximă de 10 diagnostice secundare
        var currentCodes = GetSecundareCodes();
        if (currentCodes.Count >= 10)
        {
            ToastService.ShowWarning("Limită atinsă", "Puteți adăuga maxim 10 diagnostice secundare.");
            return;
        }
        
        var newCode = $"{item.Code}|{item.ShortDescription}";
        
        if (string.IsNullOrEmpty(CoduriICD10Secundare))
        {
            CoduriICD10Secundare = newCode;
        }
        else if (!CoduriICD10Secundare.Contains(item.Code))
        {
            CoduriICD10Secundare += ";" + newCode;
        }
        
        SearchTermSecundar = string.Empty;
        SearchResultsSecundar.Clear();
        MarkFormAsDirty();
        StateHasChanged();
        Logger.LogInformation("[Consultatii] Added ICD-10 Secundar: {Code} ({Count}/10)", item.Code, currentCodes.Count + 1);
    }

    private void RemoveICD10Secundar(string codeToRemove)
    {
        var codes = GetSecundareCodes().Where(c => c != codeToRemove).ToList();
        CoduriICD10Secundare = string.Join(";", codes);
        MarkFormAsDirty();
        StateHasChanged();
    }

    private List<string> GetSecundareCodes()
    {
        if (string.IsNullOrEmpty(CoduriICD10Secundare))
            return new List<string>();
        
        return CoduriICD10Secundare.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private string GetICD10Code(string codeWithDescription)
    {
        if (string.IsNullOrEmpty(codeWithDescription)) return string.Empty;
        var parts = codeWithDescription.Split('|');
        return parts.Length > 0 ? parts[0] : codeWithDescription;
    }

    private string GetICD10Description(string codeWithDescription)
    {
        if (string.IsNullOrEmpty(codeWithDescription)) return string.Empty;
        var parts = codeWithDescription.Split('|');
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    #endregion

    #region ICD-10 Favorites Methods

    private async Task ToggleFavoritesPrincipalAsync()
    {
        ShowFavoritesPrincipal = !ShowFavoritesPrincipal;
        if (ShowFavoritesPrincipal && !FavoritesList.Any())
        {
            await LoadFavoritesAsync();
        }
        StateHasChanged();
    }

    private async Task ToggleFavoritesSecundarAsync()
    {
        ShowFavoritesSecundar = !ShowFavoritesSecundar;
        if (ShowFavoritesSecundar && !FavoritesList.Any())
        {
            await LoadFavoritesAsync();
        }
        StateHasChanged();
    }

    private async Task LoadFavoritesAsync()
    {
        if (CurrentMedicId == Guid.Empty) return;
        
        try
        {
            IsLoadingFavorites = true;
            StateHasChanged();

            var query = new GetICD10FavoritesQuery(CurrentMedicId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                FavoritesList = result.Value.ToList();
                FavoriteCodesSet = FavoritesList.Select(f => f.Code).ToHashSet();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Consultatii] Error loading ICD-10 favorites");
        }
        finally
        {
            IsLoadingFavorites = false;
            StateHasChanged();
        }
    }

    private async Task AddToFavoritesAsync(ICD10SearchResultDto item)
    {
        if (CurrentMedicId == Guid.Empty) return;
        if (FavoriteCodesSet.Contains(item.Code)) return;

        try
        {
            var command = new AddICD10FavoriteCommand(CurrentMedicId, item.ICD10_ID);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                // Add to local list - create new DTO with IsFavorite = true
                FavoritesList.Add(new ICD10SearchResultDto
                {
                    ICD10_ID = item.ICD10_ID,
                    Code = item.Code,
                    ShortDescription = item.ShortDescription,
                    LongDescription = item.LongDescription,
                    Category = item.Category,
                    IsCommon = item.IsCommon,
                    IsFavorite = true
                });
                FavoriteCodesSet.Add(item.Code);
                ToastService.ShowSuccess("Favorite", $"Cod {item.Code} adăugat la favorite!");
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Consultatii] Error adding ICD-10 to favorites: {Code}", item.Code);
            ToastService.ShowError("Eroare", "Eroare la adăugarea în favorite");
        }
    }

    private async Task RemoveFromFavoritesAsync(ICD10SearchResultDto favorite)
    {
        if (CurrentMedicId == Guid.Empty) return;

        try
        {
            var command = new RemoveICD10FavoriteCommand(CurrentMedicId, favorite.ICD10_ID);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                FavoritesList.RemoveAll(f => f.ICD10_ID == favorite.ICD10_ID);
                FavoriteCodesSet.Remove(favorite.Code);
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Consultatii] Error removing ICD-10 from favorites: {Code}", favorite.Code);
            ToastService.ShowError("Eroare", "Eroare la ștergerea din favorite");
        }
    }

    private void SelectFavoriteAsPrincipal(ICD10SearchResultDto favorite)
    {
        CoduriICD10Principal = $"{favorite.Code}|{favorite.ShortDescription}";
        ShowFavoritesPrincipal = false;
        MarkFormAsDirty();
        StateHasChanged();
        Logger.LogInformation("[Consultatii] Selected favorite as Principal: {Code}", favorite.Code);
    }

    private void SelectFavoriteAsSecundar(ICD10SearchResultDto favorite)
    {
        // Limită maximă de 10 diagnostice secundare
        var currentCodes = GetSecundareCodes();
        if (currentCodes.Count >= 10)
        {
            ToastService.ShowWarning("Limită atinsă", "Puteți adăuga maxim 10 diagnostice secundare.");
            return;
        }
        
        var newCode = $"{favorite.Code}|{favorite.ShortDescription}";
        
        if (string.IsNullOrEmpty(CoduriICD10Secundare))
        {
            CoduriICD10Secundare = newCode;
        }
        else if (!CoduriICD10Secundare.Contains(favorite.Code))
        {
            CoduriICD10Secundare += ";" + newCode;
        }
        
        MarkFormAsDirty();
        StateHasChanged();
        Logger.LogInformation("[Consultatii] Selected favorite as Secundar: {Code} ({Count}/10)", favorite.Code, currentCodes.Count + 1);
    }

    private bool IsCodeInFavorites(string code)
    {
        return FavoriteCodesSet.Contains(code);
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

    #region Diagnostic Secundar Sections Methods

    private void AddDiagnosticSecundar()
    {
        if (DiagnosticeSecundare.Count >= 10)
        {
            ToastService.ShowWarning("Limită atinsă", "Puteți adăuga maxim 10 secțiuni de diagnostic secundar.");
            return;
        }
        
        DiagnosticeSecundare.Add(new DiagnosticSecundarSection
        {
            Index = DiagnosticeSecundare.Count + 1
        });
        MarkFormAsDirty();
        StateHasChanged();
    }

    private void RemoveDiagnosticSecundar(int index)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == index);
        if (section != null)
        {
            DiagnosticeSecundare.Remove(section);
            // Renumber remaining sections
            for (int i = 0; i < DiagnosticeSecundare.Count; i++)
            {
                DiagnosticeSecundare[i].Index = i + 1;
            }
            MarkFormAsDirty();
            StateHasChanged();
        }
    }

    private void SelectICD10ForSecundar(int sectionIndex, ICD10SearchResultDto item)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section != null)
        {
            section.CodICD10 = item.Code;
            section.NumeICD10 = item.ShortDescription;
            section.SearchTerm = string.Empty;
            section.SearchResults.Clear();
            ActiveSecundarSearchIndex = null;
            MarkFormAsDirty();
            StateHasChanged();
            Logger.LogInformation("[Consultatii] Selected ICD-10 for Secundar #{Index}: {Code}", sectionIndex, item.Code);
        }
    }

    private void ClearICD10ForSecundar(int sectionIndex)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section != null)
        {
            section.CodICD10 = string.Empty;
            section.NumeICD10 = string.Empty;
            MarkFormAsDirty();
            StateHasChanged();
        }
    }

    private void SelectFavoriteForSecundar(int sectionIndex, ICD10SearchResultDto favorite)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section != null)
        {
            section.CodICD10 = favorite.Code;
            section.NumeICD10 = favorite.ShortDescription;
            section.ShowFavorites = false;
            MarkFormAsDirty();
            StateHasChanged();
            Logger.LogInformation("[Consultatii] Selected favorite for Secundar #{Index}: {Code}", sectionIndex, favorite.Code);
        }
    }

    private async Task OnSearchSecundarSectionAsync(int sectionIndex)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section == null) return;

        section.SearchTimer?.Stop();
        section.SearchTimer?.Dispose();

        if (string.IsNullOrWhiteSpace(section.SearchTerm) || section.SearchTerm.Length < 2)
        {
            section.SearchResults.Clear();
            StateHasChanged();
            return;
        }

        ActiveSecundarSearchIndex = sectionIndex;
        section.SearchTimer = new System.Timers.Timer(300);
        section.SearchTimer.Elapsed += async (s, e) =>
        {
            section.SearchTimer?.Stop();
            await ExecuteSearchSecundarSectionAsync(sectionIndex);
        };
        section.SearchTimer.AutoReset = false;
        section.SearchTimer.Start();
    }

    private async Task ExecuteSearchSecundarSectionAsync(int sectionIndex)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section == null) return;

        await InvokeAsync(async () =>
        {
            try
            {
                section.IsSearching = true;
                StateHasChanged();

                var query = new SearchICD10Query(section.SearchTerm, null, false, true, 10);
                var result = await Mediator.Send(query);

                if (result.IsSuccess)
                {
                    section.SearchResults = result.Value?.ToList() ?? new();
                }
                else
                {
                    section.SearchResults.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Consultatii] Error searching ICD-10 for Secundar #{Index}", sectionIndex);
                section.SearchResults.Clear();
            }
            finally
            {
                section.IsSearching = false;
                StateHasChanged();
            }
        });
    }

    private async Task ToggleFavoritesForSecundarAsync(int sectionIndex)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section == null) return;

        section.ShowFavorites = !section.ShowFavorites;
        if (section.ShowFavorites && !FavoritesList.Any())
        {
            await LoadFavoritesAsync();
        }
        StateHasChanged();
    }

    private void UpdateSecundarDescriere(int sectionIndex, string value)
    {
        var section = DiagnosticeSecundare.FirstOrDefault(s => s.Index == sectionIndex);
        if (section != null)
        {
            section.Descriere = value;
            MarkFormAsDirty();
        }
    }

    #endregion
}

/// <summary>
/// Model pentru o secțiune de diagnostic secundar
/// </summary>
public class DiagnosticSecundarSection
{
    public int Index { get; set; }
    public string CodICD10 { get; set; } = string.Empty;
    public string NumeICD10 { get; set; } = string.Empty;
    public string Descriere { get; set; } = string.Empty;
    
    // Search state pentru această secțiune
    public string SearchTerm { get; set; } = string.Empty;
    public bool IsSearching { get; set; } = false;
    public List<ICD10SearchResultDto> SearchResults { get; set; } = new();
    public System.Timers.Timer? SearchTimer { get; set; }
    public bool ShowFavorites { get; set; } = false;
    
    public bool HasCode => !string.IsNullOrEmpty(CodICD10);
}
