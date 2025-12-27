using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ValyanClinic.Infrastructure.Services.DraftStorage;

/// <summary>
/// Implementare LocalStorage pentru draft-uri
/// Salvează draft-urile în browser folosind JavaScript Interop
/// </summary>
/// <typeparam name="T">Tipul datelor salvate</typeparam>
public class LocalStorageDraftService<T> : IDraftStorageService<T> where T : class
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<LocalStorageDraftService<T>> _logger;

    private const string STORAGE_PREFIX = "draft_";
    private const string METADATA_KEY = "draft_metadata";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public LocalStorageDraftService(
        IJSRuntime jsRuntime,
        ILogger<LocalStorageDraftService<T>> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    /// <summary>
    /// Salvează draft-ul în LocalStorage
    /// </summary>
    public async Task SaveDraftAsync(Guid entityId, T data, string userId)
    {
        try
        {
            var draft = new Draft<T>
            {
                EntityId = entityId,
                UserId = userId,
                Data = data,
                SavedAt = DateTime.Now,
                Version = 1
            };

            var json = JsonSerializer.Serialize(draft, _jsonOptions);
            var key = GetStorageKey(entityId);

            await _jsRuntime.InvokeAsync<object>("localStorage.setItem", key, json);

            // Update metadata list
            await UpdateMetadataAsync(entityId, userId);

            _logger.LogInformation(
                "Draft saved: EntityId={EntityId}, UserId={UserId}, Size={Size} bytes",
                entityId,
                userId,
                json.Length
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error saving draft for entity {EntityId}",
                entityId);
            throw;
        }
    }

    /// <summary>
    /// Încarcă draft-ul din LocalStorage
    /// </summary>
    public async Task<DraftResult<T>> LoadDraftAsync(Guid entityId)
    {
        try
        {
            var key = GetStorageKey(entityId);
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogDebug("No draft found for entity {EntityId}", entityId);
                return DraftResult<T>.NotFound;
            }

            var draft = JsonSerializer.Deserialize<Draft<T>>(json, _jsonOptions);

            if (draft == null || draft.Data == null)
            {
                _logger.LogWarning("Invalid draft data for entity {EntityId}", entityId);
                return DraftResult<T>.Invalid;
            }

            // Check expiration (7 days)
            var age = DateTime.Now - draft.SavedAt;
            if (age.TotalDays > 7)
            {
                _logger.LogWarning(
                    "Draft expired for entity {EntityId}, age: {Age} days",
                    entityId,
                    age.TotalDays
                );
                await ClearDraftAsync(entityId);
                return DraftResult<T>.Expired;
            }

            _logger.LogInformation(
                "Draft loaded: EntityId={EntityId}, SavedAt={SavedAt}, Age={Age} hours",
                entityId,
                draft.SavedAt,
                age.TotalHours
            );

            return DraftResult<T>.Success(draft.Data, draft.SavedAt);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for entity {EntityId}", entityId);
            return DraftResult<T>.Invalid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading draft for entity {EntityId}", entityId);
            return DraftResult<T>.Error(ex.Message);
        }
    }

    /// <summary>
    /// Șterge draft-ul
    /// </summary>
    public async Task ClearDraftAsync(Guid entityId)
    {
        try
        {
            var key = GetStorageKey(entityId);
            await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", key);

            // Remove from metadata
            await RemoveFromMetadataAsync(entityId);

            _logger.LogInformation("Draft cleared for entity {EntityId}", entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing draft for entity {EntityId}", entityId);
        }
    }

    /// <summary>
    /// Verifică existența draft-ului
    /// </summary>
    public async Task<bool> HasDraftAsync(Guid entityId)
    {
        try
        {
            var key = GetStorageKey(entityId);
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            return !string.IsNullOrEmpty(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking draft existence for entity {EntityId}", entityId);
            return false;
        }
    }

    /// <summary>
    /// Obține timestamp-ul ultimei salvări
    /// </summary>
    public async Task<DateTime?> GetLastSaveTimeAsync(Guid entityId)
    {
        try
        {
            var result = await LoadDraftAsync(entityId);
            return result.IsSuccess ? result.SavedAt : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last save time for entity {EntityId}", entityId);
            return null;
        }
    }

    /// <summary>
    /// Cleanup draft-uri expirate
    /// </summary>
    public async Task<int> CleanupExpiredDraftsAsync(int expirationDays = 7)
    {
        try
        {
            var metadata = await GetMetadataAsync();
            var deletedCount = 0;
            var expiredKeys = new List<Guid>();

            foreach (var entityId in metadata)
            {
                var result = await LoadDraftAsync(entityId);
                if (!result.IsSuccess || result.ErrorType == DraftErrorType.Expired)
                {
                    expiredKeys.Add(entityId);
                    await ClearDraftAsync(entityId);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Cleanup completed: {Count} expired drafts deleted",
                    deletedCount
                );
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during draft cleanup");
            return 0;
        }
    }

    /// <summary>
    /// Obține toate draft-urile utilizatorului
    /// </summary>
    public async Task<List<Guid>> GetUserDraftsAsync(string userId)
    {
        try
        {
            var metadata = await GetMetadataAsync();
            var userDrafts = new List<Guid>();

            foreach (var entityId in metadata)
            {
                var result = await LoadDraftAsync(entityId);
                if (result.IsSuccess && result.Data != null)
                {
                    // Note: We'd need to store userId in metadata to filter properly
                    // For now, return all drafts
                    userDrafts.Add(entityId);
                }
            }

            return userDrafts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user drafts for user {UserId}", userId);
            return new List<Guid>();
        }
    }

    #region Private Helpers

    /// <summary>
    /// Generează key-ul pentru LocalStorage
    /// </summary>
    private string GetStorageKey(Guid entityId)
    {
        var typeName = typeof(T).Name;
        return $"{STORAGE_PREFIX}{typeName}_{entityId}";
    }

    /// <summary>
    /// Update metadata list cu entity IDs
    /// </summary>
    private async Task UpdateMetadataAsync(Guid entityId, string userId)
    {
        try
        {
            var metadata = await GetMetadataAsync();
            if (!metadata.Contains(entityId))
            {
                metadata.Add(entityId);
                var json = JsonSerializer.Serialize(metadata);
                await _jsRuntime.InvokeAsync<object>("localStorage.setItem", METADATA_KEY, json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating metadata for entity {EntityId}", entityId);
        }
    }

    /// <summary>
    /// Remove entity din metadata
    /// </summary>
    private async Task RemoveFromMetadataAsync(Guid entityId)
    {
        try
        {
            var metadata = await GetMetadataAsync();
            if (metadata.Remove(entityId))
            {
                var json = JsonSerializer.Serialize(metadata);
                await _jsRuntime.InvokeAsync<object>("localStorage.setItem", METADATA_KEY, json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing from metadata entity {EntityId}", entityId);
        }
    }

    /// <summary>
    /// Obține lista de entity IDs din metadata
    /// </summary>
    private async Task<List<Guid>> GetMetadataAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", METADATA_KEY);
            if (string.IsNullOrEmpty(json))
                return new List<Guid>();

            return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    #endregion
}
