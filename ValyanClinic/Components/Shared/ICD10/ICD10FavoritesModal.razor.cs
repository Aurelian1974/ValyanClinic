using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Components.Shared.ICD10;

/// <summary>
/// ICD10 Favorites Modal - Full table view with search and sort
/// ✅ OPTIMIZED: Nu folosește OnParametersSetAsync pentru a evita re-renders de la timer
/// </summary>
public partial class ICD10FavoritesModal : ComponentBase
{
    [Inject] private IICD10Repository ICD10Repository { get; set; } = default!;
    [Inject] private ILogger<ICD10FavoritesModal> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    /// <summary>Modal visibility</summary>
    [Parameter] public bool IsVisible { get; set; }
    
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    
    /// <summary>User ID for loading favorites</summary>
    [Parameter] public Guid? CurrentUserId { get; set; }
    
    /// <summary>Event when a favorite is selected</summary>
    [Parameter] public EventCallback<ICD10SearchResultDto> OnFavoriteSelected { get; set; }

    // ==================== STATE ====================
    
    private List<ICD10SearchResultDto> AllFavorites { get; set; } = new();
    
    private List<ICD10SearchResultDto> FilteredFavorites { get; set; } = new();
    
    private bool IsLoading { get; set; }
    
    private string _searchTerm = string.Empty;
    private string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                ApplyFiltersAndSort();
            }
        }
    }
    
    private string CurrentSortColumn { get; set; } = nameof(ICD10SearchResultDto.Code);
    
    private bool IsAscending { get; set; } = true;

    // ==================== LIFECYCLE ====================

    /// <summary>
    /// ✅ FIX: Block ALL re-renders when modal is CLOSED
    /// This is the ONLY lifecycle method we need - no OnParametersSetAsync!
    /// </summary>
    protected override bool ShouldRender()
    {
        // ✅ Only render when modal is visible
        return IsVisible;
    }

    // ✅ REMOVED: OnParametersSetAsync - not needed, causes problems with timer

    // ==================== DATA LOADING ====================

    private async Task LoadFavoritesAsync()
    {
        if (!CurrentUserId.HasValue)
        {
            Logger.LogDebug("[ICD10FavoritesModal] No user ID - skipping load");
            return;
        }

        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("[ICD10FavoritesModal] Loading favorites for user: {UserId}", CurrentUserId.Value);

            var favorites = await ICD10Repository.GetFavoritesAsync(CurrentUserId.Value);
            
            // Map to DTOs
            AllFavorites = favorites.Select(f => new ICD10SearchResultDto
            {
                ICD10_ID = f.ICD10_ID,
                Code = f.Code,
                FullCode = f.FullCode,
                ShortDescription = f.ShortDescription,
                LongDescription = f.LongDescription,
                Category = f.Category,
                Severity = f.Severity,
                IsCommon = f.IsCommon,
                IsLeafNode = f.IsLeafNode,
                IsBillable = f.IsBillable,
                IsTranslated = f.IsTranslated,
                IsFavorite = true,
                RelevanceScore = 100
            }).ToList();

            ApplyFiltersAndSort();

            Logger.LogInformation("[ICD10FavoritesModal] Loaded {Count} favorites", AllFavorites.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ICD10FavoritesModal] Error loading favorites");
            AllFavorites = new List<ICD10SearchResultDto>();
            FilteredFavorites = new List<ICD10SearchResultDto>();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    // ==================== SEARCH & FILTER ====================

    private void ApplyFiltersAndSort()
    {
        var query = AllFavorites.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var searchLower = SearchTerm.ToLower();
            query = query.Where(f =>
                f.Code.ToLower().Contains(searchLower) ||
                f.ShortDescription.ToLower().Contains(searchLower) ||
                (f.LongDescription?.ToLower().Contains(searchLower) ?? false));
        }

        // Apply sorting
        query = CurrentSortColumn switch
        {
            nameof(ICD10SearchResultDto.Code) => IsAscending
                ? query.OrderBy(f => f.Code)
                : query.OrderByDescending(f => f.Code),
            nameof(ICD10SearchResultDto.ShortDescription) => IsAscending
                ? query.OrderBy(f => f.ShortDescription)
                : query.OrderByDescending(f => f.ShortDescription),
            _ => query.OrderBy(f => f.Code)
        };

        FilteredFavorites = query.ToList();
    }

    // ==================== SORTING ====================

    private void SortBy(string columnName)
    {
        if (CurrentSortColumn == columnName)
        {
            // Toggle direction
            IsAscending = !IsAscending;
        }
        else
        {
            // New column, default ascending
            CurrentSortColumn = columnName;
            IsAscending = true;
        }

        ApplyFiltersAndSort();
        StateHasChanged();
        
        Logger.LogDebug("[ICD10FavoritesModal] Sorted by {Column} {Direction}", 
            columnName, IsAscending ? "ASC" : "DESC");
    }

    // ==================== SEARCH ====================

    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        StateHasChanged();
    }

    // ==================== ACTIONS ====================

    private async Task SelectFavorite(ICD10SearchResultDto favorite)
    {
        Logger.LogInformation("[ICD10FavoritesModal] SelectFavorite: {Code}", favorite.Code);

        await OnFavoriteSelected.InvokeAsync(favorite);
        await Close();
    }

    private async Task Close()
    {
        Logger.LogInformation("[ICD10FavoritesModal] Close() called");
        
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task HandleOverlayClick(MouseEventArgs e)
    {
        Logger.LogDebug("[ICD10FavoritesModal] HandleOverlayClick - closing modal");
        await Close();
    }

    // ==================== PUBLIC METHODS ====================

    /// <summary>Open modal and load favorites</summary>
    public async Task OpenAsync()
    {
        Logger.LogInformation("[ICD10FavoritesModal] OpenAsync() called");
        
        IsVisible = true;
        await IsVisibleChanged.InvokeAsync(true);
        
        // Reset search
        _searchTerm = string.Empty;
        
        // Load/refresh favorites
        await LoadFavoritesAsync();
    }
}
