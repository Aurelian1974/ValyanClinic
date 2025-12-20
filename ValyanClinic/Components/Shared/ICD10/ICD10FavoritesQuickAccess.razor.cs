using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Application.Features.ICD10Management.DTOs;

namespace ValyanClinic.Components.Shared.ICD10;

/// <summary>
/// ICD10 Favorites Quick Access Component
/// Displays user's favorite ICD10 codes for quick selection
/// </summary>
public partial class ICD10FavoritesQuickAccess : ComponentBase
{
    [Inject] private IICD10Repository ICD10Repository { get; set; } = default!;
    [Inject] private ILogger<ICD10FavoritesQuickAccess> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    /// <summary>User ID for loading favorites</summary>
    [Parameter] public Guid? CurrentUserId { get; set; }
    
    /// <summary>Maximum number of favorites to display</summary>
    [Parameter] public int MaxDisplay { get; set; } = 10;
    
    /// <summary>Event when a favorite is selected</summary>
    [Parameter] public EventCallback<ICD10SearchResultDto> OnFavoriteSelected { get; set; }

    // ==================== STATE ====================
    
    private List<ICD10Code> Favorites { get; set; } = new();
    
    private bool IsLoading { get; set; }

    // ==================== LIFECYCLE ====================

    protected override async Task OnParametersSetAsync()
    {
        if (CurrentUserId.HasValue)
        {
            await LoadFavoritesAsync();
        }
    }

    // ==================== DATA LOADING ====================

    private async Task LoadFavoritesAsync()
    {
        if (!CurrentUserId.HasValue)
        {
            Logger.LogDebug("[ICD10Favorites] No user ID - skipping load");
            return;
        }

        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("[ICD10Favorites] Loading favorites for user: {UserId}", CurrentUserId.Value);

            var favorites = await ICD10Repository.GetFavoritesAsync(CurrentUserId.Value);
            Favorites = favorites.ToList();

            Logger.LogInformation("[ICD10Favorites] Loaded {Count} favorites", Favorites.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ICD10Favorites] Error loading favorites");
            Favorites = new List<ICD10Code>();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    // ==================== ACTIONS ====================

    private async Task SelectFavorite(ICD10Code favorite)
    {
        Logger.LogInformation("[ICD10Favorites] Selected favorite: {Code}", favorite.Code);

        // Map to DTO
        var dto = new ICD10SearchResultDto
        {
            ICD10_ID = favorite.ICD10_ID,
            Code = favorite.Code,
            FullCode = favorite.FullCode,
            ShortDescription = favorite.ShortDescription,
            LongDescription = favorite.LongDescription,
            Category = favorite.Category,
            Severity = favorite.Severity,
            IsCommon = favorite.IsCommon,
            IsLeafNode = favorite.IsLeafNode,
            IsBillable = favorite.IsBillable,
            IsTranslated = favorite.IsTranslated,
            IsFavorite = true,
            RelevanceScore = 100
        };

        await OnFavoriteSelected.InvokeAsync(dto);
    }

    /// <summary>Public method pentru refresh after favorite changes</summary>
    public async Task RefreshAsync()
    {
        await LoadFavoritesAsync();
    }
}
