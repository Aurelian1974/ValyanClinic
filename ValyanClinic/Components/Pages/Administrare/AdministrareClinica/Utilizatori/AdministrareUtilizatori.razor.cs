using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorList;
using ValyanClinic.Services.DataGrid;
using ValyanClinic.Components.Pages.Administrare.AdministrareClinica.Utilizatori.Modals;

namespace ValyanClinic.Components.Pages.Administrare.AdministrareClinica.Utilizatori;

public partial class AdministrareUtilizatori : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrareUtilizatori> Logger { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;

    // Grid reference
    private SfGrid<UtilizatorListDto>? GridRef { get; set; }

    // Toast reference
    private SfToast? ToastRef { get; set; }
    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    // Modal references
    private UtilizatorFormModal? UtilizatorFormModalRef { get; set; }
    private UtilizatorViewModal? UtilizatorViewModalRef { get; set; }

    // State
  private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;

    // Data
    private List<UtilizatorListDto> AllUtilizatoriList { get; set; } = new();
    private List<UtilizatorListDto> FilteredUtilizatoriList { get; set; } = new();
    private List<UtilizatorListDto> CurrentPageData { get; set; } = new();

    // Selection
    private UtilizatorListDto? SelectedUtilizator { get; set; }

    // Search & Filters
    private string GlobalSearchText { get; set; } = string.Empty;
private bool IsAdvancedFilterExpanded { get; set; }
    private string? FilterEsteActiv { get; set; }
    private string? FilterRol { get; set; }

    // Filter Options
    private List<FilterOption> StatusOptions { get; set; } = new();
    private List<FilterOption> RolOptions { get; set; } = new();

    // Pagination
    private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    private int[] PageSizeArray { get; set; } = { 10, 20, 50, 100 };

    // Computed Properties
    private int TotalRecords => FilteredUtilizatoriList.Count;
    private int TotalPages => TotalRecords == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / CurrentPageSize);
    private bool HasPreviousPage => CurrentPage > 1;
    private bool HasNextPage => CurrentPage < TotalPages;
    private int DisplayedRecordsStart => TotalRecords == 0 ? 0 : (CurrentPage - 1) * CurrentPageSize + 1;
    private int DisplayedRecordsEnd => Math.Min(CurrentPage * CurrentPageSize, TotalRecords);
    
    private int ActiveFiltersCount
    {
        get
        {
   int count = 0;
         if (!string.IsNullOrEmpty(GlobalSearchText)) count++;
   if (!string.IsNullOrEmpty(FilterEsteActiv)) count++;
            if (!string.IsNullOrEmpty(FilterRol)) count++;
         return count;
        }
    }

    protected override async Task OnInitializedAsync()
    {
   Logger.LogInformation("Initializing AdministrareUtilizatori page");
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
   IsLoading = true;
            HasError = false;
            await InvokeAsync(StateHasChanged);

         Logger.LogInformation("Loading utilizatori data");

      var query = new GetUtilizatorListQuery
         {
          PageNumber = 1,
       PageSize = 10000, // Load all for client-side filtering
          SortColumn = "Username",
           SortDirection = "ASC"
     };

            var result = await Mediator.Send(query);

  if (result.IsSuccess && result.Value != null)
  {
       AllUtilizatoriList = result.Value.ToList();
FilteredUtilizatoriList = AllUtilizatoriList;
       
     InitializeFilterOptions();
 UpdatePagedData();

         Logger.LogInformation("Loaded {Count} utilizatori", AllUtilizatoriList.Count);
     }
            else
   {
        HasError = true;
 ErrorMessage = result.Errors?.FirstOrDefault() ?? "Eroare la incarcarea datelor";
    Logger.LogWarning("Failed to load utilizatori: {Message}", ErrorMessage);
    }
        }
     catch (Exception ex)
        {
      HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
      Logger.LogError(ex, "Error loading utilizatori data");
    }
      finally
        {
    IsLoading = false;
    await InvokeAsync(StateHasChanged);
        }
    }

    private void InitializeFilterOptions()
    {
        // Status options
StatusOptions = new List<FilterOption>
        {
         new() { Text = "Toate", Value = "" },
       new() { Text = "Activ", Value = "true" },
            new() { Text = "Inactiv", Value = "false" }
        };

        // Rol options from data
     var roluri = AllUtilizatoriList
            .Where(u => !string.IsNullOrEmpty(u.Rol))
    .Select(u => u.Rol)
            .Distinct()
     .OrderBy(r => r)
            .ToList();

        RolOptions = new List<FilterOption> { new() { Text = "Toate", Value = "" } };
   RolOptions.AddRange(roluri.Select(r => new FilterOption { Text = r, Value = r }));
    }

    private void ApplyFilters()
    {
 Logger.LogInformation("Applying filters: Search={Search}, Status={Status}, Rol={Rol}",
      GlobalSearchText, FilterEsteActiv, FilterRol);

        var filtered = AllUtilizatoriList.AsEnumerable();

 // Global search
  if (!string.IsNullOrWhiteSpace(GlobalSearchText))
        {
var searchLower = GlobalSearchText.ToLower();
        filtered = filtered.Where(u =>
   (u.Username?.ToLower().Contains(searchLower) ?? false) ||
         (u.Email?.ToLower().Contains(searchLower) ?? false) ||
     (u.NumeCompletPersonalMedical?.ToLower().Contains(searchLower) ?? false) ||
 (u.Rol?.ToLower().Contains(searchLower) ?? false)
 );
  }

        // Status filter
    if (!string.IsNullOrEmpty(FilterEsteActiv))
     {
  if (bool.TryParse(FilterEsteActiv, out bool esteActiv))
    {
     filtered = filtered.Where(u => u.EsteActiv == esteActiv);
         }
  }

        // Rol filter
        if (!string.IsNullOrEmpty(FilterRol))
 {
      filtered = filtered.Where(u => u.Rol == FilterRol);
        }

FilteredUtilizatoriList = filtered.ToList();
     CurrentPage = 1;
        UpdatePagedData();

  Logger.LogInformation("Filters applied: {Count} results", FilteredUtilizatoriList.Count);
    }

  private void ClearAllFilters()
    {
        GlobalSearchText = string.Empty;
   FilterEsteActiv = null;
        FilterRol = null;
        ApplyFilters();
    }

    private void ClearFilter(string filterName)
    {
        switch (filterName)
   {
            case nameof(GlobalSearchText):
      GlobalSearchText = string.Empty;
        break;
            case nameof(FilterEsteActiv):
 FilterEsteActiv = null;
                break;
      case nameof(FilterRol):
       FilterRol = null;
     break;
        }
        ApplyFilters();
    }

    private void ToggleAdvancedFilter()
    {
 IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        GlobalSearchText = e.Value?.ToString() ?? string.Empty;
    }

    private void OnSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
     ApplyFilters();
        }
    }

    private void ClearSearch()
    {
        GlobalSearchText = string.Empty;
    ApplyFilters();
    }

    private void UpdatePagedData()
    {
        var skip = (CurrentPage - 1) * CurrentPageSize;
   CurrentPageData = FilteredUtilizatoriList.Skip(skip).Take(CurrentPageSize).ToList();
    }

    private void OnPageSizeChangedNative(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int newSize))
        {
   CurrentPageSize = newSize;
            CurrentPage = 1;
            UpdatePagedData();
    }
    }

    private void GoToPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages)
 {
        CurrentPage = pageNumber;
            UpdatePagedData();
   }
    }

    private void GoToFirstPage() => GoToPage(1);
    private void GoToLastPage() => GoToPage(TotalPages);
    private void GoToPreviousPage() => GoToPage(CurrentPage - 1);
    private void GoToNextPage() => GoToPage(CurrentPage + 1);

    private (int start, int end) GetPagerRange(int visiblePages)
    {
        var half = visiblePages / 2;
        var start = Math.Max(1, CurrentPage - half);
        var end = Math.Min(TotalPages, start + visiblePages - 1);

        if (end - start + 1 < visiblePages)
        {
            start = Math.Max(1, end - visiblePages + 1);
        }

        return (start, end);
    }

    private void OnRowSelected(RowSelectEventArgs<UtilizatorListDto> args)
    {
        SelectedUtilizator = args.Data;
    Logger.LogInformation("Utilizator selected: {Username}", SelectedUtilizator?.Username);
    }

    private void OnRowDeselected(RowDeselectEventArgs<UtilizatorListDto> args)
    {
        SelectedUtilizator = null;
     Logger.LogInformation("Utilizator deselected");
    }

    private async Task HandleAddNew()
  {
    Logger.LogInformation("Add new utilizator clicked");
  
        if (UtilizatorFormModalRef != null)
        {
   await UtilizatorFormModalRef.OpenForAdd();
        }
        else
        {
     Logger.LogWarning("UtilizatorFormModalRef is null");
       await ShowToast("Eroare", "Modal-ul nu este disponibil", "e-toast-danger");
    }
    }

    private async Task HandleRefresh()
    {
Logger.LogInformation("Refresh clicked");
        SelectedUtilizator = null;
        await LoadData();
  await ShowToast("Succes", "Datele au fost reincarcare cu succes", "e-toast-success");
    }

    private async Task HandleViewSelected()
    {
        if (SelectedUtilizator == null) return;
        
        Logger.LogInformation("View utilizator: {UtilizatorID}", SelectedUtilizator.UtilizatorID);
      
    if (UtilizatorViewModalRef != null)
        {
            await UtilizatorViewModalRef.Open(SelectedUtilizator.UtilizatorID);
        }
 else
        {
         Logger.LogWarning("UtilizatorViewModalRef is null");
    await ShowToast("Eroare", "Modal-ul nu este disponibil", "e-toast-danger");
     }
    }

    private async Task HandleEditSelected()
    {
 if (SelectedUtilizator == null) return;

        Logger.LogInformation("Edit utilizator: {UtilizatorID}", SelectedUtilizator.UtilizatorID);
  
        if (UtilizatorFormModalRef != null)
  {
      await UtilizatorFormModalRef.OpenForEdit(SelectedUtilizator.UtilizatorID);
        }
  else
   {
  Logger.LogWarning("UtilizatorFormModalRef is null");
  await ShowToast("Eroare", "Modal-ul nu este disponibil", "e-toast-danger");
        }
    }

  private async Task HandleDeleteSelected()
 {
        if (SelectedUtilizator == null) return;
        
        // Protect Admin user
if (SelectedUtilizator.Username == "Admin")
        {
      await ShowToast("Atentie", "Nu puteti sterge utilizatorul Admin!", "e-toast-warning");
   return;
        }
        
   Logger.LogInformation("Delete utilizator: {UtilizatorID}", SelectedUtilizator.UtilizatorID);
        await ShowToast("Informare", "Functionalitate in dezvoltare", "e-toast-info");
        // TODO: Open ConfirmDeleteModal
    }

    private async Task ShowToast(string title, string content, string cssClass)
    {
  ToastTitle = title;
 ToastContent = content;
 ToastCssClass = cssClass;
        
        if (ToastRef != null)
        {
 await ToastRef.ShowAsync();
        }
    }

    // ========================================
    // MODAL EVENT HANDLERS
    // ========================================

    private async Task HandleUtilizatorSaved()
    {
        Logger.LogInformation("Utilizator saved - reloading data");
   SelectedUtilizator = null;
        await LoadData();
   await ShowToast("Succes", "Utilizatorul a fost salvat cu succes!", "e-toast-success");
    }

    private async Task HandleModalClosed()
    {
      Logger.LogInformation("Modal closed");
  // Refresh data if needed
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleEditFromModal(Guid utilizatorId)
    {
      Logger.LogInformation("Edit from modal: {UtilizatorID}", utilizatorId);
        
        if (UtilizatorFormModalRef != null)
        {
      await UtilizatorFormModalRef.OpenForEdit(utilizatorId);
  }
    }

    private async Task HandleDeleteFromModal(Guid utilizatorId)
    {
     Logger.LogInformation("Delete from modal: {UtilizatorID}", utilizatorId);
   
   // TODO: Implement ConfirmDeleteModal
        await ShowToast("Informare", "Functionalitate delete in dezvoltare", "e-toast-info");
  }
}

public class FilterOption
{
    public string Text { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
