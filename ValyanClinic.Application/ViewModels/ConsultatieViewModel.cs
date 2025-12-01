using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Services.IMC;
using ValyanClinic.Infrastructure.Services.DraftStorage;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.ViewModels;

/// <summary>
/// ViewModel pentru gestionarea state-ului în ConsultatieModal
/// Orchestrează business logic, draft management și comunicarea cu API
/// </summary>
public class ConsultatieViewModel
{
    private readonly IMediator _mediator;
    private readonly IDraftStorageService<CreateConsultatieCommand> _draftService;
    private readonly IIMCCalculatorService _imcCalculator;
    private readonly ILogger<ConsultatieViewModel> _logger;

    // ==================== COMMAND DATA ====================
    public CreateConsultatieCommand Command { get; set; } = new();

    // ==================== UI STATE ====================
    public string ActiveTab { get; private set; } = "motive";
    public string CurrentSection { get; private set; } = "motive";
    public HashSet<string> CompletedSections { get; } = new();

    // ==================== LOADING STATE ====================
    public bool IsSaving { get; private set; }
    public bool IsSavingDraft { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsInitializing { get; private set; }

    // ==================== DRAFT STATE ====================
    public DateTime? LastSaveTime { get; private set; }
    public bool HasUnsavedChanges { get; private set; }
    public bool HasDraftLoaded { get; private set; }

    // ==================== VALIDATION STATE ====================
    public Dictionary<string, List<string>> ValidationErrors { get; } = new();
    public bool HasValidationErrors => ValidationErrors.Any();

    // ==================== IMC COMPUTED ====================
    public IMCResult? IMCResult
    {
        get
        {
            if (Command.Greutate.HasValue && Command.Inaltime.HasValue)
            {
                return _imcCalculator.Calculate(Command.Greutate.Value, Command.Inaltime.Value);
            }
            return null;
        }
    }

    // ==================== EVENTS ====================
    public event EventHandler? StateChanged;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler? DraftSaved;
    public event EventHandler? ConsultatieSubmitted;

    // ==================== CONSTRUCTOR ====================
    public ConsultatieViewModel(
        IMediator mediator,
        IDraftStorageService<CreateConsultatieCommand> draftService,
        IIMCCalculatorService imcCalculator,
        ILogger<ConsultatieViewModel> logger)
    {
        _mediator = mediator;
        _draftService = draftService;
        _imcCalculator = imcCalculator;
        _logger = logger;
    }

    // ==================== INITIALIZATION ====================

    /// <summary>
    /// Inițializează ViewModel-ul cu date pentru o consultație nouă
    /// </summary>
    public async Task InitializeAsync(Guid programareId, Guid pacientId, Guid medicId, string userId)
    {
        try
        {
            IsInitializing = true;
            NotifyStateChanged();

            // Initialize command
            Command = new CreateConsultatieCommand
            {
                ProgramareID = programareId,
                PacientID = pacientId,
                MedicID = medicId,
                CreatDe = userId
            };

            // Try to load draft
            var draftResult = await _draftService.LoadDraftAsync(programareId);
            if (draftResult.IsSuccess && draftResult.Data != null)
            {
                Command = draftResult.Data;
                LastSaveTime = draftResult.SavedAt;
                HasDraftLoaded = true;
                HasUnsavedChanges = false;

                _logger.LogInformation(
                    "Draft loaded for programare {ProgramareId}, saved at {SavedAt}",
                    programareId,
                    LastSaveTime
                );
            }
            else if (draftResult.ErrorType == DraftErrorType.Expired)
            {
                _logger.LogWarning("Draft expired for programare {ProgramareId}", programareId);
                // Draft expirat - continuăm cu command nou
            }
            else
            {
                _logger.LogInformation("No draft found for programare {ProgramareId}", programareId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ConsultatieViewModel");
            OnError("Eroare la inițializare. Vă rugăm reîncărcați pagina.");
        }
        finally
        {
            IsInitializing = false;
            NotifyStateChanged();
        }
    }

    // ==================== TAB NAVIGATION ====================

    /// <summary>
    /// Schimbă tab-ul activ
    /// </summary>
    public void SetActiveTab(string tab)
    {
        if (ActiveTab != tab)
        {
            ActiveTab = tab;
            CurrentSection = tab;
            _logger.LogDebug("Tab changed to {Tab}", tab);
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Marchează o secțiune ca fiind completă
    /// </summary>
    public void MarkSectionAsCompleted(string section)
    {
        if (CompletedSections.Add(section))
        {
            _logger.LogDebug("Section {Section} marked as completed", section);
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Calculează progresul completării (0-100%)
    /// </summary>
    public int GetCompletionProgress()
    {
        var totalSections = 7; // motive, antecedente, examen, investigatii, diagnostic, tratament, concluzie
        return (int)((CompletedSections.Count / (double)totalSections) * 100);
    }

    // ==================== DRAFT MANAGEMENT ====================

    /// <summary>
    /// Salvează draft-ul în LocalStorage
    /// </summary>
    public async Task SaveDraftAsync()
    {
        if (IsSavingDraft)
        {
            _logger.LogWarning("Draft save already in progress");
            return;
        }

        try
        {
            IsSavingDraft = true;
            NotifyStateChanged();

            await _draftService.SaveDraftAsync(
                Command.ProgramareID,
                Command,
                Command.CreatDe
            );

            LastSaveTime = DateTime.Now;
            HasUnsavedChanges = false;

            _logger.LogInformation(
                "Draft saved successfully for programare {ProgramareId}",
                Command.ProgramareID
            );

            DraftSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft for programare {ProgramareId}", Command.ProgramareID);
            OnError("Eroare la salvarea draft-ului");
        }
        finally
        {
            IsSavingDraft = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Marchează că există modificări nesalvate
    /// </summary>
    public void MarkAsChanged()
    {
        if (!HasUnsavedChanges)
        {
            HasUnsavedChanges = true;
            NotifyStateChanged();
        }
    }

    // ==================== SUBMIT ====================

    /// <summary>
    /// Trimite consultația către API
    /// </summary>
    public async Task<Result<Guid>> SubmitAsync()
    {
        try
        {
            IsSaving = true;
            NotifyStateChanged();

            // Clear validation errors
            ValidationErrors.Clear();

            // Basic validation
            if (!ValidateCommand())
            {
                _logger.LogWarning("Validation failed for consultatie");
                return Result<Guid>.Failure("Validare eșuată. Verificați câmpurile obligatorii.");
            }

            // Send command
            var result = await _mediator.Send(Command);

            if (result.IsSuccess)
            {
                // Clear draft on success
                await _draftService.ClearDraftAsync(Command.ProgramareID);
                HasUnsavedChanges = false;

                _logger.LogInformation(
                    "Consultatie created successfully: {ConsultatieId} for programare {ProgramareId}",
                    result.Value,
                    Command.ProgramareID
                );

                ConsultatieSubmitted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to create consultatie for programare {ProgramareId}: {Error}",
                    Command.ProgramareID,
                    result.FirstError
                );

                OnError(result.FirstError);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting consultatie for programare {ProgramareId}", Command.ProgramareID);
            OnError("Eroare neprevăzută la salvarea consultației");
            return Result<Guid>.Failure("Eroare neprevăzută");
        }
        finally
        {
            IsSaving = false;
            NotifyStateChanged();
        }
    }

    // ==================== VALIDATION ====================

    /// <summary>
    /// Validează command-ul înainte de submit
    /// </summary>
    private bool ValidateCommand()
    {
        ValidationErrors.Clear();

        // Câmpuri obligatorii
        if (string.IsNullOrWhiteSpace(Command.MotivPrezentare))
        {
            AddValidationError("MotivPrezentare", "Motivul prezentării este obligatoriu");
        }

        if (string.IsNullOrWhiteSpace(Command.DiagnosticPozitiv))
        {
            AddValidationError("DiagnosticPozitiv", "Diagnosticul este obligatoriu");
        }

        // Validare IMC (dacă sunt completate greutate/înălțime)
        if (Command.Greutate.HasValue && Command.Inaltime.HasValue)
        {
            if (!_imcCalculator.AreValuesValid(Command.Greutate.Value, Command.Inaltime.Value))
            {
                AddValidationError("IMC", "Valorile pentru greutate și înălțime sunt invalide");
            }
        }

        return !HasValidationErrors;
    }

    /// <summary>
    /// Adaugă o eroare de validare
    /// </summary>
    private void AddValidationError(string field, string message)
    {
        if (!ValidationErrors.ContainsKey(field))
        {
            ValidationErrors[field] = new List<string>();
        }
        ValidationErrors[field].Add(message);
    }

    /// <summary>
    /// Obține erorile de validare pentru un câmp
    /// </summary>
    public List<string>? GetValidationErrors(string field)
    {
        return ValidationErrors.TryGetValue(field, out var errors) ? errors : null;
    }

    // ==================== IMC HELPERS ====================

    /// <summary>
    /// Calculează greutatea ideală pentru pacient
    /// </summary>
    public decimal? CalculateIdealWeight(string? sex)
    {
        if (Command.Inaltime.HasValue && !string.IsNullOrEmpty(sex))
        {
            var idealWeight = _imcCalculator.CalculateIdealWeight(Command.Inaltime.Value, sex);
            return idealWeight > 0 ? idealWeight : null;
        }
        return null;
    }

    // ==================== UTILITY ====================

    /// <summary>
    /// Reset ViewModel la starea inițială
    /// </summary>
    public void Reset()
    {
        Command = new CreateConsultatieCommand();
        ActiveTab = "motive";
        CurrentSection = "motive";
        CompletedSections.Clear();
        ValidationErrors.Clear();

        IsSaving = false;
        IsSavingDraft = false;
        IsLoading = false;
        LastSaveTime = null;
        HasUnsavedChanges = false;
        HasDraftLoaded = false;

        NotifyStateChanged();
    }

    /// <summary>
    /// Notifică subscribers că state-ul s-a schimbat
    /// </summary>
    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Notifică subscribers despre o eroare
    /// </summary>
    private void OnError(string message)
    {
        ErrorOccurred?.Invoke(this, message);
    }
}
