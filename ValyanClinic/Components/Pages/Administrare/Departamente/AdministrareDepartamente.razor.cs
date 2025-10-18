using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentList;
using ValyanClinic.Application.Features.DepartamentManagement.Commands.DeleteDepartament;
using ValyanClinic.Services.DataGrid;
using Microsoft.Extensions.Logging;
using ValyanClinic.Components.Shared.Modals;

namespace ValyanClinic.Components.Pages.Administrare.Departamente;

public partial class AdministrareDepartamente : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrareDepartamente> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IFilterOptionsService FilterOptionsService { get; set; } = default!;

    private SfGrid<DepartamentListDto>? GridRef;
    private SfToast? ToastRef;
    private ConfirmDeleteModal? confirmDeleteModal;

    private List<DepartamentListDto> CurrentPageData { get; set; } = new();
    
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
    
    private string CurrentSortColumn { get; set; } = "DenumireDepartament";
    private string CurrentSortDirection { get; set; } = "ASC";
    
    private DepartamentListDto? SelectedDepartament { get; set; }

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

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Initializare pagina Administrare Departamente");
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
            
            Logger.LogDebug("AdministrareDepartamente disposed");
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
                "SERVER-SIDE Load: Page={Page}, Size={Size}, Search='{Search}', Sort={Sort} {Dir}",
                CurrentPage, CurrentPageSize, GlobalSearchText, CurrentSortColumn, CurrentSortDirection);

            var query = new GetDepartamentListQuery
            {
                PageNumber = CurrentPage,
                PageSize = CurrentPageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(GlobalSearchText) ? null : GlobalSearchText,
                SortColumn = CurrentSortColumn,
                SortDirection = CurrentSortDirection
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                CurrentPageData = result.Value?.ToList() ?? new List<DepartamentListDto>();
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
                CurrentPageData = new List<DepartamentListDto>();
                TotalRecords = 0;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare neasteptata: {ex.Message}";
            Logger.LogError(ex, "Eroare la incarcarea datelor");
            CurrentPageData = new List<DepartamentListDto>();
            TotalRecords = 0;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
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

    private async Task HandleRefresh()
    {
        Logger.LogInformation("Refresh departamente");
        
        await LoadPagedData();
        await ShowToast("Succes", "Datele au fost reincarcate", "e-toast-success");
    }

    private async Task HandleAddNew()
    {
        Logger.LogInformation("TODO: Opening modal for ADD Departament");
        await ShowToast("Info", "Functionalitate in dezvoltare", "e-toast-info");
    }

    private async Task HandleViewSelected()
    {
        if (SelectedDepartament == null)
        {
            await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
            return;
        }
        
        Logger.LogInformation("TODO: Opening View modal for: {DeptID}", SelectedDepartament.IdDepartament);
        await ShowToast("Info", "Functionalitate in dezvoltare", "e-toast-info");
    }

    private async Task HandleEditSelected()
    {
        if (SelectedDepartament == null)
        {
            await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
            return;
        }
        
        Logger.LogInformation("TODO: Opening Edit modal for: {DeptID}", SelectedDepartament.IdDepartament);
        await ShowToast("Info", "Functionalitate in dezvoltare", "e-toast-info");
    }

    private async Task HandleDeleteSelected()
    {
        if (SelectedDepartament == null)
        {
            await ShowToast("Atentie", "Selecteaza un rand din tabel", "e-toast-warning");
            return;
        }
        
        Logger.LogInformation("Opening Delete modal for: {DeptID} - {Denumire}", 
            SelectedDepartament.IdDepartament, SelectedDepartament.DenumireDepartament);
        
        if (confirmDeleteModal != null)
        {
            await confirmDeleteModal.Open(SelectedDepartament.IdDepartament, SelectedDepartament.DenumireDepartament);
        }
    }

    private async Task HandleDeleteConfirmed(Guid id)
    {
        Logger.LogInformation("Delete confirmed for: {DeptID}", id);
        
        try
        {
            var command = new DeleteDepartamentCommand(id);
            var result = await Mediator.Send(command);
            
            if (result.IsSuccess)
            {
                Logger.LogInformation("Departament deleted successfully: {DeptID}", id);
                await LoadPagedData();
                await ShowToast("Succes", "Departament sters cu succes", "e-toast-success");
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
            Logger.LogError(ex, "Exception during delete: {DeptID}", id);
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

    private void OnRowSelected(RowSelectEventArgs<DepartamentListDto> args)
    {
        SelectedDepartament = args.Data;
        Logger.LogInformation("Departament selectat: {DeptID} - {Denumire}", 
            SelectedDepartament?.IdDepartament, SelectedDepartament?.DenumireDepartament);
        StateHasChanged();
    }

    private void OnRowDeselected(RowDeselectEventArgs<DepartamentListDto> args)
    {
        SelectedDepartament = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    private async Task OnGridActionBegin(ActionEventArgs<DepartamentListDto> args)
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
                    CurrentSortColumn = sortCol.Name?.ToString() ?? "DenumireDepartament";
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
}
