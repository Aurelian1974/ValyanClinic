using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Schedule;
using Syncfusion.Blazor.DropDowns;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;
using ValyanClinic.Services;

namespace ValyanClinic.Components.Pages.Programari;

public partial class CalendarProgramari : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<CalendarProgramari> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ValyanClinic.Services.Email.IEmailService EmailService { get; set; } = default!;

    // Scheduler
    private SfSchedule<ProgramareEventDto>? SchedulerRef;
    private View CurrentView = View.Week;
    private DateTime SelectedDate = DateTime.Today;

    // Data
    private List<ProgramareEventDto> EventsList = new();
    private List<PersonalMedicalListDto> DoctorsList = new();
    private Guid? FilterDoctorID;
    private int TotalProgramari => EventsList.Count;

    // UI State
    private bool IsLoading = true;
    private bool IsSendingEmails = false; // ✅ NEW - pentru spinner button
    private bool ShowAddEditModal = false;
    private bool ShowViewModal = false;
    private bool ShowSendEmailModal = false; // ✅ NEW - pentru email modal
    private Guid? SelectedProgramareId;
    private DateTime? SelectedCellStartTime;
    private DateTime? SelectedCellEndTime;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadDoctorsListAsync();
            await LoadCalendarData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing calendar");
            await NotificationService.ShowErrorAsync("Eroare la încărcarea calendarului!");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDoctorsListAsync()
    {
        var query = new GetPersonalMedicalListQuery { PageNumber = 1, PageSize = 1000, FilterEsteActiv = true };
        var result = await Mediator.Send(query);

        if (result.IsSuccess && result.Value != null)
        {
            // ✅ Filter ONLY doctors (Pozitie starts with "Medic")
            DoctorsList = result.Value
                    .Where(p => !string.IsNullOrEmpty(p.Pozitie) && p.Pozitie.StartsWith("Medic", StringComparison.OrdinalIgnoreCase))
                 .ToList();

            Logger.LogInformation("Loaded {Count} doctors (filtered from {Total} total personal)",
          DoctorsList.Count, result.Value.Count());
        }
    }

    private async Task LoadCalendarData()
    {
        try
        {
            IsLoading = true;
            var weekStart = GetWeekStart();
            var weekEnd = weekStart.AddDays(6); // Duminică

            Logger.LogInformation("⚡ OPTIMIZED: Loading calendar data for week: {WeekStart} - {WeekEnd}",
                  weekStart.ToString("yyyy-MM-dd"), weekEnd.ToString("yyyy-MM-dd"));

            // ✅ NEW - SINGLE QUERY pentru întreaga săptămână (7 zile)
            // PERFORMANCE: 7 queries → 1 query = ~85% faster load time
            var weekQuery = new ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByWeek.GetProgramariByWeekQuery(
              weekStart,
             weekEnd,
           FilterDoctorID
           );

            var result = await Mediator.Send(weekQuery);

            if (!result.IsSuccess || result.Value == null)
            {
                Logger.LogWarning("Failed to load programări for week");
                EventsList = new List<ProgramareEventDto>();
                return;
            }

            var allProgramari = result.Value.ToList();
            Logger.LogInformation("✅ Loaded {Count} programări in SINGLE query", allProgramari.Count);

            EventsList = allProgramari.Select(p => new ProgramareEventDto
            {
                Id = p.ProgramareID,
                Subject = p.PacientNumeComplet ?? "Pacient",
                StartTime = p.DataProgramare.Date + p.OraInceput,
                EndTime = p.DataProgramare.Date + p.OraSfarsit,
                Description = p.Observatii,
                IsAllDay = false,
                Status = p.Status,
                CategoryColor = GetStatusColor(p.Status),
                TipProgramare = p.TipProgramare,
                DoctorName = p.DoctorNumeComplet
            }).ToList();

            Logger.LogInformation("✅ EventsList populated with {Count} events", EventsList.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error loading calendar data");
            EventsList = new List<ProgramareEventDto>();
        }
        finally
        {
            IsLoading = false;
            Logger.LogInformation("🏁 LoadCalendarData finished. IsLoading={IsLoading}, EventsCount={Count}", IsLoading, EventsList.Count);
        }
    }

    private DateTime GetWeekStart()
    {
        var dayOfWeek = SelectedDate.DayOfWeek;
        int daysFromMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
        return SelectedDate.AddDays(-daysFromMonday).Date;
    }

    private async Task OnDoctorFilterChanged(ChangeEventArgs<Guid?, PersonalMedicalListDto> args)
    {
        FilterDoctorID = args.Value;
        await LoadCalendarData();
    }

    private async Task GoToToday()
    {
        SelectedDate = DateTime.Today;
        await LoadCalendarData();
    }

    private async Task PreviousWeek()
    {
        SelectedDate = SelectedDate.AddDays(-7);
        await LoadCalendarData();
    }

    private async Task NextWeek()
    {
        SelectedDate = SelectedDate.AddDays(7);
        await LoadCalendarData();
    }

    private void OnCellClicked(CellClickEventArgs args)
    {
        SelectedProgramareId = null;
        SelectedCellStartTime = args.StartTime;
        SelectedCellEndTime = args.StartTime.AddMinutes(30);
        ShowAddEditModal = true;
    }

    private void OnEventClicked(EventClickArgs<ProgramareEventDto> args)
    {
        // Single click - QuickInfo will show automatically via ShowQuickInfo="true"
        // We store the ID in case user wants to open modal from QuickInfo
        SelectedProgramareId = args.Event.Id;
        Logger.LogInformation("Event clicked: {Subject}", args.Event.Subject);
    }

    private void OnPopupOpen(PopupOpenEventArgs<ProgramareEventDto> args)
    {
        // Control which popup to show based on user interaction
        // QuickInfo popup type is "QuickInfo"
        // Editor popup type is "Editor"

        if (args.Type == PopupType.QuickInfo)
        {
            // Allow QuickInfo to show on single click
            Logger.LogInformation("QuickInfo popup opening for: {Subject}", args.Data?.Subject);
            // args.Cancel = false; (default - allow it)
        }
        else if (args.Type == PopupType.Editor)
        {
            // Cancel default editor - we use our custom modal instead
            args.Cancel = true;

            if (args.Data != null)
            {
                SelectedProgramareId = args.Data.Id;
                ShowViewModal = true;
                Logger.LogInformation("Opening custom modal for: {Subject}", args.Data.Subject);
            }
        }
    }

    private void OpenViewModal(Guid programareId)
    {
        SelectedProgramareId = programareId;
        SelectedCellStartTime = null;
        SelectedCellEndTime = null;
        ShowViewModal = true;
    }

    private void OpenEditModal(Guid programareId)
    {
        SelectedProgramareId = programareId;
        SelectedCellStartTime = null;
        SelectedCellEndTime = null;
        ShowAddEditModal = true;
    }

    private void OnEventRendered(EventRenderedArgs<ProgramareEventDto> args)
    {
        Logger.LogInformation("🎨 OnEventRendered called for: {Subject} at {Time}",
    args.Data.Subject,
              args.Data.StartTime.ToString("yyyy-MM-dd HH:mm"));

        var color = args.Data.CategoryColor;
        args.Attributes ??= new Dictionary<string, object>();

        // Set background color
        args.Attributes["style"] = $"background-color: {color} !important; border-left: 4px solid {color} !important;";

        // ADD data-id attribute so we can identify appointment in tooltip
        args.Attributes["data-appointment-id"] = args.Data.Id.ToString();
        args.Attributes["data-subject"] = args.Data.Subject;

        Logger.LogInformation("🎨 Applied color: {Color} and data-id: {Id} to appointment", color, args.Data.Id);
    }

    private void NavigateToList() => NavigationManager.NavigateTo("/programari/lista");

    private void OpenAddModal()
    {
        SelectedProgramareId = null;
        SelectedCellStartTime = null;
        SelectedCellEndTime = null;
        ShowAddEditModal = true;
    }

    private async Task HandleModalSaved()
    {
        ShowAddEditModal = false;
        await LoadCalendarData();
        await NotificationService.ShowSuccessAsync("Programarea a fost salvată!");
    }

    private void HandleEditRequested(Guid programareId)
    {
        ShowViewModal = false;
        SelectedProgramareId = programareId;
        ShowAddEditModal = true;
    }

    private string GetStatusColor(string? status) => status switch
    {
        "Programata" => "#3b82f6",
        "Confirmata" => "#10b981",
        "CheckedIn" => "#f59e0b",
        "InConsultatie" => "#8b5cf6",
        "Finalizata" => "#6b7280",
        "Anulata" => "#ef4444",
        _ => "#3b82f6"
    };

    private string GetStatusDisplayName(string? status) => status switch
    {
        "Programata" => "Programată",
        "Confirmata" => "Confirmată",
        "CheckedIn" => "Check-in",
        "InConsultatie" => "În consultație",
        "Finalizata" => "Finalizată",
        "Anulata" => "Anulată",
        _ => status ?? "Necunoscut"
    };

    /// <summary>
    /// ✅ NEW - Deschide modal composer pentru email programări
    /// </summary>
    private void OpenSendEmailModal()
    {
        Logger.LogInformation("📧 Opening send daily email modal");
        ShowSendEmailModal = true;
    }

    /// <summary>
    /// ✅ NEW - Callback după trimiterea email-urilor din modal
    /// </summary>
    private async Task HandleEmailsSent(int emailsSent)
    {
        ShowSendEmailModal = false;

        if (emailsSent > 0)
        {
            await NotificationService.ShowSuccessAsync(
           $"✅ Trimise {emailsSent} email-uri cu succes!",
              "Email-uri trimise");

            Logger.LogInformation("✅ Trimise {Count} email-uri cu succes", emailsSent);
        }
        else
        {
            await NotificationService.ShowWarningAsync(
         "⚠️ Nu s-au putut trimite email-urile.",
       "Atenție");

            Logger.LogWarning("⚠️ Nu s-au trimis email-uri");
        }
    }
}
public class ProgramareEventDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public bool IsAllDay { get; set; }
    public string Status { get; set; } = "Programata";
    public string CategoryColor { get; set; } = "#3b82f6";
    public string? TipProgramare { get; set; }
    public string? DoctorName { get; set; }
}
