using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Components.Shared.ICD10;

/// <summary>
/// ICD10 Search Box - Autocomplete cu favorites
/// Features: Live search, keyboard navigation, favorites management
/// </summary>
public partial class ICD10SearchBox : ComponentBase, IDisposable
{
    // ==================== INJECTED SERVICES ====================
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IICD10Repository ICD10Repository { get; set; } = default!;
    [Inject] private ILogger<ICD10SearchBox> Logger { get; set; } = default!;

    // ==================== PARAMETERS ====================
    
    [Parameter] public string Placeholder { get; set; } = "Căutați după cod sau denumire (ex: I10, Hipertensiune)...";
    
    [Parameter] public int MinSearchLength { get; set; } = 2;
    
    [Parameter] public int SearchDebounceMs { get; set; } = 300;
    
    [Parameter] public int MaxResults { get; set; } = 20;
    
    [Parameter] public Guid? CurrentUserId { get; set; }
 
    /// <summary>Event când un cod este selectat</summary>
 [Parameter] public EventCallback<ICD10SearchResultDto> OnCodeSelected { get; set; }

    // ==================== STATE ====================
    
    private string SearchTerm { get; set; } = string.Empty;
    
    private bool IsSearching { get; set; }
    
    private bool ShowResults { get; set; }
    
    private List<ICD10SearchResultDto> Results { get; set; } = new();
    
    private int SelectedIndex { get; set; } = -1;
  
    private System.Threading.Timer? _debounceTimer;

    /// <summary>✅ NEW: Set of favorite ICD10 IDs pentru user curent</summary>
    private HashSet<Guid> FavoriteIds { get; set; } = new();

    /// <summary>✅ NEW: Loading state pentru favorites</summary>
    private bool IsFavoritesLoading { get; set; }

    /// <summary>✅ NEW: Favorites modal visibility</summary>
    private bool IsFavoritesModalVisible { get; set; }

    /// <summary>✅ NEW: Reference to favorites modal</summary>
    private ICD10FavoritesModal? _favoritesModal;

    // ==================== LIFECYCLE ====================

    protected override async Task OnInitializedAsync()
    {
        await LoadFavoritesAsync();
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
    }

    // ==================== SEARCH LOGIC ====================

    /// <summary>Handler pentru schimbarea textului de căutare (cu debounce)</summary>
    private void HandleSearchTermChanged()
    {
        _debounceTimer?.Dispose();

        if (string.IsNullOrWhiteSpace(SearchTerm) || SearchTerm.Length < MinSearchLength)
    {
        Results.Clear();
            ShowResults = false;
    StateHasChanged();
            return;
        }

        _debounceTimer = new System.Threading.Timer(async _ =>
     {
    await InvokeAsync(async () =>
        {
                await PerformSearchAsync();
        });
    }, null, SearchDebounceMs, Timeout.Infinite);
    }

    /// <summary>Execută căutarea efectivă</summary>
    private async Task PerformSearchAsync()
    {
        try
        {
    IsSearching = true;
            StateHasChanged();

  Logger.LogInformation("[ICD10Search] Searching: {Term}", SearchTerm);

    var query = new SearchICD10Query(
                SearchTerm: SearchTerm,
    MaxResults: MaxResults
            );

      var result = await Mediator.Send(query);

   if (result.IsSuccess && result.Value != null)
            {
    Results = result.Value.ToList();
          
 // ✅ Mark favorites in results
                foreach (var resultItem in Results)
                {
                    resultItem.IsFavorite = FavoriteIds.Contains(resultItem.ICD10_ID);
                }

      ShowResults = Results.Any();
  SelectedIndex = -1;

           Logger.LogInformation("[ICD10Search] Found {Count} results ({FavCount} favorites)", 
                Results.Count, Results.Count(r => r.IsFavorite));
         }
          else
    {
       Results.Clear();
     ShowResults = false;
      Logger.LogWarning("[ICD10Search] Search failed");
        }
        }
  catch (Exception ex)
        {
    Logger.LogError(ex, "[ICD10Search] Error during search");
            Results.Clear();
            ShowResults = false;
        }
        finally
        {
   IsSearching = false;
  StateHasChanged();
        }
    }

    // ==================== SELECTION LOGIC ====================

/// <summary>Selectează un rezultat din dropdown</summary>
    private async Task SelectResult(ICD10SearchResultDto result)
    {
        await OnCodeSelected.InvokeAsync(result);
        
        // Clear search after selection
     SearchTerm = string.Empty;
        Results.Clear();
        ShowResults = false;
  SelectedIndex = -1;
     
        StateHasChanged();
    }

    /// <summary>Toggle favorite status</summary>
    private async Task ToggleFavorite(ICD10SearchResultDto result)
    {
        if (!CurrentUserId.HasValue)
        {
            Logger.LogWarning("[ICD10Search] Cannot toggle favorite - no user ID");
  return;
        }

        try
      {
        if (result.IsFavorite)
            {
                var success = await ICD10Repository.RemoveFavoriteAsync(CurrentUserId.Value, result.ICD10_ID);
                if (success)
                {
                    FavoriteIds.Remove(result.ICD10_ID);
                    result.IsFavorite = false;
                    Logger.LogInformation("[ICD10Search] Removed favorite: {Code}", result.Code);
                }
       }
            else
            {
       var success = await ICD10Repository.AddFavoriteAsync(CurrentUserId.Value, result.ICD10_ID);
                if (success)
                {
                    FavoriteIds.Add(result.ICD10_ID);
                    result.IsFavorite = true;
                    Logger.LogInformation("[ICD10Search] Added favorite: {Code}", result.Code);
                }
    }

     StateHasChanged();
        }
        catch (Exception ex)
        {
   Logger.LogError(ex, "[ICD10Search] Error toggling favorite: {Code}", result.Code);
      }
    }

    // ==================== KEYBOARD NAVIGATION ====================

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (!ShowResults || !Results.Any())
            return;

      switch (e.Key)
        {
       case "ArrowDown":
       SelectedIndex = Math.Min(SelectedIndex + 1, Results.Count - 1);
     StateHasChanged();
            break;

            case "ArrowUp":
    SelectedIndex = Math.Max(SelectedIndex - 1, 0);
     StateHasChanged();
              break;

      case "Enter":
                if (SelectedIndex >= 0 && SelectedIndex < Results.Count)
        {
  await SelectResult(Results[SelectedIndex]);
       }
   break;

            case "Escape":
    ShowResults = false;
        SelectedIndex = -1;
    StateHasChanged();
                break;
    }
    }

    private void HandleFocus()
    {
        if (!string.IsNullOrEmpty(SearchTerm) && Results.Any())
      {
 ShowResults = true;
    StateHasChanged();
        }
 }

    private void HandleBlur()
    {
      // Delay pentru a permite click pe rezultate
      Task.Delay(200).ContinueWith(_ =>
        {
     InvokeAsync(() =>
            {
ShowResults = false;
    StateHasChanged();
            });
        });
    }

    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        Results.Clear();
        ShowResults = false;
        SelectedIndex = -1;
 StateHasChanged();
  }

    // ==================== HELPERS ====================

    private string GetCategoryIcon(string category)
    {
        return category switch
        {
            "Cardiovascular" => "❤️",
            "Endocrin" => "🔬",
            "Respirator" => "🫁",
            "Digestiv" => "🍽️",
            "Nervos" => "🧠",
            "Simptome" => "⚕️",
            _ => "📋"
        };
    }

    // ==================== FAVORITES MODAL ====================

    /// <summary>✅ NEW: Open favorites modal</summary>
    private void OpenFavoritesModal()
    {
        Logger.LogInformation("[ICD10Search] Opening favorites modal");
        
        // ✅ SIMPLIFIED: Just set the visibility flag
        // Modal will load data when it renders (via OpenAsync if @ref is used)
        IsFavoritesModalVisible = true;
        StateHasChanged();
    }

    /// <summary>✅ NEW: Handle favorite selection from modal</summary>
    private async Task HandleFavoriteSelectedFromModal(ICD10SearchResultDto favorite)
    {
        Logger.LogInformation("[ICD10Search] Favorite selected from modal: {Code}", favorite.Code);
        
        // Same logic as SelectResult
        await OnCodeSelected.InvokeAsync(favorite);
        
        // Clear search after selection
        SearchTerm = string.Empty;
        Results.Clear();
        ShowResults = false;
        SelectedIndex = -1;
        
        StateHasChanged();
    }

    // ==================== FAVORITES LOADING ====================

    /// <summary>✅ NEW: Load user's favorite ICD10 codes</summary>
    private async Task LoadFavoritesAsync()
    {
        if (!CurrentUserId.HasValue)
        {
            Logger.LogDebug("[ICD10Search] No user ID - skipping favorites load");
            return;
        }

        try
        {
            IsFavoritesLoading = true;
            StateHasChanged();

            Logger.LogInformation("[ICD10Search] Loading favorites for user: {UserId}", CurrentUserId.Value);

            var favorites = await ICD10Repository.GetFavoritesAsync(CurrentUserId.Value);
            FavoriteIds = favorites.Select(f => f.ICD10_ID).ToHashSet();

            Logger.LogInformation("[ICD10Search] Loaded {Count} favorites", FavoriteIds.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ICD10Search] Error loading favorites");
        }
        finally
        {
            IsFavoritesLoading = false;
            StateHasChanged();
        }
    }
}
