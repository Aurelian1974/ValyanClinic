using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Commands.DeleteRol;

/// <summary>
/// Handler pentru DeleteRolCommand.
/// </summary>
public class DeleteRolCommandHandler : IRequestHandler<DeleteRolCommand, Result>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<DeleteRolCommandHandler> _logger;

    public DeleteRolCommandHandler(
        IRolRepository repository,
        ILogger<DeleteRolCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteRolCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("DeleteRol: {Id}", request.Id);

            // Verifică dacă rolul există
            var existingRol = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (existingRol == null)
            {
                return Result.Failure($"Rolul cu ID {request.Id} nu a fost găsit.");
            }

            // Verifică dacă este rol de sistem
            if (existingRol.EsteSistem)
            {
                return Result.Failure("Nu se poate șterge un rol de sistem.");
            }

            // TODO: Verifică dacă există utilizatori cu acest rol
            // var utilizatoriCount = await _utilizatorRepository.GetCountByRolAsync(existingRol.Denumire, cancellationToken);
            // if (utilizatoriCount > 0)
            // {
            //     return Result.Failure($"Nu se poate șterge rolul. Există {utilizatoriCount} utilizatori cu acest rol.");
            // }

            // Șterge rolul
            var success = await _repository.DeleteAsync(request.Id, cancellationToken);
            if (!success)
            {
                return Result.Failure("Nu s-a putut șterge rolul.");
            }

            _logger.LogInformation("DeleteRol SUCCESS: {Id}", request.Id);

            return Result.Success("Rolul a fost șters cu succes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Rol: {Id}", request.Id);
            return Result.Failure(ex.Message);
        }
    }
}
