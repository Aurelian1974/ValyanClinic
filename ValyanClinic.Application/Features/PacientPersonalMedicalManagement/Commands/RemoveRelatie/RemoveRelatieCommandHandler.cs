using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;

/// <summary>
/// Handler pentru RemoveRelatieCommand.
/// Dezactivează (soft delete) o relație între pacient și personal medical.
/// </summary>
public class RemoveRelatieCommandHandler : IRequestHandler<RemoveRelatieCommand, Result>
{
    private readonly IPacientPersonalMedicalRepository _repository;
    private readonly ILogger<RemoveRelatieCommandHandler> _logger;

    /// <summary>
    /// Constructor pentru RemoveRelatieCommandHandler.
    /// </summary>
    /// <param name="repository">
    /// Repository pentru accesarea relațiilor pacient-personal medical.
    /// </param>
    /// <param name="logger">
    /// Logger pentru înregistrarea operațiilor și erorilor.
    /// </param>
    public RemoveRelatieCommandHandler(
        IPacientPersonalMedicalRepository repository,
        ILogger<RemoveRelatieCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Procesează command-ul pentru dezactivarea unei relații.
    /// </summary>
    /// <param name="request">Command-ul cu ID-ul relației de dezactivat.</param>
    /// <param name="cancellationToken">Token pentru anularea operației.</param>
    /// <returns>
    /// Result care indică succesul sau eșecul operației.
    /// </returns>
    public async Task<Result> Handle(
        RemoveRelatieCommand request,
        CancellationToken cancellationToken)
    {
        // Validation: RelatieID must be specified
        if (!request.RelatieID.HasValue || request.RelatieID.Value == Guid.Empty)
        {
            _logger.LogWarning(
                "[RemoveRelatieCommandHandler] RelatieID was not specified or is empty");

            return Result.Failure("RelatieID este obligatoriu pentru dezactivarea relației.");
        }

        try
        {
            _logger.LogInformation(
                "[RemoveRelatieCommandHandler] Processing command to remove RelatieID={RelatieID}",
                request.RelatieID.Value);

            await _repository.RemoveRelatieAsync(
                request.RelatieID.Value,
                cancellationToken);

            _logger.LogInformation(
                "[RemoveRelatieCommandHandler] Relație dezactivată cu succes: RelatieID={RelatieID}",
                request.RelatieID.Value);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "[RemoveRelatieCommandHandler] Relația nu a fost găsită sau este deja inactivă: RelatieID={RelatieID}",
                request.RelatieID);

            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                "[RemoveRelatieCommandHandler] Invalid arguments: RelatieID={RelatieID}",
                request.RelatieID);

            return Result.Failure($"Parametri invalizi: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[RemoveRelatieCommandHandler] Unexpected error removing RelatieID={RelatieID}",
                request.RelatieID);

            return Result.Failure($"Eroare la dezactivarea relației: {ex.Message}");
        }
    }
}
