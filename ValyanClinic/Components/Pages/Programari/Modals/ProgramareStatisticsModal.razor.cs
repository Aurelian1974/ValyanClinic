using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareStatistics;

namespace ValyanClinic.Components.Pages.Programari.Modals;

public partial class ProgramareStatisticsModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ProgramareStatisticsModal> Logger { get; set; } = default!;

    private bool IsLoading = false;
    private ProgramareStatisticsDto? Statistics = null;
    private List<PersonalMedicalListDto> DoctorsList = new();

    // Filters
    private DateTime FilterDataStart = DateTime.Today.AddMonths(-1);
    private DateTime FilterDataEnd = DateTime.Today;
    private Guid? FilterDoctorID = null;

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
 {
            Logger.LogInformation("ProgramareStatisticsModal opened");
     await LoadDoctorsListAsync();
    await LoadStatistics();
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

private async Task LoadStatistics()
    {
        try
        {
   IsLoading = true;

     var query = new GetProgramareStatisticsQuery(
            FilterDataStart,
       FilterDataEnd,
 FilterDoctorID);

      var result = await Mediator.Send(query);

if (result.IsSuccess && result.Value != null)
{
                Statistics = result.Value;
      Logger.LogInformation("Statistici încărcate: {Total} programări", Statistics.TotalProgramari);
            }
   else
       {
       Logger.LogWarning("Nu s-au putut încărca statisticile");
            }
    }
   catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la încărcarea statisticilor");
        }
  finally
        {
 IsLoading = false;
 }
  }

    private List<ChartData> GetStatusData()
    {
        if (Statistics == null) return new List<ChartData>();

        return new List<ChartData>
        {
    new ChartData { Status = "Programate", Count = Statistics.Programate },
      new ChartData { Status = "Confirmate", Count = Statistics.Confirmate },
   new ChartData { Status = "Check-in", Count = Statistics.CheckedIn },
         new ChartData { Status = "În consultație", Count = Statistics.InConsultatie },
          new ChartData { Status = "Finalizate", Count = Statistics.Finalizate },
            new ChartData { Status = "Anulate", Count = Statistics.Anulate },
     new ChartData { Status = "Nu s-au prezentat", Count = Statistics.NoShow }
        }.Where(x => x.Count > 0).ToList();
    }

    private List<ChartData> GetTipData()
    {
   if (Statistics == null) return new List<ChartData>();

      return new List<ChartData>
        {
new ChartData { Tip = "Consultații Inițiale", Count = Statistics.ConsultatiiInitiale },
  new ChartData { Tip = "Controale Periodice", Count = Statistics.ControalePeriodice },
    new ChartData { Tip = "Consultații", Count = Statistics.Consultatii },
 new ChartData { Tip = "Investigații", Count = Statistics.Investigatii },
            new ChartData { Tip = "Proceduri", Count = Statistics.Proceduri },
   new ChartData { Tip = "Urgențe", Count = Statistics.Urgente },
  new ChartData { Tip = "Telemedicină", Count = Statistics.Telemedicina },
       new ChartData { Tip = "La Domiciliu", Count = Statistics.LaDomiciliu }
        }.Where(x => x.Count > 0).ToList();
    }

    private async Task CloseModal()
 {
   Statistics = null;
 await IsVisibleChanged.InvokeAsync(false);
    }

  // Helper class for chart data
    private class ChartData
    {
        public string Status { get; set; } = string.Empty;
 public string Tip { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
