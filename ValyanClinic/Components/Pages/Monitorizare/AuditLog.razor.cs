using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.Settings.Queries.GetAuditLogs;
using Syncfusion.Blazor.Grids;

namespace ValyanClinic.Components.Pages.Monitorizare;

public partial class AuditLog : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditLog> Logger { get; set; } = default!;

    private SfGrid<AuditLogDto>? GridRef;

    private List<AuditLogDto> AuditLogs = new();
    private List<string> ActionTypes = new() { "Login", "Logout", "Create", "Update", "Delete", "LoginFailed" };

    private string? SelectedAction;
    private DateTime? DataStart;
    private DateTime? DataEnd;

    private bool IsLoading = true;
    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Loading audit logs...");
            await LoadAuditLogsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audit logs");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAuditLogsAsync()
    {
        try
        {
            // Use MediatR query
            var query = new GetAuditLogsQuery(
                    PageNumber: 1,
           PageSize: 100,
             SearchText: null,
           UtilizatorID: null,
             Actiune: SelectedAction,
         DataStart: DataStart,
          DataEnd: DataEnd,
          SortColumn: "DataActiune",
              SortDirection: "DESC");

            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                AuditLogs = result.Value.Items;
                Logger.LogInformation("Loaded {Count} audit logs", AuditLogs.Count);
            }
            else
            {
                Logger.LogWarning("Failed to load audit logs: {Errors}", result.ErrorsAsString);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audit logs from mediator");
            throw;
        }
    }

    private async Task ApplyFilters()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            await LoadAuditLogsAsync();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        AuditLogs?.Clear();
        GridRef = null;
    }
}
