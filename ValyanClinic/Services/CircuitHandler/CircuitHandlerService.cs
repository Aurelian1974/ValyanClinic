using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using MsCircuit = Microsoft.AspNetCore.Components.Server.Circuits.Circuit;

namespace ValyanClinic.Services.Blazor;

/// <summary>
/// Service pentru gestionarea lifecycle-ului circuitului Blazor Server
/// Previne pierderea conexiunii la baza de date după actualizări
/// </summary>
public class ValyanCircuitHandler : CircuitHandler
{
    private readonly ILogger<ValyanCircuitHandler> _logger;

    public ValyanCircuitHandler(ILogger<ValyanCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectionUpAsync(MsCircuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId}: Conexiune stabilită", circuit.Id);
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(MsCircuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Circuit {CircuitId}: Conexiune pierdută", circuit.Id);
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitOpenedAsync(MsCircuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId}: Deschis", circuit.Id);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(MsCircuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId}: Închis", circuit.Id);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
