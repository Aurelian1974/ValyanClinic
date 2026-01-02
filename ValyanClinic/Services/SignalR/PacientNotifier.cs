using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Hubs;

namespace ValyanClinic.Services.SignalR;

public class PacientNotifier : IPacientNotifier
{
    private readonly IHubContext<PacientHub> _hubContext;

    public PacientNotifier(IHubContext<PacientHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyPacientChangedAsync(string action, Guid pacientId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("PacientChanged", action, pacientId, cancellationToken);
        }
        catch
        {
            // Swallow exceptions - notifier should not break business flow
        }
    }
}
