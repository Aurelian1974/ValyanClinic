using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using Syncfusion.Blazor.Schedule;
using Syncfusion.Blazor.DropDowns;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using Syncfusion.Blazor.Notifications;

namespace ValyanClinic.Components.Pages.Programari.Modals;

public partial class ProgramareSchedulerModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ProgramareSchedulerModal> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback OnProgramareCreated { get; set; }

    // ==================== STATE ====================
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;
    private bool ShowProgramareModal { get; set; }
    private Guid? SelectedProgramareId { get; set; }

    // ==================== SCHEDULER ====================
    private SfSchedule<ProgramareEventDto>? SchedulerRef { get; set; }
    private DateTime SelectedDate { get; set; } = DateTime.Today;

    // Use View enum directly
    private View CurrentViewEnum { get; set; } = View.Week;
    private string CurrentView { get; set; } = "Week";

    private Guid? SelectedDoctorId { get; set; }

    // Pentru cell click - data/ora selectată
    private DateTime? SelectedCellStartTime { get; set; }
    private DateTime? SelectedCellEndTime { get; set; }

    // ==================== DATA ====================
    private List<ProgramareEventDto> EventsList { get; set; } = new();
    private List<DoctorDropdownDto> DoctorsList { get; set; } = new();
    private List<DoctorResourceDto> DoctorResources { get; set; } = new();
    private int TotalProgramariAfisate => EventsList.Count;

    // ==================== DROPDOWN OPTIONS ====================
    private List<ViewOption> ViewOptions { get; set; } = new()
    {
     new ViewOption { Value = "Day", Text = "Zi" },
new ViewOption { Value = "Week", Text = "Săptămână" },
        new ViewOption { Value = "WorkWeek", Text = "Săptămână Lucrătoare" },
   new ViewOption { Value = "Month", Text = "Lună" },
        new ViewOption { Value = "Agenda", Text = "Agendă" }
    };

    // ==================== TOAST ====================
    private Syncfusion.Blazor.Notifications.SfToast? ToastRef { get; set; }

    // ==================== LIFECYCLE ====================
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("ProgramareSchedulerModal initialized");

        if (IsVisible)
        {
            await LoadData();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && EventsList.Count == 0)
        {
            await LoadData();
        }
    }

    // ==================== DATA LOADING ====================
    private async Task LoadData()
    {
        IsLoading = true;
        HasError = false;
        StateHasChanged();

        try
        {
            await LoadDoctors();
            await LoadProgramari();

            Logger.LogInformation("Scheduler data loaded: {Count} programări, {DoctorCount} doctori",
         EventsList.Count, DoctorsList.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading scheduler data");
            HasError = true;
            ErrorMessage = "Eroare la încărcarea datelor.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadDoctors()
    {
        try
        {
            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = 1,
                PageSize = 100,
                GlobalSearchText = null,
                FilterEsteActiv = true
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                // ✅ FIX: result.Value is already IEnumerable<PersonalMedicalListDto>
                var personalList = result.Value.ToList();

                DoctorsList = personalList
             .Where(d => d.Pozitie == "Doctor" || d.Pozitie == "Medic")
            .Select(d => new DoctorDropdownDto
            {
                PersonalID = d.PersonalID,
                NumeComplet = $"Dr. {d.Nume} {d.Prenume}",
                Specializare = d.Specializare
            })
             .ToList();

                DoctorResources = DoctorsList.Select((d, index) => new DoctorResourceDto
                {
                    Id = index + 1,
                    Text = d.NumeComplet,
                    Color = GetDoctorColor(index),
                    DoctorGuid = d.PersonalID
                }).ToList();

                Logger.LogInformation("Loaded {Count} doctors", DoctorsList.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading doctors");
            throw;
        }
    }

    private async Task LoadProgramari()
    {
        try
        {
            var query = new GetProgramariByDateQuery(SelectedDate, SelectedDoctorId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                EventsList = result.Value.Select(p => new ProgramareEventDto
                {
                    Id = p.ProgramareID,
                    Subject = p.PacientNumeComplet ?? "Pacient Necunoscut",
                    StartTime = p.DataProgramare.Date + p.OraInceput,
                    EndTime = p.DataProgramare.Date + p.OraSfarsit,
                    Description = p.Observatii,
                    IsAllDay = false,
                    Status = p.Status,
                    TipProgramare = p.TipProgramare,
                    PacientId = p.PacientID,
                    PacientName = p.PacientNumeComplet,
                    PacientTelefon = p.PacientTelefon,
                    DoctorId = GetDoctorResourceId(p.DoctorID),
                    DoctorGuid = p.DoctorID,
                    DoctorName = p.DoctorNumeComplet,
                    CategoryColor = GetStatusColor(p.Status)
                }).ToList();

                Logger.LogInformation("Loaded {Count} programări", EventsList.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading programări");
            throw;
        }
    }

    // ==================== TOOLBAR EVENTS ====================

    private async Task OnDoctorFilterChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid?, DoctorDropdownDto> args)
    {
        Logger.LogInformation("Doctor filter changed: {DoctorId}", args.Value);
        SelectedDoctorId = args.Value;
        await LoadProgramari();
        StateHasChanged();
    }

    private async Task OnViewChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, ViewOption> args)
    {
        Logger.LogInformation("View changed: {View}", args.Value);
        CurrentView = args.Value;
        CurrentViewEnum = GetViewEnum(args.Value);
        StateHasChanged();
    }

    private async Task OnTodayClicked()
    {
        Logger.LogInformation("Today button clicked");
        SelectedDate = DateTime.Today;
        await LoadProgramari();
        StateHasChanged();
    }

    // ==================== SCHEDULER EVENTS (SIMPLIFIED) ====================

    private void OnCellClicked(CellClickEventArgs args)
    {
        try
        {
            Logger.LogInformation("Cell clicked at: {StartTime}", args.StartTime);

            var clickedDate = args.StartTime;
            var endTime = clickedDate.AddMinutes(30);

            SelectedProgramareId = null;
            SelectedCellStartTime = clickedDate;
            SelectedCellEndTime = endTime;
            ShowProgramareModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnCellClicked");
        }
    }

    private void OnEventClicked(EventClickArgs<ProgramareEventDto> args)
    {
        try
        {
            var programare = args.Event;
            Logger.LogInformation("Event clicked: {ProgramareID}", programare.Id);

            SelectedProgramareId = programare.Id;
            SelectedCellStartTime = null;
            SelectedCellEndTime = null;
            ShowProgramareModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnEventClicked");
        }
    }

    // ✅ REMOVED: ActionBegin/ActionComplete/EventRendered - not working properly

    // ==================== PROGRAMARE ACTIONS ====================

    private void CreateNewProgramare()
    {
        Logger.LogInformation("Create new programare");
        SelectedProgramareId = null;
        SelectedCellStartTime = null;
        SelectedCellEndTime = null;
        ShowProgramareModal = true;
        StateHasChanged();
    }

    private void OpenEditModal(Guid? programareId)
    {
        if (programareId == null) return;

        Logger.LogInformation("Opening edit modal: {Id}", programareId);
        SelectedProgramareId = programareId;
        SelectedCellStartTime = null;
        SelectedCellEndTime = null;
        ShowProgramareModal = true;
        StateHasChanged();
    }

    private void ViewProgramareDetails(Guid? programareId)
    {
        if (programareId == null) return;
        Logger.LogInformation("Viewing details: {Id}", programareId);
    }

    private async Task OnProgramareSaved()
    {
        Logger.LogInformation("Programare saved");
        ShowProgramareModal = false;
        await LoadProgramari();
        await OnProgramareCreated.InvokeAsync();
        StateHasChanged();
    }

    // ==================== HELPERS ====================

    private View GetViewEnum(string viewString)
    {
        return viewString switch
        {
            "Day" => View.Day,
            "Week" => View.Week,
            "WorkWeek" => View.WorkWeek,
            "Month" => View.Month,
            "Agenda" => View.Agenda,
            _ => View.Week
        };
    }

    private int GetDoctorResourceId(Guid doctorGuid)
    {
        var resource = DoctorResources.FirstOrDefault(r => r.DoctorGuid == doctorGuid);
        return resource?.Id ?? 1;
    }

    private string GetDoctorColor(int index)
    {
        var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6", "#ef4444", "#06b6d4", "#ec4899", "#84cc16" };
        return colors[index % colors.Length];
    }

    private string GetStatusColor(string? status)
    {
        return status switch
        {
            "Programata" => "#3b82f6",
            "Confirmata" => "#10b981",
            "CheckedIn" => "#f59e0b",
            "InConsultatie" => "#8b5cf6",
            "Finalizata" => "#6b7280",
            "Anulata" => "#ef4444",
            _ => "#3b82f6"
        };
    }

    private string GetStatusBorderColor(string? status)
    {
        return status switch
        {
            "Programata" => "#2563eb",
            "Confirmata" => "#059669",
            "CheckedIn" => "#d97706",
            "InConsultatie" => "#7c3aed",
            "Finalizata" => "#4b5563",
            "Anulata" => "#dc2626",
            _ => "#2563eb"
        };
    }

    private string GetStatusDisplayText(string? status)
    {
        return status switch
        {
            "Programata" => "Programată",
            "Confirmata" => "Confirmată",
            "CheckedIn" => "Check-in",
            "InConsultatie" => "În consultație",
            "Finalizata" => "Finalizată",
            "Anulata" => "Anulată",
            _ => status ?? "Necunoscut"
        };
    }

    // ==================== MODAL CONTROL ====================

    private async Task Close()
    {
        Logger.LogInformation("Closing scheduler modal");
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task HandleOverlayClick()
    {
        Logger.LogDebug("Overlay clicked");
    }

    // ==================== DTOs ====================

    public class ViewOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class DoctorDropdownDto
    {
        public Guid PersonalID { get; set; }
        public string NumeComplet { get; set; } = string.Empty;
        public string? Specializare { get; set; }
    }

    public class DoctorResourceDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public Guid DoctorGuid { get; set; }
    }
}

// ==================== PROGRAMARE EVENT DTO ====================
public class ProgramareEventDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public bool IsAllDay { get; set; }
    public string Status { get; set; } = "Programata";
    public string? TipProgramare { get; set; }
    public Guid PacientId { get; set; }
    public string? PacientName { get; set; }
    public string? PacientTelefon { get; set; }
    public int DoctorId { get; set; }
    public Guid DoctorGuid { get; set; }
    public string? DoctorName { get; set; }
    public string CategoryColor { get; set; } = "#3b82f6";
}
