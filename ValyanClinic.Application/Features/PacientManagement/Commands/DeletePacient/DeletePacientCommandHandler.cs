using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Exceptions;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientManagement.Commands.DeletePacient;

/// <summary>
/// Handler pentru DeletePacientCommand
/// </summary>
public class DeletePacientCommandHandler : IRequestHandler<DeletePacientCommand, Result>
{
    private readonly IPacientRepository _pacientRepository;
    private readonly ILogger<DeletePacientCommandHandler> _logger;

    public DeletePacientCommandHandler(
        IPacientRepository pacientRepository,
        ILogger<DeletePacientCommandHandler> logger)
    {
        _pacientRepository = pacientRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePacientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========== DeletePacientCommandHandler START ==========");
        _logger.LogInformation("Deleting pacient ID: {Id}, HardDelete: {HardDelete}",
            request.Id, request.HardDelete);

        try
        {
            // 1. Verificare existență pacient
            var existingPacient = await _pacientRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingPacient == null)
            {
                _logger.LogWarning("Pacient not found: {Id}", request.Id);
                throw new NotFoundException($"Pacientul cu ID-ul {request.Id} nu a fost găsit.");
            }

            _logger.LogInformation("Found existing pacient: {Nume} {Prenume}",
                existingPacient.Nume, existingPacient.Prenume);

            bool success;
            string message;

            if (request.HardDelete)
            {
                // 2a. Stergere fizică (PERICULOS - folosit doar în cazuri speciale)
                _logger.LogWarning("Performing HARD DELETE for pacient: {Id}", request.Id);
                success = await _pacientRepository.HardDeleteAsync(request.Id, cancellationToken);
                message = success
                    ? $"Pacientul {existingPacient.NumeComplet} a fost șters definitiv din sistem."
                    : "Eroare la ștergerea pacientului.";
            }
            else
            {
                // 2b. Soft delete (recomandat - doar marchează ca inactiv)
                _logger.LogInformation("Performing SOFT DELETE for pacient: {Id}", request.Id);
                success = await _pacientRepository.DeleteAsync(request.Id, request.ModificatDe, cancellationToken);
                message = success
                    ? $"Pacientul {existingPacient.NumeComplet} a fost dezactivat cu succes."
                    : "Eroare la dezactivarea pacientului.";
            }

            if (!success)
            {
                _logger.LogError("Delete operation failed for pacient: {Id}", request.Id);
                return Result.Failure("Operațiunea de ștergere a eșuat.");
            }

            _logger.LogInformation("Pacient deleted successfully: {Id}", request.Id);
            _logger.LogInformation("========== DeletePacientCommandHandler END (SUCCESS) ==========");

            return Result.Success(message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pacient not found");
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "========== DeletePacientCommandHandler EXCEPTION ==========");
            _logger.LogError("Exception Type: {Type}", ex.GetType().FullName);
            _logger.LogError("Exception Message: {Message}", ex.Message);

            return Result.Failure($"Eroare la ștergerea pacientului: {ex.Message}");
        }
    }
}
