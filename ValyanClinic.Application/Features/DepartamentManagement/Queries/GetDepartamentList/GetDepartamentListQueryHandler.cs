using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentList;

public class GetDepartamentListQueryHandler : IRequestHandler<GetDepartamentListQuery, PagedResult<DepartamentListDto>>
{
    private readonly IDepartamentRepository _repository;
    private readonly ILogger<GetDepartamentListQueryHandler> _logger;

    public GetDepartamentListQueryHandler(
        IDepartamentRepository repository,
        ILogger<GetDepartamentListQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<DepartamentListDto>> Handle(
        GetDepartamentListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "GetDepartamentList: Page={Page}, Size={Size}, Search={Search}, TipDept={TipDept}",
                request.PageNumber, request.PageSize, request.GlobalSearchText, request.FilterIdTipDepartament);

            var data = await _repository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.GlobalSearchText,
                request.FilterIdTipDepartament,
                request.SortColumn,
                request.SortDirection,
                cancellationToken);

            var totalCount = await _repository.GetCountAsync(
                request.GlobalSearchText,
                request.FilterIdTipDepartament,
                cancellationToken);

            var dtoList = data.Select(d => new DepartamentListDto
            {
                IdDepartament = d.IdDepartament,
                IdTipDepartament = d.IdTipDepartament,
                DenumireDepartament = d.DenumireDepartament,
                DescriereDepartament = d.DescriereDepartament,
                DenumireTipDepartament = d.TipDepartament?.DenumireTipDepartament
            }).ToList();

            _logger.LogInformation(
                "GetDepartamentList SUCCESS: Retrieved {Count} of {Total}",
                dtoList.Count, totalCount);

            return PagedResult<DepartamentListDto>.Success(dtoList, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDepartamentListQueryHandler");
            return PagedResult<DepartamentListDto>.Failure(new List<string> { ex.Message });
        }
    }
}
