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

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical;

public partial class AdministrarePersonalMedical : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrarePersonalMedical> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;

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
        try
        {
            Logger.LogInformation("Initializare pagina Administrare Personal Medical");
            await LoadPagedData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la initializarea componentei");
            HasError = true;
            ErrorMessage = $"Eroare la initializare: {ex.Message}";
            IsLoading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        try
        {
            _searchDebounceTokenSource?.Cancel();
            _searchDebounceTokenSource?.Dispose();
            _searchDebounceTokenSource = null;
            
            Logger.LogDebug("AdministrarePersonalMedical disposed");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la dispose");
        }
        finally
        {
            _disposed = true;
        }
    }

    private async Task LoadPagedData()
    {
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
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare neasteptata: {ex.Message}";
            Logger.LogError(ex, "Eroare la incarcarea datelor");
            CurrentPageData = new List<PersonalMedicalListDto>();
            TotalRecords = 0;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task ClearAllFilters()
    {
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
        IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded;
        Logger.LogInformation("Advanced filter toggled: {State}", 
            IsAdvancedFilterExpanded ? "Expanded" : "Collapsed");
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
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
                
                if (!localToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Executing search for: '{SearchText}'", GlobalSearchText);
                    
                    await InvokeAsync(async () =>
                    {
                        CurrentPage = 1;
                        await LoadPagedData();
                    });
                }
            }
            catch (TaskCanceledException)
            {
                Logger.LogDebug("Search cancelled");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Eroare la executia search-ului");
            }
        }, localToken);
    }

    private async Task OnSearchKeyDown(KeyboardEventArgs e)
    {
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
        Logger.LogInformation("Clearing search");
        
        _searchDebounceTokenSource?.Cancel();
        
        GlobalSearchText = string.Empty;
        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task ApplyFilters()
    {
        Logger.LogInformation(
            "Applying filters: Search={Search}, Dept={Dept}, Poz={Poz}, Activ={Activ}",
            GlobalSearchText, FilterDepartament, FilterPozitie, FilterEsteActiv);

        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task HandleRefresh()
    {
        Logger.LogInformation("Refresh personal medical");
        
        await LoadPagedData();
        await ShowToast("Succes", "Datele au fost reincarcate", "e-toast-success");
    }

    private async Task HandleAddNew()
    {
        Logger.LogInformation("Opening modal for ADD PersonalMedical");
        
        if (personalMedicalFormModal != null)
        {
            await personalMedicalFormModal.OpenForAdd();
        }
    }

    private async Task HandleViewSelected()
    {
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
        Logger.LogInformation("Edit requested from modal for: {PersonalID}", personalID);
        
        if (personalMedicalFormModal != null)
        {
            await personalMedicalFormModal.OpenForEdit(personalID);
        }
    }

    private async Task HandleDeleteFromModal(Guid personalID)
    {
        var personalMedical = CurrentPageData.FirstOrDefault(p => p.PersonalID == personalID);
        if (personalMedical != null && confirmDeleteModal != null)
        {
            await confirmDeleteModal.Open(personalID, personalMedical.NumeComplet);
        }
    }

    private async Task HandlePersonalMedicalSaved()
    {
        Logger.LogInformation("PersonalMedical saved - reloading data");
        await LoadPagedData();
        await ShowToast("Succes", "Personal medical salvat cu succes", "e-toast-success");
    }

    private async Task HandleDeleteConfirmed(Guid personalID)
    {
        Logger.LogInformation("Delete confirmed for: {PersonalID}", personalID);
        
        try
        {
            var command = new DeletePersonalMedicalCommand(personalID, "CurrentUser");
            var result = await Mediator.Send(command);
            
            if (result.IsSuccess)
            {
                Logger.LogInformation("PersonalMedical deleted successfully: {PersonalID}", personalID);
                await LoadPagedData();
                await ShowToast("Succes", "Personal medical sters cu succes", "e-toast-success");
            }
            else
            {
                var errorMsg = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscuta" });
                Logger.LogWarning("Delete failed: {Error}", errorMsg);
                await ShowToast("Eroare", errorMsg, "e-toast-danger");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception during delete: {PersonalID}", personalID);
            await ShowToast("Eroare", $"Eroare la stergere: {ex.Message}", "e-toast-danger");
        }
    }

    private async Task GoToPage(int pageNumber)
    {
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

    private void OnRowSelected(RowSelectEventArgs<PersonalMedicalListDto> args)
    {
        SelectedPersonal = args.Data;
        Logger.LogInformation("Personal medical selectat: {PersonalID} - {NumeComplet}", 
            SelectedPersonal?.PersonalID, SelectedPersonal?.NumeComplet);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<PersonalMedicalListDto> args)
    {
        SelectedPersonal = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    private async Task OnGridActionBegin(ActionEventArgs<PersonalMedicalListDto> args)
    {
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
        ToastTitle = title;
        ToastContent = content;
        ToastCssClass = cssClass;

        if (ToastRef != null)
        {
            await ToastRef.ShowAsync();
        }
    }

    private class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
