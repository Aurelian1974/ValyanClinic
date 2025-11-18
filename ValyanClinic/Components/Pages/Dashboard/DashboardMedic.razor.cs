using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareList;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;
using ValyanClinic.Components.Pages.Dashboard.Modals;

namespace ValyanClinic.Components.Pages.Dashboard;

public partial class DashboardMedic : ComponentBase
{
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<DashboardMedic> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    // Modal reference
    private ConsultatieModal? ConsultatieModalRef { get; set; }

    // State
    private string DoctorName { get; set; } = "Doctor";
    private string Specializare { get; set; } = "";
    private string CurrentDate => DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("ro-RO"));
    
    private bool IsLoading { get; set; } = false;
    private Guid? PersonalMedicalID { get; set; }
    
    // Programari
    private List<ProgramareListDto> ToateProgramarile { get; set; } = new();
    private int TotalProgramariAstazi => ToateProgramarile.Count;
    private string SearchText { get; set; } = "";
    private string StatusFilter { get; set; } = "";
    
    private List<ProgramareListDto> FilteredProgramari
    {
        get
        {
            var filtered = ToateProgramarile.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    (p.PacientNumeComplet?.ToLower().Contains(search) ?? false) ||
                    (p.Observatii?.ToLower().Contains(search) ?? false)
                );
            }

            // Status filter
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                filtered = filtered.Where(p => p.Status == StatusFilter);
            }

            return filtered.OrderBy(p => p.OraInceput).ToList();
        }
    }

    // Activitati Recente
    private List<ActivitateRecentaModel> ActivitatiRecente { get; set; } = new();

    // Chart Data
    private List<ChartDataModel> ChartData { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("[DashboardMedic] OnInitializedAsync START");
        
        await LoadDoctorInfo();
        await LoadProgramariAstazi();
        await LoadActivitatiRecente();
        LoadChartData();
        
        Logger.LogInformation("[DashboardMedic] OnInitializedAsync END");
    }

    private async Task LoadDoctorInfo()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var personalMedicalIdClaim = user.FindFirst("PersonalMedicalID")?.Value;
                
                if (!string.IsNullOrEmpty(personalMedicalIdClaim) && 
                    Guid.TryParse(personalMedicalIdClaim, out Guid personalMedicalId))
                {
                    PersonalMedicalID = personalMedicalId;
                    
                    var query = new GetPersonalMedicalByIdQuery(personalMedicalId);
                    var result = await Mediator.Send(query);

                    if (result.IsSuccess && result.Value != null)
                    {
                        DoctorName = $"Dr. {result.Value.NumeComplet}";
                        Specializare = result.Value.Specializare ?? "Medicina Generala";
                        
                        Logger.LogInformation("[DashboardMedic] Loaded doctor info: {Name}", DoctorName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DashboardMedic] Error loading doctor info");
        }
    }

    private async Task LoadProgramariAstazi()
    {
        if (!PersonalMedicalID.HasValue)
        {
            Logger.LogWarning("[DashboardMedic] PersonalMedicalID is null, cannot load programari");
            return;
        }

        try
        {
            IsLoading = true;
            StateHasChanged();

            var query = new GetProgramareListQuery
            {
                PageNumber = 1,
                PageSize = 1000,
                FilterDataStart = DateTime.Today,
                FilterDataEnd = DateTime.Today.AddDays(1),
                FilterDoctorID = PersonalMedicalID,
                SortColumn = "OraInceput",
                SortDirection = "ASC"
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                ToateProgramarile = result.Value.ToList();
                Logger.LogInformation("[DashboardMedic] Loaded {Count} programari for today", ToateProgramarile.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DashboardMedic] Error loading programari");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadActivitatiRecente()
    {
        // Mock data - în viitor va veni din baza de date
        ActivitatiRecente = new List<ActivitateRecentaModel>
        {
            new() { Tip = "consultatie", Descriere = "Consultatie finalizata - Popescu Maria", Data = DateTime.Now.AddMinutes(-15) },
            new() { Tip = "analize", Descriere = "Rezultate analize incarcate - Dumitru Ana", Data = DateTime.Now.AddMinutes(-30) },
            new() { Tip = "reteta", Descriere = "Reteta electronica emisa - Stan Elena", Data = DateTime.Now.AddHours(-1) },
            new() { Tip = "programare", Descriere = "Programare noua - Marinescu Andrei", Data = DateTime.Now.AddHours(-2) }
        };

        await Task.CompletedTask;
    }

    private void LoadChartData()
    {
        // Mock data - în viitor va veni din baza de date
        ChartData = new List<ChartDataModel>
        {
            new() { Zi = "Luni", NumarConsultatii = 12 },
            new() { Zi = "Marti", NumarConsultatii = 15 },
            new() { Zi = "Miercuri", NumarConsultatii = 10 },
            new() { Zi = "Joi", NumarConsultatii = 14 },
            new() { Zi = "Vineri", NumarConsultatii = 9 }
        };
    }

    private async Task RefreshData()
    {
        Logger.LogInformation("[DashboardMedic] Refreshing data...");
        await LoadProgramariAstazi();
        await LoadActivitatiRecente();
        LoadChartData();
    }

    private string GetStatusClass(string status)
    {
        return status?.ToLower() switch
        {
            "confirmata" => "status-confirmata",
            "in asteptare" => "status-asteptare",
            "consulta" => "status-consulta",
            _ => ""
        };
    }

    private string GetActivityIconClass(string tip)
    {
        return tip?.ToLower() switch
        {
            "consultatie" => "icon-success",
            "analize" => "icon-info",
            "reteta" => "icon-warning",
            "programare" => "icon-primary",
            _ => "icon-default"
        };
    }

    private string GetActivityIcon(string tip)
    {
        return tip?.ToLower() switch
        {
            "consultatie" => "fas fa-check-circle",
            "analize" => "fas fa-vial",
            "reteta" => "fas fa-prescription",
            "programare" => "fas fa-calendar-plus",
            _ => "fas fa-info-circle"
        };
    }

    private void OpenDosarPacient(Guid pacientId)
    {
        Logger.LogInformation("[DashboardMedic] Opening dosar for pacient: {PacientId}", pacientId);
        NavigationManager.NavigateTo($"/pacienti/dosar/{pacientId}");
    }

    private async Task StartConsultatie(Guid programareId)
    {
        Logger.LogInformation("[DashboardMedic] Starting consultatie: {ProgramareId}", programareId);
        
        // Gaseste programarea pentru a obtine PacientID
        var programare = ToateProgramarile.FirstOrDefault(p => p.ProgramareID == programareId);
        
        if (programare != null && PersonalMedicalID.HasValue && ConsultatieModalRef != null)
        {
            // Seteaza parametrii modalului
            ConsultatieModalRef.ProgramareID = programareId;
            ConsultatieModalRef.PacientID = programare.PacientID;
            ConsultatieModalRef.MedicID = PersonalMedicalID.Value;
            
            // Deschide modalul
            await ConsultatieModalRef.Open();
        }
        else
        {
            Logger.LogWarning("[DashboardMedic] Cannot open consultatie modal - missing data");
        }
    }

    private async Task OnConsultatieCompleted()
    {
        Logger.LogInformation("[DashboardMedic] Consultatie completed, refreshing data...");
        await RefreshData();
    }

    private void CallPacient(string telefon)
    {
        Logger.LogInformation("[DashboardMedic] Calling pacient: {Telefon}", telefon);
        // În viitor poate integra cu sistem VoIP
    }

    // Models
    private class ActivitateRecentaModel
    {
        public string Tip { get; set; } = "";
        public string Descriere { get; set; } = "";
        public DateTime Data { get; set; }
    }

    private class ChartDataModel
    {
        public string Zi { get; set; } = "";
        public int NumarConsultatii { get; set; }
    }
}
