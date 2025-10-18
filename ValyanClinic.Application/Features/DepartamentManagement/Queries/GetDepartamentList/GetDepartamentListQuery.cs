using MediatR;
using ValyanClinic.Application.Common;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentList;

public record GetDepartamentListQuery : IRequest<PagedResult<DepartamentListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? GlobalSearchText { get; init; }
    public Guid? FilterIdTipDepartament { get; init; }
    public string SortColumn { get; init; } = "DenumireDepartament";
    public string SortDirection { get; init; } = "ASC";
}
