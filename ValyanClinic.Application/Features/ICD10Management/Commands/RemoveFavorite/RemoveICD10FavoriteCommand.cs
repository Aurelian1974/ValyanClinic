using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ICD10Management.Commands.RemoveFavorite;

/// <summary>
/// Command pentru ștergerea unui cod ICD-10 din favoritele medicului
/// </summary>
/// <param name="PersonalId">ID-ul medicului</param>
/// <param name="ICD10_ID">ID-ul codului ICD-10</param>
public record RemoveICD10FavoriteCommand(Guid PersonalId, Guid ICD10_ID) : IRequest<Result<bool>>;
