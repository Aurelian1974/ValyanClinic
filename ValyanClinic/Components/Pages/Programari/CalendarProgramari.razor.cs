using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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

    // Data
    private List<ProgramareListDto> AllProgramari = new();
    private List<PersonalMedicalListDto> DoctorsList = new();

    // Filters
    private Guid? FilterDoctorID;
    private DateTime SelectedDate = DateTime.Today;

    // UI State
    private bool IsLoading = true;
    private bool ShowAddEditModal = false;
 private bool ShowViewModal = false;
    private Guid? SelectedProgramareId = null;

    protected override async Task OnInitializedAsync()
    {
     try
        {
     Logger.LogInformation("Inițializare CalendarProgramari");

      await LoadDoctorsListAsync();
    await LoadCalendarData();
        }
   catch (Exception ex)
     {
    Logger.LogError(ex, "Eroare la inițializarea CalendarProgramari");
      await NotificationService.ShowErrorAsync("Eroare la încărcarea calendarului!");
        }
        finally
   {
  IsLoading = false;
   }
    }

    private async Task LoadDoctorsListAsync()
    {
 try
   {
  var query = new GetPersonalMedicalListQuery
    {
       PageNumber = 1,
     PageSize = 1000
       };

        var result = await Mediator.Send(query);

   if (result.IsSuccess && result.Value != null)
    {
       DoctorsList = result.Value.ToList();
        }
 }
   catch (Exception ex)
  {
      Logger.LogError(ex, "Eroare la încărcarea medicilor");
 }
  }

    private async Task LoadCalendarData()
    {
        try
 {
   IsLoading = true;

  // Calculate week range
            var weekStart = GetWeekStart();
      var weekEnd = weekStart.AddDays(4); // Luni-Vineri

    Logger.LogInformation("Încărcare calendar: {WeekStart} - {WeekEnd}, Doctor={Doctor}",
   weekStart.ToString("yyyy-MM-dd"), weekEnd.ToString("yyyy-MM-dd"), FilterDoctorID);

   var query = new GetProgramariByDateQuery(weekStart, FilterDoctorID)
            {
       // Query will be called for each day in the week
  };

       // Load programari for entire week
       var programariTasks = new List<Task<ValyanClinic.Application.Common.Results.Result<IEnumerable<ProgramareListDto>>>>();
  
       for (int i = 0; i < 5; i++)
         {
      var date = weekStart.AddDays(i);
 var dayQuery = new GetProgramariByDateQuery(date, FilterDoctorID);
       programariTasks.Add(Mediator.Send(dayQuery));
  }

   var results = await Task.WhenAll(programariTasks);

      AllProgramari = results
   .Where(r => r.IsSuccess && r.Value != null)
    .SelectMany(r => r.Value!)
       .ToList();

   Logger.LogInformation("Încărcate {Count} programări pentru săptămână", AllProgramari.Count);
        }
   catch (Exception ex)
   {
     Logger.LogError(ex, "Eroare la încărcarea datelor calendar");
     await NotificationService.ShowErrorAsync("Eroare la încărcarea programărilor!");
 }
     finally
        {
        IsLoading = false;
}
    }

    private DateTime GetWeekStart()
    {
   // Get Monday of the week containing SelectedDate
        var diff = (7 + (SelectedDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        return SelectedDate.AddDays(-1 * diff).Date;
    }

    private List<ProgramareListDto> GetSlotProgramari(DateTime date, int hour)
    {
  var slotStart = new TimeSpan(hour, 0, 0);
   var slotEnd = new TimeSpan(hour + 1, 0, 0);

        return AllProgramari
         .Where(p => p.DataProgramare.Date == date.Date &&
          p.OraInceput >= slotStart &&
  p.OraInceput < slotEnd)
    .OrderBy(p => p.OraInceput)
  .ToList();
    }

    private string GetShortName(string? numeComplet)
    {
    if (string.IsNullOrEmpty(numeComplet))
       return "-";

     var parts = numeComplet.Split(' ');
        if (parts.Length >= 2)
      return $"{parts[0]} {parts[1][0]}.";
 
return numeComplet;
    }

 private string GetDoctorInitials(string? numeComplet)
  {
  if (string.IsNullOrEmpty(numeComplet))
        return "?";

        var parts = numeComplet.Split(' ');
     if (parts.Length >= 2)
    return $"{parts[0][0]}{parts[1][0]}";
        
  return numeComplet.Substring(0, Math.Min(2, numeComplet.Length));
    }

    private async Task GoToToday()
    {
        SelectedDate = DateTime.Today;
   await LoadCalendarData();
    }

    private void NavigateToList()
    {
 NavigationManager.NavigateTo("/programari/lista");
    }

    private void OpenAddModal()
    {
     SelectedProgramareId = null;
        ShowAddEditModal = true;
    }

    private void OpenViewModal(Guid programareId)
    {
     SelectedProgramareId = programareId;
ShowViewModal = true;
    }

    private async Task HandleModalSaved()
  {
        ShowAddEditModal = false;
   await LoadCalendarData();
   await NotificationService.ShowSuccessAsync("Programarea a fost salvată cu succes!");
    }

    private void HandleEditRequested(Guid programareId)
    {
     ShowViewModal = false;
     SelectedProgramareId = programareId;
    ShowAddEditModal = true;
    }
}
