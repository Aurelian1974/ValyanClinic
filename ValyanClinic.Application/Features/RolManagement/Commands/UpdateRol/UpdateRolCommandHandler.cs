using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Commands.UpdateRol;

/// <summary>
/// Handler pentru UpdateRolCommand.
/// </summary>
public class UpdateRolCommandHandler : IRequestHandler<UpdateRolCommand, Result>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<UpdateRolCommandHandler> _logger;

    public UpdateRolCommandHandler(
        IRolRepository repository,
        ILogger<UpdateRolCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateRolCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("UpdateRol: {Id}", request.Id);

            // Verifică dacă rolul există
            var existingRol = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (existingRol == null)
            {
                return Result.Failure($"Rolul cu ID {request.Id} nu a fost găsit.");
            }

            // Verifică dacă este rol de sistem
            if (existingRol.EsteSistem)
            {
                // Pentru roluri de sistem, permitem doar actualizarea permisiunilor
                await _repository.SetPermisiuniForRolAsync(request.Id, request.Permisiuni, request.ModificatDe, cancellationToken);
                _logger.LogInformation("UpdateRol (sistem - doar permisiuni): {Id}", request.Id);
                return Result.Success("Permisiunile rolului de sistem au fost actualizate.");
            }

            // Verifică unicitatea denumirii
            var isUnique = await _repository.CheckUniqueAsync(request.Denumire, request.Id, cancellationToken);
            if (!isUnique)
            {
                return Result.Failure($"Există deja un rol cu denumirea '{request.Denumire}'.");
            }

            // Actualizează entitatea
            existingRol.Denumire = request.Denumire;
            existingRol.Descriere = request.Descriere;
            existingRol.EsteActiv = request.EsteActiv;
            existingRol.OrdineAfisare = request.OrdineAfisare;
            existingRol.ModificatDe = request.ModificatDe;
            existingRol.DataUltimeiModificari = DateTime.UtcNow;

            // Salvează modificările
            var success = await _repository.UpdateAsync(existingRol, cancellationToken);
            if (!success)
            {
                return Result.Failure("Nu s-a putut actualiza rolul.");
            }

            // Actualizează permisiunile
            await _repository.SetPermisiuniForRolAsync(request.Id, request.Permisiuni, request.ModificatDe, cancellationToken);

            _logger.LogInformation("UpdateRol SUCCESS: {Id}", request.Id);

            return Result.Success("Rolul a fost actualizat cu succes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Rol: {Id}", request.Id);
            return Result.Failure(ex.Message);
        }
    }
}
