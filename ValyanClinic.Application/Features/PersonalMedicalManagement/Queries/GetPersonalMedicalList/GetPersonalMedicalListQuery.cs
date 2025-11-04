using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

public record GetPersonalMedicalListQuery : IRequest<PagedResult<PersonalMedicalListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? GlobalSearchText { get; init; }
    public string? FilterDepartament { get; init; }
    public string? FilterPozitie { get; init; }
    public bool? FilterEsteActiv { get; init; }
    public string SortColumn { get; init; } = "Nume";
    public string SortDirection { get; init; } = "ASC";
}
