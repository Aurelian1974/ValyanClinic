using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.DepartamentManagement.Commands.UpdateDepartament;

public class UpdateDepartamentCommandHandler : IRequestHandler<UpdateDepartamentCommand, Result<bool>>
{
    private readonly IDepartamentRepository _repository;
    private readonly ILogger<UpdateDepartamentCommandHandler> _logger;

    public UpdateDepartamentCommandHandler(
        IDepartamentRepository repository,
        ILogger<UpdateDepartamentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateDepartamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating departament: {Id}", request.IdDepartament);

            // Check if exists
            var existing = await _repository.GetByIdAsync(request.IdDepartament, cancellationToken);
            if (existing == null)
            {
                _logger.LogWarning("Departament not found: {Id}", request.IdDepartament);
                return Result<bool>.Failure("Departamentul nu a fost gasit");
            }

            // Check uniqueness (exclude current)
            var nameExists = await _repository.CheckUniqueAsync(
                request.DenumireDepartament, 
                request.IdDepartament, 
                cancellationToken);
                
            if (nameExists)
            {
                _logger.LogWarning("Departament name already exists: {Denumire}", request.DenumireDepartament);
                return Result<bool>.Failure("Exista deja un departament cu aceasta denumire");
            }

            var departament = new Departament
            {
                IdDepartament = request.IdDepartament,
                IdTipDepartament = request.IdTipDepartament,
                DenumireDepartament = request.DenumireDepartament,
                DescriereDepartament = request.DescriereDepartament
            };

            var success = await _repository.UpdateAsync(departament, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Departament updated successfully: {Id}", request.IdDepartament);
                return Result<bool>.Success(true, "Departament actualizat cu succes");
            }

            _logger.LogWarning("Failed to update departament: {Id}", request.IdDepartament);
            return Result<bool>.Failure("Actualizarea departamentului a esuat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating departament: {Id}", request.IdDepartament);
            return Result<bool>.Failure(ex.Message);
        }
    }
}
