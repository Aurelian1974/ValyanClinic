using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ICD10Management.Commands.AddFavorite;

/// <summary>
/// Handler pentru adăugarea unui cod ICD-10 la favorite
/// </summary>
public class AddICD10FavoriteCommandHandler : IRequestHandler<AddICD10FavoriteCommand, Result<Guid>>
{
    private readonly IICD10FavoriteRepository _favoriteRepository;
    private readonly ILogger<AddICD10FavoriteCommandHandler> _logger;

    public AddICD10FavoriteCommandHandler(
        IICD10FavoriteRepository favoriteRepository,
        ILogger<AddICD10FavoriteCommandHandler> logger)
    {
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        AddICD10FavoriteCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[AddICD10FavoriteCommandHandler] Adding favorite - PersonalId: {PersonalId}, ICD10_ID: {ICD10_ID}",
                request.PersonalId, request.ICD10_ID);

            var favoriteId = await _favoriteRepository.AddFavoriteAsync(
                request.PersonalId,
                request.ICD10_ID,
                cancellationToken);

            _logger.LogInformation(
                "[AddICD10FavoriteCommandHandler] Favorite added successfully - FavoriteId: {FavoriteId}",
                favoriteId);

            return Result<Guid>.Success(favoriteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AddICD10FavoriteCommandHandler] Error adding favorite");
            return Result<Guid>.Failure($"Eroare la adăugarea favoritei: {ex.Message}");
        }
    }
}
