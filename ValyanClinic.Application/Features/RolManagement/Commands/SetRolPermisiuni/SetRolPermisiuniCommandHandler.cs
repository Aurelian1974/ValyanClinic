using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Commands.SetRolPermisiuni;

/// <summary>
/// Handler pentru SetRolPermisiuniCommand.
/// </summary>
public class SetRolPermisiuniCommandHandler : IRequestHandler<SetRolPermisiuniCommand, Result>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<SetRolPermisiuniCommandHandler> _logger;

    public SetRolPermisiuniCommandHandler(
        IRolRepository repository,
        ILogger<SetRolPermisiuniCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SetRolPermisiuniCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("SetRolPermisiuni: {RolId} with {Count} permisiuni", request.RolId, request.Permisiuni.Count);

            // Verifică dacă rolul există
            var existingRol = await _repository.GetByIdAsync(request.RolId, cancellationToken);
            if (existingRol == null)
            {
                return Result.Failure($"Rolul cu ID {request.RolId} nu a fost găsit.");
            }

            // Setează permisiunile
            var success = await _repository.SetPermisiuniForRolAsync(
                request.RolId, 
                request.Permisiuni, 
                request.ModificatDe, 
                cancellationToken);

            if (!success)
            {
                return Result.Failure("Nu s-au putut actualiza permisiunile.");
            }

            _logger.LogInformation("SetRolPermisiuni SUCCESS: {RolId}", request.RolId);

            return Result.Success("Permisiunile au fost actualizate cu succes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting permisiuni for Rol: {RolId}", request.RolId);
            return Result.Failure(ex.Message);
        }
    }
}
