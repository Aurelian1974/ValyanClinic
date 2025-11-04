using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareList;

public record GetSpecializareListQuery : IRequest<PagedResult<SpecializareListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? GlobalSearchText { get; init; }
    public string? Categorie { get; init; }
    public bool? EsteActiv { get; init; }
    public string SortColumn { get; init; } = "Denumire";
    public string SortDirection { get; init; } = "ASC";
}
