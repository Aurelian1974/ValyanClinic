using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.UpdateAnalizaRezultate;

/// <summary>
/// Handler pentru actualizare rezultate analiză
/// </summary>
public class UpdateAnalizaRezultateCommandHandler 
    : IRequestHandler<UpdateAnalizaRezultateCommand, Result<bool>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _repository;
    private readonly ILogger<UpdateAnalizaRezultateCommandHandler> _logger;

    public UpdateAnalizaRezultateCommandHandler(
        IConsultatieAnalizaMedicalaRepository repository,
        ILogger<UpdateAnalizaRezultateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateAnalizaRezultateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Updating rezultate for analiză {AnalizaId}",
                request.AnalizaId);

            // Get existing analiza
            var analiza = await _repository.GetByIdAsync(request.AnalizaId, cancellationToken);
            
            if (analiza == null)
            {
                return Result<bool>.Failure("Analiza nu există.");
            }

            // Update with results
            analiza.AreRezultate = true;
            analiza.DataEfectuare = request.DataEfectuare;
            analiza.LocEfectuare = request.NumeLaborator; // Mapare corectă
            analiza.InterpretareMedic = request.InterpretareMedic;
            analiza.DataUltimeiModificari = DateTime.Now;
            analiza.ModificatDe = request.ModificatDe;

            // Update status to Finalizata
            await _repository.UpdateStatusAsync(
                request.AnalizaId, 
                "Finalizata", 
                cancellationToken);

            var success = await _repository.UpdateAsync(analiza, cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Rezultate updated successfully for analiză {AnalizaId}",
                    request.AnalizaId);
            }

            return Result<bool>.Success(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error updating rezultate for analiză {AnalizaId}",
                request.AnalizaId);
            
            return Result<bool>.Failure($"Eroare la actualizare rezultate: {ex.Message}");
        }
    }
}
