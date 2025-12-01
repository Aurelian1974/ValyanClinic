using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.DeleteProgramare;

/// <summary>
/// Handler pentru comanda DeleteProgramareCommand.
/// Gestionează logica de business pentru ștergerea (anularea) unei programări.
/// </summary>
public class DeleteProgramareCommandHandler : IRequestHandler<DeleteProgramareCommand, Result<bool>>
{
    private readonly IProgramareRepository _programareRepository;
    private readonly ILogger<DeleteProgramareCommandHandler> _logger;

    public DeleteProgramareCommandHandler(
        IProgramareRepository programareRepository,
    ILogger<DeleteProgramareCommandHandler> logger)
    {
        _programareRepository = programareRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
  DeleteProgramareCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                 "Ștergere (anulare) programare: {ProgramareID} de către {ModificatDe}",
               request.ProgramareID, request.ModificatDe);

            // ==================== VALIDĂRI BUSINESS ====================

            // 1. Verificare existență programare
            var existingProgramare = await _programareRepository.GetByIdAsync(request.ProgramareID, cancellationToken);
            if (existingProgramare == null)
            {
                _logger.LogWarning("Programarea {ProgramareID} nu există", request.ProgramareID);
                return Result<bool>.Failure("Programarea specificată nu există în sistem.");
            }

            // 2. Verificare dacă programarea poate fi anulată
            // Doar programările cu statusurile: Programata, Confirmata, CheckedIn pot fi anulate
            if (existingProgramare.Status is not ("Programata" or "Confirmata" or "CheckedIn"))
            {
                _logger.LogWarning(
                      "Programarea {ProgramareID} cu status {Status} nu poate fi anulată",
                       request.ProgramareID, existingProgramare.Status);

                return Result<bool>.Failure(
                $"Programarea cu statusul '{existingProgramare.Status}' nu mai poate fi anulată. " +
               $"Doar programările cu statusul 'Programată', 'Confirmată' sau 'CheckedIn' pot fi anulate.");
            }

            // 3. Verificare dacă programarea este în trecut (opțional - warning)
            if (existingProgramare.EsteTrecuta)
            {
                _logger.LogWarning(
             "Anulare programare din trecut: {ProgramareID}, Data={Data}",
             request.ProgramareID, existingProgramare.DataProgramare);
                // Permitem anularea, dar logăm warning
            }

            // 4. Verificare dacă programarea este în desfășurare (opțional - warning)
            if (existingProgramare.EsteInDesfasurare)
            {
                _logger.LogWarning(
                       "Anulare programare în desfășurare: {ProgramareID}",
             request.ProgramareID);
                // Permitem anularea, dar logăm warning
            }

            // ==================== SOFT DELETE (ANULARE) ====================

            // Apelăm metoda DeleteAsync care face soft delete (marchează ca Anulata)
            var success = await _programareRepository.DeleteAsync(
               request.ProgramareID,
                request.ModificatDe,
                 cancellationToken);

            if (!success)
            {
                _logger.LogError("Ștergerea programării {ProgramareID} a eșuat", request.ProgramareID);
                return Result<bool>.Failure("Ștergerea programării a eșuat. Vă rugăm să încercați din nou.");
            }

            // ==================== LOGGING & AUDIT ====================

            _logger.LogInformation(
             "Programare anulată cu succes: {ProgramareID}, Pacient={Pacient}, Doctor={Doctor}, Data={Data}, Motiv={Motiv}",
              request.ProgramareID,
                   existingProgramare.PacientNumeComplet,
               existingProgramare.DoctorNumeComplet,
                      existingProgramare.DataProgramare.ToString("yyyy-MM-dd"),
            request.MotivAnulare ?? "Fără motiv specificat");

            // TODO: Aici se poate adăuga trimitere notificare către pacient și medic (email/SMS)
            // TODO: Aici se poate adăuga logare în AuditLog pentru compliance

            return Result<bool>.Success(
           true,
                $"Programarea pentru {existingProgramare.PacientNumeComplet} cu Dr. {existingProgramare.DoctorNumeComplet} " +
    $"din data de {existingProgramare.DataProgramare:dd.MM.yyyy} a fost anulată cu succes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la ștergerea programării {ProgramareID}", request.ProgramareID);
            return Result<bool>.Failure($"Eroare la ștergerea programării: {ex.Message}");
        }
    }
}
