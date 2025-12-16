using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ICD10Management.Queries.GetFavorites;

/// <summary>
/// Handler pentru obținerea codurilor ICD-10 favorite ale unui medic
/// </summary>
public class GetICD10FavoritesQueryHandler : IRequestHandler<GetICD10FavoritesQuery, Result<List<ICD10SearchResultDto>>>
{
    private readonly IICD10FavoriteRepository _favoriteRepository;
    private readonly ILogger<GetICD10FavoritesQueryHandler> _logger;

    public GetICD10FavoritesQueryHandler(
        IICD10FavoriteRepository favoriteRepository,
        ILogger<GetICD10FavoritesQueryHandler> logger)
    {
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<Result<List<ICD10SearchResultDto>>> Handle(
        GetICD10FavoritesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetICD10FavoritesQueryHandler] Loading favorites for PersonalId: {PersonalId}",
                request.PersonalId);

            var favorites = await _favoriteRepository.GetFavoritesAsync(
                request.PersonalId,
                cancellationToken);

            var results = favorites
                .Select(e => new ICD10SearchResultDto
                {
                    ICD10_ID = e.ICD10_ID,
                    Code = e.Code,
                    Category = e.Category,
                    ShortDescription = e.ShortDescription,
                    LongDescription = e.LongDescription,
                    IsCommon = e.IsCommon,
                    Severity = e.Severity,
                    RelevanceScore = 100, // Favorites au relevance maxim
                    IsFavorite = true // Marcaj că este favorit
                })
                .ToList();

            _logger.LogInformation(
                "[GetICD10FavoritesQueryHandler] Loaded {Count} favorites for PersonalId: {PersonalId}",
                results.Count, request.PersonalId);

            return Result<List<ICD10SearchResultDto>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetICD10FavoritesQueryHandler] Error loading favorites");
            return Result<List<ICD10SearchResultDto>>.Failure($"Eroare la încărcarea favoritelor: {ex.Message}");
        }
    }
}
