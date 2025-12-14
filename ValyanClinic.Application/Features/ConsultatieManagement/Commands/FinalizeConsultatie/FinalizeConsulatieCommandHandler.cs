using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.FinalizeConsultatie;

/// <summary>
/// Handler pentru FinalizeConsulatieCommand
/// Finalizeaza consultatia cu validari (campuri obligatorii) si update status programare
/// Foloseste sp_Consultatie_Finalize cu transaction
/// </summary>
public class FinalizeConsulatieCommandHandler : IRequestHandler<FinalizeConsulatieCommand, Result<bool>>
{
    private readonly IConsultatieRepository _repository;
    private readonly ILogger<FinalizeConsulatieCommandHandler> _logger;

    public FinalizeConsulatieCommandHandler(
        IConsultatieRepository repository,
        ILogger<FinalizeConsulatieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        FinalizeConsulatieCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[FinalizeConsulatieHandler] Finalizing consultatie ID: {ConsultatieID}, Duration: {DurataMinute} minutes",
                request.ConsultatieID, request.DurataMinute);

            // Validare
            if (request.ConsultatieID == Guid.Empty)
                return Result<bool>.Failure("ConsultatieID este obligatoriu");

            if (request.ModificatDe == Guid.Empty)
                return Result<bool>.Failure("ModificatDe este obligatoriu");

            if (request.DurataMinute < 0)
                return Result<bool>.Failure("DurataMinute nu poate fi negativă");

            // Call repository (executes sp_Consultatie_Finalize with validations and transaction)
            // SP validates: MotivPrezentare și DiagnosticPozitiv sunt obligatorii
            // SP updates: Status = 'Finalizata', DataFinalizare = GETDATE(), DurataMinute
            // SP also updates: Programari.Status = 'Finalizata'
            var success = await _repository.FinalizeAsync(
                request.ConsultatieID,
                request.DurataMinute,
                request.ModificatDe,
                cancellationToken);

            if (!success)
            {
                _logger.LogWarning(
                    "[FinalizeConsulatieHandler] Failed to finalize consultatie ID: {ConsultatieID}. Possible reasons: consultatie not found, already finalized, or missing required fields (MotivPrezentare/DiagnosticPozitiv)",
                    request.ConsultatieID);

                return Result<bool>.Failure(
                    "Eroare la finalizarea consultatiei. Verificați că toate câmpurile obligatorii (Motivul prezentării și Diagnosticul) sunt completate.");
            }

            _logger.LogInformation(
                "[FinalizeConsulatieHandler] Consultatie finalized successfully: {ConsultatieID}, Duration: {DurataMinute} minutes",
                request.ConsultatieID, request.DurataMinute);

            return Result<bool>.Success(
                true,
                $"Consultatie finalizată cu succes (durată: {request.DurataMinute} minute)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[FinalizeConsulatieHandler] Error finalizing consultatie ID: {ConsultatieID}",
                request.ConsultatieID);

            return Result<bool>.Failure($"Eroare la finalizarea consultatiei: {ex.Message}");
        }
    }
}
