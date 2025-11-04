using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PozitieManagement.Commands.DeletePozitie;

public class DeletePozitieCommandHandler : IRequestHandler<DeletePozitieCommand, Result<bool>>
{
    private readonly IPozitieRepository _repository;
    private readonly ILogger<DeletePozitieCommandHandler> _logger;

    public DeletePozitieCommandHandler(
        IPozitieRepository repository,
        ILogger<DeletePozitieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeletePozitieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting Pozitie: {Id}", request.Id);

            var exists = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (exists == null)
            {
                _logger.LogWarning("Pozitie not found: {Id}", request.Id);
                return Result<bool>.Failure(new List<string> { "Pozitia nu a fost gasita" });
            }

            var success = await _repository.DeleteAsync(request.Id, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Pozitie deleted successfully: {Id}", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Failed to delete Pozitie: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { "Stergerea a esuat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Pozitie: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { ex.Message });
        }
    }
}
