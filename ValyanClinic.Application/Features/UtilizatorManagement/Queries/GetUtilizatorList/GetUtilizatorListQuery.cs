using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Queries.GetUtilizatorList;

public record GetUtilizatorListQuery : IRequest<PagedResult<UtilizatorListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? GlobalSearchText { get; init; }
    public string? FilterRol { get; init; }
    public bool? FilterEsteActiv { get; init; }
    public string SortColumn { get; init; } = "Username";
    public string SortDirection { get; init; } = "ASC";
}
