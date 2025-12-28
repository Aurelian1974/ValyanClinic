using Microsoft.AspNetCore.SignalR;

namespace ValyanClinic.Hubs;

public class PersonalMedicalHub : Hub
{
    // Intentionally minimal - server broadcasts "PersonalChanged" events with (action, personalId)
}
