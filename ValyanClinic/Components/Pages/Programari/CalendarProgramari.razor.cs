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
   private bool ShowAddEditModal = false;
    private bool ShowViewModal = false;
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
      DoctorsList = result.Value.ToList();
 Logger.LogInformation("Loaded {Count} doctors", DoctorsList.Count);
    }
    }

    private async Task LoadCalendarData()
    {
        try
        {
       IsLoading = true;
    var weekStart = GetWeekStart();
         
          Logger.LogInformation("🔍 Loading calendar data for week starting: {WeekStart}", weekStart.ToString("yyyy-MM-dd"));
          
            var programariTasks = new List<Task<ValyanClinic.Application.Common.Results.Result<IEnumerable<ProgramareListDto>>>>();

            for (int i = 0; i < 7; i++)
            {
   var date = weekStart.AddDays(i);
      Logger.LogInformation("📅 Querying date: {Date}", date.ToString("yyyy-MM-dd"));
         programariTasks.Add(Mediator.Send(new GetProgramariByDateQuery(date, FilterDoctorID)));
            }

            var results = await Task.WhenAll(programariTasks);
  
            Logger.LogInformation("✅ Received {Count} results from queries", results.Length);
       
        var allProgramari = results
        .Where(r => r.IsSuccess && r.Value != null)
                .SelectMany(r => r.Value!)
   .ToList();

            Logger.LogInformation("📋 Total programari from DB: {Count}", allProgramari.Count);

       if (allProgramari.Any())
  {
         Logger.LogInformation("📝 First programare: Pacient={Pacient}, Data={Data}, Ora={Ora}", 
          allProgramari.First().PacientNumeComplet,
         allProgramari.First().DataProgramare.ToString("yyyy-MM-dd"),
    allProgramari.First().OraInceput.ToString(@"hh\:mm"));
         }

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
    
   if (EventsList.Any())
     {
     var firstEvent = EventsList.First();
   Logger.LogInformation("🎯 First event: Subject={Subject}, StartTime={Start}, EndTime={End}, Color={Color}", 
    firstEvent.Subject,
 firstEvent.StartTime.ToString("yyyy-MM-dd HH:mm"),
   firstEvent.EndTime.ToString("yyyy-MM-dd HH:mm"),
                  firstEvent.CategoryColor);
     }
            else
      {
            Logger.LogWarning("⚠️ EventsList is EMPTY!");
            }
        }
        catch (Exception ex)
     {
       Logger.LogError(ex, "❌ Error loading calendar data");
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
