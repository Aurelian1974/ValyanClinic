using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieList;
using ValyanClinic.Application.Features.PozitieManagement.Commands.DeletePozitie;
using Microsoft.Extensions.Logging;
using ValyanClinic.Components.Shared.Modals;
using ValyanClinic.Components.Pages.Administrare.Pozitii.Modals;

namespace ValyanClinic.Components.Pages.Administrare.Pozitii;

public partial class AdministrarePozitii : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePozitii> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private SfGrid<PozitieListDto>? GridRef;
    private SfToast? ToastRef;
    private ConfirmDeleteModal? confirmDeleteModal;
    private PozitieFormModal? pozitieFormModal;
    private PozitieViewModal? pozitieViewModal;

    private List<PozitieListDto> CurrentPageData { get; set; } = new();
    
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
    
    private string CurrentSortColumn { get; set; } = "Denumire";
    private string CurrentSortDirection { get; set; } = "ASC";
    
    private PozitieListDto? SelectedPozitie { get; set; }

    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    private string GlobalSearchText { get; set; } = string.Empty;
    
    private CancellationTokenSource? _searchDebounceTokenSource;
private const int SearchDebounceMs = 500;
    private bool _disposed = false;

    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    private bool HasPreviousPage => CurrentPage > 1;
    private bool HasNextPage => CurrentPage < TotalPages;
    private int DisplayedRecordsStart => TotalRecords > 0 ? ((CurrentPage - 1) * CurrentPageSize) + 1 : 0;
    private int DisplayedRecordsEnd => Math.Min(CurrentPage * CurrentPageSize, TotalRecords);

    private static readonly object _initLock = new object();
    private static bool _isInitializing = false;
    private static bool _anyInstanceInitializing = false;
    private bool _initialized = false;
    
    protected override async Task OnInitializedAsync()
    {
        // CRITICAL: GLOBAL lock la nivel de pagină
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
// CRITICAL: Delay MĂRIT pentru a permite componentei anterioare să facă cleanup COMPLET
     // Syncfusion Grid necesită timp suplimentar pentru cleanup DOM - mărit de la 300ms la 800ms
     Logger.LogInformation("Waiting for previous component cleanup...");
     await Task.Delay(800); // MĂRIT de la 300ms la 800ms pentru Syncfusion Grid cleanup
    
      Logger.LogInformation("Initializare pagina Administrare Pozitii");
   await LoadPagedData();
  
_initialized = true;
        }
 catch (ObjectDisposedException ex)
     {
         Logger.LogWarning(ex, "Component disposed during initialization (navigation away)");
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
       lock (_initLock)
     {
    _isInitializing = false;
    _anyInstanceInitializing = false;
    }
        }
}
    public void Dispose()
    {
        if (_disposed) return;
      
      // Setează flag imediat pentru a bloca noi operații
        _disposed = true;
   
        // CRITICAL: Cleanup SINCRON pentru Syncfusion Grid - FĂRĂ să setăm GridRef = null
   try
        {
  Logger.LogDebug("AdministrarePozitii disposing - SYNCHRONOUS cleanup");

            // ❌ NU setăm GridRef = null - lăsăm Syncfusion să gestioneze propriul lifecycle
            // GridRef = null; // ELIMINAT - cauzează probleme cu referințele
      
      // Cancel orice operații în curs
    _searchDebounceTokenSource?.Cancel();
  _searchDebounceTokenSource?.Dispose();
            _searchDebounceTokenSource = null;
            
      // Clear data IMEDIAT
     CurrentPageData?.Clear();
            CurrentPageData = new();
       
    Logger.LogDebug("AdministrarePozitii disposed - Data cleared, GridRef preserved");
 }
        catch (Exception ex)
        {
      Logger.LogError(ex, "Error in synchronous dispose");
        }
  
        // CRITICAL: ELIMINAT async cleanup forțat - lăsăm Blazor disposal natural
        // Nu mai folosim Task.Run cu delay-uri care cauzează race conditions
        Logger.LogDebug("AdministrarePozitii dispose COMPLETE - Natural Blazor disposal will handle GridRef");
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
    "SERVER-SIDE Load: Page={Page}, Size={Size}, Search='{Search}', Sort={Sort} {Dir}",
       CurrentPage, CurrentPageSize, GlobalSearchText, CurrentSortColumn, CurrentSortDirection);

  var query = new GetPozitieListQuery
{
     PageNumber = CurrentPage,
          PageSize = CurrentPageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(GlobalSearchText) ? null : GlobalSearchText,
SortColumn = CurrentSortColumn,
    SortDirection = CurrentSortDirection
            };

        var result = await Mediator.Send(query);

if (_disposed) return; // Check after async

      if (result.IsSuccess)
          {
       CurrentPageData = result.Value?.ToList() ?? new List<PozitieListDto>();
       TotalRecords = result.TotalCount;
      
             if (TotalPages > 0 && CurrentPage > TotalPages)
                {
  Logger.LogWarning("CurrentPage {Page} > TotalPages {Total}, ajustare", 
        CurrentPage, TotalPages);
       CurrentPage = TotalPages;
            await LoadPagedData();
         return;
             }
   
              Logger.LogInformation(
              "Data loaded: Page {Page}/{Total}, Records {Start}-{End} din {TotalRecords}",
           CurrentPage, TotalPages, DisplayedRecordsStart, DisplayedRecordsEnd, TotalRecords);
       }
            else
       {
    HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
       Logger.LogWarning("Eroare la incarcarea datelor: {Message}", ErrorMessage);
     CurrentPageData = new List<PozitieListDto>();
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
        CurrentPageData = new List<PozitieListDto>();
    TotalRecords = 0;
            }
        }
        finally
        {
        if (!_disposed)
            {
    IsLoading = false;
        await InvokeAsync(StateHasChanged);
            }
      }
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

    private async Task HandleRefresh()
    {
        if (_disposed) return; // Guard check
        
        Logger.LogInformation("Refresh pozitii");
        
        await LoadPagedData();
        await ShowToast("Succes", "Datele au fost reincarcate", "e-toast-success");
    }

    private async Task HandleAddNew()
    {
        if (_disposed) return; // Guard check
        
        Logger.LogInformation("Opening modal for ADD Pozitie");
        
      if (pozitieFormModal != null)
        {
  await pozitieFormModal.OpenForAdd();
    }
    }

    private async Task HandleViewSelected()
 {
     if (_disposed) return; // Guard check
     
  if (SelectedPozitie == null)
        {
            await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
   return;
        }
      
  Logger.LogInformation("Opening View modal for: {Id}", SelectedPozitie.Id);
 
        if (pozitieViewModal != null)
        {
  await pozitieViewModal.Open(SelectedPozitie.Id);
        }
    }

    private async Task HandleEditSelected()
    {
        if (_disposed) return; // Guard check
        
      if (SelectedPozitie == null)
        {
            await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
            return;
        }
        
  Logger.LogInformation("Opening Edit modal for: {Id}", SelectedPozitie.Id);
        
     if (pozitieFormModal != null)
   {
            await pozitieFormModal.OpenForEdit(SelectedPozitie.Id);
        }
    }

    private async Task HandlePozitieSaved()
    {
        if (_disposed) return;
     
        Logger.LogInformation("🎉 Pozitie saved - FORCING component re-initialization");
 
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
            NavigationManager.NavigateTo("/administrare/pozitii", forceLoad: true);
      
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
          await ShowToast("Succes", "Pozitie salvata cu succes", "e-toast-success");
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

    private async Task HandleEditFromView(Guid pozitieId)
    {
        if (_disposed) return; // Guard check

        Logger.LogInformation("Edit requested from View modal for: {Id}", pozitieId);
        
        if (pozitieViewModal != null)
        {
        await pozitieViewModal.Close();
        }
        
        if (pozitieFormModal != null)
        {
          await pozitieFormModal.OpenForEdit(pozitieId);
        }
    }

    private async Task HandleDeleteFromView(Guid pozitieId)
    {
        if (_disposed) return; // Guard check
        
        Logger.LogInformation("Delete requested from View modal for: {Id}", pozitieId);
      
        if (pozitieViewModal != null)
 {
         await pozitieViewModal.Close();
        }
    
        var pozitie = CurrentPageData.FirstOrDefault(p => p.Id == pozitieId);
        if (pozitie != null && confirmDeleteModal != null)
 {
          await confirmDeleteModal.Open(pozitieId, pozitie.Denumire);
        }
    }

    private async Task HandleDeleteSelected()
    {
   if (_disposed) return; // Guard check
        
     if (SelectedPozitie == null)
        {
     await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
            return;
        }
        
        Logger.LogInformation("Opening Delete modal for: {Id} - {Denumire}", 
         SelectedPozitie.Id, SelectedPozitie.Denumire);
    
        if (confirmDeleteModal != null)
   {
            await confirmDeleteModal.Open(SelectedPozitie.Id, SelectedPozitie.Denumire);
        }
    }

    private async Task HandleDeleteConfirmed(Guid id)
    {
        if (_disposed) return; // Guard check
        
        Logger.LogInformation("Delete confirmed for: {Id}", id);
      
   try
        {
            var command = new DeletePozitieCommand(id);
 var result = await Mediator.Send(command);
            
      if (_disposed) return; // Check after async
            
     if (result.IsSuccess)
      {
         Logger.LogInformation("Pozitie deleted successfully: {Id}", id);
      await LoadPagedData();
        await ShowToast("Succes", "Pozitie stearsa cu succes", "e-toast-success");
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
        Logger.LogError(ex, "Exception during delete: {Id}", id);
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

    private void OnRowSelected(RowSelectEventArgs<PozitieListDto> args)
    {
  if (_disposed) return; // Guard check
        
        SelectedPozitie = args.Data;
        Logger.LogInformation("Pozitie selectata: {Id} - {Denumire}", 
            SelectedPozitie?.Id, SelectedPozitie?.Denumire);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PozitieListDto> args)
    {
        if (_disposed) return; // Guard check
        
        SelectedPozitie = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    private async Task OnGridActionBegin(ActionEventArgs<PozitieListDto> args)
    {
     if (_disposed) return; // Guard check
    
  if (args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting)
        {
            args.Cancel = true;
     
    if (args is { Data: not null })
  {
       var sortingColumns = (args.Data as IEnumerable<object>)?.Cast<dynamic>().ToList();
         if (sortingColumns?.Any() == true)
        {
       var sortCol = sortingColumns[0];
           CurrentSortColumn = sortCol.Name?.ToString() ?? "Denumire";
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
}
