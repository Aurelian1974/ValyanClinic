using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieList;

public record GetPozitieListQuery : IRequest<PagedResult<PozitieListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? GlobalSearchText { get; init; }
    public bool? EsteActiv { get; init; }
    public string SortColumn { get; init; } = "Denumire";
    public string SortDirection { get; init; } = "ASC";
}
