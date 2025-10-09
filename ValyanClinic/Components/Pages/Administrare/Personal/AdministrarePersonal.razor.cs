using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using MediatR;
using ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;
using ValyanClinic.Services.DataGrid;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

public partial class AdministrarePersonal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonal> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;

    // Grid reference
    private SfGrid<PersonalListDto>? GridRef;

    // Toast reference
    private SfToast? ToastRef;

    // Server-side paging state
    private List<PersonalListDto> CurrentPageData { get; set; } = new();
    private int TotalRecords { get; set; } = 0;
    private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    private string CurrentSortColumn { get; set; } = "Nume";
    private string CurrentSortDirection { get; set; } = "ASC";
    
    private PersonalListDto? SelectedPersonal { get; set; }

    // State
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }
    
    private int jumpToPageNumber { get; set; } = 1;
    
    // Track previous page size to detect changes
    private int _previousPageSize = 20;

    // Grouping initial columns (empty by default - user can group manually)
    private string[] InitialGroupColumns { get; set; } = Array.Empty<string>();

    // Page size options
    private List<PageSizeOption> PageSizeOptions = new()
    {
        new() { Text = "10", Value = 10 },
        new() { Text = "20", Value = 20 },
        new() { Text = "50", Value = 50 },
        new() { Text = "100", Value = 100 }
    };

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

    // Filter Options - loaded from server
    private List<FilterOption> StatusOptions { get; set; } = new();
    private List<FilterOption> DepartamentOptions { get; set; } = new();
    private List<FilterOption> FunctieOptions { get; set; } = new();
    private List<FilterOption> JudetOptions { get; set; } = new();

    // Toast properties
    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    public class PageSizeOption
    {
        public string Text { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private int ActiveFiltersCount => 
        (string.IsNullOrEmpty(FilterStatus) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterDepartament) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterFunctie) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterJudet) ? 0 : 1) +
        (string.IsNullOrEmpty(GlobalSearchText) ? 0 : 1);

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Initializare pagina Administrare Personal cu server-side operations");
        await LoadData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        // Verifică dacă page size s-a schimbat
        if (!firstRender && CurrentPageSize != _previousPageSize && !IsLoading)
        {
            Logger.LogInformation("Page size changed detected: {OldSize} -> {NewSize}", 
                _previousPageSize, CurrentPageSize);
            
            _previousPageSize = CurrentPageSize;
            CurrentPage = 1;
            jumpToPageNumber = 1;
            
            await InvokeAsync(async () =>
            {
                await LoadData();
            });
        }
    }

    public void Dispose()
    {
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource?.Dispose();
    }

    private async Task LoadData()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            StateHasChanged();

            Logger.LogInformation("Incarcare date personal de la server: Page={Page}, Size={Size}, Search='{Search}'", 
                CurrentPage, CurrentPageSize, GlobalSearchText);

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

            if (result.IsSuccess)
            {
                CurrentPageData = result.Value?.ToList() ?? new List<PersonalListDto>();
                TotalRecords = result.TotalCount;
                
                Logger.LogInformation("Date incarcate cu succes: {Count} din {Total} angajati pentru search='{Search}'",
                    CurrentPageData.Count, TotalRecords, GlobalSearchText);
                
                // Load filter options only on first load (when we have data)
                if (!StatusOptions.Any() && CurrentPageData.Any())
                {
                    await LoadFilterOptions();
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Eroare la incarcarea datelor: {Message}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare neasteptata: {ex.Message}";
            Logger.LogError(ex, "Eroare la incarcarea datelor personal");
        }
        finally
        {
            IsLoading = false;
            jumpToPageNumber = CurrentPage;
            StateHasChanged();
        }
    }

    private async Task LoadFilterOptions()
    {
        try
        {
            Logger.LogInformation("Incarcare optiuni de filtrare...");
            
            // Load all data for filter options (without pagination)
            var allDataQuery = new GetPersonalListQuery
            {
                PageNumber = 1,
                PageSize = int.MaxValue // Get all for filters
            };
            
            var result = await Mediator.Send(allDataQuery);
            
            if (result.IsSuccess && result.Value != null)
            {
                var allData = result.Value.ToList();
                
                // Ensure we have data before generating options
                if (allData.Any())
                {
                    StatusOptions = FilterOptionsService.GenerateOptions(allData, p => p.Status_Angajat);
                    DepartamentOptions = FilterOptionsService.GenerateOptions(allData, p => p.Departament);
                    FunctieOptions = FilterOptionsService.GenerateOptions(allData, p => p.Functia);
                    JudetOptions = FilterOptionsService.GenerateOptions(allData, p => p.Judet_Domiciliu);
                    
                    Logger.LogInformation("Optiuni filtrare incarcate: Status={StatusCount}, Dept={DeptCount}, Func={FuncCount}, Judet={JudetCount}",
                        StatusOptions.Count, DepartamentOptions.Count, FunctieOptions.Count, JudetOptions.Count);
                    
                    StateHasChanged();
                }
                else
                {
                    Logger.LogWarning("No data available for filter options");
                }
            }
            else
            {
                Logger.LogWarning("Failed to load data for filter options: {Errors}", 
                    string.Join(", ", result.Errors ?? new List<string>()));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la incarcarea optiunilor de filtrare");
        }
    }

    private void ToggleAdvancedFilter()
    {
        IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
        Logger.LogInformation("Advanced filter panel toggled: {State}", 
            IsAdvancedFilterExpanded ? "Expanded" : "Collapsed");
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? string.Empty;
        
        // Previne bucla infinita - nu face nimic daca valoarea nu s-a schimbat
        if (newValue == GlobalSearchText)
            return;
        
        GlobalSearchText = newValue;
        
        Logger.LogInformation("Search input changed: '{SearchText}'", GlobalSearchText);
        
        // Anulează task-ul anterior de debounce
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource?.Dispose();
        _searchDebounceTokenSource = new CancellationTokenSource();

        var localToken = _searchDebounceTokenSource.Token;

        // Executa search dupa debounce
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(SearchDebounceMs, localToken);
                
                if (!localToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Executing search for: '{SearchText}'", GlobalSearchText);
                    
                    await InvokeAsync(async () =>
                    {
                        CurrentPage = 1;
                        jumpToPageNumber = 1;
                        await LoadData();
                    });
                }
            }
            catch (TaskCanceledException)
            {
                Logger.LogDebug("Search cancelled - user still typing");
            }
        }, localToken);
    }

    private async Task OnSearchKeyDown(KeyboardEventArgs e)
    {
        // Daca apasa Enter, executa imediat cautarea fara debounce
        if (e.Key == "Enter")
        {
            _searchDebounceTokenSource?.Cancel();
            
            Logger.LogInformation("Enter pressed - executing immediate search: '{SearchText}'", GlobalSearchText);
            
            CurrentPage = 1;
            jumpToPageNumber = 1;
            await LoadData();
        }
    }

    private async Task ClearSearch()
    {
        Logger.LogInformation("Clearing search text");
        
        // Anulează orice search in curs
        _searchDebounceTokenSource?.Cancel();
        
        GlobalSearchText = string.Empty;
        CurrentPage = 1;
        jumpToPageNumber = 1;
        
        await LoadData();
    }

    private async Task ApplyFilters()
    {
        Logger.LogInformation("Applying server-side filters: GlobalSearch={Search}, Status={Status}, Dept={Dept}",
            GlobalSearchText, FilterStatus, FilterDepartament);

        // Reset to first page when filters change
        CurrentPage = 1;
        
        // Reload data with new filters
        await LoadData();
    }

    private async Task ClearAllFilters()
    {
        Logger.LogInformation("Clearing all filters");
        
        GlobalSearchText = string.Empty;
        FilterStatus = null;
        FilterDepartament = null;
        FilterFunctie = null;
        FilterJudet = null;

        CurrentPage = 1;
        await LoadData();
    }

    private async Task ClearFilter(string filterName)
    {
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

        await ApplyFilters();
    }

    private async Task HandleRefresh()
    {
        Logger.LogInformation("Refresh date personal");
        CurrentPage = 1;
        await LoadData();
        await ShowToast("Succes", "Datele au fost reincarcate cu succes", "e-toast-success");
    }

    private void HandleAddNew()
    {
        Logger.LogInformation("Navigare catre adaugare personal");
        NavigationManager.NavigateTo("/administrare/personal/adauga");
    }

    #region Server-Side Paging Methods

    private async Task OnPageSizeChanged(int newPageSize)
    {
        Logger.LogInformation("Schimbare dimensiune pagina: {OldSize} -> {NewSize}", 
            CurrentPageSize, newPageSize);
        
        CurrentPageSize = newPageSize;
        CurrentPage = 1;
        jumpToPageNumber = 1;
        
        await LoadData();
    }

    private int GetCurrentPage() => CurrentPage;

    private int GetTotalPages()
    {
        if (TotalRecords == 0 || CurrentPageSize == 0) return 1;
        return (int)Math.Ceiling((double)TotalRecords / CurrentPageSize);
    }

    private int GetDisplayedRecordsStart()
    {
        if (TotalRecords == 0) return 0;
        return (CurrentPage - 1) * CurrentPageSize + 1;
    }

    private int GetDisplayedRecordsEnd()
    {
        var end = CurrentPage * CurrentPageSize;
        return Math.Min(end, TotalRecords);
    }

    private int GetPagerStart()
    {
        var totalPages = GetTotalPages();
        var start = Math.Max(1, CurrentPage - 2);
        if (totalPages <= 5) return 1;
        if (CurrentPage >= totalPages - 2) return Math.Max(1, totalPages - 4);
        return start;
    }

    private int GetPagerEnd()
    {
        var totalPages = GetTotalPages();
        if (totalPages <= 5) return totalPages;
        if (CurrentPage <= 3) return Math.Min(5, totalPages);
        return Math.Min(totalPages, CurrentPage + 2);
    }

    private async Task GoToPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > GetTotalPages()) return;
        
        if (CurrentPage != pageNumber)
        {
            Logger.LogInformation("Navigare la pagina {Page}", pageNumber);
            CurrentPage = pageNumber;
            jumpToPageNumber = pageNumber;
            await LoadData();
        }
    }

    private async Task JumpToPage()
    {
        if (jumpToPageNumber >= 1 && jumpToPageNumber <= GetTotalPages())
        {
            await GoToPage(jumpToPageNumber);
        }
    }

    private async Task OnJumpToPageKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await JumpToPage();
        }
    }

    #endregion

    #region Grid Selection Events

    private void OnRowSelected(RowSelectEventArgs<PersonalListDto> args)
    {
        SelectedPersonal = args.Data;
        Logger.LogInformation("Personal selectat: {PersonalId} - {NumeComplet}", 
            SelectedPersonal?.Id_Personal, SelectedPersonal?.NumeComplet);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PersonalListDto> args)
    {
        SelectedPersonal = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    #endregion

    #region Toolbar Action Methods

    private async Task HandleViewSelected()
    {
        if (SelectedPersonal == null) return;
        
        Logger.LogInformation("Vizualizare personal: {PersonalId}", SelectedPersonal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/vizualizeaza/{SelectedPersonal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleEditSelected()
    {
        if (SelectedPersonal == null) return;
        
        Logger.LogInformation("Editare personal: {PersonalId}", SelectedPersonal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/editeaza/{SelectedPersonal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleDeleteSelected()
    {
        if (SelectedPersonal == null) return;
        
        Logger.LogInformation("Stergere personal: {PersonalId}", SelectedPersonal.Id_Personal);
        
        await ShowToast("Atentie", 
            $"Functionalitatea de stergere pentru {SelectedPersonal.NumeComplet} va fi implementata", 
            "e-toast-warning");
    }

    #endregion

    private async Task OnCommandClicked(CommandClickEventArgs<PersonalListDto> args)
    {
        var personal = args.RowData;
        var commandName = args.CommandColumn?.ButtonOption?.Content ?? "";

        Logger.LogInformation("Comanda executata: {Command} pentru {PersonalId}", 
            commandName, personal.Id_Personal);

        switch (commandName)
        {
            case "Vizualizeaza":
                await HandleView(personal);
                break;
            case "Editeaza":
                await HandleEdit(personal);
                break;
            case "Sterge":
                await HandleDelete(personal);
                break;
        }
    }

    private async Task HandleView(PersonalListDto personal)
    {
        Logger.LogInformation("Vizualizare personal: {PersonalId}", personal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/vizualizeaza/{personal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleEdit(PersonalListDto personal)
    {
        Logger.LogInformation("Editare personal: {PersonalId}", personal.Id_Personal);
        NavigationManager.NavigateTo($"/administrare/personal/editeaza/{personal.Id_Personal}");
        await Task.CompletedTask;
    }

    private async Task HandleDelete(PersonalListDto personal)
    {
        Logger.LogInformation("Stergere personal: {PersonalId}", personal.Id_Personal);
        
        await ShowToast("Atentie", 
            $"Functionalitatea de stergere pentru {personal.NumeComplet} va fi implementata", 
            "e-toast-warning");
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
}
