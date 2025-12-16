using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ICD10Management.Commands.AddFavorite;

/// <summary>
/// Command pentru adăugarea unui cod ICD-10 la favoritele medicului
/// </summary>
/// <param name="PersonalId">ID-ul medicului</param>
/// <param name="ICD10_ID">ID-ul codului ICD-10</param>
public record AddICD10FavoriteCommand(Guid PersonalId, Guid ICD10_ID) : IRequest<Result<Guid>>;
