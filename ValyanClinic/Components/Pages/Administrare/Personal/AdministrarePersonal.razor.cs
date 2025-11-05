using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using MediatR;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;
using ValyanClinic.Application.Features.PersonalManagement.Commands.DeletePersonal;
using ValyanClinic.Services.DataGrid;
using Microsoft.Extensions.Logging;
using ValyanClinic.Components.Pages.Administrare.Personal.Modals;
using ValyanClinic.Components.Shared.Modals;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdministrarePersonal : ComponentBase, IDisposable
{
    // CRITICAL: Static lock pentru ABSOLUTE protection între componente
    private static readonly object _initLock = new object();
    private static bool _anyInstanceInitializing = false;
    private static string? _initializingComponentName = null; // Track CARE componentă inițializează
    
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonal> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;

    // Grid reference
    private SfGrid<PersonalListDto>? GridRef;

    // Toast reference
    private SfToast? ToastRef;

    // Modal references
    private PersonalViewModal? personalViewModal;
    private PersonalFormModal? personalFormModal;
    private ConfirmDeleteModal? confirmDeleteModal;

    // SERVER-SIDE PAGING: Data pentru pagina curenta
    private List<PersonalListDto> CurrentPageData { get; set; } = new();
    
    // SERVER-SIDE PAGING: Metadata
    private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    private int TotalRecords { get; set; } = 0;
    private int TotalPages => TotalRecords > 0 && CurrentPageSize > 0 
        ? (int)Math.Ceiling((double)TotalRecords / CurrentPageSize) 
        : 1;
    
    // SERVER-SIDE PAGING: Limits
    private const int MinPageSize = 10;
    private const int MaxPageSize = 1000;
    private const int DefaultPageSizeValue = 20;
    private int[] PageSizeArray = { 10, 20, 50, 100, 250, 500, 1000 };
    
    // SERVER-SIDE SORTING: State
    private string CurrentSortColumn { get; set; } = "Nume";
    private string CurrentSortDirection { get; set; } = "ASC";
    
    private PersonalListDto? SelectedPersonal { get; set; }

    // State
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    // Advanced Filter State
    private bool IsAdvancedFilterExpanded { get; set; } = false;
    private string GlobalSearchText { get; set; } = string.Empty;
    private string? FilterStatus { get; set; }
    private string? FilterDepartament { get; set; }
    private string? FilterFunctie { get; set; }
    private string? FilterJudet { get; set; }
    
    // Debounce timer for global search
    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int SearchDebounceMs = 500;
    private bool _disposed = false;
    private bool _initialized = false;
    private bool _isInitializing = false;

    // CACHED Filter Options - loaded once
    private List<FilterOption> StatusOptions { get; set; } = new();
    private List<FilterOption> DepartamentOptions { get; set; } = new();
    private List<FilterOption> FunctieOptions { get; set; } = new();
    private List<FilterOption> JudetOptions { get; set; } = new();
    private bool FilterOptionsLoaded { get; set; } = false;

    // Toast properties
    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    private int ActiveFiltersCount => 
        (string.IsNullOrEmpty(FilterStatus) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterDepartament) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterFunctie) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterJudet) ? 0 : 1) +
        (string.IsNullOrEmpty(GlobalSearchText) ? 0 : 1);

    // Pager helper properties
    private bool HasPreviousPage => CurrentPage > 1;
    private bool HasNextPage => CurrentPage < TotalPages;
    private int DisplayedRecordsStart => TotalRecords > 0 ? ((CurrentPage - 1) * CurrentPageSize) + 1 : 0;
    private int DisplayedRecordsEnd => Math.Min(CurrentPage * CurrentPageSize, TotalRecords);

    protected override async Task OnInitializedAsync()
    {
        var startTime = DateTime.Now;
  var threadId = Thread.CurrentThread.ManagedThreadId;
     var componentName = "[AdministrarePersonal]";
     
        Logger.LogWarning("🟢 {Component} OnInitializedAsync START - Time: {Time}, Thread: {ThreadId}", 
        componentName, startTime.ToString("HH:mm:ss.fff"), threadId);
      
  // CRITICAL: GLOBAL lock la nivel de aplicație cu retry logic
   bool canProceed = false;
        int retryCount = 0;
  const int maxRetries = 10;
        
        while (!canProceed && retryCount < maxRetries)
{
            lock (_initLock)
         {
          if (_anyInstanceInitializing)
      {
         Logger.LogWarning("🔴 {Component} BLOCKED - Another component is initializing: {Other} - Retry {Retry}/{Max} - Time: {Time}", 
     componentName, _initializingComponentName, retryCount + 1, maxRetries, DateTime.Now.ToString("HH:mm:ss.fff"));
            }
      else if (_initialized || _isInitializing)
         {
                Logger.LogWarning("🔴 {Component} SKIPPED - This instance already initialized - Time: {Time}", 
    componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
      return;
    }
     else
 {
    _isInitializing = true;
   _anyInstanceInitializing = true;
       _initializingComponentName = componentName;
  canProceed = true;
            
    Logger.LogWarning("🟡 {Component} Lock acquired - Starting init - Time: {Time}", 
    componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
            }
       }

    if (!canProceed)
       {
retryCount++;
          await Task.Delay(100); // Wait 100ms before retry
    }
     }
     
        if (!canProceed)
     {
         Logger.LogError("❌ {Component} FAILED to acquire lock after {Retries} retries - ABORTING", 
        componentName, maxRetries);
   return;
        }

   try
{
       // CRITICAL: Delay MĂRIT pentru siguranță maximă
    Logger.LogWarning("⏳ {Component} Waiting 1200ms for previous component cleanup - Time: {Time}", 
    componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
     
         await Task.Delay(1200); // MĂRIT de la 800ms la 1200ms
        
      Logger.LogWarning("✅ {Component} Delay complete - Time: {Time}, Elapsed: {Elapsed}ms", 
   componentName, DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - startTime).TotalMilliseconds);
     
            Logger.LogInformation("Initializare pagina Administrare Personal cu SERVER-SIDE paging");
   
  // Load filter options first (cached)
          Logger.LogWarning("📊 {Component} Loading filter options - Time: {Time}", 
    componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
        await LoadFilterOptionsFromServer();
   
       Logger.LogWarning("📊 {Component} Filter options loaded - Time: {Time}", 
   componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
  
       // Load first page
          Logger.LogWarning("📄 {Component} Loading paged data - Time: {Time}", 
 componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
  await LoadPagedData();
         
          Logger.LogWarning("📄 {Component} Paged data loaded - Time: {Time}", 
    componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
  
      _initialized = true;
     
      Logger.LogWarning("✅ {Component} OnInitializedAsync COMPLETE - Time: {Time}, Total elapsed: {Elapsed}ms", 
          componentName, DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - startTime).TotalMilliseconds);
  }
 catch (ObjectDisposedException ex)
      {
    Logger.LogWarning("⚠️ {Component} Component disposed during init - Time: {Time}, Message: {Message}", 
    componentName, DateTime.Now.ToString("HH:mm:ss.fff"), ex.Message);
    }
   catch (Exception ex)
      {
     Logger.LogError(ex, "❌ {Component} ERROR during init - Time: {Time}", 
   componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
    HasError = true;
     ErrorMessage = $"Eroare la initializare: {ex.Message}";
   IsLoading = false;
        }
 finally
     {
 lock (_initLock)
 {
 _isInitializing = false;
       _anyInstanceInitializing = false;
   _initializingComponentName = null;
 
   Logger.LogWarning("🔓 {Component} Lock released - Time: {Time}", 
         componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
      }
   }
  }

    public void Dispose()
    {
        var disposeTime = DateTime.Now;
  var threadId = Thread.CurrentThread.ManagedThreadId;
     
      Logger.LogWarning("🔴 [AdministrarePersonal] Dispose START - Time: {Time}, Thread: {ThreadId}, Already disposed: {Disposed}", 
    disposeTime.ToString("HH:mm:ss.fff"), threadId, _disposed);
        
        if (_disposed)
        {
            Logger.LogWarning("⚠️ [AdministrarePersonal] Already disposed - SKIPPING - Time: {Time}", 
      DateTime.Now.ToString("HH:mm:ss.fff"));
            return;
        }
        
        // Setează flag imediat pentru a bloca noi operații
        _disposed = true;
        
        Logger.LogWarning("🚫 [AdministrarePersonal] _disposed flag set to TRUE - Time: {Time}", 
            DateTime.Now.ToString("HH:mm:ss.fff"));
 
        // CRITICAL: Cleanup SINCRON - Cu logging detaliat pentru debugging
        try
        {
            Logger.LogDebug("🧹 [AdministrarePersonal] SYNC cleanup START - Time: {Time}", 
     DateTime.Now.ToString("HH:mm:ss.fff"));
     
            // 🔍 INVESTIGAȚIE: Log DOM state ÎNAINTE de cleanup
            Logger.LogWarning("🔍 [AdministrarePersonal] Pre-cleanup state:");
       Logger.LogWarning("   - GridRef: {GridRefState}", GridRef != null ? "EXISTS" : "NULL");
 Logger.LogWarning("   - ToastRef: {ToastRefState}", ToastRef != null ? "EXISTS" : "NULL");
            Logger.LogWarning("   - CurrentPageData count: {DataCount}", CurrentPageData?.Count ?? 0);
     
     // Check if modals are still attached
    Logger.LogWarning("   - personalViewModal: {ViewModalState}", personalViewModal != null ? "EXISTS" : "NULL");
  Logger.LogWarning("   - personalFormModal: {FormModalState}", personalFormModal != null ? "EXISTS" : "NULL");
  Logger.LogWarning("   - confirmDeleteModal: {DeleteModalState}", confirmDeleteModal != null ? "EXISTS" : "NULL");
       
 // ❌ NU MAI setăm GridRef = null! Lăsăm Blazor să gestioneze
     // GridRef = null; // REMOVED - causes JavaScript callback errors
            
            if (GridRef != null)
            {
                Logger.LogDebug("ℹ️ [AdministrarePersonal] GridRef exists - letting Blazor handle disposal - Time: {Time}", 
  DateTime.Now.ToString("HH:mm:ss.fff"));
                
         // 🔍 INVESTIGAȚIE: Try to log Grid ID if possible
     try
     {
    Logger.LogWarning("   - Grid element ID: personal-grid");
        }
        catch (Exception ex)
     {
        Logger.LogWarning("   - Could not read Grid ID: {Error}", ex.Message);
    }
 }
            
// Cancel orice operații în curs
    if (_searchDebounceTokenSource != null)
            {
        Logger.LogDebug("❌ [AdministrarePersonal] Cancelling search token - Time: {Time}", 
 DateTime.Now.ToString("HH:mm:ss.fff"));
      _searchDebounceTokenSource?.Cancel();
                _searchDebounceTokenSource?.Dispose();
          _searchDebounceTokenSource = null;
       }
       
      // Clear data IMEDIAT
          var dataCount = CurrentPageData?.Count ?? 0;
Logger.LogDebug("🗑️ [AdministrarePersonal] Clearing {Count} data items - Time: {Time}", 
        dataCount, DateTime.Now.ToString("HH:mm:ss.fff"));
    CurrentPageData?.Clear();
     CurrentPageData = new();
            
       // 🔍 INVESTIGAȚIE: Log state DUPĂ cleanup
      Logger.LogWarning("🔍 [AdministrarePersonal] Post-cleanup state:");
          Logger.LogWarning("   - Data cleared: TRUE");
            Logger.LogWarning("   - CurrentPageData count: {Count}", CurrentPageData?.Count ?? 0);
            
      Logger.LogDebug("✅ [AdministrarePersonal] SYNC cleanup COMPLETE - Time: {Time}, Elapsed: {Elapsed}ms", 
     DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - disposeTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
         Logger.LogError(ex, "❌ [AdministrarePersonal] ERROR in sync dispose - Time: {Time}", 
                DateTime.Now.ToString("HH:mm:ss.fff"));
  }
        
        // CRITICAL: NU MAI AVEM NEVOIE de async cleanup pentru GridRef
        // Blazor va gestiona disposal-ul componentelor JavaScript automat
        Logger.LogWarning("🏁 [AdministrarePersonal] Dispose END - Time: {Time}, Total elapsed: {Elapsed}ms (Natural Blazor disposal will handle GridRef)", 
 DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - disposeTime).TotalMilliseconds);
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

            // Validate PageSize limits
            if (CurrentPageSize < MinPageSize) CurrentPageSize = MinPageSize;
            if (CurrentPageSize > MaxPageSize) CurrentPageSize = MaxPageSize;

            Logger.LogInformation(
                "SERVER-SIDE Load: Page={Page}, Size={Size}, Search='{Search}', Filters: Status={Status}, Dept={Dept}, Func={Func}, Judet={Judet}, Sort={Sort} {Dir}",
                CurrentPage, CurrentPageSize, GlobalSearchText, FilterStatus, FilterDepartament, 
                FilterFunctie, FilterJudet, CurrentSortColumn, CurrentSortDirection);

            var query = new GetPersonalListQuery
            {
                PageNumber = CurrentPage,
                PageSize = CurrentPageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(GlobalSearchText) ? null : GlobalSearchText,
                FilterStatus = FilterStatus,
                FilterDepartament = FilterDepartament,
                FilterFunctie = FilterFunctie,
                FilterJudet = FilterJudet,
                SortColumn = CurrentSortColumn,
                SortDirection = CurrentSortDirection
            };

            var result = await Mediator.Send(query);

            if (_disposed) return; // Check after async operation

            if (result.IsSuccess)
            {
                CurrentPageData = result.Value?.ToList() ?? new List<PersonalListDto>();
                TotalRecords = result.TotalCount;
                
                // Adjust CurrentPage daca este out of bounds
                if (TotalPages > 0 && CurrentPage > TotalPages)
                {
                    Logger.LogWarning("CurrentPage {Page} > TotalPages {Total}, ajustare la ultima pagina", 
                        CurrentPage, TotalPages);
                    CurrentPage = TotalPages;
                    await LoadPagedData(); // Reload cu pagina corecta
                    return;
                }
                
                Logger.LogInformation(
                    "SERVER-SIDE Data loaded: Page {Page}/{Total}, Records {Start}-{End} din {TotalRecords}",
                    CurrentPage, TotalPages, DisplayedRecordsStart, DisplayedRecordsEnd, TotalRecords);
            }
            else
            {
                HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Eroare la incarcarea datelor: {Message}", ErrorMessage);
                CurrentPageData = new List<PersonalListDto>();
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
                Logger.LogError(ex, "Eroare la incarcarea datelor personal");
                CurrentPageData = new List<PersonalListDto>();
                TotalRecords = 0;
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
    }

    // CACHED Filter Options - incarca o singura data
    private async Task LoadFilterOptionsFromServer()
    {
        if (_disposed || FilterOptionsLoaded) return; // Guard check
        
        try
        {
            Logger.LogInformation("Incarcare filter options de pe server (CACHED)...");
            
            // Load toate datele DOAR pentru filter options (fara paging)
            var query = new GetPersonalListQuery
            {
                PageNumber = 1,
                PageSize = int.MaxValue, // Get ALL for filter options
                GlobalSearchText = null,
                FilterStatus = null,
                FilterDepartament = null,
                FilterFunctie = null,
                FilterJudet = null,
                SortColumn = "Nume",
                SortDirection = "ASC"
            };

            var result = await Mediator.Send(query);

            if (_disposed) return; // Check after async operation

            if (result.IsSuccess && result.Value != null && result.Value.Any())
            {
                var allData = result.Value.ToList();
                
                StatusOptions = FilterOptionsService.GenerateOptions(allData, p => p.Status_Angajat);
                DepartamentOptions = FilterOptionsService.GenerateOptions(allData, p => p.Departament);
                FunctieOptions = FilterOptionsService.GenerateOptions(allData, p => p.Functia);
                JudetOptions = FilterOptionsService.GenerateOptions(allData, p => p.Judet_Domiciliu);
                
                FilterOptionsLoaded = true;
                
                Logger.LogInformation(
                    "Filter options CACHED: Status={StatusCount}, Dept={DeptCount}, Func={FuncCount}, Judet={JudetCount}",
                    StatusOptions.Count, DepartamentOptions.Count, FunctieOptions.Count, JudetOptions.Count);
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
                StatusOptions = new List<FilterOption>();
                DepartamentOptions = new List<FilterOption>();
                FunctieOptions = new List<FilterOption>();
                JudetOptions = new List<FilterOption>();
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
            "Applying SERVER-SIDE filters: GlobalSearch={Search}, Status={Status}, Dept={Dept}, Func={Func}, Judet={Judet}",
            GlobalSearchText, FilterStatus, FilterDepartament, FilterFunctie, FilterJudet);

        CurrentPage = 1; // Reset to first page when filtering
        await LoadPagedData();
    }

    private async Task ClearAllFilters()
    {
        if (_disposed) return;
        
        Logger.LogInformation("Clearing all filters");
        
        GlobalSearchText = string.Empty;
        FilterStatus = null;
        FilterDepartament = null;
        FilterFunctie = null;
        FilterJudet = null;

        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task ClearFilter(string filterName)
    {
        if (_disposed) return;
        
        Logger.LogInformation("Clearing filter: {FilterName}", filterName);

        switch (filterName)
        {
            case nameof(FilterStatus):
                FilterStatus = null;
                break;
            case nameof(FilterDepartament):
                FilterDepartament = null;
                break;
            case nameof(FilterFunctie):
                FilterFunctie = null;
                break;
            case nameof(FilterJudet):
                FilterJudet = null;
                break;
            case nameof(GlobalSearchText):
                GlobalSearchText = string.Empty;
                break;
        }

        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task HandleRefresh()
    {
        if (_disposed) return;
        
        Logger.LogInformation("Refresh date personal - SERVER-SIDE reload");
        
        // Clear cached filter options pentru a reincarca cu date noi
        FilterOptionsLoaded = false;
        await LoadFilterOptionsFromServer();
        
        await LoadPagedData();
        await ShowToast("Succes", "Datele au fost reincarcate cu succes", "e-toast-success");
    }

    private async Task HandleAddNew()
    {
        if (_disposed) return;
        
        Logger.LogInformation("Deschidere modal pentru adaugare personal");
        
        if (personalFormModal != null)
        {
            await personalFormModal.OpenForAdd();
        }
    }

    #region Paging Methods

    private async Task GoToPage(int pageNumber)
    {
        if (_disposed) return;
        
        if (pageNumber < 1 || pageNumber > TotalPages) return;
        
        Logger.LogInformation("Navigare la pagina {Page}", pageNumber);
        CurrentPage = pageNumber;
        await LoadPagedData();
    }

    private async Task GoToFirstPage()
    {
        await GoToPage(1);
    }

    private async Task GoToLastPage()
    {
        await GoToPage(TotalPages);
    }

    private async Task GoToPreviousPage()
    {
        if (HasPreviousPage)
        {
            await GoToPage(CurrentPage - 1);
        }
    }

    private async Task GoToNextPage()
    {
        if (HasNextPage)
        {
            await GoToPage(CurrentPage + 1);
        }
    }

    private async Task OnPageSizeChanged(int newPageSize)
    {
        if (_disposed) return;
        
        if (newPageSize < MinPageSize || newPageSize > MaxPageSize)
        {
            Logger.LogWarning("PageSize invalid: {Size}, using default", newPageSize);
            newPageSize = DefaultPageSizeValue;
        }
        
        Logger.LogInformation("PageSize changed: {OldSize} -> {NewSize}", CurrentPageSize, newPageSize);
        
        CurrentPageSize = newPageSize;
        CurrentPage = 1; // Reset to first page
        await LoadPagedData();
    }

    private async Task OnPageSizeChangedNative(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int newPageSize))
        {
            await OnPageSizeChanged(newPageSize);
        }
    }

    private (int start, int end) GetPagerRange(int visiblePages = 5)
    {
        if (TotalPages <= visiblePages)
        {
            return (1, TotalPages);
        }

        var halfVisible = visiblePages / 2;
        var start = Math.Max(1, CurrentPage - halfVisible);
        var end = Math.Min(TotalPages, start + visiblePages - 1);

        if (end - start + 1 < visiblePages)
        {
            start = Math.Max(1, end - visiblePages + 1);
        }

        return (start, end);
    }

    #endregion

    #region Grid Events - Syncfusion

    private void OnRowSelected(RowSelectEventArgs<PersonalListDto> args)
    {
        if (_disposed) return;
        
        SelectedPersonal = args.Data;
        Logger.LogInformation("Personal selectat: {PersonalId} - {NumeComplet}", 
            SelectedPersonal?.Id_Personal, SelectedPersonal?.NumeComplet);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PersonalListDto> args)
    {
        if (_disposed) return;
        
        SelectedPersonal = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    private async Task OnGridActionBegin(ActionEventArgs<PersonalListDto> args)
    {
        if (_disposed) return;
        
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
                    
                    Logger.LogInformation("SERVER-SIDE Sort: {Column} {Direction}", 
                        CurrentSortColumn, CurrentSortDirection);
                    
                    await LoadPagedData();
                }
            }
        }
    }

    #endregion

    #region Toolbar Action Methods

    private async Task HandleViewSelected()
    {
        if (_disposed || SelectedPersonal == null) return;
        
        await OpenViewModalAsync(SelectedPersonal.Id_Personal, SelectedPersonal.NumeComplet);
    }

    private async Task HandleEditSelected()
    {
        if (_disposed || SelectedPersonal == null) return;
        
        await OpenEditModalAsync(SelectedPersonal.Id_Personal, SelectedPersonal.NumeComplet);
    }

    private async Task HandleDeleteSelected()
    {
        if (_disposed || SelectedPersonal == null) return;
        
        await OpenDeleteModalAsync(SelectedPersonal.Id_Personal, SelectedPersonal.NumeComplet);
    }

    #endregion

    #region Modal Event Handlers

    private async Task HandleEditFromModal(Guid personalId)
    {
        if (_disposed) return;
        
        if (!ValidatePersonalId(personalId, "HandleEditFromModal"))
        {
            await ShowErrorToastAsync("ID invalid pentru editare");
            return;
        }

        var personal = CurrentPageData.FirstOrDefault(p => p.Id_Personal == personalId);
        await OpenEditModalAsync(personalId, personal?.NumeComplet ?? "Unknown");
    }

    private async Task HandleDeleteFromModal(Guid personalId)
    {
        if (_disposed) return;
        
        var personal = CurrentPageData.FirstOrDefault(p => p.Id_Personal == personalId);
        if (personal != null)
        {
            await OpenDeleteModalAsync(personalId, personal.NumeComplet);
        }
    }

    private async Task HandlePersonalSaved()
    {
        if (_disposed) return;
      
 Logger.LogInformation("🎉 Personal salvat - FORCING component re-initialization");
 
        // ❌ NU reîncărcăm doar datele! Asta lasă DOM-ul în stare inconsistentă
        // ✅ Force COMPLETE re-initialization prin navigare

     // 🔄 STRATEGY: Navigate AWAY și apoi BACK pentru a forța re-render complet
     try
    {
    // 1️⃣ Wait pentru modal să se închidă complet
    Logger.LogInformation("⏳ Waiting 700ms for modal close...");
    await Task.Delay(700);
 
         if (_disposed) return;
          
      // 2️⃣ Show loading state
     IsLoading = true;
     await InvokeAsync(StateHasChanged);
      
            // 3️⃣ Force navigation to SAME page (triggers full re-init)
            Logger.LogInformation("🔄 Force navigation to trigger re-initialization");
            NavigationManager.NavigateTo("/administrare/personal", forceLoad: true);
        
            // Note: forceLoad: true forces a FULL page reload, not just component refresh
     // This clears ALL Blazor state and starts fresh - exactly like F5!
        }
        catch (Exception ex)
        {
      Logger.LogError(ex, "Error during forced re-initialization");
      
            // Fallback: Reload data normally if navigation fails
  if (!_disposed)
       {
 FilterOptionsLoaded = false;
      await LoadFilterOptionsFromServer();
   await LoadPagedData();
                await ShowSuccessToastAsync("Personal salvat cu succes");
    }
        }
        finally
   {
            if (!_disposed)
            {
       IsLoading = false;
         }
        }
 }

    private async Task HandleDeleteConfirmed(Guid personalId)
    {
        if (_disposed) return;
        
        LogOperation("Confirmare stergere", personalId);
        
        try
        {
            var command = new DeletePersonalCommand(personalId, "CurrentUser");
            var result = await Mediator.Send(command);
            
            if (_disposed) return; // Check after async
            
            if (result.IsSuccess)
            {
                LogOperation("Stergere reusita", personalId);
                
                // Invalidate cached filter options
                FilterOptionsLoaded = false;
                await LoadFilterOptionsFromServer();
                
                await LoadPagedData();
                await ShowSuccessToastAsync("Personal sters cu succes");
            }
            else
            {
                await HandleOperationFailureAsync("stergere", result.Errors);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("Component disposed during delete operation");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                await HandleOperationExceptionAsync("stergere", personalId, ex);
            }
        }
    }

    #endregion

    #region Helper Methods - Modal Operations

    private async Task OpenViewModalAsync(Guid personalId, string personalName)
    {
        if (_disposed) return;
        
        LogOperation("Deschidere View Modal", personalId, personalName);
        
        if (personalViewModal == null)
        {
            await HandleModalNotInitializedAsync("View Modal");
            return;
        }

        try
        {
            await personalViewModal.Open(personalId);
        }
        catch (Exception ex)
        {
            await HandleModalOperationExceptionAsync("vizualizare", personalName, ex);
        }
    }

    private async Task OpenEditModalAsync(Guid personalId, string personalName)
    {
        if (_disposed) return;
        
        LogOperation("Deschidere Edit Modal", personalId, personalName);
        
        if (personalFormModal == null)
        {
            await HandleModalNotInitializedAsync("Edit Modal");
            return;
        }

        try
        {
            await personalFormModal.OpenForEdit(personalId);
        }
        catch (Exception ex)
        {
            await HandleModalOperationExceptionAsync("editare", personalName, ex);
        }
    }

    private async Task OpenDeleteModalAsync(Guid personalId, string personalName)
    {
        if (_disposed) return;
        
        LogOperation("Deschidere Delete Modal", personalId, personalName);
        
        if (confirmDeleteModal == null)
        {
            await HandleModalNotInitializedAsync("Delete Modal");
            return;
        }

        try
        {
            await confirmDeleteModal.Open(personalId, personalName);
        }
        catch (Exception ex)
        {
            await HandleModalOperationExceptionAsync("stergere", personalName, ex);
        }
    }

    #endregion

    #region Helper Methods - Error Handling

    private bool ValidatePersonalId(Guid personalId, string context)
    {
        if (personalId == Guid.Empty)
        {
            Logger.LogError("ID invalid in {Context}: Guid.Empty", context);
            return false;
        }
        return true;
    }

    private async Task HandleModalNotInitializedAsync(string modalName)
    {
        Logger.LogError("Modal nu este initializat: {ModalName}", modalName);
        await ShowErrorToastAsync($"{modalName} nu este initializat");
    }

    private async Task HandleModalOperationExceptionAsync(string operation, string personalName, Exception ex)
    {
        Logger.LogError(ex, "Eroare la {Operation} pentru {PersonalName}", operation, personalName);
        await ShowErrorToastAsync($"Eroare la {operation}: {ex.Message}");
    }

    private async Task HandleOperationFailureAsync(string operation, List<string>? errors)
    {
        var errorMsg = string.Join(", ", errors ?? new List<string> { "Eroare necunoscuta" });
        Logger.LogWarning("Eroare la {Operation}: {Errors}", operation, errorMsg);
        await ShowErrorToastAsync(errorMsg);
    }

    private async Task HandleOperationExceptionAsync(string operation, Guid personalId, Exception ex)
    {
        Logger.LogError(ex, "Exceptie la {Operation} pentru {PersonalId}", operation, personalId);
        await ShowErrorToastAsync($"Eroare la {operation}: {ex.Message}");
    }

    #endregion

    #region Helper Methods - Logging

    private void LogOperation(string operation, Guid? id = null, string? additionalInfo = null)
    {
        if (id.HasValue && !string.IsNullOrEmpty(additionalInfo))
        {
            Logger.LogInformation("{Operation}: {PersonalId} - {Info}", operation, id.Value, additionalInfo);
        }
        else if (id.HasValue)
        {
            Logger.LogInformation("{Operation}: {PersonalId}", operation, id.Value);
        }
        else if (!string.IsNullOrEmpty(additionalInfo))
        {
            Logger.LogInformation("{Operation}: {Info}", operation, additionalInfo);
        }
        else
        {
            Logger.LogInformation("{Operation}", operation);
        }
    }

    #endregion

    #region Helper Methods - Toast Notifications

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
        if (_disposed) return; // Guard check
        
        if (ToastRef == null)
        {
     Logger.LogWarning("ToastRef is null, cannot show toast");
   return;
 }

        try
        {
            // CRITICAL: Folosește ToastModel pentru a asigura că datele sunt transmise corect
       var toastModel = new ToastModel
      {
       Title = title,
   Content = content,
     CssClass = cssClass,
       ShowCloseButton = true,
   Timeout = 3000
 };

   Logger.LogDebug("Showing toast: Title='{Title}', Content='{Content}', CssClass='{CssClass}'", 
                title, content, cssClass);

            await ToastRef.ShowAsync(toastModel);
   }
    catch (ObjectDisposedException)
       {
      Logger.LogDebug("Toast reference disposed");
        }
        catch (Exception ex)
 {
       Logger.LogError(ex, "Error showing toast");
        }
    }

    #endregion
}
