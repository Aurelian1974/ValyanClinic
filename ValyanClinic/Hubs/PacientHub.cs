using Microsoft.AspNetCore.SignalR;

namespace ValyanClinic.Hubs;

public class PacientHub : Hub
{
    // Intentionally minimal - server broadcasts "PacientChanged" events with (action, pacientId)
}
