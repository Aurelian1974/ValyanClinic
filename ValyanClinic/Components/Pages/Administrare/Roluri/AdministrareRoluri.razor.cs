using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Routing;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using MediatR;
using ValyanClinic.Application.Features.RolManagement.Queries.GetRolList;
using ValyanClinic.Application.Features.RolManagement.Commands.DeleteRol;
using Microsoft.Extensions.Logging;
using ValyanClinic.Components.Shared.Modals;
using ValyanClinic.Components.Pages.Administrare.Roluri.Modals;
using Microsoft.JSInterop;

namespace ValyanClinic.Components.Pages.Administrare.Roluri;

public partial class AdministrareRoluri : ComponentBase, IDisposable
{
    // CRITICAL: Static lock pentru ABSOLUTE protection
    private static readonly object _initLock = new object();
    private static bool _anyInstanceInitializing = false;

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrareRoluri> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private SfGrid<RolListDto>? GridRef;
    private SfToast? ToastRef;
    private ConfirmDeleteModal? confirmDeleteModal;
    private RolFormModal? rolFormModal;
    private RolViewModal? rolViewModal;
    private RolPermisiuniModal? rolPermisiuniModal;
    private IDisposable? _locationChangingRegistration;

    private List<RolListDto> CurrentPageData { get; set; } = new();

    private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    private int TotalRecords { get; set; } = 0;
    private int TotalPages => TotalRecords > 0 && CurrentPageSize > 0
        ? (int)Math.Ceiling((double)TotalRecords / CurrentPageSize)
        : 1;

    private const int MinPageSize = 10;
    private const int MaxPageSize = 100;
    private int[] PageSizeArray = { 10, 20, 50, 100 };

    private string CurrentSortColumn { get; set; } = "OrdineAfisare";
    private string CurrentSortDirection { get; set; } = "ASC";

    private RolListDto? SelectedRol { get; set; }

    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; } = false;
    private string? ErrorMessage { get; set; }

    private string GlobalSearchText { get; set; } = string.Empty;

    private CancellationTokenSource? _searchDebounceTokenSource;
    private const int SearchDebounceMs = 500;
    private bool _disposed = false;
    private bool _initialized = false;
    private bool _isInitializing = false;

    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    private bool HasPreviousPage => CurrentPage > 1;
    private bool HasNextPage => CurrentPage < TotalPages;
    private int DisplayedRecordsStart => TotalRecords > 0 ? ((CurrentPage - 1) * CurrentPageSize) + 1 : 0;
    private int DisplayedRecordsEnd => Math.Min(CurrentPage * CurrentPageSize, TotalRecords);

    protected override async Task OnInitializedAsync()
    {
        // Register navigation handler to cleanup before navigation
        _locationChangingRegistration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);

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
            Logger.LogInformation("Inițializare pagină Administrare Roluri");
            await LoadPagedData();

            _initialized = true;
        }
        catch (ObjectDisposedException ex)
        {
            Logger.LogWarning(ex, "Component disposed during initialization (navigation away)");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la inițializarea componentei");
            HasError = true;
            ErrorMessage = $"Eroare la inițializare: {ex.Message}";
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

    private ValueTask OnLocationChanging(LocationChangingContext context)
    {
        Logger.LogInformation("Navigation detected in AdministrareRoluri - marking as disposed");
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    protected override bool ShouldRender() => !_disposed;

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            Logger.LogDebug("AdministrareRoluri disposing - SYNCHRONOUS cleanup");

            // Dispose navigation registration first
            _locationChangingRegistration?.Dispose();
            _locationChangingRegistration = null;

            _searchDebounceTokenSource?.Cancel();
            _searchDebounceTokenSource?.Dispose();
            _searchDebounceTokenSource = null;

            CurrentPageData?.Clear();
            CurrentPageData = new();

            GridRef = null;
            ToastRef = null;
            confirmDeleteModal = null;
            rolFormModal = null;
            rolViewModal = null;
            rolPermisiuniModal = null;
            SelectedRol = null;

            Logger.LogDebug("AdministrareRoluri disposed - Data cleared");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in synchronous dispose");
        }
    }

    private async Task LoadPagedData()
    {
        if (_disposed) return;

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            Logger.LogInformation(
                "SERVER-SIDE Load: Page={Page}, Size={Size}, Search='{Search}', Sort={Sort} {Dir}",
                CurrentPage, CurrentPageSize, GlobalSearchText, CurrentSortColumn, CurrentSortDirection);

            var query = new GetRolListQuery
            {
                PageNumber = CurrentPage,
                PageSize = CurrentPageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(GlobalSearchText) ? null : GlobalSearchText,
                SortColumn = CurrentSortColumn,
                SortDirection = CurrentSortDirection
            };

            var result = await Mediator.Send(query);

            if (_disposed) return;

            if (result.IsSuccess)
            {
                CurrentPageData = result.Value?.ToList() ?? new List<RolListDto>();
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
                ErrorMessage = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscută" });
                Logger.LogWarning("Eroare la încărcarea datelor: {Message}", ErrorMessage);
                CurrentPageData = new List<RolListDto>();
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
                ErrorMessage = $"Eroare neașteptată: {ex.Message}";
                Logger.LogError(ex, "Eroare la încărcarea datelor");
                CurrentPageData = new List<RolListDto>();
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
        if (_disposed) return;

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
                    Logger.LogError(ex, "Eroare la execuția search-ului");
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

            Logger.LogInformation("Enter pressed - immediate search: '{SearchText}'", GlobalSearchText);

            CurrentPage = 1;
            await LoadPagedData();
        }
    }

    private async Task ClearSearch()
    {
        if (_disposed) return;

        Logger.LogInformation("Clearing search");

        _searchDebounceTokenSource?.Cancel();

        GlobalSearchText = string.Empty;
        CurrentPage = 1;
        await LoadPagedData();
    }

    private async Task HandleRefresh()
    {
        if (_disposed) return;

        Logger.LogInformation("Refresh roluri");

        SelectedRol = null;
        await LoadPagedData();
        await ShowToast("Succes", "Datele au fost reîncărcate", "e-toast-success");
    }

    private async Task HandleAddNew()
    {
        if (_disposed) return;

        Logger.LogInformation("Opening modal for ADD Rol");

        if (rolFormModal != null)
        {
            await rolFormModal.OpenForAdd();
        }
    }

    private async Task HandleViewSelected()
    {
        if (_disposed) return;

        if (SelectedRol == null)
        {
            await ShowToast("Atenție", "Selectează un rând din tabel", "e-toast-warning");
            return;
        }

        Logger.LogInformation("Opening View modal for: {Id}", SelectedRol.Id);

        if (rolViewModal != null)
        {
            await rolViewModal.Open(SelectedRol.Id);
        }
    }

    private async Task HandleEditSelected()
    {
        if (_disposed) return;

        if (SelectedRol == null)
        {
            await ShowToast("Atenție", "Selectează un rând din tabel", "e-toast-warning");
            return;
        }

        if (SelectedRol.EsteSistem)
        {
            await ShowToast("Atenție", "Rolurile de sistem nu pot fi editate", "e-toast-warning");
            return;
        }

        Logger.LogInformation("Opening Edit modal for: {Id}", SelectedRol.Id);

        if (rolFormModal != null)
        {
            await rolFormModal.OpenForEdit(SelectedRol.Id);
        }
    }

    private async Task HandleManagePermissions()
    {
        if (_disposed) return;

        if (SelectedRol == null)
        {
            await ShowToast("Atenție", "Selectează un rând din tabel", "e-toast-warning");
            return;
        }

        Logger.LogInformation("Opening Permisiuni modal for: {Id}", SelectedRol.Id);

        if (rolPermisiuniModal != null)
        {
            await rolPermisiuniModal.Open(SelectedRol.Id);
        }
    }

    private async Task HandleRolSaved()
    {
        if (_disposed) return;

        Logger.LogInformation("Rol saved - reloading data");

        try
        {
            await Task.Delay(500);

            if (_disposed) return;

            await LoadPagedData();
            await ShowToast("Succes", "Rolul a fost salvat cu succes", "e-toast-success");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during rol saved handling");
            if (!_disposed)
            {
                await LoadPagedData();
            }
        }
    }

    private async Task HandlePermisiuniSaved()
    {
        if (_disposed) return;

        Logger.LogInformation("Permisiuni saved - reloading data");

        try
        {
            await Task.Delay(500);

            if (_disposed) return;

            await LoadPagedData();
            await ShowToast("Succes", "Permisiunile au fost salvate cu succes", "e-toast-success");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during permisiuni saved handling");
            if (!_disposed)
            {
                await LoadPagedData();
            }
        }
    }

    private async Task HandleEditFromView(Guid rolId)
    {
        if (_disposed) return;

        Logger.LogInformation("Edit requested from View modal for: {Id}", rolId);

        if (rolViewModal != null)
        {
            await rolViewModal.Close();
        }

        if (rolFormModal != null)
        {
            await rolFormModal.OpenForEdit(rolId);
        }
    }

    private async Task HandleDeleteFromView(Guid rolId)
    {
        if (_disposed) return;

        Logger.LogInformation("Delete requested from View modal for: {Id}", rolId);

        if (rolViewModal != null)
        {
            await rolViewModal.Close();
        }

        var rol = CurrentPageData.FirstOrDefault(r => r.Id == rolId);
        if (rol != null && confirmDeleteModal != null)
        {
            await confirmDeleteModal.Open(rolId, rol.Denumire);
        }
    }

    private async Task HandleManagePermissionsFromView(Guid rolId)
    {
        if (_disposed) return;

        Logger.LogInformation("Manage permissions requested from View modal for: {Id}", rolId);

        if (rolViewModal != null)
        {
            await rolViewModal.Close();
        }

        if (rolPermisiuniModal != null)
        {
            await rolPermisiuniModal.Open(rolId);
        }
    }

    private async Task HandleDeleteSelected()
    {
        if (_disposed) return;

        if (SelectedRol == null)
        {
            await ShowToast("Atenție", "Selectează un rând din tabel", "e-toast-warning");
            return;
        }

        if (SelectedRol.EsteSistem)
        {
            await ShowToast("Atenție", "Rolurile de sistem nu pot fi șterse", "e-toast-warning");
            return;
        }

        Logger.LogInformation("Opening Delete modal for: {Id} - {Denumire}",
            SelectedRol.Id, SelectedRol.Denumire);

        if (confirmDeleteModal != null)
        {
            await confirmDeleteModal.Open(SelectedRol.Id, SelectedRol.Denumire);
        }
    }

    private async Task HandleDeleteConfirmed(Guid id)
    {
        if (_disposed) return;

        Logger.LogInformation("Delete confirmed for: {Id}", id);

        try
        {
            var command = new DeleteRolCommand(id);
            var result = await Mediator.Send(command);

            if (_disposed) return;

            if (result.IsSuccess)
            {
                Logger.LogInformation("Rol deleted successfully: {Id}", id);
                SelectedRol = null;
                await LoadPagedData();
                await ShowToast("Succes", "Rolul a fost șters cu succes", "e-toast-success");
            }
            else
            {
                var errorMsg = string.Join(", ", result.Errors ?? new List<string> { "Eroare necunoscută" });
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
                await ShowToast("Eroare", $"Eroare la ștergere: {ex.Message}", "e-toast-danger");
            }
        }
    }

    private void OnRowSelected(RowSelectEventArgs<RolListDto> args)
    {
        if (_disposed) return;

        SelectedRol = args.Data;
        Logger.LogDebug("Row selected: {Id} - {Denumire}", SelectedRol?.Id, SelectedRol?.Denumire);
    }

    private void OnRowDeselected(RowDeselectEventArgs<RolListDto> args)
    {
        if (_disposed) return;

        SelectedRol = null;
        Logger.LogDebug("Row deselected");
    }

    private void OnGridActionBegin(ActionEventArgs<RolListDto> args)
    {
        if (_disposed) return;

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting)
        {
            Logger.LogDebug("Grid action: Sorting");
        }
    }

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

    private (int start, int end) GetPagerRange(int maxButtons)
    {
        int half = maxButtons / 2;
        int start = Math.Max(1, CurrentPage - half);
        int end = Math.Min(TotalPages, start + maxButtons - 1);

        if (end - start + 1 < maxButtons)
        {
            start = Math.Max(1, end - maxButtons + 1);
        }

        return (start, end);
    }

    private string GetRolIcon(string denumire)
    {
        return denumire?.ToLower() switch
        {
            "admin" or "administrator" => "fa-user-shield",
            "doctor" or "medic" => "fa-user-md",
            "asistent" => "fa-user-nurse",
            "receptioner" => "fa-user-tie",
            "manager" => "fa-briefcase",
            _ => "fa-user"
        };
    }

    private async Task ShowToast(string title, string content, string cssClass)
    {
        if (_disposed || ToastRef == null) return;

        try
        {
            await ToastRef.ShowAsync(new ToastModel
            {
                Title = title,
                Content = content,
                CssClass = cssClass,
                Timeout = 3000,
                ShowCloseButton = true
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error showing toast");
        }
    }
}
