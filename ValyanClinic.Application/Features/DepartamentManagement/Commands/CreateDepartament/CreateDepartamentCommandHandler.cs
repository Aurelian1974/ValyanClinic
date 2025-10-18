using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.DepartamentManagement.Commands.CreateDepartament;

public class CreateDepartamentCommandHandler : IRequestHandler<CreateDepartamentCommand, Result<Guid>>
{
    private readonly IDepartamentRepository _repository;
    private readonly ILogger<CreateDepartamentCommandHandler> _logger;

    public CreateDepartamentCommandHandler(
        IDepartamentRepository repository,
        ILogger<CreateDepartamentCommandHandler> _logger)
    {
        _repository = repository;
        this._logger = _logger;
    }

    public async Task<Result<Guid>> Handle(CreateDepartamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating departament: {Denumire}", request.DenumireDepartament);

            // Check uniqueness
            var exists = await _repository.CheckUniqueAsync(request.DenumireDepartament, null, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Departament already exists: {Denumire}", request.DenumireDepartament);
                return Result<Guid>.Failure("Exista deja un departament cu aceasta denumire");
            }

            var departament = new Departament
            {
                IdTipDepartament = request.IdTipDepartament,
                DenumireDepartament = request.DenumireDepartament,
                DescriereDepartament = request.DescriereDepartament
            };

            var id = await _repository.CreateAsync(departament, cancellationToken);

            if (id != Guid.Empty)
            {
                _logger.LogInformation("Departament created successfully: {Id}", id);
                return Result<Guid>.Success(id, "Departament creat cu succes");
            }

            _logger.LogWarning("Failed to create departament");
            return Result<Guid>.Failure("Crearea departamentului a esuat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating departament");
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
