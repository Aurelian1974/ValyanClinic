using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.ActivateRelatie;

public class ActivateRelatieCommandHandler : IRequestHandler<ActivateRelatieCommand, Result>
{
    private readonly IPacientPersonalMedicalRepository _relatieRepository;
    private readonly ILogger<ActivateRelatieCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ActivateRelatieCommandHandler"/> with required dependencies.
    /// </summary>
    /// <param name="relatieRepository">Repository used to activate and manage doctor–patient relations.</param>
    public ActivateRelatieCommandHandler(
        IPacientPersonalMedicalRepository relatieRepository,
        ILogger<ActivateRelatieCommandHandler> logger)
    {
        _relatieRepository = relatieRepository;
        _logger = logger;
    }

    /// <summary>
    /// Reactivates a doctor–patient relation using the data provided in the command.
    /// </summary>
    /// <param name="request">Command containing the relation identifier and activation details (Observatii, Motiv, ModificatDe).</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A Result indicating operation outcome: on success contains a confirmation message, on failure contains an error message.</returns>
    public async Task<Result> Handle(ActivateRelatieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Reactivare relație doctor-pacient: RelatieID={RelatieID}",
                request.RelatieID);

            await _relatieRepository.ActivateRelatieAsync(
                request.RelatieID,
                request.Observatii,
                request.Motiv,
                request.ModificatDe,
                cancellationToken);

            _logger.LogInformation(
                "Relație reactivată cu succes: RelatieID={RelatieID}",
                request.RelatieID);

            return Result.Success("Relația doctor-pacient a fost reactivată cu succes.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Eroare de operație la reactivarea relației: RelatieID={RelatieID}", request.RelatieID);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare neașteptată la reactivarea relației: RelatieID={RelatieID}", request.RelatieID);
            return Result.Failure($"Eroare la reactivarea relației: {ex.Message}");
        }
    }
}