using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Services.Export;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;

namespace ValyanClinic.Components.Pages.Pacienti;

public partial class VizualizarePacienti : ComponentBase, IAsyncDisposable
{
    // ✅ ADDED: Static lock pentru ABSOLUTE protection
    private static readonly object _initLock = new object();
    private static bool _anyInstanceInitializing = false;

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<VizualizarePacienti> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ValyanClinic.Application.Services.Pacienti.IPacientDataService DataService { get; set; } = default!;
    [Inject] private IExcelExportService ExcelExportService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Grid reference
    private SfGrid<PacientListDto>? GridRef;
    
    // ✅ CRITICAL: Unique key for Grid - forces recreation on navigation
    private string _gridKey = Guid.NewGuid().ToString();

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

    // Saved Filters State
    private List<SavedFilter> SavedFilters { get; set; } = new();
    private string SelectedSavedFilter { get; set; } = string.Empty;
    private bool ShowSaveFilterModal { get; set; } = false;
    private string NewFilterName { get; set; } = string.Empty;
    private const string SavedFiltersStorageKey = "VizualizarePacienti_SavedFilters";

    // Debounce timer for global search
    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int SearchDebounceMs = 500;

    // ✅ ADDED: Guard flags
    private bool _disposed = false;
    private bool _initialized = false;
    private bool _isInitializing = false;
    private bool _isExporting = false; // ✅ NEW: Track export in progress
    private bool _hasRendered = false;

    // SignalR Hub Connection
    private HubConnection? _pacientHubConnection;

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
            // ✅ CRITICAL: Subscribe to navigation BEFORE it happens
            NavigationManager.LocationChanged += OnLocationChanged;

            // ✅ ADDED: CRITICAL - Delay MĂRIT pentru cleanup complet
            Logger.LogInformation("Waiting for previous component cleanup...");
            await Task.Delay(800); // MĂRIT pentru Syncfusion Grid cleanup

            Logger.LogInformation("========== Initializare pagina Vizualizare Pacienti ==========");

            // Initialize static filter options
            InitializeStaticFilterOptions();

            // Load filter options
            await LoadFilterOptionsFromServer();

            // Load saved filters from localStorage
            await LoadSavedFiltersAsync();

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

    // ✅ ADDED: OnAfterRenderAsync pentru SignalR connection
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_hasRendered && !_disposed)
        {
            _hasRendered = true;

            try
            {
                // Connect to SignalR hub
                _pacientHubConnection = new HubConnectionBuilder()
                    .WithUrl(NavigationManager.ToAbsoluteUri("/pacientHub"))
                    .WithAutomaticReconnect()
                    .Build();

                _pacientHubConnection.On<string, Guid>("PacientChanged", async (action, id) =>
                {
                    if (_disposed) return;

                    Logger.LogDebug($"SignalR: Received PacientChanged event - Action: {action}, Id: {id}");

                    try
                    {
                        await HandlePacientChangedAsync(action, id);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error handling PacientChanged event for {action}");
                    }
                });

                await _pacientHubConnection.StartAsync();
                Logger.LogInformation("SignalR connection established for VizualizarePacienti");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to establish SignalR connection");
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    // Handle SignalR events
    private async Task HandlePacientChangedAsync(string action, Guid pacientId)
    {
        await InvokeAsync(async () =>
        {
            try
            {
                switch (action)
                {
                    case "Created":
                        await ApplyCreatedAsync(pacientId);
                        break;
                    case "Updated":
                        await ApplyUpdatedAsync(pacientId);
                        break;
                    case "Deleted":
                        await ApplyDeletedAsync(pacientId);
                        break;
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error applying {action} for pacient {pacientId}");
            }
        });
    }

    private async Task ApplyCreatedAsync(Guid pacientId)
    {
        Logger.LogDebug($"Applying Created for pacient {pacientId}");
        // Reload current page to show new record
        await LoadPagedData();
    }

    private async Task ApplyUpdatedAsync(Guid pacientId)
    {
        Logger.LogDebug($"Applying Updated for pacient {pacientId}");
        
        // Find and update the record in current page
        var existingPacient = CurrentPageData.FirstOrDefault(p => p.Id == pacientId);
        if (existingPacient != null)
        {
            // Reload the page to get updated data
            await LoadPagedData();
        }
    }

    private async Task ApplyDeletedAsync(Guid pacientId)
    {
        Logger.LogDebug($"Applying Deleted for pacient {pacientId}");
        
        // Remove from current page and reload
        var existingPacient = CurrentPageData.FirstOrDefault(p => p.Id == pacientId);
        if (existingPacient != null)
        {
            await LoadPagedData();
        }
    }

    // ✅ CRITICAL: Handle location changes - cleanup BEFORE navigation completes
    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        Logger.LogDebug("Location changed - cleaning up Syncfusion components");
        
        // Set flag immediately
        _disposed = true;
        
        // Cleanup Syncfusion via JavaScript
        try
        {
            _ = JSRuntime.InvokeVoidAsync("cleanupSyncfusionBeforeNavigation");
        }
        catch
        {
            // Ignore - might already be disposed
        }
    }

    // ✅ FIXED: Complete disposal with Syncfusion cleanup via JS
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        Logger.LogDebug("VizualizarePacienti Dispose() START");
        _disposed = true;

        // ✅ CRITICAL: Unsubscribe from navigation events
        NavigationManager.LocationChanged -= OnLocationChanged;

        if (_isExporting)
        {
            Logger.LogWarning("Export in progress - skipping cleanup");
            return;
        }

        try
        {
            // ✅ CRITICAL: Stop SignalR connection
            await StopAndDisposeSignalRAsync();

            // ✅ CRITICAL: Cleanup Syncfusion via JavaScript FIRST
            _ = JSRuntime.InvokeVoidAsync("cleanupSyncfusionBeforeNavigation");
            
            // Clear grid reference
            GridRef = null;

            // Cancel operations
            _searchDebounceTokenSource?.Cancel();
            _searchDebounceTokenSource?.Dispose();
            _searchDebounceTokenSource = null;

            // Clear data
            CurrentPageData?.Clear();
            CurrentPageData = new();

            Logger.LogDebug("VizualizarePacienti Dispose() COMPLETE");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in Dispose");
        }
    }

    private async Task StopAndDisposeSignalRAsync()
    {
        if (_pacientHubConnection != null)
        {
            try
            {
                // Remove event handler
                _pacientHubConnection.Remove("PacientChanged");

                // Stop connection
                await _pacientHubConnection.StopAsync();
                await _pacientHubConnection.DisposeAsync();

                Logger.LogDebug("SignalR connection stopped and disposed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error stopping SignalR connection");
            }
            finally
            {
                _pacientHubConnection = null;
            }
        }
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
            Logger.LogInformation("Incarcare filter options de pe server (via DataService)...");

            // ✅ REFACTORED: Use PacientDataService instead of direct MediatR call
            var result = await DataService.LoadFilterOptionsAsync();

            if (_disposed) return; // Check after async operation

            if (result.IsSuccess)
            {
                // Map filter options
                JudetOptions = result.Value.Judete
                    .Select(j => new FilterOption { Value = j, Text = j })
                    .ToList();

                FilterOptionsLoaded = true;

                Logger.LogInformation(
                       "Filter options loaded via DataService: Judet={JudetCount}, Asigurat={AsiguratCount}, Status={StatusCount}",
              JudetOptions.Count, AsiguratOptions.Count, StatusOptions.Count);
            }
            else
            {
                Logger.LogWarning("Failed to load filter options: {Errors}",
                    string.Join(", ", result.Errors ?? new List<string>()));
                JudetOptions = new List<FilterOption>();
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

            // ✅ REFACTORED: Use PacientDataService instead of direct MediatR call
            bool? asiguratFilter = string.IsNullOrEmpty(FilterAsigurat) ? null : FilterAsigurat == "true";
            bool? statusFilter = string.IsNullOrEmpty(FilterStatus) ? null : FilterStatus == "true";

            var filters = new ValyanClinic.Application.Services.Pacienti.PacientFilters
            {
                SearchText = GlobalSearchText,
                Judet = FilterJudet,
                Asigurat = asiguratFilter,
                Activ = statusFilter
            };

            var pagination = new ValyanClinic.Application.Services.Pacienti.PaginationOptions
            {
                PageNumber = CurrentPage,
                PageSize = CurrentPageSize
            };

            var sorting = new ValyanClinic.Application.Services.Pacienti.SortOptions
            {
                Column = CurrentSortColumn,
                Direction = CurrentSortDirection
            };

            var result = await DataService.LoadPagedDataAsync(filters, pagination, sorting);

            if (_disposed) return; // Check after async operation

            Logger.LogInformation("DataService result: IsSuccess={IsSuccess}", result.IsSuccess);

            if (result.IsSuccess)
            {
                CurrentPageData = result.Value.Items;
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

    // ✅ PAGINATION: Navigation methods
    private int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / CurrentPageSize) : 0;

    private async Task GoToFirstPage()
    {
        if (_disposed || CurrentPage == 1) return;
        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task GoToPreviousPage()
    {
        if (_disposed || CurrentPage <= 1) return;
        CurrentPage--;
        await LoadPagedData();
    }

    private async Task GoToNextPage()
    {
        if (_disposed || CurrentPage >= TotalPages) return;
        CurrentPage++;
        await LoadPagedData();
    }

    private async Task GoToLastPage()
    {
        if (_disposed || CurrentPage == TotalPages) return;
        CurrentPage = TotalPages;
        await LoadPagedData();
    }

    private async Task ChangePageSize(int newPageSize)
    {
        if (_disposed || CurrentPageSize == newPageSize) return;
        
        CurrentPageSize = newPageSize;
        CurrentPage = 1; // Reset to first page
        await LoadPagedData();
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

        // Force UI update BEFORE reloading data
        await InvokeAsync(StateHasChanged);

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

        // Force UI update BEFORE reloading data
        await InvokeAsync(StateHasChanged);

        await LoadPagedData();
    }

    private async Task ClearFilter(string filterName)
    {
        if (_disposed) return; // ✅ FIXED: Changed from (_disposed = true) to (_disposed)

        Logger.LogInformation("Clearing filter: {FilterName}", filterName);

        switch (filterName)
        {
            case FilterNames.GlobalSearchText:
                GlobalSearchText = string.Empty;
                break;
            case FilterNames.FilterJudet:
                FilterJudet = null;
                break;
            case FilterNames.FilterAsigurat:
                FilterAsigurat = null;
                break;
            case FilterNames.FilterStatus:
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
        DataService.InvalidateFilterOptionsCache(); // Invalidate cache on manual refresh
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

    /// <summary>
    /// Export current filtered data to Excel
    /// </summary>
    private async Task HandleExportExcel()
    {
        if (_disposed || CurrentPageData.Count == 0) return;

        // ✅ GUARD: Prevent concurrent exports
        if (_isExporting)
        {
            Logger.LogWarning("[Export] Export already in progress - skipping");
            return;
        }

        try
        {
            _isExporting = true; // ✅ Set flag BEFORE export starts

            Logger.LogInformation("[Export] Starting Excel export for {Count} pacienti", CurrentPageData.Count);
            
            // Export all filtered data, not just current page
            var filters = new ValyanClinic.Application.Services.Pacienti.PacientFilters
            {
                SearchText = GlobalSearchText,
                Judet = FilterJudet,
                Asigurat = string.IsNullOrEmpty(FilterAsigurat) ? null : FilterAsigurat == "true",
                Activ = string.IsNullOrEmpty(FilterStatus) ? null : FilterStatus == "true"
            };

            var pagination = new ValyanClinic.Application.Services.Pacienti.PaginationOptions
            {
                PageNumber = 1,
                PageSize = TotalRecords > 0 ? TotalRecords : 10000 // Export all filtered records
            };

            var sorting = new ValyanClinic.Application.Services.Pacienti.SortOptions
            {
                Column = CurrentSortColumn,
                Direction = CurrentSortDirection
            };

            var result = await DataService.LoadPagedDataAsync(filters, pagination, sorting);

            if (_disposed) return; // ✅ Check after async operation

            if (!result.IsSuccess || result.Value.Items.Count == 0)
            {
                await ShowErrorToastAsync("Nu există date pentru export");
                return;
            }

            var exportData = result.Value.Items;
            var bytes = await ExcelExportService.ExportPacientiToExcelAsync(exportData);

            if (_disposed) return; // ✅ Check after async operation

            // Download file using JS Interop
            var fileName = $"Pacienti_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            await DownloadFileFromBytes(bytes, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            if (_disposed) return; // ✅ Check after async operation

            // ✅ CRITICAL: Wait for JavaScript download to complete (200ms setTimeout in fileDownload.js)
            await Task.Delay(250); // 200ms + 50ms safety margin

            if (!_disposed) // ✅ Only show toast if still mounted
            {
                await ShowSuccessToastAsync($"Exportate {exportData.Count} înregistrări în Excel");
                Logger.LogInformation("[Export] Excel export completed: {FileName}", fileName);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[Export] Component disposed during export (navigation away)");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[Export] Error exporting to Excel");
                await ShowErrorToastAsync($"Eroare la export: {ex.Message}");
            }
        }
        finally
        {
            _isExporting = false; // ✅ Clear flag when done
        }
    }

    /// <summary>
    /// Downloads a file from byte array using JS interop
    /// </summary>
    private async Task DownloadFileFromBytes(byte[] bytes, string fileName, string contentType)
    {
        var base64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", base64, fileName, contentType);
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

    /// <summary>
    /// Quick action - View pacient from row hover button
    /// </summary>
    private async Task HandleQuickView(Guid pacientId, string pacientName)
    {
        if (_disposed) return;

        Logger.LogInformation("[QuickAction] View pacient: {PacientId} - {PacientName}", pacientId, pacientName);
        await OpenViewModalAsync(pacientId, pacientName);
    }

    /// <summary>
    /// Quick action - Manage doctors from row hover button
    /// </summary>
    private async Task HandleQuickDoctors(Guid pacientId, string pacientName)
    {
        if (_disposed) return;

        Logger.LogInformation("[QuickAction] Manage doctors for: {PacientId} - {PacientName}", pacientId, pacientName);

        try
        {
            SelectedPacientId = pacientId;
            SelectedPacientNume = pacientName;

            ShowViewModal = false;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(50);

            ShowDoctoriModal = true;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[QuickAction] Error opening doctors modal for {PacientName}", pacientName);
            await ShowErrorToastAsync($"Eroare: {ex.Message}");
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

    /// <summary>
    /// Gets initials from a full name (e.g., "Ion Popescu" -> "IP")
    /// </summary>
    private static string GetInitials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "?";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 0)
            return "?";
        
        if (parts.Length == 1)
            return parts[0][0].ToString().ToUpper();
        
        // Take first letter of first and last name
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    /// <summary>
    /// Checks if there are no results to display (for empty state)
    /// </summary>
    private bool HasNoResults => DataLoaded && !IsLoading && CurrentPageData.Count == 0;

    #region Saved Filters

    /// <summary>
    /// Load saved filters from localStorage on initialization
    /// </summary>
    private async Task LoadSavedFiltersAsync()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", SavedFiltersStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                SavedFilters = System.Text.Json.JsonSerializer.Deserialize<List<SavedFilter>>(json) ?? new();
                Logger.LogInformation("[SavedFilters] Loaded {Count} saved filters", SavedFilters.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[SavedFilters] Error loading saved filters from localStorage");
            SavedFilters = new();
        }
    }

    /// <summary>
    /// Save filters to localStorage
    /// </summary>
    private async Task PersistSavedFiltersAsync()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(SavedFilters);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", SavedFiltersStorageKey, json);
            Logger.LogInformation("[SavedFilters] Persisted {Count} filters to localStorage", SavedFilters.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[SavedFilters] Error persisting saved filters");
        }
    }

    private void ShowSaveFilterDialog()
    {
        NewFilterName = string.Empty;
        ShowSaveFilterModal = true;
    }

    private void CloseSaveFilterDialog()
    {
        ShowSaveFilterModal = false;
        NewFilterName = string.Empty;
    }

    private async Task SaveCurrentFilter()
    {
        if (string.IsNullOrWhiteSpace(NewFilterName)) return;

        // Check for duplicate name
        if (SavedFilters.Any(f => f.Name.Equals(NewFilterName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            await ShowErrorToastAsync("Există deja un filtru cu acest nume");
            return;
        }

        var newFilter = new SavedFilter
        {
            Name = NewFilterName.Trim(),
            SearchText = GlobalSearchText,
            Judet = FilterJudet,
            Asigurat = FilterAsigurat,
            Status = FilterStatus,
            CreatedAt = DateTime.Now
        };

        SavedFilters.Add(newFilter);
        await PersistSavedFiltersAsync();

        CloseSaveFilterDialog();
        await ShowSuccessToastAsync($"Filtrul '{newFilter.Name}' a fost salvat");
        Logger.LogInformation("[SavedFilters] Created new filter: {Name}", newFilter.Name);
    }

    private async Task OnSavedFilterSelected(ChangeEventArgs e)
    {
        var filterName = e.Value?.ToString();
        if (string.IsNullOrEmpty(filterName))
        {
            SelectedSavedFilter = string.Empty;
            return;
        }

        SelectedSavedFilter = filterName;
        var filter = SavedFilters.FirstOrDefault(f => f.Name == filterName);
        if (filter != null)
        {
            // Apply saved filter values
            GlobalSearchText = filter.SearchText ?? string.Empty;
            FilterJudet = filter.Judet;
            FilterAsigurat = filter.Asigurat;
            FilterStatus = filter.Status;

            Logger.LogInformation("[SavedFilters] Applied filter: {Name}", filterName);

            // Apply filters and reload data
            CurrentPage = 1;
            await LoadPagedData();
        }
    }

    private async Task DeleteCurrentSavedFilter()
    {
        if (string.IsNullOrEmpty(SelectedSavedFilter)) return;

        var filter = SavedFilters.FirstOrDefault(f => f.Name == SelectedSavedFilter);
        if (filter != null)
        {
            SavedFilters.Remove(filter);
            await PersistSavedFiltersAsync();
            await ShowSuccessToastAsync($"Filtrul '{SelectedSavedFilter}' a fost șters");
            Logger.LogInformation("[SavedFilters] Deleted filter: {Name}", SelectedSavedFilter);
            SelectedSavedFilter = string.Empty;
        }
    }

    #endregion

    /// <summary>
    /// Static class containing filter name constants to avoid magic strings.
    /// </summary>
    private static class FilterNames
    {
        public const string GlobalSearchText = nameof(GlobalSearchText);
        public const string FilterJudet = nameof(FilterJudet);
        public const string FilterAsigurat = nameof(FilterAsigurat);
        public const string FilterStatus = nameof(FilterStatus);
    }

    /// <summary>
    /// Represents a saved filter configuration
    /// </summary>
    public class SavedFilter
    {
        public string Name { get; set; } = string.Empty;
        public string? SearchText { get; set; }
        public string? Judet { get; set; }
        public string? Asigurat { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
