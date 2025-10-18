using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

/// <summary>
/// Query pentru obtinerea listei de pacienti cu filtrare si paginare
/// </summary>
public class GetPacientListQuery : IRequest<Result<PagedResult<PacientListDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchText { get; set; }
    public string? Judet { get; set; }
    public bool? Asigurat { get; set; }
    public bool? Activ { get; set; }
    public string SortColumn { get; set; } = "Nume";
    public string SortDirection { get; set; } = "ASC";
}
