using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Services.IMC;
using ValyanClinic.Infrastructure.Services.DraftStorage;
using ValyanClinic.Application.Services.Draft;
using ValyanClinic.Application.Interfaces;

namespace ValyanClinic.Components.Pages.Dashboard.Modals;

public partial class ConsultatieModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ConsultatieModal> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IIMCCalculatorService IMCCalculator { get; set; } = default!;
    [Inject] private IDraftStorageService<CreateConsultatieCommand> DraftService { get; set; } = default!;
    [Inject] private DraftAutoSaveHelper<CreateConsultatieCommand> DraftAutoSaveHelper { get; set; } = default!;
    [Inject] private IFieldPermissionService FieldPermissions { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

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

    // ✅ DRAFT AUTO-SAVE (Hybrid Approach)
    private bool _hasUnsavedChanges = false;
    private DateTime _cachedLastSaveTime = DateTime.MinValue;

    // ✅ Property pentru LastSaveTime - returnează cached value
    private DateTime LastSaveTime => _cachedLastSaveTime;

    #region Field Permissions
    
    private bool CanEditField(string fieldName) => 
        FieldPermissions.CanEditField("Consultatie", fieldName);
    
    private bool CanViewField(string fieldName) => 
        FieldPermissions.CanViewField("Consultatie", fieldName);
    
    private FieldState GetFieldState(string fieldName, bool isEditMode = true) => 
        FieldPermissions.GetFieldState("Consultatie", fieldName, isEditMode);
    
    private async Task LoadFieldPermissionsAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await FieldPermissions.LoadPermissionsAsync(userId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading field permissions");
        }
    }
    
    #endregion

    // Public Methods
    public async Task Open()
    {
        try
        {
            Logger.LogInformation("[ConsultatieModal] ===== OPENING MODAL =====");

            // ✅ Validare parametrii
            if (ProgramareID == Guid.Empty || PacientID == Guid.Empty || MedicID == Guid.Empty)
            {
                throw new InvalidOperationException("Invalid parameters");
            }

            IsVisible = true;
            await InvokeAsync(StateHasChanged);

            // ✅ Load field permissions
            await LoadFieldPermissionsAsync();

            // ✅ Load cached LastSaveTime
            await LoadLastSaveTimeFromStorage();

            // Load pacient data
            await LoadPacientData();

            // ✅ Încarcă draft existent
            await LoadDraftFromStorage();

            // Initialize model if needed
            if (Model.ProgramareID == Guid.Empty)
            {
                InitializeModel();
            }

            // ✅ Start auto-save using helper (Hybrid Approach)
            DraftAutoSaveHelper.Start(
                shouldSaveCallback: async () => await Task.FromResult(_hasUnsavedChanges && IsVisible && !IsSaving && !IsSavingDraft),
                saveCallback: SaveDraft
            );

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

    // ✅ ADAUGĂ lifecycle hooks pentru debugging
    protected override void OnInitialized()
    {
        Logger.LogInformation("[ConsultatieModal] OnInitialized - Component initialized");
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("[ConsultatieModal] OnInitializedAsync - Component async initialized");
        await base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("[ConsultatieModal] OnAfterRender - First render completed");
        }
        base.OnAfterRender(firstRender);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("[ConsultatieModal] OnAfterRenderAsync - First render async completed");
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public void Close()
    {
        Logger.LogInformation("[ConsultatieModal] Closing modal");

        // ✅ Stop auto-save using helper
        DraftAutoSaveHelper.Stop();

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

        // Note: Patient's allergies and chronic conditions are displayed read-only from PacientInfo
        // They are not copied to the consultation antecedente anymore after schema simplification

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
            var lastSaveTime = await DraftService.GetLastSaveTimeAsync(ProgramareID);
            _cachedLastSaveTime = lastSaveTime ?? DateTime.MinValue;
            Logger.LogDebug("[ConsultatieModal] Loaded LastSaveTime from storage: {Time}", _cachedLastSaveTime);
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

            // Salvează folosind serviciul
            await DraftService.SaveDraftAsync(ProgramareID, Model, MedicID.ToString());

            // Update cached LastSaveTime
            _cachedLastSaveTime = DateTime.Now;
            _hasUnsavedChanges = false;

            Logger.LogInformation("[ConsultatieModal] Draft saved successfully at {Time}", _cachedLastSaveTime);

            // Trigger UI update pentru a afișa checkmark
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
            var draftResult = await DraftService.LoadDraftAsync(ProgramareID);

            if (draftResult.IsSuccess && draftResult.Data != null)
            {
                Logger.LogInformation("[ConsultatieModal] Loaded draft from {SavedAt}", draftResult.SavedAt);

                // Restore form data
                Model = draftResult.Data;

                // Note: ActiveTab și CompletedSections nu sunt în Command, deci le păstrăm pe cele default
                // Dacă vrei să le salvezi, trebuie să extindi Draft<T> să includă metadata

                // Update cached LastSaveTime
                _cachedLastSaveTime = draftResult.SavedAt ?? DateTime.MinValue;

                // TODO: Show notification "Draft încărcat din {SavedAt}"
            }
            else if (draftResult.ErrorType == DraftErrorType.Expired)
            {
                Logger.LogWarning("[ConsultatieModal] Draft expired for programare {ProgramareID}", ProgramareID);
                // TODO: Show warning "Draft expirat - începe un formular nou"
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
            await DraftService.ClearDraftAsync(ProgramareID);

            // Reset cached LastSaveTime
            _cachedLastSaveTime = DateTime.MinValue;

            Logger.LogInformation("[ConsultatieModal] Draft cleared from storage");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error clearing draft");
        }
    }

    /// <summary>
    /// Marchează că au fost făcute modificări nesalvate
    /// </summary>
    private void MarkAsChanged()
    {
        _hasUnsavedChanges = true;
    }

    // ==================== END DRAFT MANAGEMENT ====================

    private async Task PreviewScrisoare()
    {
        Logger.LogInformation("[ConsultatieModal] Generating preview...");
        // TODO: Generate PDF preview of scrisoare medicala
        await Task.CompletedTask;
    }

    private async Task CloseModal()
    {
        // ✅ Verifică dacă există modificări nesalvate
        var lastSave = LastSaveTime;
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

    /// <summary>
    /// Returnează string-ul "acum X minute" pentru timpul trecut de la ultima salvare
    /// </summary>
    private string GetTimeSinceSave()
    {
        var lastSave = LastSaveTime;
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

    private async Task HandleTabChanged(string newTab)
    {
        Logger.LogInformation("[ConsultatieModal] Changing tab from {OldTab} to {NewTab}", ActiveTab, newTab);

        // Save current tab state if needed
        if (_hasUnsavedChanges)
        {
            await SaveDraft();
        }

        // Change active tab
        ActiveTab = newTab;
        CurrentSection = newTab;

        StateHasChanged();
    }

    // ✅ IDisposable implementation
    public void Dispose()
    {
        DraftAutoSaveHelper?.Dispose();
    }
}
