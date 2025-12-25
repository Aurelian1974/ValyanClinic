using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Queries.GetRolList;

/// <summary>
/// Handler pentru GetRolListQuery.
/// </summary>
public class GetRolListQueryHandler : IRequestHandler<GetRolListQuery, PagedResult<RolListDto>>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<GetRolListQueryHandler> _logger;

    public GetRolListQueryHandler(
        IRolRepository repository,
        ILogger<GetRolListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<RolListDto>> Handle(
        GetRolListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "GetRolList: Page={Page}, Size={Size}, Search={Search}, EsteActiv={EsteActiv}",
                request.PageNumber, request.PageSize, request.GlobalSearchText, request.EsteActiv);

            var data = await _repository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.GlobalSearchText,
                request.EsteActiv,
                request.SortColumn,
                request.SortDirection,
                cancellationToken);

            var totalCount = await _repository.GetCountAsync(
                request.GlobalSearchText,
                request.EsteActiv,
                cancellationToken);

            var dtoList = new List<RolListDto>();

            foreach (var rol in data)
            {
                var permisiuni = await _repository.GetPermisiuniForRolAsync(rol.RolID, cancellationToken);
                
                dtoList.Add(new RolListDto
                {
                    Id = rol.RolID,
                    Denumire = rol.Denumire,
                    Descriere = rol.Descriere,
                    EsteActiv = rol.EsteActiv,
                    EsteSistem = rol.EsteSistem,
                    OrdineAfisare = rol.OrdineAfisare,
                    NumarPermisiuni = permisiuni.Count(),
                    NumarUtilizatori = 0, // TODO: Count from Utilizatori table
                    DataCrearii = rol.DataCrearii,
                    DataUltimeiModificari = rol.DataUltimeiModificari,
                    CreatDe = rol.CreatDe,
                    ModificatDe = rol.ModificatDe
                });
            }

            _logger.LogInformation(
                "GetRolList SUCCESS: Retrieved {Count} of {Total}",
                dtoList.Count, totalCount);

            return PagedResult<RolListDto>.Success(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRolListQueryHandler");
            return PagedResult<RolListDto>.Failure(new List<string> { ex.Message });
        }
    }
}
