using Microsoft.AspNetCore.Components;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using System.Text.Json;

namespace ValyanClinic.Components.Pages.Dashboard.Modals;

public partial class ConsultatieModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ConsultatieModal> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Parameters
    [Parameter] public Guid ProgramareID { get; set; }
    [Parameter] public Guid PacientID { get; set; }
    [Parameter] public Guid MedicID { get; set; }
    [Parameter] public EventCallback OnConsultatieCompleted { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    // State
    private bool IsVisible { get; set; }
    private bool IsSaving { get; set; }
    private bool IsSavingDraft { get; set; }
    private bool IsLoadingPacient { get; set; }
    
    // Data
    private CreateConsultatieCommand Model { get; set; } = new();
    private PacientDetailDto? PacientInfo { get; set; }

    // UI State
    private string ActiveTab { get; set; } = "motive";
    private string CurrentSection { get; set; } = "motive";
    private List<string> Sections { get; set; } = new()
    {
        "motive",
        "antecedente",
        "examen",
        "investigatii",
        "diagnostic",
        "tratament",
        "concluzie"
    };
    private HashSet<string> CompletedSections { get; set; } = new();

    // ✅ AUTO-SAVE
    private System.Threading.Timer? _autoSaveTimer;
    private bool _hasUnsavedChanges = false;
    private const int AutoSaveIntervalSeconds = 60; // Auto-save la fiecare 60 secunde
    
    // ✅ CACHED LastSaveTime pentru a evita blocking calls
    private DateTime _cachedLastSaveTime = DateTime.MinValue;

    // Computed Properties
    private string CalculatedIMC
    {
        get
        {
            if (Model.Greutate.HasValue && Model.Inaltime.HasValue && Model.Inaltime > 0)
            {
                var inaltimeMetri = Model.Inaltime.Value / 100;
                var imc = Model.Greutate.Value / (inaltimeMetri * inaltimeMetri);
                return Math.Round(imc, 2).ToString("F2");
            }
            return "-";
        }
    }

    private string IMCInterpretation
    {
        get
        {
            if (Model.Greutate.HasValue && Model.Inaltime.HasValue && Model.Inaltime > 0)
            {
                var inaltimeMetri = Model.Inaltime.Value / 100;
                var imc = Model.Greutate.Value / (inaltimeMetri * inaltimeMetri);

                return imc switch
                {
                    < 18.5m => "Subponderal",
                    >= 18.5m and < 25m => "Normal",
                    >= 25m and < 30m => "Supraponderal",
                    >= 30m and < 35m => "Obezitate I",
                    >= 35m and < 40m => "Obezitate II",
                    >= 40m => "Obezitate morbida"
                };
            }
            return "";
        }
    }

    // ✅ Property pentru LastSaveTime - returnează cached value
    private DateTime LastSaveTime => _cachedLastSaveTime;

    // Public Methods
    public async Task Open()
    {
        try
        {
            Logger.LogInformation("[ConsultatieModal] ===== OPENING MODAL =====");
            Logger.LogInformation("[ConsultatieModal] ProgramareID: {ProgramareID}", ProgramareID);
            Logger.LogInformation("[ConsultatieModal] PacientID: {PacientID}", PacientID);
            Logger.LogInformation("[ConsultatieModal] MedicID: {MedicID}", MedicID);
            
            // ✅ Set visible FIRST
            IsVisible = true;
            Logger.LogInformation("[ConsultatieModal] IsVisible set to TRUE");
            
            // Force immediate UI update
            await InvokeAsync(StateHasChanged);
            Logger.LogInformation("[ConsultatieModal] StateHasChanged called (1st)");
            
            // ✅ Load cached LastSaveTime from storage
            await LoadLastSaveTimeFromStorage();
            
            // Load pacient data
            await LoadPacientData();
            
            // ✅ Încarcă draft existent (dacă există)
            await LoadDraftFromStorage();
            
            // Initialize model if needed
            if (Model.ProgramareID == Guid.Empty)
            {
                InitializeModel();
            }
            
            // ✅ Start auto-save timer
            StartAutoSaveTimer();
            
            // Final state update
            await InvokeAsync(StateHasChanged);
            Logger.LogInformation("[ConsultatieModal] ===== MODAL OPENED SUCCESSFULLY =====");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] ERROR opening modal!");
            IsVisible = false;
            throw;
        }
    }

    public void Close()
    {
        Logger.LogInformation("[ConsultatieModal] Closing modal");
        
        // ✅ Stop auto-save timer
        StopAutoSaveTimer();
        
        IsVisible = false;
        ResetModal();
        StateHasChanged();
    }

    // Private Methods
    private async Task LoadPacientData()
    {
        try
        {
            IsLoadingPacient = true;
            StateHasChanged();

            var query = new GetPacientByIdQuery(PacientID);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PacientInfo = result.Value;
                Logger.LogInformation("[ConsultatieModal] Loaded pacient: {Nume}", PacientInfo.NumeComplet);
            }
            else
            {
                Logger.LogWarning("[ConsultatieModal] Failed to load pacient data: {Errors}", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error loading pacient data");
        }
        finally
        {
            IsLoadingPacient = false;
            StateHasChanged();
        }
    }

    private void InitializeModel()
    {
        Model = new CreateConsultatieCommand
        {
            ProgramareID = ProgramareID,
            PacientID = PacientID,
            MedicID = MedicID,
            TipConsultatie = "Prima consultatie",
            CreatDe = MedicID.ToString()
        };

        // Pre-populate with existing pacient data if available
        if (PacientInfo != null)
        {
            Model.APP_Alergii = PacientInfo.Alergii;
            Model.APP_BoliAdult = PacientInfo.Boli_Cronice;
        }

        Logger.LogInformation("[ConsultatieModal] Model initialized");
    }

    private void ResetModal()
    {
        Model = new CreateConsultatieCommand();
        PacientInfo = null;
        ActiveTab = "motive";
        CurrentSection = "motive";
        CompletedSections.Clear();
        _hasUnsavedChanges = false;
        
        // ✅ Reset cached LastSaveTime
        _cachedLastSaveTime = DateTime.MinValue;
    }

    private async Task HandleSubmit()
    {
        try
        {
            Logger.LogInformation("[ConsultatieModal] Submitting consultatie...");
            IsSaving = true;
            StateHasChanged();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(Model.MotivPrezentare))
            {
                Logger.LogWarning("[ConsultatieModal] MotivPrezentare is required");
                // TODO: Show validation message
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.DiagnosticPozitiv))
            {
                Logger.LogWarning("[ConsultatieModal] DiagnosticPozitiv is required");
                // TODO: Show validation message
                return;
            }

            var result = await Mediator.Send(Model);

            if (result.IsSuccess)
            {
                Logger.LogInformation("[ConsultatieModal] Consultatie created successfully: {Id}", result.Value);
                
                // ✅ Șterge draft-ul după salvare finală
                await ClearDraftFromStorage();
                
                await OnConsultatieCompleted.InvokeAsync();
                Close();
            }
            else
            {
                Logger.LogError("[ConsultatieModal] Failed to create consultatie: {Errors}", string.Join(", ", result.Errors));
                // TODO: Show error message
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error submitting consultatie");
            // TODO: Show error message
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    // ✅ ==================== DRAFT MANAGEMENT ====================

    /// <summary>
    /// Încarcă LastSaveTime din LocalStorage (cached pentru a evita blocking calls)
    /// </summary>
    private async Task LoadLastSaveTimeFromStorage()
    {
        try
        {
            var storageKey = $"consultatie_draft_{ProgramareID}";
            var jsonDraft = await JSRuntime.InvokeAsync<string>("localStorage.getItem", storageKey);
            
            if (!string.IsNullOrEmpty(jsonDraft))
            {
                var draft = JsonSerializer.Deserialize<ConsultatieDraft>(jsonDraft);
                _cachedLastSaveTime = draft?.SavedAt ?? DateTime.MinValue;
                Logger.LogDebug("[ConsultatieModal] Loaded LastSaveTime from storage: {Time}", _cachedLastSaveTime);
            }
            else
            {
                _cachedLastSaveTime = DateTime.MinValue;
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "[ConsultatieModal] Error reading LastSaveTime from draft");
            _cachedLastSaveTime = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Salvează draft-ul consultației în LocalStorage
    /// </summary>
    private async Task SaveDraft()
    {
        if (IsSavingDraft) return;

        try
        {
            IsSavingDraft = true;
            Logger.LogInformation("[ConsultatieModal] Saving draft for Programare: {ProgramareID}", ProgramareID);

            // Construiește obiect draft
            var draft = new ConsultatieDraft
            {
                ProgramareID = ProgramareID,
                PacientID = PacientID,
                MedicID = MedicID,
                SavedAt = DateTime.Now, // ✅ Timestamp salvat ÎN draft
                ActiveTab = ActiveTab,
                CompletedSections = CompletedSections.ToList(),
                FormData = Model
            };

            // Serializează în JSON
            var jsonDraft = JsonSerializer.Serialize(draft);

            // Salvează în LocalStorage
            var storageKey = $"consultatie_draft_{ProgramareID}";
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, jsonDraft);

            // ✅ Update cached LastSaveTime
            _cachedLastSaveTime = draft.SavedAt;
            _hasUnsavedChanges = false;

            Logger.LogInformation("[ConsultatieModal] Draft saved successfully at {Time}", draft.SavedAt);

            // ✅ Trigger UI update pentru a afișa checkmark
            await InvokeAsync(StateHasChanged);

            // TODO: Show toast notification "Draft salvat!"
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error saving draft");
            // TODO: Show error toast
        }
        finally
        {
            IsSavingDraft = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Încarcă draft-ul consultației din LocalStorage
    /// </summary>
    private async Task LoadDraftFromStorage()
    {
        try
        {
            var storageKey = $"consultatie_draft_{ProgramareID}";
            var jsonDraft = await JSRuntime.InvokeAsync<string>("localStorage.getItem", storageKey);

            if (!string.IsNullOrEmpty(jsonDraft))
            {
                var draft = JsonSerializer.Deserialize<ConsultatieDraft>(jsonDraft);
                
                if (draft != null)
                {
                    Logger.LogInformation("[ConsultatieModal] Loaded draft from {SavedAt}", draft.SavedAt);

                    // Restore form data
                    Model = draft.FormData;
                    ActiveTab = draft.ActiveTab;
                    CompletedSections = draft.CompletedSections.ToHashSet();
                    
                    // ✅ Update cached LastSaveTime
                    _cachedLastSaveTime = draft.SavedAt;

                    // TODO: Show notification "Draft încărcat din {SavedAt}"
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error loading draft from storage");
        }
    }

    /// <summary>
    /// Șterge draft-ul din LocalStorage
    /// </summary>
    private async Task ClearDraftFromStorage()
    {
        try
        {
            var storageKey = $"consultatie_draft_{ProgramareID}";
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", storageKey);
            
            // ✅ Reset cached LastSaveTime
            _cachedLastSaveTime = DateTime.MinValue;
            
            Logger.LogInformation("[ConsultatieModal] Draft cleared from storage");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error clearing draft");
        }
    }

    /// <summary>
    /// Auto-save timer - salvează automat la fiecare 60 secunde
    /// </summary>
    private void StartAutoSaveTimer()
    {
        _autoSaveTimer = new System.Threading.Timer(async _ =>
        {
            if (_hasUnsavedChanges && IsVisible && !IsSaving && !IsSavingDraft)
            {
                await SaveDraft();
            }
        }, null, TimeSpan.FromSeconds(AutoSaveIntervalSeconds), TimeSpan.FromSeconds(AutoSaveIntervalSeconds));

        Logger.LogDebug("[ConsultatieModal] Auto-save timer started (interval: {Seconds}s)", AutoSaveIntervalSeconds);
    }

    private void StopAutoSaveTimer()
    {
        _autoSaveTimer?.Dispose();
        _autoSaveTimer = null;
        Logger.LogDebug("[ConsultatieModal] Auto-save timer stopped");
    }

    /// <summary>
    /// Marchează că au fost făcute modificări nesalvate
    /// </summary>
    private void MarkAsChanged()
    {
        _hasUnsavedChanges = true;
    }

    // ==================== END DRAFT MANAGEMENT ====================
    
    // ✅ ==================== ICD-10 MANAGEMENT ====================
    
    /// <summary>
    /// Handler pentru schimbarea codului ICD-10 din autocomplete (two-way binding)
    /// </summary>
    private Task HandleICD10CodeChanged(string? code)
    {
        Model.CoduriICD10 = code;
        MarkAsChanged();
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Handler pentru selecția unui cod ICD-10 din autocomplete
    /// Permite selecții multiple prin separare cu virgulă
    /// </summary>
    private void HandleICD10Selected(ValyanClinic.Application.Features.ICD10Management.DTOs.ICD10SearchResultDto? selectedCode)
    {
        if (selectedCode == null) return;

        try
        {
            Logger.LogInformation("[ConsultatieModal] ICD-10 code selected: {Code} - {Description}", 
                selectedCode.Code, selectedCode.ShortDescription);

            // Verifică dacă codul există deja
            var existingCodes = string.IsNullOrEmpty(Model.CoduriICD10) 
                ? new List<string>() 
                : Model.CoduriICD10.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

            if (!existingCodes.Contains(selectedCode.Code))
            {
                // Adaugă noul cod
                if (string.IsNullOrEmpty(Model.CoduriICD10))
                {
                    Model.CoduriICD10 = selectedCode.Code;
                }
                else
                {
                    Model.CoduriICD10 += $", {selectedCode.Code}";
                }

                MarkAsChanged();
                StateHasChanged();

                Logger.LogInformation("[ConsultatieModal] ICD-10 code added. Total codes: {Codes}", Model.CoduriICD10);
            }
            else
            {
                Logger.LogDebug("[ConsultatieModal] ICD-10 code {Code} already exists", selectedCode.Code);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error handling ICD-10 selection");
        }
    }

    /// <summary>
    /// Șterge un cod ICD-10 din lista de coduri selectate
    /// </summary>
    private void RemoveICD10Code(string codeToRemove)
    {
        try
        {
            Logger.LogInformation("[ConsultatieModal] Removing ICD-10 code: {Code}", codeToRemove);

            if (string.IsNullOrEmpty(Model.CoduriICD10)) return;

            var existingCodes = Model.CoduriICD10
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c != codeToRemove)
                .ToList();

            Model.CoduriICD10 = existingCodes.Any() 
                ? string.Join(", ", existingCodes) 
                : string.Empty;

            MarkAsChanged();
            StateHasChanged();

            Logger.LogInformation("[ConsultatieModal] ICD-10 code removed. Remaining codes: {Codes}", Model.CoduriICD10);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error removing ICD-10 code");
        }
    }
    
    // ==================== END ICD-10 MANAGEMENT ====================

    // ✅ ==================== ICD-10 SECUNDARE MANAGEMENT ====================
    
    /// <summary>
    /// Handler pentru schimbarea codurilor ICD-10 secundare (two-way binding)
    /// </summary>
    private Task HandleICD10SecundareChanged(string? code)
    {
        Model.CoduriICD10Secundare = code;
        MarkAsChanged();
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Handler pentru selecția unui cod ICD-10 secundar
    /// Permite adăugarea MULTIPLĂ de coduri
    /// </summary>
    private void HandleICD10SecundarSelected(ValyanClinic.Application.Features.ICD10Management.DTOs.ICD10SearchResultDto? selectedCode)
    {
        if (selectedCode == null) return;

        try
        {
            Logger.LogInformation("[ConsultatieModal] ICD-10 SECUNDAR selected: {Code} - {Description}", 
                selectedCode.Code, selectedCode.ShortDescription);

            // Verifică dacă codul există deja în secundare
            var existingCodes = string.IsNullOrEmpty(Model.CoduriICD10Secundare) 
                ? new List<string>() 
                : Model.CoduriICD10Secundare.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

            // Verifică dacă codul este deja în principal
            var principalCodes = string.IsNullOrEmpty(Model.CoduriICD10)
                ? new List<string>()
                : Model.CoduriICD10.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

            if (principalCodes.Contains(selectedCode.Code))
            {
                Logger.LogWarning("[ConsultatieModal] ICD-10 code {Code} already exists in PRINCIPAL codes", selectedCode.Code);
                // TODO: Show warning toast "Codul există deja ca diagnostic principal!"
                return;
            }

            if (!existingCodes.Contains(selectedCode.Code))
            {
                // Adaugă noul cod secundar
                if (string.IsNullOrEmpty(Model.CoduriICD10Secundare))
                {
                    Model.CoduriICD10Secundare = selectedCode.Code;
                }
                else
                {
                    Model.CoduriICD10Secundare += $", {selectedCode.Code}";
                }

                MarkAsChanged();
                StateHasChanged();

                Logger.LogInformation("[ConsultatieModal] ICD-10 SECUNDAR added. Total: {Codes}", Model.CoduriICD10Secundare);
            }
            else
            {
                Logger.LogDebug("[ConsultatieModal] ICD-10 SECUNDAR {Code} already exists", selectedCode.Code);
                // TODO: Show info toast "Codul a fost deja adăugat!"
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error handling ICD-10 SECUNDAR selection");
        }
    }

    /// <summary>
    /// Șterge un cod ICD-10 secundar din listă
    /// </summary>
    private void RemoveICD10SecundarCode(string codeToRemove)
    {
        try
        {
            Logger.LogInformation("[ConsultatieModal] Removing ICD-10 SECUNDAR: {Code}", codeToRemove);

            if (string.IsNullOrEmpty(Model.CoduriICD10Secundare)) return;

            var existingCodes = Model.CoduriICD10Secundare
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c != codeToRemove)
                .ToList();

            Model.CoduriICD10Secundare = existingCodes.Any() 
                ? string.Join(", ", existingCodes) 
                : string.Empty;

            MarkAsChanged();
            StateHasChanged();

            Logger.LogInformation("[ConsultatieModal] ICD-10 SECUNDAR removed. Remaining: {Codes}", Model.CoduriICD10Secundare);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error removing ICD-10 SECUNDAR");
        }
    }
    
    // ==================== END ICD-10 SECUNDARE ====================

    private async Task PreviewScrisoare()
    {
        Logger.LogInformation("[ConsultatieModal] Generating preview...");
        // TODO: Generate PDF preview of scrisoare medicala
        await Task.CompletedTask;
    }

    private async Task CloseModal()
    {
        // ✅ Verifică dacă există modificări nesalvate
        var lastSave = LastSaveTime; // ✅ Citește din property
        if (_hasUnsavedChanges && (DateTime.Now - lastSave).TotalMinutes > 1)
        {
            // TODO: Show confirmation dialog "Ai modificări nesalvate. Salvezi draft?"
            // Pentru acum, salvează automat
            await SaveDraft();
        }

        await OnClose.InvokeAsync();
        Close();
    }

    private async Task HandleOverlayClick()
    {
        // Permite inchiderea modalului prin click pe overlay
        await CloseModal();
    }

    private void CalculateIMC()
    {
        MarkAsChanged();
        StateHasChanged();
    }

    /// <summary>
    /// Returnează clasa CSS pentru badge-ul IMC bazat pe interpretare
    /// </summary>
    private string GetIMCBadgeClass()
    {
        if (!Model.Greutate.HasValue || !Model.Inaltime.HasValue || Model.Inaltime <= 0)
            return "";

        var inaltimeMetri = Model.Inaltime.Value / 100;
        var imc = Model.Greutate.Value / (inaltimeMetri * inaltimeMetri);

        return imc switch
        {
            < 18.5m => "imc-badge-subponderal",
            < 25m => "imc-badge-normal",
            < 30m => "imc-badge-supraponderal",
            < 35m => "imc-badge-obezitate1",
            < 40m => "imc-badge-obezitate2",
            _ => "imc-badge-obezitate-morbida" // >= 40
        };
    }

    /// <summary>
    /// Returnează iconița FontAwesome pentru badge-ul IMC
    /// </summary>
    private string GetIMCIcon()
    {
        if (!Model.Greutate.HasValue || !Model.Inaltime.HasValue || Model.Inaltime <= 0)
            return "fas fa-calculator";

        var inaltimeMetri = Model.Inaltime.Value / 100;
        var imc = Model.Greutate.Value / (inaltimeMetri * inaltimeMetri);

        return imc switch
        {
            < 18.5m => "fas fa-arrow-down", // Subponderal
            < 25m => "fas fa-check-circle", // Normal
            < 30m => "fas fa-exclamation-triangle", // Supraponderal
            < 35m => "fas fa-exclamation-circle", // Obezitate I
            < 40m => "fas fa-times-circle", // Obezitate II
            _ => "fas fa-skull-crossbones" // Obezitate morbida >= 40
        };
    }

    /// <summary>
    /// Returnează string-ul "acum X minute" pentru timpul trecut de la ultima salvare
    /// </summary>
    private string GetTimeSinceSave()
    {
        var lastSave = LastSaveTime; // ✅ Citește din property (bound la ProgramareID)
        if (lastSave == DateTime.MinValue) return "";

        var timeSince = DateTime.Now - lastSave;

        if (timeSince.TotalSeconds < 60)
            return "acum";
        else if (timeSince.TotalMinutes < 60)
            return $"acum {(int)timeSince.TotalMinutes} min";
        else if (timeSince.TotalHours < 24)
            return $"acum {(int)timeSince.TotalHours}h";
        else
            return $"acum {(int)timeSince.TotalDays} zile";
    }

    private string GetSectionLabel(string section)
    {
        return section switch
        {
            "motive" => "Motive",
            "antecedente" => "Antecedente",
            "examen" => "Examen",
            "investigatii" => "Investigatii",
            "diagnostic" => "Diagnostic",
            "tratament" => "Tratament",
            "concluzie" => "Concluzie",
            _ => section
        };
    }

    private void MarkSectionCompleted(string section)
    {
        if (!CompletedSections.Contains(section))
        {
            CompletedSections.Add(section);
            MarkAsChanged();
            Logger.LogDebug("[ConsultatieModal] Section completed: {Section}", section);
        }
    }

    private bool IsSectionCompleted(string section)
    {
        return CompletedSections.Contains(section);
    }

    // ✅ IDisposable implementation
    public void Dispose()
    {
        StopAutoSaveTimer();
    }

    // ✅ DTO pentru draft storage
    private class ConsultatieDraft
    {
        public Guid ProgramareID { get; set; }
        public Guid PacientID { get; set; }
        public Guid MedicID { get; set; }
        public DateTime SavedAt { get; set; }
        public string ActiveTab { get; set; } = "motive";
        public List<string> CompletedSections { get; set; } = new();
        public CreateConsultatieCommand FormData { get; set; } = new();
    }
}
