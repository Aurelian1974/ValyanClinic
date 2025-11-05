using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Application.Features.PacientManagement.Commands.DeletePacient;

namespace ValyanClinic.Components.Pages.Pacienti;

public partial class AdministrarePacienti : ComponentBase, IDisposable
{
    // Static lock pentru ABSOLUTE protection
    private static readonly object _initLock = new object();
    private static bool _anyInstanceInitializing = false;
    
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AdministrarePacienti> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!; // ✅ ADDED

    // Syncfusion Grid Reference
  private SfGrid<PacientListDto>? GridRef { get; set; }
    
    // Toast reference
    private SfToast? ToastRef { get; set; }

    // State Management
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    
    // Guard flags
    private bool _disposed = false;
    private bool _initialized = false;
    private bool _isInitializing = false;
    
    // Data
    private List<PacientListDto>? AllPacienti { get; set; }
    private List<PacientListDto> FilteredPacienti => ApplyClientFilters();

    // Filters
    private string SearchText { get; set; } = string.Empty;
    private string FilterActiv { get; set; } = string.Empty;
  private string FilterAsigurat { get; set; } = string.Empty;
private string FilterJudet { get; set; } = string.Empty;
    private List<string> JudeteList { get; set; } = new();

    // Computed Properties
    private bool HasActiveFilters => 
  !string.IsNullOrEmpty(SearchText) || 
  !string.IsNullOrEmpty(FilterActiv) || 
        !string.IsNullOrEmpty(FilterAsigurat) || 
      !string.IsNullOrEmpty(FilterJudet);

    // Timer pentru debounce search
    private System.Timers.Timer? _searchDebounceTimer;

    // Modal States
    private bool ShowAddEditModal { get; set; }
    private bool ShowViewModal { get; set; }
    private bool ShowHistoryModal { get; set; }
    private bool ShowDocumentsModal { get; set; }
    private bool ShowDeleteModal { get; set; }
    private Guid? SelectedPacientId { get; set; }
    private string DeleteConfirmMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
  // CRITICAL - GLOBAL lock la nivel de pagină
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
            // CRITICAL - Delay MĂRIT pentru cleanup complet
            Logger.LogInformation("Waiting for previous component cleanup...");
      await Task.Delay(800);
            
         Logger.LogInformation("Initializare pagina Administrare Pacienti");
 
         await LoadDataAsync();
            await LoadJudeteAsync();
  
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

    private async Task LoadDataAsync()
    {
        if (_disposed) return;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var query = new GetPacientListQuery
            {
         PageNumber = 1,
   PageSize = 10000, // Load all for client-side filtering
     SortColumn = "Nume",
      SortDirection = "ASC"
       };

     var result = await Mediator.Send(query);

         if (_disposed) return;

         if (result.IsSuccess && result.Value != null)
{
    AllPacienti = result.Value.Value?.ToList() ?? new List<PacientListDto>();
      }
        else
       {
                HasError = true;
       ErrorMessage = result.FirstError ?? "Eroare la încărcarea datelor.";
     AllPacienti = new List<PacientListDto>();
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
                ErrorMessage = $"Eroare neașteptată: {ex.Message}";
  AllPacienti = new List<PacientListDto>();
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

    private List<PacientListDto> ApplyClientFilters()
    {
        if (AllPacienti == null)
            return new List<PacientListDto>();

        var filtered = AllPacienti.AsEnumerable();

        // Search filter
    if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
   filtered = filtered.Where(p =>
    (p.NumeComplet?.ToLower().Contains(search) ?? false) ||
            (p.CNP?.ToLower().Contains(search) ?? false) ||
       (p.Telefon?.ToLower().Contains(search) ?? false) ||
       (p.Email?.ToLower().Contains(search) ?? false) ||
     (p.Cod_Pacient?.ToLower().Contains(search) ?? false)
          );
        }

      // Activ filter
 if (!string.IsNullOrEmpty(FilterActiv))
        {
     var isActiv = bool.Parse(FilterActiv);
            filtered = filtered.Where(p => p.Activ == isActiv);
        }

      // Asigurat filter
 if (!string.IsNullOrEmpty(FilterAsigurat))
        {
          var isAsigurat = bool.Parse(FilterAsigurat);
            filtered = filtered.Where(p => p.Asigurat == isAsigurat);
        }

 // Judet filter
        if (!string.IsNullOrEmpty(FilterJudet))
        {
            filtered = filtered.Where(p => p.Judet == FilterJudet);
        }

   return filtered.ToList();
    }

    private Task LoadJudeteAsync()
    {
        if (_disposed) return Task.CompletedTask;
        
   try
        {
            JudeteList = new List<string>
            {
      "Bucuresti", "Alba", "Arad", "Arges", "Bacau", "Bihor", "Bistrita-Nasaud",
     "Botosani", "Brasov", "Braila", "Buzau", "Caras-Severin", "Calarasi",
       "Cluj", "Constanta", "Covasna", "Dambovita", "Dolj", "Galati", "Giurgiu",
       "Gorj", "Harghita", "Hunedoara", "Ialomita", "Iasi", "Ilfov", "Maramures",
       "Mehedinti", "Mures", "Neamt", "Olt", "Prahova", "Satu Mare", "Salaj",
       "Sibiu", "Suceava", "Teleorman", "Timis", "Tulcea", "Vaslui", "Valcea", "Vrancea"
    };
        }
        catch
        {
            JudeteList = new List<string>();
        }
        
 return Task.CompletedTask;
    }

    #region Filter & Search Methods

    private void HandleSearchKeyUp()
    {
        if (_disposed) return;
        
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Dispose();

        _searchDebounceTimer = new System.Timers.Timer(300);
      _searchDebounceTimer.Elapsed += async (sender, e) =>
 {
            _searchDebounceTimer?.Dispose();
 if (!_disposed)
 {
     await InvokeAsync(() =>
          {
             StateHasChanged();
     });
      }
 };
        _searchDebounceTimer.AutoReset = false;
   _searchDebounceTimer.Start();
    }

    private void ClearSearch()
    {
        if (_disposed) return;
        
 SearchText = string.Empty;
        StateHasChanged();
    }

    private void ApplyFilters()
    {
        if (_disposed) return;
        
        StateHasChanged();
    }

    private void ClearAllFilters()
    {
        if (_disposed) return;
        
        SearchText = string.Empty;
   FilterActiv = string.Empty;
  FilterAsigurat = string.Empty;
        FilterJudet = string.Empty;
        StateHasChanged();
    }

    #endregion

    #region Modal Methods

    private void OpenAddModal()
    {
        if (_disposed) return;
        
        SelectedPacientId = null;
        ShowAddEditModal = true;
    }

    private void OpenViewModal(Guid pacientId)
    {
    if (_disposed) return;
        
        SelectedPacientId = pacientId;
        ShowViewModal = true;
    }

    private void OpenEditModal(Guid pacientId)
  {
        if (_disposed) return;
        
      SelectedPacientId = pacientId;
        ShowAddEditModal = true;
    }

    private void OpenHistoryModal(Guid pacientId)
    {
        if (_disposed) return;
  
        SelectedPacientId = pacientId;
        ShowHistoryModal = true;
    }

    private void OpenDocumentsModal(Guid pacientId)
    {
        if (_disposed) return;
   
        SelectedPacientId = pacientId;
        ShowDocumentsModal = true;
    }

    private void ToggleStatusConfirm(PacientListDto pacient)
    {
        if (_disposed) return;
        
        SelectedPacientId = pacient.Id;
        var action = pacient.Activ ? "dezactivarea" : "activarea";
        DeleteConfirmMessage = $"Sunteți sigur că doriți {action} pacientului {pacient.NumeComplet}?";
        ShowDeleteModal = true;
    }

    private async Task HandleDeleteConfirmed()
    {
 if (_disposed || !SelectedPacientId.HasValue) return;

        try
        {
            var pacient = AllPacienti?.FirstOrDefault(p => p.Id == SelectedPacientId.Value);
            if (pacient == null)
      return;

            var command = new DeletePacientCommand(
         SelectedPacientId.Value,
"System",
         hardDelete: false
      );

 var result = await Mediator.Send(command);

    if (_disposed) return;

         if (result.IsSuccess)
        {
         await JSRuntime.InvokeVoidAsync("alert", result.SuccessMessage ?? "Operațiune efectuată cu succes!");
      await LoadDataAsync();
    }
            else
      {
 await JSRuntime.InvokeVoidAsync("alert", $"Eroare: {result.FirstError}");
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
                await JSRuntime.InvokeVoidAsync("alert", $"Eroare la modificarea statusului: {ex.Message}");
            }
        }
finally
        {
   if (!_disposed)
         {
             ShowDeleteModal = false;
          SelectedPacientId = null;
            }
        }
    }

    // ✅ CHANGED: Force COMPLETE re-initialization prin navigare (pattern din AdministrarePersonal)
    private async Task HandleModalSaved()
    {
 if (_disposed) return;
     
  Logger.LogInformation("🎉 Pacient saved - FORCING component re-initialization");

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
      NavigationManager.NavigateTo("/pacienti/administrare", forceLoad: true); // ✅ FIXED: calea corectă
        
            // Note: forceLoad: true forces a FULL page reload, not just component refresh
// This clears ALL Blazor state and starts fresh - exactly like F5!
        }
   catch (Exception ex)
        {
          Logger.LogError(ex, "Error during forced re-initialization");
 
       // Fallback: Reload data normally if navigation fails
        if (!_disposed)
  {
           await LoadDataAsync();
  await JSRuntime.InvokeVoidAsync("alert", "Pacient salvat cu succes!");
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

    #endregion

    // Dispose with proper cleanup
    public void Dispose()
    {
        if (_disposed) return;
   
        // Setează flag imediat pentru a bloca noi operații
        _disposed = true;
        
     // CRITICAL: Cleanup SINCRON pentru Syncfusion Grid
        try
  {
            Logger.LogDebug("AdministrarePacienti disposing - SYNCHRONOUS cleanup");
    
      // ❌ NU setăm GridRef = null - lăsăm Syncfusion să gestioneze propriul lifecycle
    // GridRef = null; // ELIMINAT - cauzează probleme cu referințele
    
   // Cancel orice operații în curs
    _searchDebounceTimer?.Stop();
    _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = null;
  
            // Clear data IMEDIAT
   AllPacienti?.Clear();
   AllPacienti = new();
            
       Logger.LogDebug("AdministrarePacienti disposed - Data cleared, GridRef preserved");
}
     catch (Exception ex)
      {
  Logger.LogError(ex, "Error in synchronous dispose");
        }
        
      // CRITICAL: ELIMINAT async cleanup forțat - lăsăm Blazor disposal natural
        Logger.LogDebug("AdministrarePacienti dispose COMPLETE - Natural Blazor disposal will handle GridRef");
    }
}
