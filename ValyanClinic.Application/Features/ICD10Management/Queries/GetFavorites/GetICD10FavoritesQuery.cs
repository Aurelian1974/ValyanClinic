using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Application.Features.ICD10Management.Queries.GetFavorites;

/// <summary>
/// Query pentru obținerea codurilor ICD-10 favorite ale unui medic
/// </summary>
/// <param name="PersonalId">ID-ul medicului</param>
public record GetICD10FavoritesQuery(Guid PersonalId) : IRequest<Result<List<ICD10SearchResultDto>>>;
