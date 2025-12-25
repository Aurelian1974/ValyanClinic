using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Queries.GetRolById;

/// <summary>
/// Handler pentru GetRolByIdQuery.
/// </summary>
public class GetRolByIdQueryHandler : IRequestHandler<GetRolByIdQuery, Result<RolDetailDto>>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<GetRolByIdQueryHandler> _logger;

    public GetRolByIdQueryHandler(
        IRolRepository repository,
        ILogger<GetRolByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<RolDetailDto>> Handle(
        GetRolByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GetRolById: {Id}", request.Id);

            var rol = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (rol == null)
            {
                return Result<RolDetailDto>.Failure($"Rolul cu ID {request.Id} nu a fost găsit.");
            }

            var permisiuni = await _repository.GetPermisiuniForRolAsync(request.Id, cancellationToken);

            var dto = new RolDetailDto
            {
                Id = rol.RolID,
                Denumire = rol.Denumire,
                Descriere = rol.Descriere,
                EsteActiv = rol.EsteActiv,
                EsteSistem = rol.EsteSistem,
                OrdineAfisare = rol.OrdineAfisare,
                DataCrearii = rol.DataCrearii,
                DataUltimeiModificari = rol.DataUltimeiModificari,
                CreatDe = rol.CreatDe,
                ModificatDe = rol.ModificatDe,
                Permisiuni = permisiuni.ToList()
            };

            _logger.LogInformation("GetRolById SUCCESS: {Denumire}", dto.Denumire);

            return Result<RolDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRolByIdQueryHandler: {Id}", request.Id);
            return Result<RolDetailDto>.Failure(ex.Message);
        }
    }
}
