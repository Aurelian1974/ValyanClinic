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
using Microsoft.AspNetCore.SignalR.Client;

namespace ValyanClinic.Components.Pages.Administrare.PersonalMedical;

public partial class AdministrarePersonalMedical : ComponentBase, IDisposable
{
    // Initialization flags - simplified to instance-level to avoid global locking
    // Removed global static locks and retry logic to reduce complexity and prevent cross-component blocking.

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

    // UI render/persist guards to avoid JS interop during prerender
    private bool _hasRendered = false;
    private bool _pendingPersist = false;

    // UI state persistence key
    private const string UiStateStorageKey = "PersonalMedical_UIState_v1";

    private List<FilterOption> StatusOptions { get; set; } = new();
    private List<FilterOption> DepartamentOptions { get; set; } = new();
    private List<FilterOption> PozitieOptions { get; set; } = new();

    private string ToastTitle { get; set; } = string.Empty;
    private string ToastContent { get; set; } = string.Empty;
    private string ToastCssClass { get; set; } = string.Empty;

    private class UiState
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string GlobalSearchText { get; set; } = string.Empty;
        public string? FilterDepartament { get; set; }
        public string? FilterPozitie { get; set; }
        public string? FilterEsteActiv { get; set; }
        public string SortColumn { get; set; } = "Nume";
        public string SortDirection { get; set; } = "ASC";
    }

    private string? AdvancedNume { get; set; }
    private string AdvancedNumeOperator { get; set; } = "Contains";

    private string? AdvancedSpecializare { get; set; }
    private string AdvancedSpecializareOperator { get; set; } = "Contains";

    private string? AdvancedNumarLicenta { get; set; }
    private string AdvancedNumarLicentaOperator { get; set; } = "Contains";

    private List<FilterOption> OperatorOptions { get; set; } = new()
    {
        new FilterOption { Value = "Contains", Text = "Conține" },
        new FilterOption { Value = "Equals", Text = "Egal cu" },
        new FilterOption { Value = "StartsWith", Text = "Începe cu" },
        new FilterOption { Value = "EndsWith", Text = "Se termină cu" }
    };

    private int ActiveFiltersCount =>
        (string.IsNullOrEmpty(FilterDepartament) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterPozitie) ? 0 : 1) +
        (string.IsNullOrEmpty(FilterEsteActiv) ? 0 : 1) +
        (string.IsNullOrEmpty(GlobalSearchText) ? 0 : 1) +
        (string.IsNullOrEmpty(AdvancedNume) ? 0 : 1) +
        (string.IsNullOrEmpty(AdvancedSpecializare) ? 0 : 1) +
        (string.IsNullOrEmpty(AdvancedNumarLicenta) ? 0 : 1);

    private bool HasPreviousPage => CurrentPage > 1;
    private bool HasNextPage => CurrentPage < TotalPages;
    private int DisplayedRecordsStart => TotalRecords > 0 ? ((CurrentPage - 1) * CurrentPageSize) + 1 : 0;
    private int DisplayedRecordsEnd => Math.Min(CurrentPage * CurrentPageSize, TotalRecords);

    protected override async Task OnInitializedAsync()
    {
        if (_initialized || _isInitializing)
        {
            Logger.LogDebug("[PersonalMedical] Initialization skipped - already initialized or running. Key: {KeyValue}", KeyValue);
            return;
        }

        _isInitializing = true;
        IsLoading = true;

        try
        {
            Logger.LogInformation("[PersonalMedical] Starting initialization - Key: {KeyValue}", KeyValue);

            // Load the first page of data from server (UI state will be loaded after first render)
            await LoadPagedData();

            _initialized = true;

            Logger.LogInformation("[PersonalMedical] Initialization complete - Key: {KeyValue}", KeyValue);
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PersonalMedical] Component disposed during initialization");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PersonalMedical] ERROR during initialization");
            HasError = true;
            ErrorMessage = $"Eroare la initializare: {ex.Message}";
        }
        finally
        {
            _isInitializing = false;
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private DotNetObjectReference<AdministrarePersonalMedical>? _dotNetRef;

    private Microsoft.AspNetCore.SignalR.Client.HubConnection? _personalHubConnection;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // JS interop only safe after first render
            _hasRendered = true;

            try
            {
                // Load UI state from localStorage after render
                await LoadUiStateAsync();

                // If a persist was requested during prerender, do it now
                if (_pendingPersist)
                {
                    _pendingPersist = false;
                    await PersistUiStateAsync();
                }

                // Register keyboard shortcuts
                _dotNetRef = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("keyboardShortcuts.register", _dotNetRef);
                Logger.LogDebug("Registered keyboard shortcuts");

                // Setup SignalR connection for realtime updates
                try
                {
                    _personalHubConnection = new Microsoft.AspNetCore.SignalR.Client.HubConnectionBuilder()
                        .WithUrl(NavigationManager.ToAbsoluteUri("/personalmedicalHub"))
                        .WithAutomaticReconnect()
                        .Build();

                    _personalHubConnection.On<string, Guid>("PersonalChanged", async (action, id) =>
                    {
                        Logger.LogInformation("SignalR PersonalChanged received: {Action} {Id}", action, id);

                        if (_disposed)
                        {
                            Logger.LogDebug("SignalR message ignored because component is disposed");
                            return;
                        }

                        // Apply in-place updates where possible to avoid full reload and UI flicker
                        await InvokeAsync(async () =>
                        {
                            if (_disposed) return;

                            try
                            {
                                await HandlePersonalChangedAsync(action, id);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogDebug(ex, "Error applying in-place SignalR update");
                                // fallback: attempt a full refresh if in-place update fails
                                try { await LoadPagedData(); } catch { }
                            }
                        });
                    });

                    await _personalHubConnection.StartAsync();
                    Logger.LogDebug("PersonalMedical SignalR connected");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to start PersonalMedical SignalR connection");
                }
            }
            catch (Exception ex)
            {
                // Log at debug level to avoid noisy warnings from prerender
                Logger.LogDebug(ex, "OnAfterRenderAsync - initialization step failed");
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

        // Fire-and-forget: stop and dispose SignalR connection gracefully
        _ = StopAndDisposeSignalRAsync();

        Logger.LogWarning("🏁 [PersonalMedical] Dispose END - Time: {Time}, Total elapsed: {Elapsed}ms (Syncfusion cleanup handled by Blazor)",
                DateTime.Now.ToString("HH:mm:ss.fff"), (DateTime.Now - disposeTime).TotalMilliseconds);
    }

    private async Task StopAndDisposeSignalRAsync()
    {
        try
        {
            if (_personalHubConnection != null)
            {
                try
                {
                    // Remove handlers first to avoid receiving callbacks while stopping
                    _personalHubConnection.Remove("PersonalChanged");
                }
                catch { }

                await _personalHubConnection.StopAsync();
                await _personalHubConnection.DisposeAsync();
                Logger.LogDebug("PersonalMedical SignalR connection stopped and disposed");
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Error stopping SignalR connection");
        }
        finally
        {
            _personalHubConnection = null;
        }
    }

    // Apply a minimal in-place update in response to a SignalR event
    private async Task HandlePersonalChangedAsync(string action, Guid personalId)
    {
        if (_disposed) return;

        action = action?.Trim() ?? string.Empty;

        switch (action)
        {
            case "Created":
                await ApplyCreatedAsync(personalId);
                break;
            case "Updated":
                await ApplyUpdatedAsync(personalId);
                break;
            case "Deleted":
                await ApplyDeletedAsync(personalId);
                break;
            default:
                // Unknown action - fallback to full refresh
                await LoadPagedData();
                break;
        }
    }

    private PersonalMedicalListDto MapToListDto(ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById.PersonalMedicalDetailDto detail)
    {
        return new PersonalMedicalListDto
        {
            PersonalID = detail.PersonalID,
            Nume = detail.Nume,
            Prenume = detail.Prenume,
            Specializare = detail.Specializare,
            NumarLicenta = detail.NumarLicenta,
            Telefon = detail.Telefon,
            Email = detail.Email,
            Departament = detail.Departament,
            Pozitie = detail.Pozitie,
            EsteActiv = detail.EsteActiv,
            DataCreare = detail.DataCreare
        };
    }

    private bool MatchesCurrentFilters(PersonalMedicalListDto dto)
    {
        // Simple filter checks: GlobalSearchText, FilterDepartament, FilterPozitie, FilterEsteActiv
        if (!string.IsNullOrWhiteSpace(FilterDepartament) && !string.Equals(dto.Departament, FilterDepartament, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(FilterPozitie) && !string.Equals(dto.Pozitie, FilterPozitie, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(FilterEsteActiv))
        {
            var filterVal = FilterEsteActiv == "true";
            if (dto.EsteActiv.HasValue && dto.EsteActiv.Value != filterVal) return false;
        }

        if (!string.IsNullOrWhiteSpace(GlobalSearchText))
        {
            var s = GlobalSearchText.Trim();
            if (!( (dto.Nume?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ||
                   (dto.Prenume?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ||
                   (dto.NumeComplet?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ||
                   (dto.Departament?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ||
                   (dto.Pozitie?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ))
                return false;
        }

        return true;
    }

    private async Task ApplyCreatedAsync(Guid personalId)
    {
        // Only fetch and insert if created item matches current filters
        try
        {
            var result = await Mediator.Send(new ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById.GetPersonalMedicalByIdQuery(personalId));
            if (!result.IsSuccess || result.Value == null) return;

            var dto = MapToListDto(result.Value);

            if (!MatchesCurrentFilters(dto))
            {
                // Item doesn't match current filters; just increment TotalRecords
                TotalRecords++;
                await InvokeAsync(StateHasChanged);
                return;
            }

            TotalRecords++;

            // If we are on first page, insert at top to reflect recency
            if (CurrentPage == 1)
            {
                CurrentPageData.Insert(0, dto);

                // Trim to page size
                if (CurrentPageData.Count > CurrentPageSize)
                {
                    CurrentPageData.RemoveAt(CurrentPageData.Count - 1);
                }

                await InvokeAsync(StateHasChanged);

                // Try to highlight newly inserted row
                try
                {
                    if (!_disposed)
                    {
                        await InvokeAsync(async () =>
                        {
                            await Task.Yield();
                            await JSRuntime.InvokeVoidAsync("personalMedicalEffects.highlightRow", personalId, 2000);
                        });
                    }
                }
                catch (Exception jsEx)
                {
                    Logger.LogDebug(jsEx, "Highlight JS failed for created PersonalID {PersonalId}", personalId);
                }
            }
            else
            {
                // Not on first page - we don't modify data but total count changed
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Error applying Created event for PersonalID {PersonalId}", personalId);
        }
    }

    private async Task ApplyUpdatedAsync(Guid personalId)
    {
        try
        {
            var result = await Mediator.Send(new ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById.GetPersonalMedicalByIdQuery(personalId));
            if (!result.IsSuccess || result.Value == null) return;

            var dto = MapToListDto(result.Value);

            var idx = CurrentPageData.FindIndex(p => p.PersonalID == personalId);
            if (idx >= 0)
            {
                // If updated item still matches filters, replace; otherwise remove
                if (MatchesCurrentFilters(dto))
                {
                    CurrentPageData[idx] = dto;
                    await InvokeAsync(StateHasChanged);

                    // Highlight updated row
                    try
                    {
                        if (!_disposed)
                        {
                            await InvokeAsync(async () =>
                            {
                                await Task.Yield();
                                await JSRuntime.InvokeVoidAsync("personalMedicalEffects.highlightRow", personalId, 2000);
                            });
                        }
                    }
                    catch (Exception jsEx)
                    {
                        Logger.LogDebug(jsEx, "Highlight JS failed for updated PersonalID {PersonalId}", personalId);
                    }
                }
                else
                {
                    CurrentPageData.RemoveAt(idx);
                    TotalRecords = Math.Max(0, TotalRecords - 1);
                    await InvokeAsync(StateHasChanged);
                }
            }
            else
            {
                // Not on current page - if it matches filters and we're on first page, we can insert
                if (MatchesCurrentFilters(dto) && CurrentPage == 1)
                {
                    CurrentPageData.Insert(0, dto);
                    if (CurrentPageData.Count > CurrentPageSize) CurrentPageData.RemoveAt(CurrentPageData.Count - 1);
                    await InvokeAsync(StateHasChanged);

                    // Highlight updated/inserted row after bringing it to current page
                    try
                    {
                        if (!_disposed)
                        {
                            await InvokeAsync(async () =>
                            {
                                await Task.Yield();
                                await JSRuntime.InvokeVoidAsync("personalMedicalEffects.highlightRow", personalId, 2000);
                            });
                        }
                    }
                    catch (Exception jsEx)
                    {
                        Logger.LogDebug(jsEx, "Highlight JS failed for updated PersonalID {PersonalId}", personalId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Error applying Updated event for PersonalID {PersonalId}", personalId);
        }
    }

    private async Task ApplyDeletedAsync(Guid personalId)
    {
        try
        {
            var idx = CurrentPageData.FindIndex(p => p.PersonalID == personalId);
            if (idx >= 0)
            {
                CurrentPageData.RemoveAt(idx);
                TotalRecords = Math.Max(0, TotalRecords - 1);

                // If page is now empty but there are more pages, step back and reload
                if (CurrentPageData.Count == 0 && CurrentPage > 1)
                {
                    CurrentPage = Math.Max(1, CurrentPage - 1);
                    await LoadPagedData();
                    return;
                }

                await InvokeAsync(StateHasChanged);
            }
            else
            {
                // Item not on current page; just decrement total if applicable
                if (TotalRecords > 0) TotalRecords = Math.Max(0, TotalRecords - 1);
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Error applying Deleted event for PersonalID {PersonalId}", personalId);
        }
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

            // Attach column filters if any advanced column filters are supplied
            var columnFilters = new List<ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList.ColumnFilterDto>();
            if (!string.IsNullOrWhiteSpace(AdvancedNume)) columnFilters.Add(new ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList.ColumnFilterDto { Column = "Nume", Operator = AdvancedNumeOperator ?? "Contains", Value = AdvancedNume });
            if (!string.IsNullOrWhiteSpace(AdvancedSpecializare)) columnFilters.Add(new ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList.ColumnFilterDto { Column = "Specializare", Operator = AdvancedSpecializareOperator ?? "Contains", Value = AdvancedSpecializare });
            if (!string.IsNullOrWhiteSpace(AdvancedNumarLicenta)) columnFilters.Add(new ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList.ColumnFilterDto { Column = "NumarLicenta", Operator = AdvancedNumarLicentaOperator ?? "Contains", Value = AdvancedNumarLicenta });

            if (columnFilters.Any())
            {
                query = query with { ColumnFilters = columnFilters };
            }

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

                // Prefer backend-provided metadata for filters & stats
                if (result.MetaData is ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList.PersonalMedicalListMeta meta)
                {
                    // Map domain/application metadata to component filter option model
                    StatusOptions = meta.Filters.AvailableStatuses.Select(s => new FilterOption { Value = s.Value, Text = s.Text }).ToList();
                    DepartamentOptions = meta.Filters.AvailableDepartamente.Select(d => new FilterOption { Value = d.Value, Text = d.Text }).ToList();
                    PozitieOptions = meta.Filters.AvailablePozitii.Select(p => new FilterOption { Value = p.Value, Text = p.Text }).ToList();

                    Logger.LogDebug("Loaded filter metadata from server: Dept={DeptCount}, Poz={PozCount}", DepartamentOptions.Count, PozitieOptions.Count);
                }
                else
                {
                    LoadFilterOptionsFromData();
                }

                // Persist UI state after successful load (light write)
                _ = PersistUiStateAsync();

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

        // Clear all filters including advanced ones
        GlobalSearchText = string.Empty;
        FilterDepartament = null;
        FilterPozitie = null;
        FilterEsteActiv = null;

        AdvancedNume = null;
        AdvancedNumeOperator = "Contains";
        AdvancedSpecializare = null;
        AdvancedSpecializareOperator = "Contains";
        AdvancedNumarLicenta = null;
        AdvancedNumarLicentaOperator = "Contains";

        CurrentPage = 1;
        await LoadPagedData();

        // Persist state after clearing
        _ = PersistUiStateAsync();
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
            case nameof(AdvancedNume):
                AdvancedNume = null;
                AdvancedNumeOperator = "Contains";
                break;
            case nameof(AdvancedSpecializare):
                AdvancedSpecializare = null;
                AdvancedSpecializareOperator = "Contains";
                break;
            case nameof(AdvancedNumarLicenta):
                AdvancedNumarLicenta = null;
                AdvancedNumarLicentaOperator = "Contains";
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

            // Persist search state
            _ = PersistUiStateAsync();
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

        // Persist selected filters
        _ = PersistUiStateAsync();
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

        Logger.LogInformation("🎉 Personal Medical saved - reloading data while preserving UI state");

        try
        {
            // Wait a short moment for modal to finish closing animations
            await Task.Delay(300);

            if (_disposed) return;

            // Show loading state and reload data WITHOUT forcing full page reload
            IsLoading = true;
            await InvokeAsync(StateHasChanged);

            await LoadPagedData();

            // If view modal is open, refresh it (best-effort)
            if (personalMedicalViewModal != null)
            {
                try
                {
                    await personalMedicalViewModal.RefreshData();
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Failed to refresh view modal after save");
                }
            }

            await ShowActionToast("Salvat", "Personal medical", "Modificările au fost salvate", "e-toast-success");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during post-save reload");
            if (!_disposed)
            {
                await ShowToast("Eroare", "A apărut o eroare la reîncărcare", "e-toast-danger");
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
                var deletedName = CurrentPageData.FirstOrDefault(p => p.PersonalID == personalID)?.NumeComplet ?? "Personal medical";
                await ShowActionToast("Șters", deletedName, "Înregistrarea a fost ștearsă", "e-toast-success");
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

        // Persist page change
        _ = PersistUiStateAsync();
    }

    private async Task PersistUiStateAsync()
    {
        if (_disposed) return;

        // If we haven't rendered yet (prerender), avoid JS interop and defer persist until after render
        if (!_hasRendered)
        {
            _pendingPersist = true;
            Logger.LogDebug("Deferring UI state persist until after render");
            return;
        }

        try
        {
            var state = new UiState
            {
                CurrentPage = CurrentPage,
                PageSize = CurrentPageSize,
                GlobalSearchText = GlobalSearchText,
                FilterDepartament = FilterDepartament,
                FilterPozitie = FilterPozitie,
                FilterEsteActiv = FilterEsteActiv,
                SortColumn = CurrentSortColumn,
                SortDirection = CurrentSortDirection
            };

            var json = System.Text.Json.JsonSerializer.Serialize(state);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", UiStateStorageKey, json);
            Logger.LogDebug("UI state persisted to localStorage: {Key}", UiStateStorageKey);
        }
        catch (Exception ex)
        {
            // Lower the severity: failures are non-critical and often due to prerender/JS availability
            Logger.LogDebug(ex, "PersistUiStateAsync - failed to invoke JS interop");
        }
    }

    private async Task LoadUiStateAsync()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", UiStateStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var state = System.Text.Json.JsonSerializer.Deserialize<UiState>(json);
                if (state != null)
                {
                    Logger.LogInformation("Loaded UI state from localStorage: Page={Page}, Size={Size}", state.CurrentPage, state.PageSize);

                    CurrentPage = Math.Max(1, state.CurrentPage);
                    CurrentPageSize = state.PageSize >= MinPageSize && state.PageSize <= MaxPageSize ? state.PageSize : CurrentPageSize;
                    GlobalSearchText = state.GlobalSearchText ?? string.Empty;
                    FilterDepartament = state.FilterDepartament;
                    FilterPozitie = state.FilterPozitie;
                    FilterEsteActiv = state.FilterEsteActiv;
                    CurrentSortColumn = state.SortColumn ?? CurrentSortColumn;
                    CurrentSortDirection = state.SortDirection ?? CurrentSortDirection;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load UI state from localStorage");
        }
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

        // Persist state after page size change
        _ = PersistUiStateAsync();
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

    // Export UI
    private bool IsExportMenuOpen { get; set; } = false;

    private void ToggleExportMenu()
    {
        IsExportMenuOpen = !IsExportMenuOpen;
    }

    private async Task ExportDataAsync(string format)
    {
        // Build export URL with current filters
        var sb = new System.Text.StringBuilder("api/personalmedical/export?");
        sb.Append($"format={Uri.EscapeDataString(format)}");
        if (!string.IsNullOrEmpty(GlobalSearchText)) sb.Append($"&search={Uri.EscapeDataString(GlobalSearchText)}");
        if (!string.IsNullOrEmpty(FilterDepartament)) sb.Append($"&departament={Uri.EscapeDataString(FilterDepartament)}");
        if (!string.IsNullOrEmpty(FilterPozitie)) sb.Append($"&pozitie={Uri.EscapeDataString(FilterPozitie)}");
        if (!string.IsNullOrEmpty(FilterEsteActiv)) sb.Append($"&esteActiv={Uri.EscapeDataString(FilterEsteActiv)}");
        sb.Append($"&sortColumn={Uri.EscapeDataString(CurrentSortColumn)}&sortDirection={Uri.EscapeDataString(CurrentSortDirection)}");

        var url = sb.ToString();

        // Close menu
        IsExportMenuOpen = false;

        try
        {
            var absUrl = NavigationManager.ToAbsoluteUri(url).ToString();

            var jsResult = await JSRuntime.InvokeAsync<System.Text.Json.JsonElement>("exportDownload", absUrl);

            if (jsResult.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
            {
                var filename = jsResult.TryGetProperty("filename", out var f) ? f.GetString() : format.ToUpper();
                await ShowActionToast("Export", filename ?? "Export", "Descărcare export reușită", "e-toast-success");
            }
            else
            {
                var reason = jsResult.TryGetProperty("reason", out var r) ? r.GetString() : null;
                var message = jsResult.TryGetProperty("message", out var m) ? m.GetString() : "Eroare la export";

                if (reason == "no-data")
                {
                    await ShowActionToast("Export", "Nu există date pentru export", message, "e-toast-warning");
                }
                else
                {
                    await ShowActionToast("Export", "Export eșuat", message, "e-toast-danger");
                }
            }
        }
        catch (JSException jsEx)
        {
            Logger.LogError(jsEx, "JS error during export");
            await ShowActionToast("Export", "Export eșuat", jsEx.Message, "e-toast-danger");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during export");
            await ShowActionToast("Export", "Export eșuat", ex.Message, "e-toast-danger");
        }
    }

    private async Task ExportCsv()
    {
        await ExportDataAsync("csv");
    }

    private async Task ExportExcel()
    {
        await ExportDataAsync("excel");
    }
    private void OnRowDeselected(RowDeselectEventArgs<PersonalMedicalListDto> args)
    {
        if (_disposed) return;

        SelectedPersonal = null;
        Logger.LogInformation("Selectie anulata");
        StateHasChanged();
    }

    // Quick actions from inline buttons
    private async Task QuickView(Guid personalID)
    {
        if (_disposed) return;
        Logger.LogInformation("QuickView requested for {Id}", personalID);
        if (personalMedicalViewModal != null)
        {
            await personalMedicalViewModal.Open(personalID);
        }
    }

    private async Task QuickEdit(Guid personalID)
    {
        if (_disposed) return;
        Logger.LogInformation("QuickEdit requested for {Id}", personalID);

        if (personalMedicalFormModal != null)
        {
            await personalMedicalFormModal.OpenForEdit(personalID);
        }
    }

    private async Task QuickToggleStatus(Guid personalID, bool? currentStatus)
    {
        if (_disposed) return;

        try
        {
            var personal = CurrentPageData.FirstOrDefault(p => p.PersonalID == personalID);
            if (personal == null)
            {
                await ShowToast("Eroare", "Personal medical nu a fost gasit pe pagina curenta", "e-toast-warning");
                return;
            }

            var newStatus = !(personal.EsteActiv ?? false);

            var command = new ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.UpdatePersonalMedical.UpdatePersonalMedicalCommand
            {
                PersonalID = personal.PersonalID,
                Nume = personal.Nume,
                Prenume = personal.Prenume,
                Specializare = personal.Specializare,
                NumarLicenta = personal.NumarLicenta,
                Telefon = personal.Telefon,
                Email = personal.Email,
                Departament = personal.Departament,
                Pozitie = personal.Pozitie,
                EsteActiv = newStatus
            };

            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                var name = personal.NumeComplet ?? "Personal medical";
                await ShowActionToast(newStatus ? "Activat" : "Dezactivat", name, null, "e-toast-success");
                await LoadPagedData();
            }
            else
            {
                await ShowToast("Eroare", string.Join(", ", result.Errors ?? new List<string> { "Eroare" }), "e-toast-danger");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la QuickToggleStatus");
            await ShowToast("Eroare", "A apărut o eroare la modificarea statusului", "e-toast-danger");
        }
    }

    private async Task QuickDelete(Guid personalID)
    {
        if (_disposed) return;

        var personal = CurrentPageData.FirstOrDefault(p => p.PersonalID == personalID);
        if (personal == null)
        {
            await ShowToast("Eroare", "Personal medical nu a fost gasit pe pagina curenta", "e-toast-warning");
            return;
        }

        if (confirmDeleteModal != null)
        {
            await confirmDeleteModal.Open(personalID, personal.NumeComplet);
        }
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

        // Persist UI state before showing important notifications
        _ = PersistUiStateAsync();

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

    // Enhanced action-based toast helper
    private async Task ShowActionToast(string action, string entityName, string? details = null, string cssClass = "e-toast-success")
    {
        if (_disposed) return;

        var title = action switch
        {
            "Salvat" => "✅ Salvat",
            "Editat" => "✏️ Modificat",
            "Șters" or "Sters" => "🗑️ Șters",
            "Activat" => "▶️ Activat",
            "Dezactivat" => "⏸️ Dezactivat",
            "Activat/Dezactivat" => "🔀 Schimbare status",
            _ => action
        };

        var content = string.IsNullOrWhiteSpace(details) ? entityName : $"{entityName} - {details}";

        await ShowToast(title, content, cssClass);
    }

    [JSInvokable]
    public async Task OnKeyboardShortcut(string action)
    {
        if (_disposed) return;

        try
        {
            var a = (action ?? string.Empty).ToLowerInvariant();
            Logger.LogInformation("Keyboard shortcut invoked: {Action}", a);

            switch (a)
            {
                case "new":
                    await HandleAddNew();
                    break;
                case "edit":
                    if (SelectedPersonal != null)
                        await HandleEditSelected();
                    else
                        await ShowActionToast("Info", "Selectati o inregistrare pentru editare", null, "e-toast-warning");
                    break;
                case "delete":
                    if (SelectedPersonal != null)
                        await HandleDeleteSelected();
                    else
                        await ShowActionToast("Info", "Selectati o inregistrare pentru stergere", null, "e-toast-warning");
                    break;
                case "focussearch":
                    await JSRuntime.InvokeVoidAsync("keyboardShortcuts.focusSearch");
                    break;
                case "reload":
                    await HandleRefresh();
                    break;
                case "escape":
                    // Close modals if any open, otherwise collapse filters or clear search
                    if (personalMedicalFormModal != null)
                    {
                        await personalMedicalFormModal.Close();
                        return;
                    }

                    if (personalMedicalViewModal != null)
                    {
                        await personalMedicalViewModal.Close();
                        return;
                    }

                    if (confirmDeleteModal != null)
                    {
                        await confirmDeleteModal.Close();
                        return;
                    }

                    if (IsAdvancedFilterExpanded)
                    {
                        IsAdvancedFilterExpanded = false;
                        StateHasChanged();
                        return;
                    }

                    if (!string.IsNullOrEmpty(GlobalSearchText))
                    {
                        await ClearSearch();
                        return;
                    }

                    break;
                default:
                    Logger.LogDebug("Unhandled keyboard shortcut: {Action}", a);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling keyboard shortcut: {Action}", action);
        }
    }

    private class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
