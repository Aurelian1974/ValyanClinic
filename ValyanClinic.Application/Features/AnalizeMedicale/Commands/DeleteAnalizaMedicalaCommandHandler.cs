using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands;

/// <summary>
/// Handler pentru ștergerea unei analize medicale
/// </summary>
public class DeleteAnalizaMedicalaCommandHandler
    : IRequestHandler<DeleteAnalizaMedicalaCommand, Result<bool>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _analizaRepository;

    public DeleteAnalizaMedicalaCommandHandler(IConsultatieAnalizaMedicalaRepository analizaRepository)
    {
        _analizaRepository = analizaRepository;
    }

    public async Task<Result<bool>> Handle(
        DeleteAnalizaMedicalaCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _analizaRepository.DeleteAsync(request.Id, cancellationToken);

            return deleted
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Analiza nu a fost găsită");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Eroare la ștergerea analizei: {ex.Message}");
        }
    }
}
