using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Commands.CreateRol;

/// <summary>
/// Handler pentru CreateRolCommand.
/// </summary>
public class CreateRolCommandHandler : IRequestHandler<CreateRolCommand, Result<Guid>>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<CreateRolCommandHandler> _logger;

    public CreateRolCommandHandler(
        IRolRepository repository,
        ILogger<CreateRolCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateRolCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("CreateRol: {Denumire}", request.Denumire);

            // Verifică unicitatea denumirii
            var isUnique = await _repository.CheckUniqueAsync(request.Denumire, null, cancellationToken);
            if (!isUnique)
            {
                return Result<Guid>.Failure($"Există deja un rol cu denumirea '{request.Denumire}'.");
            }

            // Creează entitatea
            var rol = new Rol
            {
                Denumire = request.Denumire,
                Descriere = request.Descriere,
                EsteActiv = request.EsteActiv,
                OrdineAfisare = request.OrdineAfisare,
                CreatDe = request.CreatDe,
                DataCrearii = DateTime.UtcNow,
                DataUltimeiModificari = DateTime.UtcNow
            };

            // Salvează rolul
            var rolId = await _repository.CreateAsync(rol, cancellationToken);

            // Setează permisiunile dacă există
            if (request.Permisiuni.Any())
            {
                await _repository.SetPermisiuniForRolAsync(rolId, request.Permisiuni, request.CreatDe, cancellationToken);
            }

            _logger.LogInformation("CreateRol SUCCESS: {Id}", rolId);

            return Result<Guid>.Success(rolId, "Rolul a fost creat cu succes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Rol: {Denumire}", request.Denumire);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
