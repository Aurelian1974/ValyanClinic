using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Commands.UpdateUtilizator;

public class UpdateUtilizatorCommandHandler : IRequestHandler<UpdateUtilizatorCommand, Result<bool>>
{
    private readonly IUtilizatorRepository _repository;
    private readonly ILogger<UpdateUtilizatorCommandHandler> _logger;

    public UpdateUtilizatorCommandHandler(
      IUtilizatorRepository repository,
        ILogger<UpdateUtilizatorCommandHandler> logger)
    {
_repository = repository;
        _logger = logger;
    }

  public async Task<Result<bool>> Handle(UpdateUtilizatorCommand request, CancellationToken cancellationToken)
    {
        try
  {
    _logger.LogInformation("Actualizare utilizator: {UtilizatorID}", request.UtilizatorID);

// Get existing utilizator
       var utilizator = await _repository.GetByIdAsync(request.UtilizatorID, cancellationToken);
     if (utilizator == null)
    {
      _logger.LogWarning("Utilizatorul nu a fost gasit: {UtilizatorID}", request.UtilizatorID);
      return Result<bool>.Failure("Utilizatorul nu a fost gasit");
    }

   // Validate username uniqueness (exclude current user)
            var existingByUsername = await _repository.GetByUsernameAsync(request.Username, cancellationToken);
       if (existingByUsername != null && existingByUsername.UtilizatorID != request.UtilizatorID)
          {
            _logger.LogWarning("Username deja existent: {Username}", request.Username);
   return Result<bool>.Failure("Username-ul este deja folosit de alt utilizator");
  }

  // Validate email uniqueness (exclude current user)
       var existingByEmail = await _repository.GetByEmailAsync(request.Email, cancellationToken);
 if (existingByEmail != null && existingByEmail.UtilizatorID != request.UtilizatorID)
{
                _logger.LogWarning("Email deja existent: {Email}", request.Email);
    return Result<bool>.Failure("Email-ul este deja folosit de alt utilizator");
          }

   // Update properties
     utilizator.Username = request.Username;
 utilizator.Email = request.Email;
  utilizator.Rol = request.Rol;
   utilizator.EsteActiv = request.EsteActiv;
   utilizator.ModificatDe = request.ModificatDe;
       utilizator.DataUltimeiModificari = DateTime.Now;

    var success = await _repository.UpdateAsync(utilizator, cancellationToken);

 if (!success)
            {
   _logger.LogError("Eroare la actualizarea utilizatorului in repository");
       return Result<bool>.Failure("Eroare la salvarea modificarilor");
     }

      _logger.LogInformation("Utilizator actualizat cu succes: {UtilizatorID}", request.UtilizatorID);
  return Result<bool>.Success(true, "Utilizator actualizat cu succes");
        }
  catch (Exception ex)
  {
_logger.LogError(ex, "Eroare la actualizarea utilizatorului: {UtilizatorID}", request.UtilizatorID);
            return Result<bool>.Failure($"Eroare: {ex.Message}");
    }
    }
}
