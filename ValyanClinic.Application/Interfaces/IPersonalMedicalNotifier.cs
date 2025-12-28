using System;
using System.Threading;
using System.Threading.Tasks;

namespace ValyanClinic.Application.Interfaces
{
    public interface IPersonalMedicalNotifier
    {
        Task NotifyPersonalChangedAsync(string action, Guid personalId, CancellationToken cancellationToken = default);
    }
}