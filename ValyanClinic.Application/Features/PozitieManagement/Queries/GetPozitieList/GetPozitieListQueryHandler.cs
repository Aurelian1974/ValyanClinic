using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieList;

public class GetPozitieListQueryHandler : IRequestHandler<GetPozitieListQuery, PagedResult<PozitieListDto>>
{
    private readonly IPozitieRepository _repository;
    private readonly ILogger<GetPozitieListQueryHandler> _logger;

    public GetPozitieListQueryHandler(
        IPozitieRepository repository,
        ILogger<GetPozitieListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<PozitieListDto>> Handle(
        GetPozitieListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "GetPozitieList: Page={Page}, Size={Size}, Search={Search}, EsteActiv={EsteActiv}",
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

            var dtoList = data.Select(p => new PozitieListDto
            {
                Id = p.Id,
                Denumire = p.Denumire,
                Descriere = p.Descriere,
                EsteActiv = p.EsteActiv,
                DataCrearii = p.DataCrearii,
                DataUltimeiModificari = p.DataUltimeiModificari,
                CreatDe = p.CreatDe,
                ModificatDe = p.ModificatDe
            }).ToList();

            _logger.LogInformation(
                "GetPozitieList SUCCESS: Retrieved {Count} of {Total}",
                dtoList.Count, totalCount);

            return PagedResult<PozitieListDto>.Success(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPozitieListQueryHandler");
            return PagedResult<PozitieListDto>.Failure(new List<string> { ex.Message });
        }
    }
}
