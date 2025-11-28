using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;

/// <summary>
/// Query pentru căutare coduri ICD-10
/// </summary>
public record SearchICD10Query(
    string SearchTerm,
    string? Category = null,
    bool OnlyCommon = false,
    bool OnlyLeafNodes = true,
    int MaxResults = 20
) : IRequest<Result<List<ICD10SearchResultDto>>>;
