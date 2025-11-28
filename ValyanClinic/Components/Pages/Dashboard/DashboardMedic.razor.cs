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
    
    // ✅ Variabile STATICE partajate între toate instanțele (persist între navigări)
    private static List<ProgramareListDto> _sharedProgramari = new();
    private static DateTime _lastLoadTime = DateTime.MinValue;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
    
    // Programari - folosește lista partajată
    private List<ProgramareListDto> ToateProgramarile
    {
        get => _sharedProgramari;
        set => _sharedProgramari = value;
    }
    
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

    // ✅ Flag simplu pentru a preveni multiple inițializări în aceeași instanță
    private bool _hasInitialized = false;

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("[DashboardMedic] OnInitializedAsync START");
        
        // ✅ Previne multiple inițializări în ACEEAȘI instanță
        if (_hasInitialized)
        {
            Logger.LogInformation("[DashboardMedic] Already initialized in this instance, skipping");
            return;
        }

        _hasInitialized = true;

        try
        {
            await LoadDoctorInfo();
            
            if (PersonalMedicalID.HasValue)
            {
                Logger.LogInformation("[DashboardMedic] PersonalMedicalID set, checking cache");
                
                // ✅ Verifică dacă trebuie să reîncarce datele (cache expirat sau listă goală)
                var cacheAge = DateTime.Now - _lastLoadTime;
                var needsReload = ToateProgramarile.Count == 0 || cacheAge > CacheExpiration;
                
                if (needsReload)
                {
                    Logger.LogInformation("[DashboardMedic] Cache expired or empty, loading data");
                    await LoadProgramariAstazi();
                    _lastLoadTime = DateTime.Now;
                }
                else
                {
                    Logger.LogInformation("[DashboardMedic] Using cached data ({Count} programări, age: {Age})", 
                        ToateProgramarile.Count, cacheAge);
                    
                    // ✅ Forțează UI update când folosim cache
                    StateHasChanged();
                }
            }
            else
            {
                Logger.LogWarning("[DashboardMedic] PersonalMedicalID not available after LoadDoctorInfo");
            }
            
            await LoadActivitatiRecente();
            LoadChartData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DashboardMedic] Error during initialization");
        }
        
        Logger.LogInformation("[DashboardMedic] OnInitializedAsync END");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        Logger.LogInformation("[DashboardMedic] OnAfterRenderAsync - First render completed");
        
        // Forțează re-render pentru a asigura că UI-ul este actualizat
        StateHasChanged();
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

            Logger.LogInformation("[DashboardMedic] ===== LoadProgramariAstazi START =====");
            Logger.LogInformation("[DashboardMedic] PersonalMedicalID: {PersonalMedicalID}", PersonalMedicalID);
            Logger.LogInformation("[DashboardMedic] FilterDataStart: {FilterDataStart}", DateTime.Today);
            Logger.LogInformation("[DashboardMedic] FilterDataEnd: {FilterDataEnd}", DateTime.Today.AddDays(1));
            Logger.LogInformation("[DashboardMedic] Current Time: {CurrentTime}", DateTime.Now);

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
                
                Logger.LogInformation("[DashboardMedic] ===== PROGRAMĂRI ÎNCĂRCATE =====");
                Logger.LogInformation("[DashboardMedic] Total programări: {Count}", ToateProgramarile.Count);
                
                foreach (var prog in ToateProgramarile)
                {
                    Logger.LogInformation("[DashboardMedic] Programare: {Pacient} - Ora: {Ora} - Status: {Status}", 
                        prog.PacientNumeComplet, 
                        prog.OraInceput, 
                        prog.Status);
                }
                
                Logger.LogInformation("[DashboardMedic] ===== END PROGRAMĂRI =====");
            }
            else
            {
                Logger.LogWarning("[DashboardMedic] Failed to load programari: {Errors}", 
                    string.Join(", ", result.Errors));
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
            Logger.LogInformation("[DashboardMedic] ===== LoadProgramariAstazi END =====");
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
        
        if (programare == null)
        {
            Logger.LogWarning("[DashboardMedic] Programare not found: {ProgramareId}", programareId);
            return;
        }
        
        if (!PersonalMedicalID.HasValue)
        {
            Logger.LogWarning("[DashboardMedic] PersonalMedicalID is null");
            return;
        }
        
        if (ConsultatieModalRef == null)
        {
            Logger.LogError("[DashboardMedic] ConsultatieModalRef is null!");
            return;
        }
        
        try
        {
            // Seteaza parametrii modalului
            ConsultatieModalRef.ProgramareID = programareId;
            ConsultatieModalRef.PacientID = programare.PacientID;
            ConsultatieModalRef.MedicID = PersonalMedicalID.Value;
            
            Logger.LogInformation("[DashboardMedic] Opening modal with ProgramareID={ProgramareId}, PacientID={PacientId}, MedicID={MedicId}",
                programareId, programare.PacientID, PersonalMedicalID.Value);
            
            // Deschide modalul
            await ConsultatieModalRef.Open();
            
            // Force UI update
            await InvokeAsync(StateHasChanged);
            
            Logger.LogInformation("[DashboardMedic] Modal opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DashboardMedic] Error opening consultatie modal");
        }
    }

    private async Task OnConsultatieCompleted()
    {
        Logger.LogInformation("[DashboardMedic] Consultatie completed, refreshing data...");
        await RefreshData();
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
