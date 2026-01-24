using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaEfectuata;

/// <summary>
/// Handler pentru ștergerea unei analize efectuate
/// </summary>
public class DeleteAnalizaEfectuataCommandHandler
    : IRequestHandler<DeleteAnalizaEfectuataCommand, Result<bool>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _analizaRepository;
    private readonly ILogger<DeleteAnalizaEfectuataCommandHandler> _logger;

    public DeleteAnalizaEfectuataCommandHandler(
        IConsultatieAnalizaMedicalaRepository analizaRepository,
        ILogger<DeleteAnalizaEfectuataCommandHandler> logger)
    {
        _analizaRepository = analizaRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteAnalizaEfectuataCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting analiză efectuată {AnalizaId}", request.Id);

            var deleted = await _analizaRepository.DeleteAsync(request.Id, cancellationToken);

            if (deleted)
            {
                _logger.LogInformation("Successfully deleted analiză efectuată {AnalizaId}", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Analiză efectuată not found: {AnalizaId}", request.Id);
            return Result<bool>.Failure("Analiza efectuată nu a fost găsită");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analiză efectuată {AnalizaId}", request.Id);
            return Result<bool>.Failure($"Eroare la ștergerea analizei: {ex.Message}");
        }
    }
}
