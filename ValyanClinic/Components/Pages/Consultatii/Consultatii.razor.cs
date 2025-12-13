using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Pages.Consultatii;

public partial class Consultatii : ComponentBase, IDisposable
{
    [Inject] private ILogger<Consultatii> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    // Query Parameters
    [Parameter]
    [SupplyParameterFromQuery(Name = "pacientId")]
    public Guid? PacientId { get; set; }

    // State Management
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }
    private bool IsSaving { get; set; } = false;
    private DateTime? LastSaveTime { get; set; }

    // Patient Data (mock for now - will be replaced with MediatR query)
    private PacientDataDto? PacientData { get; set; }

    // Tab Management
    private int ActiveTab { get; set; } = 1;

    // Tab Completion Tracking
    private bool IsTabCompleted(int tabNumber)
    {
        return tabNumber switch
        {
            1 => !string.IsNullOrWhiteSpace(MotivPrezentare) &&
                 !string.IsNullOrWhiteSpace(AntecedentePatologice) &&
                 !string.IsNullOrWhiteSpace(TratamenteActuale),
            2 => (TensiuneSistolica.HasValue || TensiuneDiastolica.HasValue) &&
                 Puls.HasValue &&
                 Temperatura.HasValue &&
                 FreqventaRespiratorie.HasValue &&
                 (Greutate.HasValue || Inaltime.HasValue) &&
                 !string.IsNullOrWhiteSpace(ExamenObiectiv),
            3 => !string.IsNullOrWhiteSpace(DiagnosticPrincipal) &&
                 !string.IsNullOrWhiteSpace(PlanTerapeutic),
            4 => !string.IsNullOrWhiteSpace(Concluzii),
            _ => false
        };
    }

    // Timer Management
    private bool IsTimerRunning { get; set; } = false;
    private DateTime TimerStartTime { get; set; }
    private Timer? ConsultationTimer { get; set; }
    private TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;
    private string FormattedTime => $"{(int)ElapsedTime.TotalMinutes:D2}:{ElapsedTime.Seconds:D2}";

    // Timer Warning Classes (template design)
    private string TimerWarningClass
    {
        get
        {
            var minutes = (int)ElapsedTime.TotalMinutes;
            if (minutes >= 20) return "danger";
            if (minutes >= 15) return "warning";
            return string.Empty;
        }
    }

    // Allergies
    private bool HasAllergies => !string.IsNullOrEmpty(PacientData?.Allergies);
    private string AllergiesText => PacientData?.Allergies ?? string.Empty;

    // Conditions (chronic diseases)
    private bool HasConditions => !string.IsNullOrEmpty(PacientData?.ChronicConditions);
    private string ConditionsText => PacientData?.ChronicConditions ?? string.Empty;

    // Progress Calculation
    private int ProgressPercentage
    {
        get
        {
            var completedFields = 0;
            var totalFields = 13;

            if (!string.IsNullOrEmpty(MotivPrezentare)) completedFields++;
            if (!string.IsNullOrEmpty(AntecedentePatologice)) completedFields++;
            if (!string.IsNullOrEmpty(TratamenteActuale)) completedFields++;
            if (TensiuneSistolica.HasValue || TensiuneDiastolica.HasValue) completedFields++;
            if (Puls.HasValue) completedFields++;
            if (Temperatura.HasValue) completedFields++;
            if (FreqventaRespiratorie.HasValue) completedFields++;
            if (Greutate.HasValue || Inaltime.HasValue) completedFields++;
            if (!string.IsNullOrEmpty(ExamenObiectiv)) completedFields++;
            if (!string.IsNullOrEmpty(DiagnosticPrincipal)) completedFields++;
            if (!string.IsNullOrEmpty(PlanTerapeutic)) completedFields++;
            if (!string.IsNullOrEmpty(Concluzii)) completedFields++;
            if (DataUrmatoareiVizite.HasValue) completedFields++;

            return (int)((completedFields / (double)totalFields) * 100);
        }
    }

    // Tab 1: Motiv Prezentare & Antecedente
    private string MotivPrezentare { get; set; } = string.Empty;
    private string AntecedentePatologice { get; set; } = string.Empty;
    private string TratamenteActuale { get; set; } = string.Empty;

    // Tab 2: Examen Clinic & Investigații
    private int? TensiuneSistolica { get; set; }
    private int? TensiuneDiastolica { get; set; }
    private int? Puls { get; set; }
    private decimal? Temperatura { get; set; }
    private int? FreqventaRespiratorie { get; set; }
    private decimal? Greutate { get; set; }
    private decimal? Inaltime { get; set; }
    private string ExamenObiectiv { get; set; } = string.Empty;
    private string InvestigatiiParaclinice { get; set; } = string.Empty;

    // Exam Clinic Grid - New Fields
    private string StareGenerala { get; set; } = string.Empty;
    private string Tegumente { get; set; } = string.Empty;
    private string Mucoase { get; set; } = string.Empty;
    private int? SpO2 { get; set; }
    private string Edeme { get; set; } = string.Empty;

    // IMC Calculation
    private decimal? IMC
    {
        get
        {
            if (Greutate.HasValue && Inaltime.HasValue && Inaltime.Value > 0)
            {
                var inaltimeMetri = Inaltime.Value / 100m;
                return Greutate.Value / (inaltimeMetri * inaltimeMetri);
            }
            return null;
        }
    }

    private string IMCCategory
    {
        get
        {
            if (!IMC.HasValue) return string.Empty;

            return IMC.Value switch
            {
                < 18.5m => "imc-underweight",
                >= 18.5m and < 25m => "imc-normal",
                >= 25m and < 30m => "imc-overweight",
                _ => "imc-obese"
            };
        }
    }

    private string IMCText
    {
        get
        {
            if (!IMC.HasValue) return string.Empty;

            return IMC.Value switch
            {
                < 18.5m => "Subponderal",
                >= 18.5m and < 25m => "Normal",
                >= 25m and < 30m => "Supraponderal",
                _ => "Obez"
            };
        }
    }

    private string IMCIcon
    {
        get
        {
            if (!IMC.HasValue) return "fa-question";

            return IMC.Value switch
            {
                < 18.5m => "fa-arrow-down",
                >= 18.5m and < 25m => "fa-check-circle",
                >= 25m and < 30m => "fa-arrow-up",
                _ => "fa-exclamation-triangle"
            };
        }
    }

    // Tab 3: Diagnostic & Tratament
    private string DiagnosticPrincipal { get; set; } = string.Empty;
    private string DiagnosticSecundar { get; set; } = string.Empty;
    private string PlanTerapeutic { get; set; } = string.Empty;
    private string Recomandari { get; set; } = string.Empty;

    // Tab 4: Concluzii
    private string Concluzii { get; set; } = string.Empty;
    private DateTime? DataUrmatoareiVizite { get; set; }
    private string NoteUrmatoareaVizita { get; set; } = string.Empty;

    // Lifecycle
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Initializing Consultatii page for PacientId: {PacientId}", PacientId);

            if (!PacientId.HasValue)
            {
                HasError = true;
                ErrorMessage = "ID pacient lipsește. Vă rugăm selectați un pacient din listă.";
                IsLoading = false;
                return;
            }

            // Load patient data (mock for now)
            await LoadPacientData();

            // Start consultation timer
            StartConsultationTimer();

            IsLoading = false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing Consultatii page");
            HasError = true;
            ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            IsLoading = false;
        }
    }

    private async Task LoadPacientData()
    {
        // TODO: Replace with MediatR GetPacientDataQuery
        // For now, mock data
        await Task.Delay(500); // Simulate loading

        PacientData = new PacientDataDto
        {
            NumeComplet = "Ionescu Maria",
            CNP = "2850315123456",
            Varsta = 39,
            Sex = "F",
            Telefon = "0721 234 567",
            Allergies = "Penicilină",
            ChronicConditions = "Hipertensiune"
        };

        Logger.LogInformation("Patient data loaded: {NumeComplet}", PacientData.NumeComplet);
    }

    private void StartConsultationTimer()
    {
        TimerStartTime = DateTime.Now;
        IsTimerRunning = true;

        ConsultationTimer = new Timer(_ =>
        {
            ElapsedTime = DateTime.Now - TimerStartTime;
            InvokeAsync(StateHasChanged);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        Logger.LogInformation("Consultation timer started at {StartTime}", TimerStartTime);
    }

    private void StopConsultationTimer()
    {
        IsTimerRunning = false;
        ConsultationTimer?.Dispose();
        ConsultationTimer = null;

        Logger.LogInformation("Consultation timer stopped. Duration: {Duration}", ElapsedTime);
    }

    // Tab Management
    private void SwitchTab(int tabNumber)
    {
        if (tabNumber < 1 || tabNumber > 4) return;

        ActiveTab = tabNumber;
        Logger.LogInformation("Switched to tab {TabNumber}", tabNumber);
    }

    // Actions
    private async Task HandleSaveDraft()
    {
        try
        {
            IsSaving = true;
            Logger.LogInformation("Saving consultation draft...");

            // TODO: Implement SaveConsultatieDraftCommand via MediatR
            await Task.Delay(1000); // Simulate save

            LastSaveTime = DateTime.Now;
            Logger.LogInformation("Consultation draft saved successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving consultation draft");
            // TODO: Show error toast
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleFinalize()
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(MotivPrezentare))
            {
                // TODO: Show validation error for MotivPrezentare
                Logger.LogWarning("Validation failed: MotivPrezentare is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(DiagnosticPrincipal))
            {
                // TODO: Show validation error for DiagnosticPrincipal
                Logger.LogWarning("Validation failed: DiagnosticPrincipal is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(PlanTerapeutic))
            {
                // TODO: Show validation error for PlanTerapeutic
                Logger.LogWarning("Validation failed: PlanTerapeutic is required");
                return;
            }

            IsSaving = true;
            Logger.LogInformation("Finalizing consultation...");

            // Stop timer
            StopConsultationTimer();

            // TODO: Implement FinalizeConsulatieCommand via MediatR
            await Task.Delay(1500); // Simulate finalize

            Logger.LogInformation("Consultation finalized successfully. Duration: {Duration}", ElapsedTime);

            // Navigate back
            NavigationManager.NavigateTo("/pacienti/vizualizare");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finalizing consultation");
            // TODO: Show error toast
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void HandleBack()
    {
        StopConsultationTimer();
        NavigationManager.NavigateTo("/pacienti/vizualizare");
    }

    private async Task HandleGenerateLetter()
    {
        try
        {
            Logger.LogInformation("Generating medical letter...");

            // TODO: Navigate to Scrisoare Medicală page with consultation data
            // For now, just log and show placeholder
            await Task.CompletedTask;

            Logger.LogInformation("Medical letter generation requested");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating medical letter");
        }
    }

    // Dispose
    public void Dispose()
    {
        StopConsultationTimer();
        Logger.LogInformation("Consultatii component disposed");
    }

    // DTO (temporary - will be moved to Application layer)
    private class PacientDataDto
    {
        public string NumeComplet { get; set; } = string.Empty;
        public string CNP { get; set; } = string.Empty;
        public int Varsta { get; set; }
        public string Sex { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
    }
}
