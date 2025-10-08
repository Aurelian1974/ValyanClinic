using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;

/// <summary>
/// Query pentru lista paginata de personal cu suport pentru server-side operations
/// </summary>
public record GetPersonalListQuery : IRequest<PagedResult<PersonalListDto>>
{
    // Paging parameters
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    
    // Search parameters
    public string? GlobalSearchText { get; init; }
    
    // Filter parameters
    public string? FilterStatus { get; init; }
    public string? FilterDepartament { get; init; }
    public string? FilterFunctie { get; init; }
    public string? FilterJudet { get; init; }
    
    // Sorting parameters
    public string SortColumn { get; init; } = "Nume";
    public string SortDirection { get; init; } = "ASC";
}
