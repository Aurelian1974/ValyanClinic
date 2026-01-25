using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ValyanClinic.Domain.Interfaces.Services;

/// <summary>
/// Serviciu pentru gestionarea draft-urilor (aplicație/browser)
/// Interfață definită în Domain pentru a permite Application și Infrastructure să o folosească fără dependențe circulare.
/// </summary>
/// <typeparam name="T">Tipul datelor salvate (ex: CreateConsultatieCommand)</typeparam>
public interface IDraftStorageService<T> where T : class
{
    Task SaveDraftAsync(Guid entityId, T data, string userId);
    Task<ValyanClinic.Domain.Models.DraftResult<T>> LoadDraftAsync(Guid entityId);
    Task ClearDraftAsync(Guid entityId);
    Task<bool> HasDraftAsync(Guid entityId);
    Task<DateTime?> GetLastSaveTimeAsync(Guid entityId);
    Task<int> CleanupExpiredDraftsAsync(int expirationDays = 7);
    Task<List<Guid>> GetUserDraftsAsync(string userId);
}
