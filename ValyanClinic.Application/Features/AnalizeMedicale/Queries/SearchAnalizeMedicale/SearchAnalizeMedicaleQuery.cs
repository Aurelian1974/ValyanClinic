using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.SearchAnalizeMedicale;

/// <summary>
/// Query pentru căutare analize medicale în nomenclator
/// </summary>
public record SearchAnalizeMedicaleQuery : IRequest<Result<PagedResult<AnalizaMedicalaListDto>>>
{
    public string? SearchTerm { get; init; }
    public Guid? CategorieId { get; init; }
    public Guid? LaboratorId { get; init; }
    public bool DoarActive { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
