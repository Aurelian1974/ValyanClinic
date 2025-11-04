using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentById;

public class GetDepartamentByIdQueryHandler : IRequestHandler<GetDepartamentByIdQuery, Result<DepartamentDetailDto>>
{
    private readonly IDepartamentRepository _repository;
    private readonly ILogger<GetDepartamentByIdQueryHandler> _logger;

    public GetDepartamentByIdQueryHandler(
        IDepartamentRepository repository,
        ILogger<GetDepartamentByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<DepartamentDetailDto>> Handle(GetDepartamentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting departament by ID: {Id}", request.IdDepartament);

            var departament = await _repository.GetByIdAsync(request.IdDepartament, cancellationToken);

            if (departament == null)
            {
                _logger.LogWarning("Departament not found: {Id}", request.IdDepartament);
                return Result<DepartamentDetailDto>.Failure("Departamentul nu a fost gasit");
            }

            var dto = new DepartamentDetailDto
            {
                IdDepartament = departament.IdDepartament,
                IdTipDepartament = departament.IdTipDepartament,
                DenumireDepartament = departament.DenumireDepartament,
                DescriereDepartament = departament.DescriereDepartament,
                DenumireTipDepartament = departament.TipDepartament?.DenumireTipDepartament
            };

            _logger.LogInformation("Departament retrieved successfully: {Id}", request.IdDepartament);
            return Result<DepartamentDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departament by ID: {Id}", request.IdDepartament);
            return Result<DepartamentDetailDto>.Failure(ex.Message);
        }
    }
}
