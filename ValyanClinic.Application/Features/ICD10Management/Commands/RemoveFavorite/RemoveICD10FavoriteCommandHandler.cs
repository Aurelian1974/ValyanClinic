using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ICD10Management.Commands.RemoveFavorite;

/// <summary>
/// Handler pentru ștergerea unui cod ICD-10 din favorite
/// </summary>
public class RemoveICD10FavoriteCommandHandler : IRequestHandler<RemoveICD10FavoriteCommand, Result<bool>>
{
    private readonly IICD10FavoriteRepository _favoriteRepository;
    private readonly ILogger<RemoveICD10FavoriteCommandHandler> _logger;

    public RemoveICD10FavoriteCommandHandler(
        IICD10FavoriteRepository favoriteRepository,
        ILogger<RemoveICD10FavoriteCommandHandler> logger)
    {
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        RemoveICD10FavoriteCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[RemoveICD10FavoriteCommandHandler] Removing favorite - PersonalId: {PersonalId}, ICD10_ID: {ICD10_ID}",
                request.PersonalId, request.ICD10_ID);

            await _favoriteRepository.RemoveFavoriteAsync(
                request.PersonalId,
                request.ICD10_ID,
                cancellationToken);

            _logger.LogInformation("[RemoveICD10FavoriteCommandHandler] Favorite removed successfully");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemoveICD10FavoriteCommandHandler] Error removing favorite");
            return Result<bool>.Failure($"Eroare la ștergerea favoritei: {ex.Message}");
        }
    }
}
