using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.Settings.Queries.GetActiveSessions;
using Syncfusion.Blazor.Grids;

namespace ValyanClinic.Components.Pages.Monitorizare;

public partial class ActiveSessions : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ActiveSessions> Logger { get; set; } = default!;

    private SfGrid<UserSessionDto>? GridRef;
    
    private List<UserSessionDto> Sessions = new();
    
    private bool IsLoading = true;
    private bool _disposed = false;

    private int ExpiringSoonCount => Sessions.Count(s => 
  (s.DataExpirare - DateTime.UtcNow).TotalMinutes < 5 && s.EsteActiva);

  protected override async Task OnInitializedAsync()
    {
        try
     {
  Logger.LogInformation("Loading active sessions...");
   await LoadSessionsAsync();
      }
  catch (Exception ex)
 {
   Logger.LogError(ex, "Error loading sessions");
        }
  finally
        {
  IsLoading = false;
        }
    }

    private async Task LoadSessionsAsync()
    {
  try
      {
   // Use MediatR query
    var query = new GetActiveSessionsQuery();
    var result = await Mediator.Send(query);

   if (result.IsSuccess)
            {
      Sessions = result.Value!;
      Logger.LogInformation("Loaded {Count} active sessions", Sessions.Count);
      }
    else
      {
       Logger.LogWarning("Failed to load sessions: {Errors}", result.ErrorsAsString);
      }
   }
        catch (Exception ex)
  {
   Logger.LogError(ex, "Error loading sessions from mediator");
   throw;
        }
    }

    private async Task RefreshSessions()
    {
  IsLoading = true;
  StateHasChanged();
 
        try
        {
   await LoadSessionsAsync();
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
  
 Sessions?.Clear();
GridRef = null;
}
}
