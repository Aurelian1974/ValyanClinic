using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;

/// <summary>
/// Query pentru căutare coduri ICD-10
/// Folosește sp_ICD10_Search din ValyanMed database
/// </summary>
public record SearchICD10Query(
    string? SearchTerm = null,
    string? Category = null,
    bool OnlyCommon = false,
    bool OnlyLeafNodes = true,
    int MaxResults = 50
) : IRequest<Result<IEnumerable<ICD10SearchResultDto>>>;
