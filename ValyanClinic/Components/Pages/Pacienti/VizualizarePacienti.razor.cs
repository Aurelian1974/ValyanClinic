using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Pages.Pacienti;

public partial class VizualizarePacienti : ComponentBase, IDisposable
{
    // ✅ ADDED: Static lock pentru ABSOLUTE protection
    private static readonly object _initLock = new object();
    private static bool _anyInstanceInitializing = false;
    
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<VizualizarePacienti> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    // Grid reference
private SfGrid<PacientListDto>? GridRef;

    // Toast reference
 private SfToast? ToastRef;

    // Modal references
    private bool ShowViewModal { get; set; }
    private bool ShowDoctoriModal { get; set; }
    private Guid? SelectedPacientId { get; set; }
    private string SelectedPacientNume { get; set; } = string.Empty;

    // SERVER-SIDE PAGING: Data pentru pagina curenta
    private List<PacientListDto> CurrentPageData { get; set; } = new();
    
    // SERVER-SIDE PAGING: Metadata
  private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    private int TotalRecords { get; set; } = 0;
    
    // SERVER-SIDE PAGING: Limits
    private int[] PageSizeArray = new int[] { 10, 20, 50, 100, 250 };
    
    // SERVER-SIDE SORTING: State
    private string CurrentSortColumn { get; set; } = "Nume";
    private string CurrentSortDirection { get; set; } = "ASC";

    private PacientListDto? SelectedPacient { get; set; }

    // State
    private bool IsLoading { get; set; } = true;
    private bool DataLoaded { get; set; } = false;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    // Advanced Filter State
    private bool IsAdvancedFilterExpanded { get; set; } = false;
    private string GlobalSearchText { get; set; } = string.Empty;
    private string? FilterJudet { get; set; }
    private string? FilterAsigurat { get; set; }
    private string? FilterStatus { get; set; }
    
    // Debounce timer for global search
    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int SearchDebounceMs = 500;
    
    // ✅ ADDED: Guard flags
    private bool _disposed = false;
    private bool _initialized = false;
    private bool _isInitializing = false;

    // Filter Options
    private List<FilterOption> JudetOptions { get; set; } = new();
    private List<FilterOption> AsiguratOptions { get; set; } = new();
    private List<FilterOption> StatusOptions { get; set; } = new();
    private bool FilterOptionsLoaded { get; set; } = false;

    // Toast properties
  private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

private int ActiveFiltersCount => 
        (string.IsNullOrEmpty(GlobalSearchText) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterJudet) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterAsigurat) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterStatus) ? 0 : 1);

    protected override async Task OnInitializedAsync()
    {
    // ✅ ADDED: CRITICAL - GLOBAL lock la nivel de pagină
        lock (_initLock)
        {
            if (_anyInstanceInitializing)
 {
          Logger.LogWarning("Another instance is ALREADY initializing - BLOCKING this call");
            return;
      }
            
   if (_initialized || _isInitializing)
     {
         Logger.LogWarning("This instance already initialized/initializing - SKIPPING");
        return;
         }

       _isInitializing = true;
    _anyInstanceInitializing = true;
        }

      try
 {
 // ✅ ADDED: CRITICAL - Delay MĂRIT pentru cleanup complet
       Logger.LogInformation("Waiting for previous component cleanup...");
     await Task.Delay(800); // MĂRIT pentru Syncfusion Grid cleanup
            
            Logger.LogInformation("========== Initializare pagina Vizualizare Pacienti ==========");
 
          // Initialize static filter options
            InitializeStaticFilterOptions();
            
         // Load filter options
     await LoadFilterOptionsFromServer();
   
            // Load first page
            await LoadPagedData();
      
            DataLoaded = true;
            _initialized = true; // ✅ ADDED
        }
     catch (ObjectDisposedException ex)
    {
            Logger.LogWarning(ex, "Component disposed during initialization (navigation away)");
          // Don't set error state - user navigated away
        }
    catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la initializarea componentei");
      HasError = true;
            ErrorMessage = $"Eroare la initializare: {ex.Message}";
            IsLoading = false;
        }
        finally
        {
     // ✅ ADDED: Release lock
     lock (_initLock)
  {
       _isInitializing = false;
            _anyInstanceInitializing = false;
       }
        }
 }

    // ✅ IMPROVED: Dispose with proper cleanup
    public void Dispose()
    {
 if (_disposed) return;
   
        // Setează flag imediat pentru a bloca noi operații
        _disposed = true;
  
     // CRITICAL: Cleanup SINCRON pentru Syncfusion Grid
        try
        {
            Logger.LogDebug("VizualizarePacienti disposing - SYNCHRONOUS cleanup");
          
            // ❌ NU setăm GridRef = null - lăsăm Syncfusion să gestioneze propriul lifecycle
      // GridRef = null; // ELIMINAT - cauzează probleme cu referințele
            
            // Cancel orice operații în curs
            _searchDebounceTokenSource?.Cancel();
          _searchDebounceTokenSource?.Dispose();
            _searchDebounceTokenSource = null;
       
  // Clear data IMEDIAT
            CurrentPageData?.Clear();
    CurrentPageData = new();
      
         Logger.LogDebug("VizualizarePacienti disposed - Data cleared, GridRef preserved");
 }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in synchronous dispose");
}
        
// CRITICAL: ELIMINAT async cleanup forțat - lăsăm Blazor disposal natural
     Logger.LogDebug("VizualizarePacienti dispose COMPLETE - Natural Blazor disposal will handle GridRef");
    }

    private void InitializeStaticFilterOptions()
    {
        // Static filter options for Asigurat
        AsiguratOptions = new List<FilterOption>
        {
      new FilterOption { Value = "true", Text = "Da" },
     new FilterOption { Value = "false", Text = "Nu" }
        };

   // Static filter options for Status
        StatusOptions = new List<FilterOption>
        {
     new FilterOption { Value = "true", Text = "Activ" },
 new FilterOption { Value = "false", Text = "Inactiv" }
  };
    }

    private async Task LoadFilterOptionsFromServer()
    {
        if (_disposed || FilterOptionsLoaded) return;
        
      try
        {
         Logger.LogInformation("Incarcare filter options de pe server (CACHED)...");
      
      // Load toate datele DOAR pentru filter options (fara paging)
    var query = new GetPacientListQuery
       {
    PageNumber = 1,
       PageSize = int.MaxValue,
    SearchText = null,
                Judet = null,
        Asigurat = null,
    Activ = null,
                SortColumn = "Nume",
 SortDirection = "ASC"
         };

     var result = await Mediator.Send(query);

  if (_disposed) return; // Check after async operation

     if (result.IsSuccess && result.Value != null && result.Value.Value != null && result.Value.Value.Any())
            {
          var allData = result.Value.Value.ToList();
      
            // Generate Judet options
                JudetOptions = allData
       .Where(p => !string.IsNullOrEmpty(p.Judet))
      .Select(p => p.Judet!)
      .Distinct()
           .OrderBy(j => j)
         .Select(j => new FilterOption { Value = j, Text = j })
  .ToList();
      
      FilterOptionsLoaded = true;
                
    Logger.LogInformation(
           "Filter options CACHED: Judet={JudetCount}, Asigurat={AsiguratCount}, Status={StatusCount}",
  JudetOptions.Count, AsiguratOptions.Count, StatusOptions.Count);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("Component disposed while loading filter options (navigation away)");
        }
        catch (Exception ex)
        {
        if (!_disposed)
{
      Logger.LogError(ex, "Eroare la incarcarea filter options");
    JudetOptions = new List<FilterOption>();
 }
        }
    }

  // SERVER-SIDE PAGING: Load data pentru pagina curenta
  private async Task LoadPagedData()
    {
        if (_disposed) return; // Guard check

     try
        {
            IsLoading = true;
       HasError = false;
            ErrorMessage = null;

 Logger.LogInformation(
   "========== SERVER-SIDE Load START: Page={Page}, Size={Size}, Search='{Search}', Judet={Judet}, Asigurat={Asigurat}, Status={Status}, Sort={Sort} {Dir} ==========",
      CurrentPage, CurrentPageSize, GlobalSearchText, FilterJudet, FilterAsigurat, FilterStatus, CurrentSortColumn, CurrentSortDirection);

         // Convert string filters to boolean
       bool? asiguratFilter = string.IsNullOrEmpty(FilterAsigurat) ? null : FilterAsigurat == "true";
 bool? statusFilter = string.IsNullOrEmpty(FilterStatus) ? null : FilterStatus == "true";

          var query = new GetPacientListQuery
            {
           PageNumber = CurrentPage,
     PageSize = CurrentPageSize,
      SearchText = string.IsNullOrWhiteSpace(GlobalSearchText) ? null : GlobalSearchText,
                Judet = FilterJudet,
       Asigurat = asiguratFilter,
   Activ = statusFilter,
 SortColumn = CurrentSortColumn,
       SortDirection = CurrentSortDirection
            };

      var result = await Mediator.Send(query);
          
            if (_disposed) return; // Check after async operation
            
       Logger.LogInformation("MediatR result: IsSuccess={IsSuccess}", result.IsSuccess);

         if (result.IsSuccess && result.Value != null)
          {
  CurrentPageData = result.Value.Value?.ToList() ?? new List<PacientListDto>();
     TotalRecords = result.Value.TotalCount;
                
   Logger.LogInformation(
           "========== SERVER-SIDE Data loaded: Page {Page}, Records {Count}, Total {Total} ==========",
         CurrentPage, CurrentPageData.Count, TotalRecords);
            }
            else
   {
        HasError = true;
 ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
      Logger.LogError("========== Eroare la incarcarea datelor: {Message} ==========", ErrorMessage);
    CurrentPageData = new List<PacientListDto>();
    TotalRecords = 0;
         }
      }
  catch (ObjectDisposedException)
        {
  Logger.LogDebug("Component disposed while loading paged data (navigation away)");
      // Don't set error state - user navigated away
        }
        catch (Exception ex)
        {
            if (!_disposed)
    {
        HasError = true;
     ErrorMessage = $"Eroare neasteptata: {ex.Message}";
     Logger.LogError(ex, "========== EXCEPTION la incarcarea datelor ==========");
           CurrentPageData = new List<PacientListDto>();
    TotalRecords = 0;
     }
        }
    finally
        {
        if (!_disposed)
            {
             IsLoading = false;
     await InvokeAsync(StateHasChanged); // ✅ ADDED: InvokeAsync
            }
        }
    }

    private void ToggleAdvancedFilter()
    {
        if (_disposed) return;
        
      IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
        Logger.LogInformation("Advanced filter panel toggled: {State}", 
       IsAdvancedFilterExpanded ? "Expanded" : "Collapsed");
    }

  private void OnSearchInput(ChangeEventArgs e)
    {
        if (_disposed) return;
        
        var newValue = e.Value?.ToString() ?? string.Empty;
  
     if (newValue == GlobalSearchText) return;
        
        GlobalSearchText = newValue;
        
        Logger.LogInformation("Search input changed: '{SearchText}'", GlobalSearchText);
        
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource?.Dispose();
        _searchDebounceTokenSource = new CancellationTokenSource();

        var localToken = _searchDebounceTokenSource.Token;

        _ = Task.Run(async () =>
        {
        try
      {
        await Task.Delay(SearchDebounceMs, localToken);
   
     if (!localToken.IsCancellationRequested && !_disposed)
                {
        Logger.LogInformation("Executing SERVER-SIDE search for: '{SearchText}'", GlobalSearchText);
    
       await InvokeAsync(async () =>
          {
   if (!_disposed)
       {
       CurrentPage = 1; // Reset to first page when searching
     await LoadPagedData();
         }
  });
    }
            }
            catch (TaskCanceledException)
  {
            Logger.LogDebug("Search cancelled - user still typing");
       }
            catch (ObjectDisposedException)
            {
    Logger.LogDebug("Component disposed during search");
        }
 catch (Exception ex)
          {
        if (!_disposed)
     {
   Logger.LogError(ex, "Eroare la executia search-ului");
        }
       }
        }, localToken);
    }

  private async Task OnSearchKeyDown(KeyboardEventArgs e)
    {
        if (_disposed) return;
    
        if (e.Key == "Enter")
 {
            _searchDebounceTokenSource?.Cancel();
       
          Logger.LogInformation("Enter pressed - executing immediate SERVER-SIDE search: '{SearchText}'", GlobalSearchText);
            
            CurrentPage = 1; // Reset to first page
            await LoadPagedData();
        }
    }

    private async Task ClearSearch()
    {
        if (_disposed) return;
      
        Logger.LogInformation("Clearing search text");
        
        _searchDebounceTokenSource?.Cancel();
        
        GlobalSearchText = string.Empty;
        CurrentPage = 1;
  await LoadPagedData();
    }

  private async Task ApplyFilters()
  {
        if (_disposed) return;
        
        Logger.LogInformation(
  "Applying SERVER-SIDE filters: GlobalSearch={Search}, Judet={Judet}, Asigurat={Asigurat}, Status={Status}",
          GlobalSearchText, FilterJudet, FilterAsigurat, FilterStatus);

  CurrentPage = 1; // Reset to first page when filtering
        await LoadPagedData();
    }

    private async Task ClearAllFilters()
    {
   if (_disposed) return;
        
      Logger.LogInformation("Clearing all filters");
        
        GlobalSearchText = string.Empty;
   FilterJudet = null;
        FilterAsigurat = null;
        FilterStatus = null;

        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task ClearFilter(string filterName)
    {
        if (_disposed) return;
        
        Logger.LogInformation("Clearing filter: {FilterName}", filterName);

        switch (filterName)
     {
      case nameof(GlobalSearchText):
          GlobalSearchText = string.Empty;
         break;
      case nameof(FilterJudet):
          FilterJudet = null;
        break;
   case nameof(FilterAsigurat):
       FilterAsigurat = null;
      break;
            case nameof(FilterStatus):
        FilterStatus = null;
     break;
   }

        CurrentPage = 1;
     await LoadPagedData();
    }

    private void OnRowSelected(RowSelectEventArgs<PacientListDto> args)
    {
  if (_disposed) return;
        
        SelectedPacient = args.Data;
        Logger.LogInformation("Pacient selectat: {PacientId} - {NumeComplet}", 
  SelectedPacient?.Id, SelectedPacient?.NumeComplet);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PacientListDto> args)
    {
  if (_disposed) return;
        
        SelectedPacient = null;
      Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    private async Task OnGridActionBegin(ActionEventArgs<PacientListDto> args)
    {
     if (_disposed) return;
        
        Logger.LogInformation("Grid action begin: {RequestType}", args.RequestType);

   // Handle sorting
        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting)
   {
      args.Cancel = true; // Cancel client-side sorting
  
      if (args is { Data: not null })
            {
        var sortingColumns = (args.Data as IEnumerable<object>)?.Cast<dynamic>().ToList();
       if (sortingColumns?.Any() == true)
        {
        var sortCol = sortingColumns[0];
        CurrentSortColumn = sortCol.Name?.ToString() ?? "Nume";
          CurrentSortDirection = sortCol.Direction?.ToString()?.ToUpper() ?? "ASC";
            
     Logger.LogInformation("Sorting by column: {Column}, Direction: {Direction}", 
               CurrentSortColumn, CurrentSortDirection);
  
           CurrentPage = 1; // Reset to first page when sorting
    await LoadPagedData();
  }
            }
    }
        // Handle paging
     else if (args.RequestType == Syncfusion.Blazor.Grids.Action.Paging)
        {
            args.Cancel = true; // Cancel client-side paging
         
 if (GridRef?.PageSettings != null)
  {
         var newPage = (GridRef.PageSettings.CurrentPage > 0) ? GridRef.PageSettings.CurrentPage : 1;
         var newPageSize = GridRef.PageSettings.PageSize;
       
          Logger.LogInformation("Paging: Page={Page}, PageSize={PageSize}", newPage, newPageSize);
       
  if (newPage != CurrentPage || newPageSize != CurrentPageSize)
       {
        CurrentPage = newPage;
              CurrentPageSize = newPageSize;
  await LoadPagedData();
       }
}
        }
    }

    private async Task OnGridActionComplete(ActionEventArgs<PacientListDto> args)
    {
        if (_disposed) return;
        
      Logger.LogInformation("Grid action complete: {RequestType}", args.RequestType);
     
        // Update grid's page settings to reflect total records
        if (GridRef?.PageSettings != null && args.RequestType == Syncfusion.Blazor.Grids.Action.Paging)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleRefresh()
    {
        if (_disposed) return;
      
        Logger.LogInformation("Refresh date pacienti - SERVER-SIDE reload");

   // Clear cached filter options pentru a reincarca cu date noi
        FilterOptionsLoaded = false;
        await LoadFilterOptionsFromServer();
        
        CurrentPage = 1;
        await LoadPagedData();
        
   // Refresh grid
        if (GridRef != null)
        {
         await GridRef.Refresh();
    }
      
        await ShowToast("Succes", "Datele au fost reincarcate cu succes", "e-toast-success");
    }

    private async Task HandleViewSelected()
    {
        if (_disposed || SelectedPacient == null) return;
   
      await OpenViewModalAsync(SelectedPacient.Id, SelectedPacient.NumeComplet);
    }

    private async Task HandleManageDoctors()
    {
        if (_disposed || SelectedPacient == null)
        {
            Logger.LogWarning("[HandleManageDoctors] Aborted - disposed={Disposed}, SelectedPacient is null={IsNull}",
     _disposed, SelectedPacient == null);
      return;
        }
    
        Logger.LogInformation("[HandleManageDoctors] START - Opening modal for pacient: {PacientId} - {PacientName}", 
            SelectedPacient.Id, SelectedPacient.NumeComplet);

        try
      {
            // Set values
            SelectedPacientId = SelectedPacient.Id;
       SelectedPacientNume = SelectedPacient.NumeComplet;
      
  Logger.LogInformation("[HandleManageDoctors] Set SelectedPacientId={PacientId}, SelectedPacientNume={PacientName}",
 SelectedPacientId, SelectedPacientNume);
 
            // Close ViewModal if open
            ShowViewModal = false;

    // Force await to ensure UI updates
            await InvokeAsync(StateHasChanged);
 
            // Small delay to ensure previous modal closes
          await Task.Delay(50);
    
            // Open Doctori Modal
    ShowDoctoriModal = true;

 Logger.LogInformation("[HandleManageDoctors] ShowDoctoriModal set to TRUE");
     
      // Force re-render asynchronously
     await InvokeAsync(StateHasChanged);
          
     Logger.LogInformation("[HandleManageDoctors] END - StateHasChanged called");
        }
        catch (Exception ex)
    {
       Logger.LogError(ex, "[HandleManageDoctors] EXCEPTION for {PacientName}", SelectedPacient.NumeComplet);
            await ShowErrorToastAsync($"Eroare: {ex.Message}");
        }
 }

    private async Task OpenViewModalAsync(Guid pacientId, string pacientName)
    {
        if (_disposed) return;
        
    Logger.LogInformation("Deschidere View Modal pentru pacient: {PacientId} - {PacientName}", 
            pacientId, pacientName);

        try
        {
          SelectedPacientId = pacientId;
       ShowViewModal = true;
          StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la deschiderea modalului pentru {PacientName}", pacientName);
            await ShowErrorToastAsync($"Eroare la vizualizare: {ex.Message}");
        }
    }

    private async Task HandleModalClosed()
    {
        if (_disposed) return;
    
     ShowViewModal = false;
        ShowDoctoriModal = false;
        SelectedPacientId = null;
        SelectedPacientNume = string.Empty;
   StateHasChanged();
    }

    private async Task ShowSuccessToastAsync(string message)
    {
        if (!_disposed)
        {
        await ShowToast("Succes", message, "e-toast-success");
        }
    }

    private async Task ShowErrorToastAsync(string message)
    {
        if (!_disposed)
        {
            await ShowToast("Eroare", message, "e-toast-danger");
        }
  }

    private async Task ShowToast(string title, string content, string cssClass)
    {
        if (_disposed) return;
        
        ToastTitle = title;
        ToastContent = content;
        ToastCssClass = cssClass;

        if (ToastRef != null)
        {
     await ToastRef.ShowAsync();
        }
    }

  // Helper class for filter options
    public class FilterOption
 {
      public string Value { get; set; } = string.Empty;
     public string Text { get; set; } = string.Empty;
  }
}
