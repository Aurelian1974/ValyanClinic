using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalManagement.Commands.DeletePersonal;

/// <summary>
/// Handler pentru DeletePersonalCommand - ALINIAT CU DB REALA
/// </summary>
public class DeletePersonalCommandHandler : IRequestHandler<DeletePersonalCommand, Result<bool>>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<DeletePersonalCommandHandler> _logger;

    public DeletePersonalCommandHandler(
        IPersonalRepository personalRepository,
        ILogger<DeletePersonalCommandHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeletePersonalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stergere personal: {PersonalId}", request.Id);

            // Verificam daca personalul exista
            var existingPersonal = await _personalRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingPersonal == null)
            {
                _logger.LogWarning("Personalul cu ID-ul {PersonalId} nu a fost gasit", request.Id);
                return Result<bool>.Failure($"Angajatul cu ID-ul {request.Id} nu a fost gasit");
            }

            // Stergem Personal (soft delete in SP)
            var deleted = await _personalRepository.DeleteAsync(request.Id, request.StersDe, cancellationToken);
            if (!deleted)
            {
                _logger.LogError("Eroare la stergerea personalului {PersonalId}", request.Id);
                return Result<bool>.Failure("Eroare la stergerea angajatului");
            }

            _logger.LogInformation("Personal sters cu succes: {PersonalId}", request.Id);

            return Result<bool>.Success(true, "Angajatul a fost sters cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la stergerea personalului: {PersonalId}", request.Id);
            return Result<bool>.Failure($"Eroare la stergerea angajatului: {ex.Message}");
        }
    }
}
