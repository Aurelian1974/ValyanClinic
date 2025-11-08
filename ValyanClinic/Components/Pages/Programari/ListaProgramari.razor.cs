using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Grids;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Application.Features.ProgramareManagement.Commands.DeleteProgramare;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareList;
using ValyanClinic.Services;

namespace ValyanClinic.Components.Pages.Programari;

public partial class ListaProgramari : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<ListaProgramari> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private ValyanClinic.Services.Export.IExcelExportService ExcelExportService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Grid reference
    private SfGrid<ProgramareListDto>? GridRef;

    // Data collections
    private List<ProgramareListDto> FilteredProgramari = new();
    private List<PersonalMedicalListDto> DoctorsList = new();
    private List<PacientListDto> PacientiList = new();

    // Filter state
    private string? SearchText;
    private Guid? FilterDoctorID;
    private Guid? FilterPacientID;
    private DateTime? FilterDataStart;
    private DateTime? FilterDataEnd;
    private string? FilterStatus;
    private string? FilterTipProgramare;

    // ✅ Advanced Filter Panel State
    private bool IsAdvancedFilterExpanded = false;

    // UI state
    private bool IsLoading = true;
    private bool HasError = false;
    private string ErrorMessage = string.Empty;
    private bool HasActiveFilters => !string.IsNullOrEmpty(SearchText) ||
        FilterDoctorID.HasValue ||
    FilterPacientID.HasValue ||
        FilterDataStart.HasValue ||
        FilterDataEnd.HasValue ||
        !string.IsNullOrEmpty(FilterStatus) ||
    !string.IsNullOrEmpty(FilterTipProgramare);

    // Modal state
    private bool ShowAddEditModal = false;
    private bool ShowViewModal = false;
    private bool ShowCancelModal = false;
 private bool ShowStatisticsModal = false;
    private Guid? SelectedProgramareId = null;
    private string CancelConfirmMessage = string.Empty;
    private ProgramareListDto? SelectedProgramareForCancel = null;

    // Debounce for search
    private System.Threading.Timer? _searchDebounceTimer;
    private const int DEBOUNCE_DELAY_MS = 500;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Inițializare ListaProgramari");

    // Set default filters
      FilterDataStart = DateTime.Today;
        FilterDataEnd = DateTime.Today.AddDays(30);

      // Load dropdown data
            await Task.WhenAll(
           LoadDoctorsListAsync(),
    LoadPacientiListAsync()
            );

       // Load initial data
          await LoadDataAsync();
        }
        catch (Exception ex)
        {
    Logger.LogError(ex, "Eroare la inițializarea paginii ListaProgramari");
     HasError = true;
      ErrorMessage = "Eroare la încărcarea datelor. Vă rugăm să reîncărcați pagina.";
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
        PageSize = 1000 // Load all for dropdown
       };

  var result = await Mediator.Send(query);

     if (result.IsSuccess && result.Value != null)
  {
         DoctorsList = result.Value.ToList();
      Logger.LogInformation("Încărcați {Count} medici pentru dropdown", DoctorsList.Count);
   }
      }
        catch (Exception ex)
   {
     Logger.LogError(ex, "Eroare la încărcarea listei de medici");
  }
    }

    private async Task LoadPacientiListAsync()
    {
        try
        {
     var query = new GetPacientListQuery
            {
PageNumber = 1,
    PageSize = 1000, // Load all for dropdown
    Activ = true
      };

      var result = await Mediator.Send(query);

    if (result.IsSuccess && result.Value != null && result.Value.Value != null)
    {
      PacientiList = result.Value.Value.ToList();
      Logger.LogInformation("Încărcați {Count} pacienți pentru dropdown", PacientiList.Count);
}
  }
        catch (Exception ex)
        {
  Logger.LogError(ex, "Eroare la încărcarea listei de pacienți");
 }
    }

    private async Task LoadDataAsync()
    {
        try
    {
        IsLoading = true;
            HasError = false;

            Logger.LogInformation(
                "Încărcare programări: Search={Search}, Doctor={Doctor}, Pacient={Pacient}, DataStart={DataStart}, DataEnd={DataEnd}, Status={Status}, Tip={Tip}",
 SearchText, FilterDoctorID, FilterPacientID, FilterDataStart, FilterDataEnd, FilterStatus, FilterTipProgramare);

   var query = new GetProgramareListQuery
     {
         PageNumber = 1,
           PageSize = 1000, // Load all for client-side filtering in grid
              GlobalSearchText = SearchText,
 FilterDoctorID = FilterDoctorID,
    FilterPacientID = FilterPacientID,
 FilterDataStart = FilterDataStart,
     FilterDataEnd = FilterDataEnd,
      FilterStatus = FilterStatus,
                FilterTipProgramare = FilterTipProgramare,
  SortColumn = "DataProgramare",
      SortDirection = "ASC"
            };

            var result = await Mediator.Send(query);

         if (result.IsSuccess && result.Value != null)
      {
       FilteredProgramari = result.Value.ToList();
   Logger.LogInformation("Încărcate {Count} programări", FilteredProgramari.Count);
         }
  else
      {
    HasError = true;
    ErrorMessage = "Eroare la încărcarea programărilor.";
       Logger.LogWarning("Eroare la încărcarea programărilor");
      }

        }
    catch (Exception ex)
        {
  Logger.LogError(ex, "Eroare la încărcarea programărilor");
            HasError = true;
            ErrorMessage = "Eroare neașteptată la încărcarea programărilor.";
        }
        finally
        {
        IsLoading = false;
        }
    }

    private async Task ApplyFilters()
 {
   await LoadDataAsync();
    }

    // ✅ Toggle Advanced Filter Panel
    private void ToggleAdvancedFilter()
 {
        IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
        Logger.LogInformation("Advanced filter panel toggled: {State}", 
            IsAdvancedFilterExpanded ? "Expanded" : "Collapsed");
    }

    private void HandleSearchKeyUp()
    {
      _searchDebounceTimer?.Dispose();
        _searchDebounceTimer = new System.Threading.Timer(async _ =>
   {
      await InvokeAsync(async () =>
       {
          await ApplyFilters();
         StateHasChanged();
            });
        }, null, DEBOUNCE_DELAY_MS, Timeout.Infinite);
    }

    private async Task ClearSearch()
    {
        SearchText = null;
        await ApplyFilters();
    }

    private async Task ClearAllFilters()
    {
        SearchText = null;
        FilterDoctorID = null;
      FilterPacientID = null;
        FilterDataStart = DateTime.Today;
    FilterDataEnd = DateTime.Today.AddDays(30);
     FilterStatus = null;
        FilterTipProgramare = null;

      await ApplyFilters();
    }

    private void OpenAddModal()
    {
        SelectedProgramareId = null;
        ShowAddEditModal = true;
    }

    private void OpenEditModal(Guid programareId)
    {
        SelectedProgramareId = programareId;
     ShowAddEditModal = true;
    }

    private void OpenViewModal(Guid programareId)
    {
        SelectedProgramareId = programareId;
        ShowViewModal = true;
 }

    private void OpenCancelModal(ProgramareListDto programare)
    {
        SelectedProgramareId = programare.ProgramareID;
        SelectedProgramareForCancel = programare;
  CancelConfirmMessage = $"Sigur doriți să anulați programarea pentru {programare.PacientNumeComplet} " +
              $"cu Dr. {programare.DoctorNumeComplet} din data de {programare.DataProgramare:dd.MM.yyyy} " +
               $"la ora {programare.OraInceput:hh\\:mm}?";
        ShowCancelModal = true;
    }

    private void OpenStatisticsModal()
    {
        Logger.LogInformation("Opening statistics modal");
        ShowStatisticsModal = true;
    }

    private async Task ExportToExcel()
  {
        try
        {
      Logger.LogInformation("Starting Excel export for {Count} programări", FilteredProgramari.Count);

      if (!FilteredProgramari.Any())
    {
     await NotificationService.ShowWarningAsync("Nu există programări de exportat!");
         return;
   }

       // Generate Excel file
       var excelBytes = await ExcelExportService.ExportProgramariToExcelAsync(FilteredProgramari);

      // Generate filename cu data curentă
   var fileName = $"Programari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

    // Download file prin JavaScript
  await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelBytes);

    await NotificationService.ShowSuccessAsync($"Export Excel generat: {FilteredProgramari.Count} programări");
   }
        catch (Exception ex)
{
    Logger.LogError(ex, "Eroare la exportul Excel");
         await NotificationService.ShowErrorAsync("Eroare la generarea exportului Excel!");
    }
    }

    private void PrintProgramare(Guid programareId)
    {
     Logger.LogInformation("Print requested for programare {ID}", programareId);
        // TODO: Implementare print functionality
        // Pentru moment, redirect către view modal sau print page
      NavigationManager.NavigateTo($"/programari/print/{programareId}");
    }

    private async Task HandleModalSaved()
    {
        ShowAddEditModal = false;
        await LoadDataAsync();
     await NotificationService.ShowSuccessAsync("Programarea a fost salvată cu succes!");
    }

private async Task HandleCancelConfirmed()
    {
        try
        {
            if (!SelectedProgramareId.HasValue)
            {
  await NotificationService.ShowErrorAsync("ID programare invalid!");
    return;
            }

          var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst("PersonalID");
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
     await NotificationService.ShowErrorAsync("Nu s-a putut identifica utilizatorul curent!");
     return;
         }

            var command = new DeleteProgramareCommand
            {
      ProgramareID = SelectedProgramareId.Value,
              ModificatDe = userId
            };

            var result = await Mediator.Send(command);

    if (result.IsSuccess)
  {
       ShowCancelModal = false;
      await LoadDataAsync();
       await NotificationService.ShowSuccessAsync("Programarea a fost anulată cu succes!");
  }
 else
    {
   await NotificationService.ShowErrorAsync(result.Errors?.FirstOrDefault() ?? "Eroare la anularea programării!");
  }
    }
        catch (Exception ex)
        {
         Logger.LogError(ex, "Eroare la anularea programării {ProgramareID}", SelectedProgramareId);
            await NotificationService.ShowErrorAsync("Eroare neașteptată la anularea programării!");
        }
  }

 private void HandleEditRequested(Guid programareId)
    {
 Logger.LogInformation("Edit requested from ViewModal for programare {ID}", programareId);
    ShowViewModal = false;
  SelectedProgramareId = programareId;
   ShowAddEditModal = true;
    }

    private bool CanEditProgramare(string status)
 {
        return status is "Programata" or "Confirmata";
    }

    private bool CanCancelProgramare(string status)
    {
        return status is "Programata" or "Confirmata" or "CheckedIn";
    }

    private string GetStatusDisplay(string? status)
  {
 return status switch
        {
   "Programata" => "Programată",
          "Confirmata" => "Confirmată",
"CheckedIn" => "Check-in",
   "InConsultatie" => "În consultație",
            "Finalizata" => "Finalizată",
     "Anulata" => "Anulată",
 "NoShow" => "Nu s-a prezentat",
            _ => status ?? "-"
 };
    }

    private string GetTipProgramareDisplay(string? tipProgramare)
    {
        return tipProgramare switch
     {
     "ConsultatieInitiala" => "Consultație Inițială",
            "ControlPeriodic" => "Control Periodic",
            "Consultatie" => "Consultație",
            "Investigatie" => "Investigație",
            "Procedura" => "Procedură",
    "Urgenta" => "Urgență",
      "Telemedicina" => "Telemedicină",
 "LaDomiciliu" => "La Domiciliu",
       _ => tipProgramare ?? "-"
 };
    }

    public void Dispose()
    {
        _searchDebounceTimer?.Dispose();
    }
}
