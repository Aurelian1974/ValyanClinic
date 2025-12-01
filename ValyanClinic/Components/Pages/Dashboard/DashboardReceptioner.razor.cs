using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;
using ValyanClinic.Application.Features.DashboardManagement.Queries.GetReceptionerStats;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;

namespace ValyanClinic.Components.Pages.Dashboard;

public partial class DashboardReceptioner : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<DashboardReceptioner> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    // State
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private string UserName { get; set; } = "Receptioner";

    // Modal state
    private bool ShowAddPacientModal { get; set; }
    private bool ShowSchedulerModal { get; set; } // ✅ ADDED

    // Statistics - DATELE REALE DIN DB
    private int ProgramariAstazi { get; set; }
    private int ProgramariGrowth { get; set; }
    private int PacientiInAsteptare { get; set; }
    private int TimpMediuAsteptare { get; set; }
    private int ProgramariFinalizate { get; set; }
    private int ProgramariRamase { get; set; }
    private int PacientiNoi { get; set; }

    // Data Lists
    private List<ProgramareDto>? ProgramariUrmatoare { get; set; }
    private List<MedicDto>? MediciAstazi { get; set; }
    private List<NotificareDto>? Notificari { get; set; }

    // Search
    private string SearchText { get; set; } = string.Empty;
    private List<PacientSearchDto>? SearchResults { get; set; }
    private Timer? _searchDebounceTimer;
    private const int SearchDebounceMs = 300;

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("========== DashboardReceptioner OnInitializedAsync START ==========");

        try
        {
            await LoadUserInfo();
            await LoadDashboardData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing dashboard");
            HasError = true;
            ErrorMessage = "Eroare la încărcarea dashboard-ului. Vă rugăm reîncercați.";
        }
        finally
        {
            IsLoading = false;
            Logger.LogInformation("========== DashboardReceptioner OnInitializedAsync END ==========");
        }
    }

    private async Task LoadUserInfo()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                // Default fallback
                UserName = user.Identity.Name ?? "Receptioner";

                // ✅ Get PersonalMedicalID from claims and load full name
                var personalMedicalIdClaim = user.FindFirst("PersonalMedicalID")?.Value;
                if (!string.IsNullOrEmpty(personalMedicalIdClaim) && Guid.TryParse(personalMedicalIdClaim, out Guid personalMedicalId))
                {
                    Logger.LogInformation("Loading PersonalMedical details for ID: {PersonalMedicalID}", personalMedicalId);
                    await LoadPersonalMedicalDetails(personalMedicalId);
                }
                else
                {
                    Logger.LogWarning("PersonalMedicalID not found in claims for user: {Username}", UserName);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user info");
        }
    }

    private async Task LoadPersonalMedicalDetails(Guid personalMedicalId)
    {
        try
        {
            var query = new GetPersonalMedicalByIdQuery(personalMedicalId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                // ✅ Update UserName with full name (COMPACT - fără titlu Dr./As.)
                UserName = result.Value.NumeComplet;

                Logger.LogInformation("Loaded PersonalMedical full name: {NumeComplet}", UserName);
            }
            else
            {
                Logger.LogWarning("Failed to load PersonalMedical details for ID: {PersonalMedicalID}", personalMedicalId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading PersonalMedical details");
        }
    }

    private async Task LoadDashboardData()
    {
        Logger.LogInformation("Loading dashboard data from database...");

        try
        {
            // ✅ 1. STATISTICI REALE din DB
            await LoadStatisticsFromDatabase();

            // ✅ 2. PROGRAMĂRI URMĂTOARE din DB
            await LoadProgramariUrmatoareFromDatabase();

            // TODO: 3. Medici astăzi (mock deocamdată - trebuie implementat query)
            await LoadMediciAstazi();

            // TODO: 4. Notificări (mock deocamdată - trebuie implementat sistem de notificări)
            await LoadNotificari();

            Logger.LogInformation("Dashboard data loaded successfully from database");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dashboard data");
            throw;
        }
    }

    private async Task LoadStatisticsFromDatabase()
    {
        try
        {
            // ✅ Folosește query-ul real pentru statistici
            var query = new GetReceptionerStatsQuery(DateTime.Today);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var stats = result.Value;

                ProgramariAstazi = stats.ProgramariAstazi;
                ProgramariGrowth = stats.ProgramariGrowth;
                PacientiInAsteptare = stats.PacientiInAsteptare;
                TimpMediuAsteptare = stats.TimpMediuAsteptare;
                ProgramariFinalizate = stats.ProgramariFinalizate;
                ProgramariRamase = stats.ProgramariRamase;
                PacientiNoi = stats.PacientiNoi;

                Logger.LogInformation(
        "✅ STATISTICI REALE: Astăzi={Astazi}, Growth={Growth}%, InAșteptare={InAsteptare}, Finalizate={Finalizate}, Noi={Noi}",
              ProgramariAstazi, ProgramariGrowth, PacientiInAsteptare, ProgramariFinalizate, PacientiNoi);
            }
            else
            {
                Logger.LogWarning("Failed to load statistics: {Errors}", string.Join(", ", result.Errors));
                // Fallback la valori 0
                ProgramariAstazi = 0;
                ProgramariGrowth = 0;
                PacientiInAsteptare = 0;
                TimpMediuAsteptare = 0;
                ProgramariFinalizate = 0;
                ProgramariRamase = 0;
                PacientiNoi = 0;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading statistics from database");
            throw;
        }
    }

    private async Task LoadProgramariUrmatoareFromDatabase()
    {
        try
        {
            // ✅ Folosește query-ul real pentru programări
            var query = new GetProgramariByDateQuery(DateTime.Today);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                ProgramariUrmatoare = result.Value
            .Where(p => p.OraInceput >= DateTime.Now.TimeOfDay) // Doar programările viitoare
              .OrderBy(p => p.OraInceput)
           .Take(10) // Primele 10
                 .Select(p => new ProgramareDto
                 {
                     ProgramareID = p.ProgramareID,
                     OraInceput = p.OraInceput,
                     Durata = (int)(p.OraSfarsit - p.OraInceput).TotalMinutes,
                     PacientNumeComplet = p.PacientNumeComplet ?? "Unknown",
                     DoctorNumeComplet = p.DoctorNumeComplet ?? "Unknown",
                     DoctorSpecializare = p.DoctorSpecializare ?? "",
                     TipProgramare = p.TipProgramare,
                     Status = p.Status
                 })
             .ToList();

                Logger.LogInformation("✅ PROGRAMĂRI REALE: Loaded {Count} programări următoare", ProgramariUrmatoare.Count);
            }
            else
            {
                Logger.LogWarning("Failed to load programări: {Errors}", string.Join(", ", result.Errors));
                ProgramariUrmatoare = new List<ProgramareDto>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading programări from database");
            throw;
        }
    }

    private async Task LoadMediciAstazi()
    {
        // TODO: Implementare query real pentru medici activi astăzi
        // Pentru moment folosim mock data
        await Task.Delay(200);

        MediciAstazi = new List<MedicDto>
        {
    new MedicDto
     {
                PersonalID = Guid.NewGuid(),
     NumeComplet = "Dr. A. Marinescu",
     Specializare = "Cardiologie",
          EsteActiv = true
      },
       new MedicDto
 {
    PersonalID = Guid.NewGuid(),
        NumeComplet = "Dr. M. Ionescu",
  Specializare = "Dermatologie",
      EsteActiv = true
            },
  new MedicDto
            {
            PersonalID = Guid.NewGuid(),
    NumeComplet = "Dr. V. Radu",
       Specializare = "Oftalmologie",
                EsteActiv = false
            }
        };
    }

    private async Task LoadNotificari()
    {
        // TODO: Implementare sistem de notificări real
        // Pentru moment folosim mock data
        await Task.Delay(200);

        Notificari = new List<NotificareDto>
        {
            new NotificareDto
     {
           Tip = "confirmare",
              Titlu = "Confirmare necesară",
    Mesaj = "5 programări necesită confirmare",
     Data = DateTime.Now.AddMinutes(-30)
            },
    new NotificareDto
            {
        Tip = "intarziere",
    Titlu = "Întârziere",
      Mesaj = "Dr. Ionescu are 10 minute întârziere",
         Data = DateTime.Now.AddMinutes(-15)
       },
 new NotificareDto
 {
       Tip = "plata",
                Titlu = "Plată efectuată",
       Mesaj = "Ion Popescu - 250 RON",
    Data = DateTime.Now.AddMinutes(-5)
      }
        };
    }

    // Helper Methods
    private string GetCurrentDate()
    {
        var now = DateTime.Now;
        var dayOfWeek = now.ToString("dddd", new System.Globalization.CultureInfo("ro-RO"));
        var date = now.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("ro-RO"));
        return $"{dayOfWeek}, {date}";
    }

    private string GetProgramareStatusClass(string status)
    {
        return status.ToLower() switch
        {
            "confirmata" => "status-confirmed",
            "inasteptare" => "status-waiting",
            "finalizata" => "status-completed",
            "anulata" => "status-cancelled",
            _ => ""
        };
    }

    private string GetStatusDisplayText(string status)
    {
        return status switch
        {
            "Confirmata" => "Confirmată",
            "InAsteptare" => "În Așteptare",
            "Finalizata" => "Finalizată",
            "Anulata" => "Anulată",
            _ => status
        };
    }

    private string GetNotificareTypeClass(string tip)
    {
        return $"notif-{tip.ToLower()}";
    }

    private string GetNotificareIcon(string tip)
    {
        return tip.ToLower() switch
        {
            "confirmare" => "check-circle",
            "intarziere" => "clock",
            "plata" => "dollar-sign",
            "urgent" => "exclamation-triangle",
            _ => "bell"
        };
    }

    private string GetRelativeTime(DateTime date)
    {
        var diff = DateTime.Now - date;

        if (diff.TotalMinutes < 1)
            return "Acum";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h";
        return date.ToString("dd MMM");
    }

    private string GetRandomAvatarColor()
    {
        var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6", "#ef4444" };
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }

    private string GetInitials(string numeComplet)
    {
        if (string.IsNullOrEmpty(numeComplet))
            return "??";

        var parts = numeComplet.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0].Substring(0, 2).ToUpper();
        return "??";
    }

    // Event Handlers
    private async Task HandleNewAppointment()
    {
        Logger.LogInformation("Opening scheduler modal for new appointment");
        ShowSchedulerModal = true; // ✅ Deschide scheduler modal
        StateHasChanged();
    }

    private async Task HandleNewPatient()
    {
        Logger.LogInformation("Opening add patient modal");
        ShowAddPacientModal = true; // ✅ Deschide modalul
    }

    // Callback după salvarea pacientului
    private async Task OnPacientSaved()
    {
        Logger.LogInformation("Pacient saved successfully - refreshing dashboard");
        ShowAddPacientModal = false;

        // Reload statistici pentru a reflecta noul pacient
        await LoadDashboardData();
        StateHasChanged();
    }

    // ✅ ADDED: Callback după crearea programării
    private async Task OnProgramareCreated()
    {
        Logger.LogInformation("Programare created successfully - refreshing dashboard");
        ShowSchedulerModal = false;

        // Reload statistici și programări
        await LoadDashboardData();
        StateHasChanged();
    }

    private async Task HandleCheckIn()
    {
        Logger.LogInformation("Opening check-in modal");
        // TODO: Show check-in modal
    }

    private async Task HandlePayment()
    {
        Logger.LogInformation("Opening payment modal");
        // TODO: Show payment modal
    }

    private async Task HandleProgramareCheckIn(Guid programareId)
    {
        Logger.LogInformation("Check-in programare: {ProgramareID}", programareId);
        // TODO: Implement check-in logic
    }

    private async Task HandleStartConsultatie(Guid programareId)
    {
        Logger.LogInformation("Start consultație: {ProgramareID}", programareId);
        // TODO: Implement start consultație
    }

    private async Task HandleProgramareDetails(Guid programareId)
    {
        Logger.LogInformation("View programare details: {ProgramareID}", programareId);
        NavigationManager.NavigateTo($"/programari/detalii/{programareId}");
    }

    private async Task HandleSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults = null;
            return;
        }

        Logger.LogInformation("Searching for: {SearchText}", SearchText);

        // TODO: Implement real search query
        // var query = new SearchPacientiQuery { SearchTerm = SearchText, Top = 5 };
        // var result = await Mediator.Send(query);

        // Mock data
        await Task.Delay(300);
        SearchResults = new List<PacientSearchDto>
        {
          new PacientSearchDto
      {
 Id = Guid.NewGuid(),
 NumeComplet = "Ion Popescu",
    CNP = "1234567890123",
     Telefon = "0722123456"
          }
        };
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await HandleSearch();
        }
        else
        {
            // Debounce search
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(async _ => await InvokeAsync(async () => await HandleSearch()), null, SearchDebounceMs, Timeout.Infinite);
        }
    }

    private void HandleSelectPatient(Guid pacientId)
    {
        Logger.LogInformation("Selected patient: {PacientID}", pacientId);
        NavigationManager.NavigateTo($"/pacienti/vizualizare/{pacientId}");
    }

    public void Dispose()
    {
        _searchDebounceTimer?.Dispose();
    }

    // DTOs (temporary - should be in Application layer)
    public class ProgramareDto
    {
        public Guid ProgramareID { get; set; }
        public TimeSpan OraInceput { get; set; }
        public int Durata { get; set; }
        public string PacientNumeComplet { get; set; } = string.Empty;
        public string DoctorNumeComplet { get; set; } = string.Empty;
        public string DoctorSpecializare { get; set; } = string.Empty;
        public string? TipProgramare { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MedicDto
    {
        public Guid PersonalID { get; set; }
        public string NumeComplet { get; set; } = string.Empty;
        public string Specializare { get; set; } = string.Empty;
        public bool EsteActiv { get; set; }
    }

    public class NotificareDto
    {
        public string Tip { get; set; } = string.Empty;
        public string Titlu { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public DateTime Data { get; set; }
    }

    public class PacientSearchDto
    {
        public Guid Id { get; set; }
        public string NumeComplet { get; set; } = string.Empty;
        public string? CNP { get; set; }
        public string? Telefon { get; set; }
    }
}
