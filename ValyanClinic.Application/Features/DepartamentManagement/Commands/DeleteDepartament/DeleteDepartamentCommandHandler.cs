using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.DepartamentManagement.Commands.DeleteDepartament;

public class DeleteDepartamentCommandHandler : IRequestHandler<DeleteDepartamentCommand, Result<bool>>
{
    private readonly IDepartamentRepository _repository;
    private readonly ILogger<DeleteDepartamentCommandHandler> _logger;

    public DeleteDepartamentCommandHandler(
        IDepartamentRepository repository,
        ILogger<DeleteDepartamentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteDepartamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting Departament: {Id}", request.IdDepartament);

            var exists = await _repository.GetByIdAsync(request.IdDepartament, cancellationToken);
            if (exists == null)
            {
                _logger.LogWarning("Departament not found: {Id}", request.IdDepartament);
                return Result<bool>.Failure(new List<string> { "Departamentul nu a fost gasit" });
            }

            var success = await _repository.DeleteAsync(request.IdDepartament, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Departament deleted successfully: {Id}", request.IdDepartament);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Failed to delete Departament: {Id}", request.IdDepartament);
            return Result<bool>.Failure(new List<string> { "Stergerea a esuat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Departament: {Id}", request.IdDepartament);
            return Result<bool>.Failure(new List<string> { ex.Message });
        }
    }
}
