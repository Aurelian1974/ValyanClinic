using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.SpecializareManagement.Commands.DeleteSpecializare;

public class DeleteSpecializareCommandHandler : IRequestHandler<DeleteSpecializareCommand, Result<bool>>
{
    private readonly ISpecializareRepository _repository;
    private readonly ILogger<DeleteSpecializareCommandHandler> _logger;

    public DeleteSpecializareCommandHandler(
        ISpecializareRepository repository,
        ILogger<DeleteSpecializareCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteSpecializareCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting Specializare: {Id}", request.Id);

            var exists = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (exists == null)
            {
                _logger.LogWarning("Specializare not found: {Id}", request.Id);
                return Result<bool>.Failure(new List<string> { "Specializarea nu a fost gasita" });
            }

            var success = await _repository.DeleteAsync(request.Id, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Specializare deleted successfully: {Id}", request.Id);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Failed to delete Specializare: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { "Stergerea a esuat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Specializare: {Id}", request.Id);
            return Result<bool>.Failure(new List<string> { ex.Message });
        }
    }
}
