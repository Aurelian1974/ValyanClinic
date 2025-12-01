using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.UserSessions.Queries.GetActiveSessions;
using ValyanClinic.Application.Features.UserSessions.Commands.EndSession;
using ValyanClinic.Services;
using System.Timers;
using Syncfusion.Blazor.Notifications;

namespace ValyanClinic.Components.Pages.Monitorizare.Sesiuni;

/// <summary>
/// Pagină pentru monitorizarea sesiunilor active
/// </summary>
public partial class AdministrareSesiuniActive : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AdministrareSesiuniActive> Logger { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!;

    // Toast reference for notifications
    private SfToast? ToastRef;

    // State
    private bool IsLoading { get; set; } = true;
    private bool HasError { get; set; }
    private string ErrorMessage { get; set; } = string.Empty;

    // Data
    private List<ActiveSessionDto> ActiveSessions { get; set; } = new();
    private ActiveSessionDto? SelectedSession { get; set; }

    // Statistics
    private int TotalActive { get; set; }
    private int ExpiraInCurand { get; set; }
    private int InactiviAzi { get; set; }

    // Filters
    private string GlobalSearchText { get; set; } = string.Empty;
    private bool ShowOnlyExpiring { get; set; } = false;

    // Auto-refresh
    private System.Timers.Timer? _refreshTimer;
    private const int RefreshIntervalSeconds = 15;
    private bool _disposed = false;

    // Modal state
    private bool ShowDetailsModal { get; set; }
    private bool ShowEndSessionModal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Initializing AdministrareSesiuniActive page");

            await LoadData();
            await LoadStatistics();

            // Setup auto-refresh timer
            SetupAutoRefresh();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing page");
            HasError = true;
            ErrorMessage = $"Eroare la initializare: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadData()
    {
        if (_disposed) return;

        try
        {
            IsLoading = true;

            var query = new GetActiveSessionsQuery
            {
                DoarExpiraInCurand = ShowOnlyExpiring,
                SortColumn = "DataUltimaActivitate",
                SortDirection = "DESC"
            };

            var result = await Mediator.Send(query);

            if (_disposed) return;

            if (result.IsSuccess && result.Value != null)
            {
                ActiveSessions = result.Value.ToList();

                // Apply global search if any
                if (!string.IsNullOrWhiteSpace(GlobalSearchText))
                {
                    ApplyGlobalSearch();
                }

                Logger.LogInformation("Loaded {Count} active sessions", ActiveSessions.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.ErrorsAsString ?? "Eroare la incarcarea sesiunilor";
                Logger.LogWarning("Failed to load sessions: {Errors}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                HasError = true;
                ErrorMessage = $"Eroare: {ex.Message}";
                Logger.LogError(ex, "Error loading sessions");
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

    private async Task LoadStatistics()
    {
        if (_disposed) return;

        try
        {
            // Statistics sunt calculate din lista de sesiuni
            TotalActive = ActiveSessions.Count(s => s.EsteActiva);
            ExpiraInCurand = ActiveSessions.Count(s => s.ExpiraInCurând);
            InactiviAzi = ActiveSessions.Count(s => !s.EsteActiva &&
           s.DataUltimaActivitate.Date == DateTime.Today);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading statistics");
        }
    }

    private void SetupAutoRefresh()
    {
        _refreshTimer = new System.Timers.Timer(RefreshIntervalSeconds * 1000);
        _refreshTimer.Elapsed += async (sender, e) => await OnRefreshTimerElapsed();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();

        Logger.LogInformation("Auto-refresh enabled: every {Seconds} seconds", RefreshIntervalSeconds);
    }

    private async Task OnRefreshTimerElapsed()
    {
        if (_disposed) return;

        Logger.LogDebug("Auto-refresh triggered");

        await InvokeAsync(async () =>
            {
                if (!_disposed)
                {
                    await LoadData();
                    await LoadStatistics();
                }
            });
    }

    private void ApplyGlobalSearch()
    {
        if (string.IsNullOrWhiteSpace(GlobalSearchText))
        {
            return;
        }

        var searchLower = GlobalSearchText.ToLower();

        ActiveSessions = ActiveSessions.Where(s =>
            s.Username.ToLower().Contains(searchLower) ||
      s.Email.ToLower().Contains(searchLower) ||
            s.Rol.ToLower().Contains(searchLower) ||
     s.AdresaIP.ToLower().Contains(searchLower) ||
     (s.UserAgent?.ToLower().Contains(searchLower) == true) ||
      (s.Dispozitiv?.ToLower().Contains(searchLower) == true)
        ).ToList();
    }

    private async Task HandleSearch(string searchText)
    {
        if (_disposed) return;

        GlobalSearchText = searchText;
        await LoadData();
    }

    private async Task HandleClearSearch()
    {
        if (_disposed) return;

        GlobalSearchText = string.Empty;
        await LoadData();
    }

    private async Task HandleToggleExpiringFilter()
    {
        if (_disposed) return;

        ShowOnlyExpiring = !ShowOnlyExpiring;
        await LoadData();
    }

    private async Task HandleRefresh()
    {
        if (_disposed) return;

        Logger.LogInformation("Manual refresh triggered");
        await LoadData();
        await LoadStatistics();

        await NotificationService.ShowSuccessAsync("Sesiunile au fost actualizate");
    }

    private void HandleViewDetails(ActiveSessionDto session)
    {
        if (_disposed) return;

        Logger.LogInformation("Viewing details for session: {SessionID}", session.SessionID);

        SelectedSession = session;
        ShowDetailsModal = true;
    }

    private void HandleEndSession(ActiveSessionDto session)
    {
        if (_disposed) return;

        Logger.LogInformation("Initiating end session for: {SessionID}", session.SessionID);

        SelectedSession = session;
        ShowEndSessionModal = true;
    }

    private async Task ConfirmEndSession()
    {
        if (_disposed || SelectedSession == null) return;

        try
        {
            Logger.LogInformation("Ending session: {SessionID}", SelectedSession.SessionID);

            var command = new EndSessionCommand(SelectedSession.SessionID, "Admin");
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                await NotificationService.ShowSuccessAsync($"Sesiunea pentru {SelectedSession.Username} a fost închisă");

                ShowEndSessionModal = false;
                SelectedSession = null;

                await LoadData();
                await LoadStatistics();
            }
            else
            {
                await NotificationService.ShowErrorAsync(result.ErrorsAsString ?? "Eroare la închiderea sesiunii");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error ending session");
            await NotificationService.ShowErrorAsync($"Eroare: {ex.Message}");
        }
    }

    private void CancelEndSession()
    {
        ShowEndSessionModal = false;
        SelectedSession = null;
    }

    private void CloseDetailsModal()
    {
        ShowDetailsModal = false;
        SelectedSession = null;
    }

    // ✅ Register Toast component after render
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && ToastRef != null)
        {
            NotificationService.RegisterToast(ToastRef);
            Logger.LogDebug("Toast component registered successfully");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Elapsed -= async (sender, e) => await OnRefreshTimerElapsed();
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }

            _disposed = true;

            Logger.LogDebug("AdministrareSesiuniActive disposed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error disposing component");
        }
    }
}
