using System;
using System.Threading;
using System.Threading.Tasks;

namespace ValyanClinic.Application.Interfaces;

public interface IPacientNotifier
{
    Task NotifyPacientChangedAsync(string action, Guid pacientId, CancellationToken cancellationToken = default);
}
