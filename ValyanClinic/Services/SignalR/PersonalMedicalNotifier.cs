using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Hubs;

namespace ValyanClinic.Services.SignalR
{
    public class PersonalMedicalNotifier : IPersonalMedicalNotifier
    {
        private readonly IHubContext<PersonalMedicalHub> _hubContext;

        public PersonalMedicalNotifier(IHubContext<PersonalMedicalHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyPersonalChangedAsync(string action, Guid personalId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("PersonalChanged", action, personalId, cancellationToken);
            }
            catch
            {
                // Swallow exceptions - notifier should not break business flow
            }
        }
    }
}