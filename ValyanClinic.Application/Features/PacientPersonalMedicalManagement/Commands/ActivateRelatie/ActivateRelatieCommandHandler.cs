using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.ActivateRelatie;

public class ActivateRelatieCommandHandler : IRequestHandler<ActivateRelatieCommand, Result>
{
    private readonly IPacientPersonalMedicalRepository _relatieRepository;
    private readonly ILogger<ActivateRelatieCommandHandler> _logger;

    public ActivateRelatieCommandHandler(
        IPacientPersonalMedicalRepository relatieRepository,
        ILogger<ActivateRelatieCommandHandler> logger)
    {
        _relatieRepository = relatieRepository;
        _logger = logger;
    }

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
