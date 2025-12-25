using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Queries.GetRolList;

/// <summary>
/// Query pentru obținerea listei de roluri cu paginare.
/// </summary>
public record GetRolListQuery : IRequest<PagedResult<RolListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? GlobalSearchText { get; init; }
    public bool? EsteActiv { get; init; }
    public string SortColumn { get; init; } = "OrdineAfisare";
    public string SortDirection { get; init; } = "ASC";
}
