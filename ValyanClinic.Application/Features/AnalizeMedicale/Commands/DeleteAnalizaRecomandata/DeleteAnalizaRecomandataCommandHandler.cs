using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaRecomandata;

/// <summary>
/// Handler pentru ștergere analiză recomandată
/// </summary>
public class DeleteAnalizaRecomandataCommandHandler 
    : IRequestHandler<DeleteAnalizaRecomandataCommand, Result<bool>>
{
    private readonly IConsultatieAnalizaRecomandataRepository _repository;
    private readonly ILogger<DeleteAnalizaRecomandataCommandHandler> _logger;

    public DeleteAnalizaRecomandataCommandHandler(
        IConsultatieAnalizaRecomandataRepository repository,
        ILogger<DeleteAnalizaRecomandataCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteAnalizaRecomandataCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting analiză recomandată {AnalizaId}", request.AnalizaId);

            var success = await _repository.DeleteAsync(request.AnalizaId, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Analiză recomandată {AnalizaId} deleted successfully", request.AnalizaId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogWarning("Analiză recomandată {AnalizaId} not found for deletion", request.AnalizaId);
                return Result<bool>.Failure("Analiza nu a fost găsită.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analiză recomandată {AnalizaId}", request.AnalizaId);
            return Result<bool>.Failure($"Eroare la ștergere: {ex.Message}");
        }
    }
}
