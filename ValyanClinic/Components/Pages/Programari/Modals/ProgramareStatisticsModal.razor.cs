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

    // ✅ Filters - CONSISTENT cu ListaProgramari (prima zi luna curentă)
    private DateTime FilterDataStart;
    private DateTime FilterDataEnd;
    private Guid? FilterDoctorID = null;

    protected override void OnInitialized()
    {
        // ✅ Set default dates to FIRST and LAST day of current month
        var today = DateTime.Today;
        FilterDataStart = new DateTime(today.Year, today.Month, 1); // Prima zi a lunii
        FilterDataEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)); // Ultima zi a lunii

        Logger.LogInformation("ProgramareStatisticsModal initialized with dates: {Start} - {End}",
  FilterDataStart.ToString("yyyy-MM-dd"), FilterDataEnd.ToString("yyyy-MM-dd"));
    }

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
            new ChartData { Label = "Programate", Value = Statistics.Programate, Color = "#94a3b8" },
            new ChartData { Label = "Confirmate", Value = Statistics.Confirmate, Color = "#3b82f6" },
            new ChartData { Label = "Check-in", Value = Statistics.CheckedIn, Color = "#1e40af" },
      new ChartData { Label = "În consultație", Value = Statistics.InConsultatie, Color = "#f59e0b" },
      new ChartData { Label = "Finalizate", Value = Statistics.Finalizate, Color = "#10b981" },
  new ChartData { Label = "Anulate", Value = Statistics.Anulate, Color = "#ef4444" },
        new ChartData { Label = "Nu s-au prezentat", Value = Statistics.NoShow, Color = "#64748b" }
     }.Where(x => x.Value > 0).ToList();
    }

    private List<ChartData> GetTipData()
    {
        if (Statistics == null) return new List<ChartData>();

        return new List<ChartData>
        {
            new ChartData { Label = "Consultații Inițiale", Value = Statistics.ConsultatiiInitiale, Color = "#3b82f6" },
         new ChartData { Label = "Controale Periodice", Value = Statistics.ControalePeriodice, Color = "#06b6d4" },
            new ChartData { Label = "Consultații", Value = Statistics.Consultatii, Color = "#8b5cf6" },
    new ChartData { Label = "Investigații", Value = Statistics.Investigatii, Color = "#f59e0b" },
        new ChartData { Label = "Proceduri", Value = Statistics.Proceduri, Color = "#10b981" },
     new ChartData { Label = "Urgențe", Value = Statistics.Urgente, Color = "#ef4444" },
            new ChartData { Label = "Telemedicină", Value = Statistics.Telemedicina, Color = "#6366f1" },
            new ChartData { Label = "La Domiciliu", Value = Statistics.LaDomiciliu, Color = "#ec4899" }
      }.Where(x => x.Value > 0).ToList();
    }

    private async Task CloseModal()
    {
        Statistics = null;
        await IsVisibleChanged.InvokeAsync(false);
    }

    /// <summary>
    /// Obține numele complet al medicului selectat pentru tooltip.
    /// </summary>
    private string GetSelectedDoctorName()
    {
        if (!FilterDoctorID.HasValue)
            return "Toți medicii";

        var doctor = DoctorsList.FirstOrDefault(d => d.PersonalID == FilterDoctorID.Value);
        return doctor != null ? $"Dr. {doctor.Nume} {doctor.Prenume}" : "Toți medicii";
    }

    // ✅ Helper class for chart data with colors
    private class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = "#3b82f6";
        public double Percentage { get; set; }
    }
}
