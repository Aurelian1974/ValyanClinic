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
using ValyanClinic.Application.Features.AnalizeMedicale.Queries;
using ValyanClinic.Application.ViewModels;
using Syncfusion.Blazor.RichTextEditor;

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
    
    // ✅ Navigation service - context fără ID-uri în URL
    [Inject] private IConsultatieNavigationService ConsultatieNav { get; set; } = default!;

    #endregion

    #region State from Navigation Service

    // ID-urile vin din serviciul de navigare, nu din URL
    private Guid? PacientId => ConsultatieNav.PacientId;
    private Guid? ProgramareId => ConsultatieNav.ProgramareId;
    private Guid? ConsultatieId { get; set; } // Setat local după creare/încărcare

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
    
    // Analize medicale (OCR)
    private List<AnalizeMedicaleGroupDto>? AnalizeMedicaleGroups { get; set; }
    private bool IsLoadingAnalize { get; set; } = false;

    #endregion

    #region JS Interop State

    private DotNetObjectReference<Consultatii>? _dotNetRef;
    private bool _jsInitialized = false;
    private const int AutoSaveIntervalMs = 30000;
    private DateTime? _lastAutoSaveTime;
    private bool _autoSaveEnabled = true;

    #endregion

    #region UI State

    private const int TotalTabs = 6;
    private bool ShowShortcutsModal { get; set; } = false;
    private bool ShowScrisoarePreview { get; set; } = false;
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

    // ✅ REFACTORED: IMC folosește serviciul dedicat cu cache pentru a evita recalculare la fiecare render
    private IMCResult? _cachedIMCResult;
    private decimal? _lastGreutate;
    private decimal? _lastInaltime;
    
    private IMCResult? IMCResult
    {
        get
        {
            // Recalculează doar dacă valorile s-au schimbat
            if (Greutate != _lastGreutate || Inaltime != _lastInaltime)
            {
                _lastGreutate = Greutate;
                _lastInaltime = Inaltime;
                _cachedIMCResult = (Greutate.HasValue && Inaltime.HasValue && Inaltime.Value > 0)
                    ? IMCCalculator.Calculate(Greutate.Value, Inaltime.Value)
                    : null;
            }
            return _cachedIMCResult;
        }
    }

    private decimal? IMC => IMCResult?.Value;
    private string IMCCategory => IMCResult?.ColorClass ?? string.Empty;
    private string IMCText => IMCResult?.Interpretation ?? string.Empty;

    // ✅ REFACTORED: Progress folosește serviciul dedicat
    private ConsultationProgressResult? _cachedProgress;
    private ConsultationProgressResult ProgressResult
    {
        get
        {
            // Folosește fie noul sistem ICD-10, fie legacy DiagnosticPrincipal
            var diagnosticForProgress = !string.IsNullOrWhiteSpace(ConsultatieCommand.CodICD10Principal) 
                ? ConsultatieCommand.CodICD10Principal 
                : (!string.IsNullOrWhiteSpace(ConsultatieCommand.NumeDiagnosticPrincipal)
                    ? ConsultatieCommand.NumeDiagnosticPrincipal
                    : (!string.IsNullOrWhiteSpace(ConsultatieCommand.DiagnosticPozitiv)
                        ? ConsultatieCommand.DiagnosticPozitiv
                        : DiagnosticPrincipal));
            
            _cachedProgress = ProgressService.CalculateConsultationProgress(new ConsultationProgressInput
            {
                MotivPrezentare = MotivPrezentare,
                IstoricBoalaActuala = IstoricBoalaActuala,
                IstoricMedicalPersonal = IstoricMedicalPersonal,
                IstoricFamilial = IstoricFamilial,
                TensiuneSistolica = TensiuneSistolica,
                TensiuneDiastolica = TensiuneDiastolica,
                Puls = Puls,
                Temperatura = Temperatura,
                FreqventaRespiratorie = FreqventaRespiratorie,
                Greutate = Greutate,
                Inaltime = Inaltime,
                ExamenObiectiv = ExamenObiectiv,
                DiagnosticPrincipal = diagnosticForProgress,
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
        1 => ProgressResult.IsTab1Complete,  // Anamneză
        2 => ProgressResult.IsTab2Complete,  // Examen Clinic
        3 => true,                           // Analize Medicale (opțional)
        4 => ProgressResult.IsTab3Complete,  // Diagnostic & Tratament
        5 => ProgressResult.IsTab4Complete,  // Concluzii
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

    #region Form Fields - Tab 1: Anamneză

    private string MotivPrezentare { get; set; } = string.Empty;
    private string IstoricBoalaActuala { get; set; } = string.Empty;
    private string IstoricMedicalPersonal { get; set; } = string.Empty;
    private string IstoricFamilial { get; set; } = string.Empty;
    
    // Câmpuri noi conform Scrisoare Medicală (Anexa 43)
    private string TratamentAnterior { get; set; } = string.Empty;
    private string FactoriDeRisc { get; set; } = string.Empty;
    private string Alergii { get; set; } = string.Empty;

    /// <summary>
    /// Toolbar items pentru Rich Text Editor - compact pentru uz medical
    /// </summary>
    private List<ToolbarItemModel> RteToolbarItems { get; set; } = new List<ToolbarItemModel>
    {
        new ToolbarItemModel() { Command = ToolbarCommand.Bold },
        new ToolbarItemModel() { Command = ToolbarCommand.Italic },
        new ToolbarItemModel() { Command = ToolbarCommand.Underline },
        new ToolbarItemModel() { Command = ToolbarCommand.FontColor },
        new ToolbarItemModel() { Command = ToolbarCommand.BackgroundColor },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.FontSize },
        new ToolbarItemModel() { Command = ToolbarCommand.LowerCase },
        new ToolbarItemModel() { Command = ToolbarCommand.UpperCase },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.OrderedList },
        new ToolbarItemModel() { Command = ToolbarCommand.UnorderedList },
        new ToolbarItemModel() { Command = ToolbarCommand.Undo },
        new ToolbarItemModel() { Command = ToolbarCommand.Redo },
        new ToolbarItemModel() { Command = ToolbarCommand.ClearFormat }
    };

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
    private decimal? Glicemie { get; set; }
    private string Edeme { get; set; } = string.Empty;
    private string GanglioniLimfatici { get; set; } = string.Empty;
    private string AlteObservatiiClinice { get; set; } = string.Empty;

    #endregion

    #region Form Fields - Tab 3: Diagnostic & Tratament

    private string DiagnosticPrincipal { get; set; } = string.Empty;
    private string DiagnosticSecundar { get; set; } = string.Empty;
    private string PlanTerapeutic { get; set; } = string.Empty;
    private string Recomandari { get; set; } = string.Empty;
    private List<DiagnosisCardDto> DiagnosisList { get; set; } = new();
    private List<MedicationRowDto> MedicationList { get; set; } = new();

    // ✅ NEW: Command object pentru DiagnosticTab (ICD10 integration)
    private ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie.CreateConsultatieCommand ConsultatieCommand { get; set; } = new();
    private bool ShowValidationErrors { get; set; } = false;

    private void MarkTabAsCompleted(int tabNumber)
{
        // Tab completion logic
        StateHasChanged();
    }

    #endregion

    #region Form Fields - Tab 4: Concluzii

    private string Concluzii { get; set; } = string.Empty;
    private DateTime? DataUrmatoareiVizite { get; set; }
    private string NoteUrmatoareaVizita { get; set; } = string.Empty;

    // Câmpuri Scrisoare Medicală (Anexa 43)
    private bool EsteAfectiuneOncologica { get; set; } = false;
    private bool AreIndicatieInternare { get; set; } = false;
    private bool SaEliberatPrescriptie { get; set; } = false;
    private string SeriePrescriptie { get; set; } = string.Empty;
    private bool SaEliberatConcediuMedical { get; set; } = false;
    private string SerieConcediuMedical { get; set; } = string.Empty;
    private bool SaEliberatIngrijiriDomiciliu { get; set; } = false;
    private bool SaEliberatDispozitiveMedicale { get; set; } = false;
    private bool TransmiterePrinEmail { get; set; } = false;
    private string EmailTransmitere { get; set; } = string.Empty;

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

            // ✅ Timer UI is handled by isolated ConsultationTimer component
            // No subscription needed here - avoids full page re-render every second

            await LoadCurrentUserInfo();
            await LoadPacientDataViaMediatr();
            
            // ✅ Load analize medicale (OCR) în paralel - nu blochează UI
            _ = LoadAnalizeMedicaleAsync();

            // ✅ SIMPLIFIED: Toate consultațiile vin din programări (nu walk-in pentru moment)
            // Contextul vine din IConsultatieNavigationService (setat de Dashboard/ListaProgramari)
            if (!ConsultatieNav.HasValidContext)
            {
                Logger.LogWarning("❌ Navigation context invalid - ProgramareId or PacientId missing");
                HasError = true;
                ErrorMessage = "Context navigare invalid. Vă rugăm să accesați consultația din Dashboard sau Lista Programări.";
                IsLoading = false;
                return;
            }

            if (IsEditMode && ConsultatieId.HasValue)
            {
                // Editare consultație existentă
                await LoadExistingConsultatieViaMediatr();
            }
            else
            {
                // Consultație nouă sau draft din programare
                await CheckOrCreateConsultatieForProgramare();
            }

            // ✅ REFACTORED: Start timer prin serviciu
            TimerService.Start();
            IsLoading = false;
            
            // ✅ CRITICAL FIX: Log CurrentUserId for debugging
            Logger.LogInformation("✅ CurrentUserId loaded: {UserId}", CurrentUserId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error initializing Consultatii page");
            HasError = true;
            ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            IsLoading = false;
        }
    }

    // ✅ REMOVED: OnTimerTick - Timer UI now handled by isolated ConsultationTimer component
    // This prevents full page re-render every second, improving performance significantly

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

    /// <summary>
    /// Încarcă analizele medicale importate (OCR) pentru pacient
    /// </summary>
    private async Task LoadAnalizeMedicaleAsync()
    {
        if (!PacientId.HasValue) return;
        
        IsLoadingAnalize = true;
        
        try
        {
            var result = await Mediator.Send(new GetAnalizeMedicaleByPacientQuery(PacientId.Value));
            if (result.IsSuccess)
            {
                AnalizeMedicaleGroups = result.Value;
                Logger.LogInformation("[Consultatii] Loaded {Count} analize groups for PacientId: {PacientId}", 
                    AnalizeMedicaleGroups?.Count ?? 0, PacientId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[Consultatii] Failed to load analize medicale - continuing without them");
            AnalizeMedicaleGroups = new List<AnalizeMedicaleGroupDto>();
        }
        finally
        {
            IsLoadingAnalize = false;
        }
    }

    private async Task LoadExistingConsultatieViaMediatr()
    {
        var query = new ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieById.GetConsulatieByIdQuery(ConsultatieId!.Value);
        var result = await Mediator.Send(query);
        if (result.IsSuccess && result.Value != null)
            MapConsultatieToFormFields(result.Value);
    }

    /// <summary>
    /// SIMPLIFIED: Caută sau creează consultație pentru programare
    /// Toate consultațiile vin din programări - nu avem walk-in
    /// </summary>
    private async Task CheckOrCreateConsultatieForProgramare()
    {
        Logger.LogInformation("[Consultatii] CheckOrCreateConsultatieForProgramare - ProgramareId: {ProgramareId}", ProgramareId);
        
        // 1. Caută consultație existentă pentru această programare
        var query = new ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieByProgramare.GetConsulatieByProgramareQuery(ProgramareId!.Value);
        var result = await Mediator.Send(query);
        
        if (result.IsSuccess && result.Value != null)
        {
            // Consultație existentă găsită - încarcă datele
            Logger.LogInformation("[Consultatii] Found existing consultatie: {ConsultatieId} for ProgramareId: {ProgramareId}", 
                result.Value.ConsultatieID, ProgramareId);
            ConsultatieId = result.Value.ConsultatieID;
            MapConsultatieToFormFields(result.Value);
        }
        else
        {
            // Nu există consultație - CREĂM UNA NOUĂ IMEDIAT
            Logger.LogInformation("[Consultatii] No consultatie found for ProgramareId: {ProgramareId} - creating new one NOW", ProgramareId);
            await CreateNewConsultatieForProgramare();
        }
    }

    /// <summary>
    /// Creează o consultație nouă pentru programarea curentă
    /// Se apelează automat la intrarea în pagină dacă nu există deja o consultație
    /// </summary>
    private async Task CreateNewConsultatieForProgramare()
    {
        try
        {
            var command = new ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft.SaveConsultatieDraftCommand
            {
                ConsultatieID = null, // Nou
                ProgramareID = ProgramareId,
                PacientID = PacientId!.Value,
                MedicID = CurrentMedicId,
                DataConsultatie = DateTime.Today,
                OraConsultatie = DateTime.Now.TimeOfDay,
                TipConsultatie = "Prima consultatie",
                CreatDeSauModificatDe = CurrentUserId
            };

            var result = await Mediator.Send(command);
            
            if (result.IsSuccess)
            {
                ConsultatieId = result.Value;
                Logger.LogInformation("[Consultatii] ✅ Created new consultatie: {ConsultatieId} for ProgramareId: {ProgramareId}", 
                    ConsultatieId, ProgramareId);
            }
            else
            {
                Logger.LogError("[Consultatii] ❌ Failed to create consultatie for ProgramareId: {ProgramareId}. Errors: {Errors}", 
                    ProgramareId, string.Join(", ", result.Errors ?? new List<string>()));
                ErrorMessage = "Nu s-a putut crea consultația. Încercați din nou.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Consultatii] ❌ Exception creating consultatie for ProgramareId: {ProgramareId}", ProgramareId);
            ErrorMessage = $"Eroare la crearea consultației: {ex.Message}";
        }
    }

    private void MapConsultatieToFormFields(ConsulatieDetailDto consultatie)
    {
        // Tab 1: Anamneză
        MotivPrezentare = consultatie.MotivPrezentare ?? string.Empty;
        IstoricBoalaActuala = consultatie.IstoricBoalaActuala ?? string.Empty;
        IstoricMedicalPersonal = consultatie.IstoricMedicalPersonal ?? string.Empty;
        IstoricFamilial = consultatie.IstoricFamilial ?? string.Empty;
        
        // Tab 1: Anamneză - Câmpuri noi Anexa 43 (din ConsultatieAntecedente)
        TratamentAnterior = consultatie.TratamentAnterior ?? string.Empty;
        FactoriDeRisc = consultatie.FactoriDeRisc ?? string.Empty;
        Alergii = consultatie.Alergii ?? consultatie.PacientAlergii ?? string.Empty;

        // Tab 2: Semne Vitale
        Greutate = consultatie.Greutate;
        Inaltime = consultatie.Inaltime;
        Temperatura = consultatie.Temperatura;
        Puls = consultatie.Puls;
        FreqventaRespiratorie = consultatie.FreccventaRespiratorie;
        SpO2 = consultatie.SaturatieO2;

        if (!string.IsNullOrEmpty(consultatie.TensiuneArteriala) && consultatie.TensiuneArteriala.Contains('/'))
        {
            var parts = consultatie.TensiuneArteriala.Split('/');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out var sistolica)) TensiuneSistolica = sistolica;
                if (int.TryParse(parts[1], out var diastolica)) TensiuneDiastolica = diastolica;
            }
        }

        // Tab 2: Examen General
        StareGenerala = consultatie.StareGenerala ?? string.Empty;
        Tegumente = consultatie.Tegumente ?? string.Empty;
        Mucoase = consultatie.Mucoase ?? string.Empty;
        Edeme = consultatie.Edeme ?? string.Empty;
        Glicemie = consultatie.Glicemie;
        GanglioniLimfatici = consultatie.GanglioniLimfatici ?? string.Empty;
        ExamenObiectiv = consultatie.ExamenObiectivDetaliat ?? string.Empty;
        AlteObservatiiClinice = consultatie.AlteObservatiiClinice ?? string.Empty;
        
        // DEBUG: Log ExamenObiectiv values
        Logger.LogInformation("[Consultatii] LoadConsultatieData - ExamenObiectivDetaliat from DTO: '{Value}'", 
            consultatie.ExamenObiectivDetaliat ?? "NULL");
        Logger.LogInformation("[Consultatii] LoadConsultatieData - AlteObservatiiClinice from DTO: '{Value}'", 
            consultatie.AlteObservatiiClinice ?? "NULL");
        Logger.LogInformation("[Consultatii] LoadConsultatieData - Assigned ExamenObiectiv: '{Value}'", ExamenObiectiv);

        // Tab 2: Investigații
        InvestigatiiParaclinice = consultatie.InvestigatiiLaborator ?? string.Empty;
        ConsultatieCommand.AlteInvestigatii = consultatie.AlteInvestigatii;

        // Tab 3: Diagnostic - NEW normalized structure
        // These are mapped to ConsultatieCommand which is used by DiagnosticTab
        ConsultatieCommand.CodICD10Principal = consultatie.CodICD10Principal;
        ConsultatieCommand.NumeDiagnosticPrincipal = consultatie.NumeDiagnosticPrincipal;
        ConsultatieCommand.DescriereDetaliataPrincipal = consultatie.DescriereDetaliataPrincipal;
        
        // Map secondary diagnoses from DTO to Command
        Logger.LogInformation("[Consultatii] LoadConsultatieData - consultatie.DiagnosticeSecundare count: {Count}",
            consultatie.DiagnosticeSecundare?.Count ?? 0);
        
        if (consultatie.DiagnosticeSecundare?.Any() == true)
        {
            ConsultatieCommand.DiagnosticeSecundare = consultatie.DiagnosticeSecundare
                .Select(d => new ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft.DiagnosticSecundarDto
                {
                    // Preserve Id for MERGE logic (existing diagnostics keep their DataCreare/CreatDe)
                    Id = d.Id,
                    OrdineAfisare = d.OrdineAfisare,
                    CodICD10 = d.CodICD10,
                    NumeDiagnostic = d.NumeDiagnostic,
                    Descriere = d.Descriere
                })
                .ToList();
                
            Logger.LogInformation("[Consultatii] Mapped {Count} secondary diagnoses to ConsultatieCommand",
                ConsultatieCommand.DiagnosticeSecundare.Count);
        }
        
        // LEGACY fields for display
        DiagnosticPrincipal = consultatie.DiagnosticPozitiv ?? string.Empty;
        DiagnosticSecundar = string.Empty; // Legacy field removed
        
        // Also populate legacy fields in command for backwards compatibility
        ConsultatieCommand.CoduriICD10 = consultatie.CoduriICD10;
        ConsultatieCommand.DiagnosticPozitiv = consultatie.DiagnosticPozitiv;
        
        // Tab 3: Tratament
        PlanTerapeutic = consultatie.TratamentMedicamentos ?? string.Empty;
        Recomandari = consultatie.RecomandariRegimViata ?? string.Empty;
        
        // Load MedicationList from DTO
        if (consultatie.MedicationList?.Any() == true)
        {
            MedicationList = consultatie.MedicationList.ToList();
            ConsultatieCommand.MedicationList = MedicationList;
            Logger.LogInformation("[Consultatii] Loaded {Count} medications from consultatie", MedicationList.Count);
        }
        else
        {
            MedicationList = new List<MedicationRowDto>();
            ConsultatieCommand.MedicationList = MedicationList;
        }
        
        // Tab 4: Concluzii
        Concluzii = consultatie.Concluzie ?? consultatie.ObservatiiMedic ?? string.Empty;
        NoteUrmatoareaVizita = consultatie.RecomandariSupraveghere ?? string.Empty;
        
        // Parse DataUrmatoareiProgramari if exists
        if (!string.IsNullOrEmpty(consultatie.DataUrmatoareiProgramari))
        {
            if (DateTime.TryParse(consultatie.DataUrmatoareiProgramari, out var dataVizita))
            {
                DataUrmatoareiVizite = dataVizita;
            }
        }
        
        // Tab 4: Scrisoare Medicală - Anexa 43
        EsteAfectiuneOncologica = consultatie.EsteAfectiuneOncologica;
        AreIndicatieInternare = consultatie.AreIndicatieInternare;
        SaEliberatPrescriptie = consultatie.SaEliberatPrescriptie ?? false;
        SeriePrescriptie = consultatie.SeriePrescriptie ?? string.Empty;
        SaEliberatConcediuMedical = consultatie.SaEliberatConcediuMedical ?? false;
        SerieConcediuMedical = consultatie.SerieConcediuMedical ?? string.Empty;
        SaEliberatIngrijiriDomiciliu = consultatie.SaEliberatIngrijiriDomiciliu ?? false;
        SaEliberatDispozitiveMedicale = consultatie.SaEliberatDispozitiveMedicale ?? false;
        TransmiterePrinEmail = consultatie.TransmiterePrinEmail;
        EmailTransmitere = consultatie.EmailTransmitere ?? string.Empty;

        // ✅ Sync to ConsultatieCommand for DiagnosticTab
        SyncToConsultatieCommand();
    }

    /// <summary>
    /// Sincronizează datele din form fields în ConsultatieCommand
    /// Necesar pentru DiagnosticTab (ICD10 integration)
    /// </summary>
    private void SyncToConsultatieCommand()
    {
        ConsultatieCommand.DiagnosticPozitiv = DiagnosticPrincipal;
        ConsultatieCommand.TratamentMedicamentos = PlanTerapeutic;
        ConsultatieCommand.RecomandariRegimViata = Recomandari;
      
        // Sync ICD10 codes from DiagnosisList if exists
     if (DiagnosisList.Any())
        {
   var principalDiag = DiagnosisList.FirstOrDefault(d => d.IsPrincipal);
            if (principalDiag != null)
 {
         ConsultatieCommand.CoduriICD10 = principalDiag.Code;
          }
      }
    }

    /// <summary>
    /// Sincronizează datele din ConsultatieCommand înapoi în form fields
    /// Apelat când DiagnosticTab modifică codurile ICD10
  /// </summary>
    private void SyncFromConsultatieCommand()
    {
        DiagnosticPrincipal = ConsultatieCommand.DiagnosticPozitiv ?? string.Empty;
        DiagnosticSecundar = string.Empty; // Legacy field removed
        PlanTerapeutic = ConsultatieCommand.TratamentMedicamentos ?? string.Empty;
        Recomandari = ConsultatieCommand.RecomandariRegimViata ?? string.Empty;
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
        if (tabNumber < 1 || tabNumber > TotalTabs) return;
        ActiveTab = tabNumber;
    }

    #endregion

    #region Actions

    private async Task HandleSaveDraft()
    {
        try
        {
            IsSaving = true;
            
            // Debug logging for ICD10 codes and ProgramareID
            Logger.LogInformation("[Consultatii] HandleSaveDraft - ProgramareId from URL: {ProgramareId}, ConsultatieId: {ConsultatieId}",
                ProgramareId?.ToString() ?? "NULL",
                ConsultatieId?.ToString() ?? "NULL");
            Logger.LogInformation("[Consultatii] HandleSaveDraft - ConsultatieCommand.CoduriICD10: {ICD10}",
                ConsultatieCommand.CoduriICD10 ?? "NULL");
            Logger.LogInformation("[Consultatii] HandleSaveDraft - DescriereDetaliataPrincipal: {Desc}",
                ConsultatieCommand.DescriereDetaliataPrincipal?.Substring(0, Math.Min(50, ConsultatieCommand.DescriereDetaliataPrincipal?.Length ?? 0)) ?? "NULL");
            
            var command = new ValyanClinic.Application.Features.ConsultatieManagement.Commands.SaveConsultatieDraft.SaveConsultatieDraftCommand
            {
                ConsultatieID = ConsultatieId,
                ProgramareID = ProgramareId,
                PacientID = PacientId!.Value,
                MedicID = CurrentMedicId,
                DataConsultatie = DateTime.Today,
                OraConsultatie = DateTime.Now.TimeOfDay,
                TipConsultatie = "Prima consultatie",
                
                // Tab 1: Anamneză
                MotivPrezentare = string.IsNullOrWhiteSpace(MotivPrezentare) ? null : MotivPrezentare,
                IstoricBoalaActuala = string.IsNullOrWhiteSpace(IstoricBoalaActuala) ? null : IstoricBoalaActuala,
                IstoricMedicalPersonal = string.IsNullOrWhiteSpace(IstoricMedicalPersonal) ? null : IstoricMedicalPersonal,
                IstoricFamilial = string.IsNullOrWhiteSpace(IstoricFamilial) ? null : IstoricFamilial,
                TratamentAnterior = string.IsNullOrWhiteSpace(TratamentAnterior) ? null : TratamentAnterior,
                FactoriDeRisc = string.IsNullOrWhiteSpace(FactoriDeRisc) ? null : FactoriDeRisc,
                Alergii = string.IsNullOrWhiteSpace(Alergii) ? null : Alergii,
                
                // Tab 2: Semne Vitale
                Greutate = Greutate,
                Inaltime = Inaltime,
                IMC = IMC,
                Temperatura = Temperatura,
                TensiuneArteriala = (TensiuneSistolica.HasValue && TensiuneDiastolica.HasValue) ? $"{TensiuneSistolica}/{TensiuneDiastolica}" : null,
                Puls = Puls,
                FreccventaRespiratorie = FreqventaRespiratorie,
                SaturatieO2 = SpO2,
                
                // Tab 2: Examen General
                StareGenerala = string.IsNullOrWhiteSpace(StareGenerala) ? null : StareGenerala,
                Tegumente = string.IsNullOrWhiteSpace(Tegumente) ? null : Tegumente,
                Mucoase = string.IsNullOrWhiteSpace(Mucoase) ? null : Mucoase,
                Edeme = string.IsNullOrWhiteSpace(Edeme) ? null : Edeme,
                Glicemie = Glicemie,
                GanglioniLimfatici = string.IsNullOrWhiteSpace(GanglioniLimfatici) ? null : GanglioniLimfatici,
                ExamenObiectivDetaliat = string.IsNullOrWhiteSpace(ExamenObiectiv) ? null : ExamenObiectiv,
                AlteObservatiiClinice = string.IsNullOrWhiteSpace(AlteObservatiiClinice) ? null : AlteObservatiiClinice,
                
                // Tab 2: Investigații
                InvestigatiiLaborator = string.IsNullOrWhiteSpace(InvestigatiiParaclinice) ? null : InvestigatiiParaclinice,
                AlteInvestigatii = ConsultatieCommand.AlteInvestigatii,
                
                // Tab 3: Diagnostic - NEW normalized structure
                // Use ConsultatieCommand values - they are synced from DiagnosticTab via SyncToModel()
                CodICD10Principal = ConsultatieCommand.CodICD10Principal,
                NumeDiagnosticPrincipal = ConsultatieCommand.NumeDiagnosticPrincipal,
                DescriereDetaliataPrincipal = ConsultatieCommand.DescriereDetaliataPrincipal,
                DiagnosticeSecundare = ConsultatieCommand.DiagnosticeSecundare,
                
                // LEGACY fields for backwards compatibility
                DiagnosticPozitiv = !string.IsNullOrWhiteSpace(ConsultatieCommand.DiagnosticPozitiv) 
                    ? ConsultatieCommand.DiagnosticPozitiv 
                    : (string.IsNullOrWhiteSpace(DiagnosticPrincipal) ? null : DiagnosticPrincipal),
                CoduriICD10 = ConsultatieCommand.CoduriICD10,
                
                // Tab 3: Tratament
                TratamentMedicamentos = string.IsNullOrWhiteSpace(PlanTerapeutic) ? null : PlanTerapeutic,
                RecomandariRegimViata = string.IsNullOrWhiteSpace(Recomandari) ? null : Recomandari,
                MedicationList = ConsultatieCommand.MedicationList, // Use Model's list bound to DiagnosticTab
                
                // Tab 4: Concluzii
                Concluzie = string.IsNullOrWhiteSpace(Concluzii) ? null : Concluzii,
                ObservatiiMedic = string.IsNullOrWhiteSpace(Concluzii) ? null : Concluzii,
                DataUrmatoareiProgramari = DataUrmatoareiVizite.HasValue ? DataUrmatoareiVizite.Value.ToString("dd.MM.yyyy") : null,
                RecomandariSupraveghere = string.IsNullOrWhiteSpace(NoteUrmatoareaVizita) ? null : NoteUrmatoareaVizita,
                
                // Tab 4: Scrisoare Medicală - Anexa 43
                EsteAfectiuneOncologica = EsteAfectiuneOncologica,
                AreIndicatieInternare = AreIndicatieInternare,
                SaEliberatPrescriptie = SaEliberatPrescriptie,
                SeriePrescriptie = string.IsNullOrWhiteSpace(SeriePrescriptie) ? null : SeriePrescriptie,
                SaEliberatConcediuMedical = SaEliberatConcediuMedical,
                SerieConcediuMedical = string.IsNullOrWhiteSpace(SerieConcediuMedical) ? null : SerieConcediuMedical,
                SaEliberatIngrijiriDomiciliu = SaEliberatIngrijiriDomiciliu,
                SaEliberatDispozitiveMedicale = SaEliberatDispozitiveMedicale,
                TransmiterePrinEmail = TransmiterePrinEmail,
                EmailTransmitere = string.IsNullOrWhiteSpace(EmailTransmitere) ? null : EmailTransmitere,
                
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

    private void HandleGenerateLetter()
    {
        Logger.LogInformation("Generate letter requested - opening Scrisoare Medicală preview");
        ShowScrisoarePreview = true;
        StateHasChanged();
    }

    private void CloseScrisoarePreview()
    {
        ShowScrisoarePreview = false;
        StateHasChanged();
    }

    /// <summary>
    /// Construiește un ConsulatieDetailDto din datele curente ale formularului
    /// pentru generarea Scrisorii Medicale (Anexa 43)
    /// </summary>
    private ConsulatieDetailDto BuildConsultatieDetailDto()
    {
        return new ConsulatieDetailDto
        {
            // Primary keys
            ConsultatieID = ConsultatieId ?? Guid.NewGuid(),
            ProgramareID = ProgramareId,
            PacientID = PacientId ?? Guid.Empty,
            MedicID = CurrentMedicId,

            // Date consultație
            DataConsultatie = DateTime.Now,
            OraConsultatie = TimeSpan.FromHours(DateTime.Now.Hour).Add(TimeSpan.FromMinutes(DateTime.Now.Minute)),
            TipConsultatie = "Consultație",

            // Informații pacient (din PacientData)
            PacientNumeComplet = PacientData?.NumeComplet ?? string.Empty,
            PacientCNP = PacientData?.CNP,
            PacientDataNasterii = PacientData?.Data_Nasterii,
            PacientSex = PacientData?.Sex,
            PacientTelefon = PacientData?.Telefon,
            PacientAlergii = !string.IsNullOrEmpty(Alergii) ? Alergii : PacientData?.Alergii, // Preferă alergiile din consultație, fallback la profil

            // I. Motive prezentare
            MotivPrezentare = MotivPrezentare,
            IstoricBoalaActuala = IstoricBoalaActuala,

            // II. Antecedente
            IstoricMedicalPersonal = IstoricMedicalPersonal,
            IstoricFamilial = IstoricFamilial,
            TratamentAnterior = TratamentAnterior,
            FactoriDeRisc = FactoriDeRisc,

            // III.A. Examen general
            StareGenerala = StareGenerala,
            Tegumente = Tegumente,
            Mucoase = Mucoase,
            Edeme = Edeme,
            Glicemie = Glicemie,
            GanglioniLimfatici = GanglioniLimfatici,

            // III.B. Semne vitale
            Greutate = Greutate,
            Inaltime = Inaltime,
            IMC = IMC,
            Temperatura = Temperatura,
            TensiuneArteriala = TensiuneSistolica.HasValue && TensiuneDiastolica.HasValue 
                ? $"{TensiuneSistolica}/{TensiuneDiastolica}" 
                : null,
            Puls = Puls,
            FreccventaRespiratorie = FreqventaRespiratorie,
            SaturatieO2 = SpO2,

            // III.C. Examen obiectiv detaliat
            ExamenObiectivDetaliat = ExamenObiectiv,
            AlteObservatiiClinice = AlteObservatiiClinice,
            
            // IV. Investigații
            AlteInvestigatii = ConsultatieCommand.AlteInvestigatii,

            // V. Diagnostic
            DiagnosticPozitiv = DiagnosticPrincipal,
            CoduriICD10 = DiagnosisList.FirstOrDefault(d => d.IsPrincipal)?.Code,
            // Normalized diagnostic fields from DiagnosticTab
            CodICD10Principal = ConsultatieCommand.CodICD10Principal,
            NumeDiagnosticPrincipal = ConsultatieCommand.NumeDiagnosticPrincipal,
            DescriereDetaliataPrincipal = ConsultatieCommand.DescriereDetaliataPrincipal,

            // VI. Tratament
            TratamentMedicamentos = PlanTerapeutic,
            MedicationList = MedicationList,
            
            // VII. Recomandări
            RecomandariRegimViata = Recomandari,
            DataUrmatoareiProgramari = DataUrmatoareiVizite?.ToString("dd.MM.yyyy"),
            RecomandariSupraveghere = NoteUrmatoareaVizita,

            // VIII. Concluzie
            Concluzie = Concluzii,

            // IX. Anexa 43 - Checkbox fields
            EsteAfectiuneOncologica = EsteAfectiuneOncologica,
            AreIndicatieInternare = AreIndicatieInternare,
            SaEliberatPrescriptie = SaEliberatPrescriptie,
            SeriePrescriptie = SeriePrescriptie,
            SaEliberatConcediuMedical = SaEliberatConcediuMedical,
            SerieConcediuMedical = SerieConcediuMedical,
            SaEliberatIngrijiriDomiciliu = SaEliberatIngrijiriDomiciliu,
            SaEliberatDispozitiveMedicale = SaEliberatDispozitiveMedicale,
            TransmiterePrinEmail = TransmiterePrinEmail,
            EmailTransmitere = EmailTransmitere,
            
            // Diagnostice secundare - from ConsultatieCommand (synced from DiagnosticTab)
            DiagnosticeSecundare = ConsultatieCommand.DiagnosticeSecundare?
                .Select(d => new ValyanClinic.Application.Features.ConsultatieManagement.DTOs.DiagnosticSecundarDetailDto
                {
                    Id = d.Id,
                    OrdineAfisare = d.OrdineAfisare,
                    CodICD10 = d.CodICD10,
                    NumeDiagnostic = d.NumeDiagnostic,
                    Descriere = d.Descriere
                })
                .ToList(),

            // Status
            Status = "In desfasurare",
            DurataMinute = (int)ElapsedTime.TotalMinutes,
            DataCreare = DateTime.Now,
            CreatDe = CurrentUserId
        };
    }

    private void MarkFormAsDirty()
    {
        if (!HasUnsavedChanges)
        {
            HasUnsavedChanges = true;
        }

    // ✅ NEW: Sync to ConsultatieCommand când datele se schimbă
        SyncToConsultatieCommand();
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
    [JSInvokable] public void OnKeyboardNextTab() { if (ActiveTab < TotalTabs) { SwitchTab(ActiveTab + 1); StateHasChanged(); } }
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

        // ✅ Timer events handled by ConsultationTimer component (isolated re-renders)

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
