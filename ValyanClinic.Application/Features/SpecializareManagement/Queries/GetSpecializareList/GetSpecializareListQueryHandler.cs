using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareList;

public class GetSpecializareListQueryHandler : IRequestHandler<GetSpecializareListQuery, PagedResult<SpecializareListDto>>
{
    private readonly ISpecializareRepository _repository;
    private readonly ILogger<GetSpecializareListQueryHandler> _logger;

    public GetSpecializareListQueryHandler(
        ISpecializareRepository repository,
        ILogger<GetSpecializareListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<SpecializareListDto>> Handle(
        GetSpecializareListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "GetSpecializareList: Page={Page}, Size={Size}, Search={Search}, Categorie={Categorie}, EsteActiv={EsteActiv}",
                request.PageNumber, request.PageSize, request.GlobalSearchText, request.Categorie, request.EsteActiv);

            var data = await _repository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.GlobalSearchText,
                request.Categorie,
                request.EsteActiv,
                request.SortColumn,
                request.SortDirection,
                cancellationToken);

            var totalCount = await _repository.GetCountAsync(
                request.GlobalSearchText,
                request.Categorie,
                request.EsteActiv,
                cancellationToken);

            var dtoList = data.Select(s => new SpecializareListDto
            {
                Id = s.Id,
                Denumire = s.Denumire,
                Categorie = s.Categorie,
                Descriere = s.Descriere,
                EsteActiv = s.EsteActiv,
                DataCrearii = s.DataCrearii,
                DataUltimeiModificari = s.DataUltimeiModificari,
                CreatDe = s.CreatDe,
                ModificatDe = s.ModificatDe
            }).ToList();

            _logger.LogInformation(
                "GetSpecializareList SUCCESS: Retrieved {Count} of {Total}",
                dtoList.Count, totalCount);

            return PagedResult<SpecializareListDto>.Success(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSpecializareListQueryHandler");
            return PagedResult<SpecializareListDto>.Failure(new List<string> { ex.Message });
        }
    }
}
