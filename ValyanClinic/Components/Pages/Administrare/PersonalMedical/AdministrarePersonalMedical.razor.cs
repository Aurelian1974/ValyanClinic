using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.DeletePersonalMedical;
using ValyanClinic.Services.DataGrid;
using Microsoft.Extensions.Logging;
using ValyanClinic.Components.Pages.Administrare.PersonalMedical.Modals;
using ValyanClinic.Components.Shared.Modals;
using Microsoft.JSInterop;

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical;

public partial class AdministrarePersonalMedical : ComponentBase, IDisposable
{
    // CRITICAL: Static lock pentru ABSOLUTE protection
    private static readonly object _initLock = new object();
    private static bool _anyInstanceInitializing = false;
    private static string? _initializingComponentName = null;
    
    // Unique key pentru tracking disposal
    private readonly string KeyValue = Guid.NewGuid().ToString();
  
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonalMedical> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Grid and Toast references
    private SfGrid<PersonalMedicalListDto>? GridRef;
    private SfToast? ToastRef;

    // Modal references
    private PersonalMedicalViewModal? personalMedicalViewModal;
    private PersonalMedicalFormModal? personalMedicalFormModal;
    private ConfirmDeleteModal? confirmDeleteModal;

    private List<PersonalMedicalListDto> CurrentPageData { get; set; } = new();
    
    private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    private int TotalRecords { get; set; } = 0;
    private int TotalPages => TotalRecords > 0 && CurrentPageSize > 0 
        ? (int)Math.Ceiling((double)TotalRecords / CurrentPageSize) 
        : 1;
    
    private const int MinPageSize = 10;
    private const int MaxPageSize = 1000;
    private const int DefaultPageSizeValue = 20;
    private int[] PageSizeArray = { 10, 20, 50, 100, 250, 500, 1000 };
    
    private string CurrentSortColumn { get; set; } = "Nume";
    private string CurrentSortDirection { get; set; } = "ASC";
    
    private PersonalMedicalListDto? SelectedPersonal { get; set; }

    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    private bool IsAdvancedFilterExpanded { get; set; } = false;
    private string GlobalSearchText { get; set; } = string.Empty;
    private string? FilterDepartament { get; set; }
    private string? FilterPozitie { get; set; }
    private string? FilterEsteActiv { get; set; }
    
    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int SearchDebounceMs = 500;
    private bool _disposed = false;
    private bool _initialized = false;
    private bool _isInitializing = false;

    private List<FilterOption> StatusOptions { get; set; } = new();
    private List<FilterOption> DepartamentOptions { get; set; } = new();
    private List<FilterOption> PozitieOptions { get; set; } = new();

    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    private int ActiveFiltersCount => 
        (string.IsNullOrEmpty(FilterDepartament) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterPozitie) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterEsteActiv) ? 0 : 1) +
        (string.IsNullOrEmpty(GlobalSearchText) ? 0 : 1);

    private bool HasPreviousPage => CurrentPage > 1;
    private bool HasNextPage => CurrentPage < TotalPages;
    private int DisplayedRecordsStart => TotalRecords > 0 ? ((CurrentPage - 1) * CurrentPageSize) + 1 : 0;
    private int DisplayedRecordsEnd => Math.Min(CurrentPage * CurrentPageSize, TotalRecords);

    protected override async Task OnInitializedAsync()
    {
        var startTime = DateTime.Now;
        var threadId = Thread.CurrentThread.ManagedThreadId;
     var componentName = "[PersonalMedical]";
        
        Logger.LogWarning("🟢 {Component} OnInitializedAsync START - Time: {Time}, Thread: {ThreadId}, KeyValue: {KeyValue}", 
      componentName, startTime.ToString("HH:mm:ss.fff"), threadId, KeyValue);
        
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
          // CRITICAL: Simple delay de 500ms - GARANTAT că funcționează
   // Elimină complexity JavaScript check care poate da false positive
  Logger.LogWarning("⏳ {Component} Waiting 500ms for component cleanup - Time: {Time}", 
          componentName, DateTime.Now.ToString("HH:mm:ss.fff"));
            
            await Task.Delay(500); // Simple, reliable, tested
            
            Logger.LogWarning("✅ {Component} Delay complete - Time: {Time}, Elapsed: {Elapsed}ms", 
                componentName, DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - startTime).TotalMilliseconds);
      
          Logger.LogInformation("Initializare pagina Administrare Personal Medical");
          
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
  
  Logger.LogWarning("🔴 [PersonalMedical] Dispose START - Time: {Time}, Thread: {ThreadId}, Already disposed: {Disposed}", 
  disposeTime.ToString("HH:mm:ss.fff"), threadId, _disposed);
        
  if (_disposed)
  {
  Logger.LogWarning("⚠️ [PersonalMedical] Already disposed - SKIPPING - Time: {Time}", 
  DateTime.Now.ToString("HH:mm:ss.fff"));
            return;
   }
  
    // Setează flag imediat pentru a bloca noi operații
      _disposed = true;
        
 Logger.LogWarning("🚫 [PersonalMedical] _disposed flag set to TRUE - Time: {Time}", 
  DateTime.Now.ToString("HH:mm:ss.fff"));
  
  // CRITICAL: Cleanup SINCRON - DAR NU atingem GridRef!
   try
    {
  Logger.LogDebug("🧹 [PersonalMedical] SYNC cleanup START - Time: {Time}", 
  DateTime.Now.ToString("HH:mm:ss.fff"));
   
    // ❌ NU MAI setăm GridRef = null! Lăsăm Blazor să gestioneze
          // GridRef = null; // REMOVED - causes JavaScript callback errors
   
     if (GridRef != null)
   {
  Logger.LogDebug("ℹ️ [PersonalMedical] GridRef exists - Blazor will handle disposal - Time: {Time}", 
 DateTime.Now.ToString("HH:mm:ss.fff"));
}
     
       // Cancel orice operații în curs
     if (_searchDebounceTokenSource != null)
  {
  Logger.LogDebug("❌ [PersonalMedical] Cancelling search token - Time: {Time}", 
   DateTime.Now.ToString("HH:mm:ss.fff"));
      _searchDebounceTokenSource?.Cancel();
     _searchDebounceTokenSource?.Dispose();
    _searchDebounceTokenSource = null;
 }
 
        // Clear data IMEDIAT
     var dataCount = CurrentPageData?.Count ?? 0;
     Logger.LogDebug("🗑️ [PersonalMedical] Clearing {Count} data items - Time: {Time}", 
          dataCount, DateTime.Now.ToString("HH:mm:ss.fff"));
   CurrentPageData?.Clear();
   CurrentPageData = new();
 
   Logger.LogDebug("✅ [PersonalMedical] SYNC cleanup COMPLETE - Time: {Time}, Elapsed: {Elapsed}ms", 
        DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - disposeTime).TotalMilliseconds);
   }
        catch (Exception ex)
  {
    Logger.LogError(ex, "❌ [PersonalMedical] ERROR in sync dispose - Time: {Time}", 
    DateTime.Now.ToString("HH:mm:ss.fff"));
      }
      
  Logger.LogWarning("🏁 [PersonalMedical] Dispose END - Time: {Time}, Total elapsed: {Elapsed}ms (Syncfusion cleanup handled by Blazor)", 
          DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - disposeTime).TotalMilliseconds);
    }


    private async Task LoadPagedData()
    {
        if (_disposed) return; // Guard check

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            if (CurrentPageSize < MinPageSize) CurrentPageSize = MinPageSize;
            if (CurrentPageSize > MaxPageSize) CurrentPageSize = MaxPageSize;

            Logger.LogInformation(
           "SERVER-SIDE Load: Page={Page}, Size={Size}, Search='{Search}', Dept={Dept}, Poz={Poz}, Activ={Activ}, Sort={Sort} {Dir}",
    CurrentPage, CurrentPageSize, GlobalSearchText, FilterDepartament, 
         FilterPozitie, FilterEsteActiv, CurrentSortColumn, CurrentSortDirection);

            bool? esteActivFilter = null;
            if (!string.IsNullOrEmpty(FilterEsteActiv))
            {
                esteActivFilter = FilterEsteActiv == "true";
            }

            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = CurrentPage,
                PageSize = CurrentPageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(GlobalSearchText) ? null : GlobalSearchText,
                FilterDepartament = FilterDepartament,
                FilterPozitie = FilterPozitie,
                FilterEsteActiv = esteActivFilter,
                SortColumn = CurrentSortColumn,
                SortDirection = CurrentSortDirection
            };

            var result = await Mediator.Send(query);

            if (_disposed) return; // Check after async

            if (result.IsSuccess)
            {
                CurrentPageData = result.Value?.ToList() ?? new List<PersonalMedicalListDto>();
                TotalRecords = result.TotalCount;
                
                if (TotalPages > 0 && CurrentPage > TotalPages)
                {
                    Logger.LogWarning("CurrentPage {Page} > TotalPages {Total}, ajustare", 
                        CurrentPage, TotalPages);
                    CurrentPage = TotalPages;
                    await LoadPagedData();
                    return;
                }
                
                LoadFilterOptionsFromData();
                
                Logger.LogInformation(
                    "Data loaded: Page {Page}/{Total}, Records {Start}-{End} din {TotalRecords}",
                    CurrentPage, TotalPages, DisplayedRecordsStart, DisplayedRecordsEnd, TotalRecords);
            }
            else
            {
                HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Eroare la incarcarea datelor: {Message}", ErrorMessage);
                CurrentPageData = new List<PersonalMedicalListDto>();
                TotalRecords = 0;
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("Component disposed while loading data (navigation away)");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                HasError = true;
                ErrorMessage = $"Eroare neasteptata: {ex.Message}";
                Logger.LogError(ex, "Eroare la incarcarea datelor");
                CurrentPageData = new List<PersonalMedicalListDto>();
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

    private async Task ClearAllFilters()
    {
        if (_disposed) return; // Guard check
  
      Logger.LogInformation("Clearing all filters");
        
        GlobalSearchText = string.Empty;
        FilterDepartament = null;
        FilterPozitie = null;
        FilterEsteActiv = null;

     CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task ClearFilter(string filterName)
    {
        if (_disposed) return; // Guard check
     
        Logger.LogInformation("Clearing filter: {FilterName}", filterName);

        switch (filterName)
        {
     case nameof(FilterEsteActiv):
         FilterEsteActiv = null;
                break;
            case nameof(FilterDepartament):
 FilterDepartament = null;
            break;
        case nameof(FilterPozitie):
    FilterPozitie = null;
  break;
   case nameof(GlobalSearchText):
                GlobalSearchText = string.Empty;
       break;
      }

        CurrentPage = 1;
        await LoadPagedData();
    }

    private void LoadFilterOptionsFromData()
    {
        if (!CurrentPageData.Any()) return;

        // Status Options (static)
        StatusOptions = new List<FilterOption>
        {
            new FilterOption { Value = "true", Text = "Activ" },
            new FilterOption { Value = "false", Text = "Inactiv" }
        };

        var currentDeptOptions = CurrentPageData
            .Where(p => !string.IsNullOrEmpty(p.Departament))
            .Select(p => p.Departament!)
            .Distinct()
            .OrderBy(d => d)
            .Select(d => new FilterOption { Value = d, Text = d })
            .ToList();

        var currentPozOptions = CurrentPageData
            .Where(p => !string.IsNullOrEmpty(p.Pozitie))
            .Select(p => p.Pozitie!)
            .Distinct()
            .OrderBy(p => p)
            .Select(p => new FilterOption { Value = p, Text = p })
            .ToList();

        DepartamentOptions = DepartamentOptions.Union(currentDeptOptions).Distinct().OrderBy(o => o.Value).ToList();
        PozitieOptions = PozitieOptions.Union(currentPozOptions).Distinct().OrderBy(o => o.Value).ToList();
        
        Logger.LogDebug(
            "Filter options updated: Status={StatusCount}, Dept={DeptCount}, Poz={PozCount}",
            StatusOptions.Count, DepartamentOptions.Count, PozitieOptions.Count);
    }

    private void ToggleAdvancedFilter()
    {
   if (_disposed) return; // Guard check
  
        IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
  Logger.LogInformation("Advanced filter toggled: {State}", 
    IsAdvancedFilterExpanded ? "Expanded" : "Collapsed");
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        if (_disposed) return; // Guard check
        
        var newValue = e.Value?.ToString() ?? string.Empty;
   
      if (newValue == GlobalSearchText) return;
      
   GlobalSearchText = newValue;
   
        Logger.LogDebug("Search input changed: '{SearchText}'", GlobalSearchText);
        
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
       Logger.LogInformation("Executing search for: '{SearchText}'", GlobalSearchText);
           
          await InvokeAsync(async () =>
      {
     if (!_disposed)
             {
            CurrentPage = 1;
     await LoadPagedData();
        }
      });
        }
    }
 catch (TaskCanceledException)
 {
   Logger.LogDebug("Search cancelled");
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
      if (_disposed) return; // Guard check 
 
    if (e.Key == "Enter")
        {
       _searchDebounceTokenSource?.Cancel();
  
            Logger.LogInformation("Enter pressed - immediate search: '{SearchText}'", GlobalSearchText);
            
     CurrentPage = 1;
      await LoadPagedData();
        }
    }

    private async Task ClearSearch()
   {
        if (_disposed) return; // Guard check
        
    Logger.LogInformation("Clearing search");
    
        _searchDebounceTokenSource?.Cancel();
      
        GlobalSearchText = string.Empty;
   CurrentPage = 1;
    await LoadPagedData();
    }

    private async Task ApplyFilters()
    {
        if (_disposed) return; // Guard check
  
        Logger.LogInformation(
            "Applying filters: Search={Search}, Dept={Dept}, Poz={Poz}, Activ={Activ}",
            GlobalSearchText, FilterDepartament, FilterPozitie, FilterEsteActiv);

        CurrentPage = 1;
 await LoadPagedData();
  }

    private async Task HandleRefresh()
    {
        if (_disposed) return; // Guard check
        
        Logger.LogInformation("Refresh personal medical");
        
    await LoadPagedData();
        await ShowToast("Succes", "Datele au fost reincarcate", "e-toast-success");
    }

    private async Task HandleAddNew()
 {
  if (_disposed) return; // Guard check 
      Logger.LogInformation("Opening modal for ADD PersonalMedical");
        
        if (personalMedicalFormModal != null)
 {
      await personalMedicalFormModal.OpenForAdd();
    }
    }

    private async Task HandleViewSelected()
{
        if (_disposed) return; // Guard check
        
        if (SelectedPersonal == null)
      {
            await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
    return;
        }
   
        Logger.LogInformation("Opening View modal for: {PersonalID}", SelectedPersonal.PersonalID);
 
        if (personalMedicalViewModal != null)
        {
  await personalMedicalViewModal.Open(SelectedPersonal.PersonalID);
        }
    }

    private async Task HandleEditSelected()
    {
        if (_disposed) return; // Guard check 
        if (SelectedPersonal == null)
        {
     await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
    return;
        }
        
 Logger.LogInformation("Opening Edit modal for: {PersonalID}", SelectedPersonal.PersonalID);
        
     if (personalMedicalFormModal != null)
        {
            await personalMedicalFormModal.OpenForEdit(SelectedPersonal.PersonalID);
        }
    }

    private async Task HandleDeleteSelected()
    {
      if (_disposed) return; // Guard check
        
        if (SelectedPersonal == null)
        {
 await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
      return;
        }
  
        Logger.LogInformation("Opening Delete modal for: {PersonalID} - {NumeComplet}", 
   SelectedPersonal.PersonalID, SelectedPersonal.NumeComplet);
        
if (confirmDeleteModal != null)
        {
       await confirmDeleteModal.Open(SelectedPersonal.PersonalID, SelectedPersonal.NumeComplet);
        }
 }

    // Modal event handlers
    private async Task HandleEditFromModal(Guid personalID)
    {
        if (_disposed) return; // Guard check
      
     Logger.LogInformation("Edit requested from modal for: {PersonalID}", personalID);
        
        // IMPORTANT: Inchide modalul de view inainte de a deschide modalul de edit
        if (personalMedicalViewModal != null)
     {
 await personalMedicalViewModal.Close();
        }
 
      if (personalMedicalFormModal != null)
      {
            await personalMedicalFormModal.OpenForEdit(personalID);
     }
    }

    private async Task HandleDeleteFromModal(Guid personalID)
    {
   if (_disposed) return; // Guard check
        
   var personalMedical = CurrentPageData.FirstOrDefault(p => p.PersonalID == personalID);
  if (personalMedical != null && confirmDeleteModal != null)
    {
   await confirmDeleteModal.Open(personalID, personalMedical.NumeComplet);
        }
    }

    private async Task HandlePersonalMedicalSaved()
   {
        if (_disposed) return;
      
        Logger.LogInformation("🎉 Personal Medical saved - FORCING component re-initialization");
 
        try
        {
        // 1️⃣ Wait for modal to close completely
   Logger.LogInformation("⏳ Waiting 700ms for modal close...");
            await Task.Delay(700);
 
if (_disposed) return;
          
        // 2️⃣ Show loading state
       IsLoading = true;
     await InvokeAsync(StateHasChanged);
      
            // 3️⃣ Force navigation to SAME page (triggers full re-init)
            Logger.LogInformation("🔄 Force navigation to trigger re-initialization");
       NavigationManager.NavigateTo("/administrare/personal-medical", forceLoad: true);
      
          // Note: forceLoad: true forces a FULL page reload, not just component refresh
            // This clears ALL Blazor state and starts fresh - exactly like F5!
        }
        catch (Exception ex)
        {
      Logger.LogError(ex, "Error during forced re-initialization");
   
            // Fallback: Reload data normally if navigation fails
          if (!_disposed)
 {
            await LoadPagedData();
 
           // Refresh view modal if open
           if (personalMedicalViewModal != null)
            {
   Logger.LogInformation("Refreshing view modal data after save");
       await personalMedicalViewModal.RefreshData();
}
  
   await ShowToast("Succes", "Personal medical salvat cu succes", "e-toast-success");
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

    private async Task HandleDeleteConfirmed(Guid personalID)
    {
        if (_disposed) return;
        
        Logger.LogInformation("Delete confirmed for: {PersonalID}", personalID);
        
        try
        {
            var command = new DeletePersonalMedicalCommand(personalID, "CurrentUser");
            var result = await Mediator.Send(command);
            
            if (_disposed) return; // Check after async
            
            if (result.IsSuccess)
            {
                Logger.LogInformation("PersonalMedical deleted successfully: {PersonalID}", personalID);
                await LoadPagedData();
                await ShowToast("Succes", "Personal medical sters cuSucces", "e-toast-success");
            }
            else
            {
                var errorMsg = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Delete failed: {Error}", errorMsg);
                await ShowToast("Eroare", errorMsg, "e-toast-danger");
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("Component disposed during delete");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "Exception during delete: {PersonalID}", personalID);
                await ShowToast("Eroare", $"Eroare la stergere: {ex.Message}", "e-toast-danger");
            }
        }
    }

    private async Task GoToPage(int pageNumber)
    {
        if (_disposed) return; // Guard check
        
    if (pageNumber < 1 || pageNumber > TotalPages) return;
   
        Logger.LogInformation("Navigare la pagina {Page}", pageNumber);
CurrentPage = pageNumber;
        await LoadPagedData();
    }

    private async Task GoToFirstPage()
    {
        if (_disposed) return; // Guard check
        await GoToPage(1);
    }

  private async Task GoToLastPage()
  {
    if (_disposed) return; // Guard check
    await GoToPage(TotalPages);
  }

    private async Task GoToPreviousPage()
    {
        if (_disposed) return; // Guard check
        
        if (HasPreviousPage)
  {
            await GoToPage(CurrentPage - 1);
        }
    }

    private async Task GoToNextPage()
    {
        if (_disposed) return; // Guard check
    
        if (HasNextPage)
   {
      await GoToPage(CurrentPage + 1);
      }
    }

    private async Task OnPageSizeChanged(int newPageSize)
    {
        if (_disposed) return; // Guard check
        
        if (newPageSize < MinPageSize || newPageSize > MaxPageSize)
     {
         Logger.LogWarning("PageSize invalid: {Size}, using default", newPageSize);
   newPageSize = DefaultPageSizeValue;
        }
        
        Logger.LogInformation("PageSize changed: {OldSize} -> {NewSize}", CurrentPageSize, newPageSize);
 
        CurrentPageSize = newPageSize;
      CurrentPage = 1;
   await LoadPagedData();
    }

    private async Task OnPageSizeChangedNative(ChangeEventArgs e)
    {
 if (_disposed) return; // Guard check
        
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

    private void OnRowSelected(RowSelectEventArgs<PersonalMedicalListDto> args)
    {
        if (_disposed) return;
        
        SelectedPersonal = args.Data;
        Logger.LogInformation("Personal medical selectat: {PersonalID} - {NumeComplet}", 
            SelectedPersonal?.PersonalID, SelectedPersonal?.NumeComplet);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PersonalMedicalListDto> args)
    {
        if (_disposed) return;
        
        SelectedPersonal = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    private async Task OnGridActionBegin(ActionEventArgs<PersonalMedicalListDto> args)
    {
        if (_disposed) return;
        
        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting)
        {
            args.Cancel = true;
            
            if (args is { Data: not null })
            {
                var sortingColumns = (args.Data as IEnumerable<object>)?.Cast<dynamic>().ToList();
                if (sortingColumns?.Any() == true)
                {
                    var sortCol = sortingColumns[0];
                    CurrentSortColumn = sortCol.Name?.ToString() ?? "Nume";
                    CurrentSortDirection = sortCol.Direction?.ToString()?.ToUpper() ?? "ASC";
                    
                    Logger.LogInformation("Sort: {Column} {Direction}", 
                        CurrentSortColumn, CurrentSortDirection);
                    
                    await LoadPagedData();
                }
            }
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

    private class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
