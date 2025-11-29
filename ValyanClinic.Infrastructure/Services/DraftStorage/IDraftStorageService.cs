namespace ValyanClinic.Infrastructure.Services.DraftStorage;

/// <summary>
/// Serviciu pentru gestionarea draft-urilor în LocalStorage
/// Permite salvarea/încărcarea automată a formularelor incomplete
/// </summary>
/// <typeparam name="T">Tipul datelor salvate (ex: CreateConsultatieCommand)</typeparam>
public interface IDraftStorageService<T> where T : class
{
    /// <summary>
    /// Salvează draft-ul în LocalStorage
    /// </summary>
    /// <param name="entityId">ID-ul entității (ex: ProgramareID)</param>
    /// <param name="data">Datele de salvat</param>
    /// <param name="userId">ID-ul utilizatorului</param>
    /// <returns>Task</returns>
    Task SaveDraftAsync(Guid entityId, T data, string userId);
    
    /// <summary>
    /// Încarcă draft-ul din LocalStorage
    /// </summary>
    /// <param name="entityId">ID-ul entității</param>
    /// <returns>Rezultat cu datele draft-ului sau eroare</returns>
    Task<DraftResult<T>> LoadDraftAsync(Guid entityId);
    
    /// <summary>
    /// Șterge draft-ul (după salvare cu succes)
    /// </summary>
    /// <param name="entityId">ID-ul entității</param>
    /// <returns>Task</returns>
    Task ClearDraftAsync(Guid entityId);
    
    /// <summary>
    /// Verifică dacă există draft pentru entitate
    /// </summary>
    /// <param name="entityId">ID-ul entității</param>
    /// <returns>True dacă există draft</returns>
    Task<bool> HasDraftAsync(Guid entityId);
    
    /// <summary>
    /// Obține timestamp-ul ultimei salvări
    /// </summary>
    /// <param name="entityId">ID-ul entității</param>
    /// <returns>Data salvării sau null</returns>
    Task<DateTime?> GetLastSaveTimeAsync(Guid entityId);
    
    /// <summary>
    /// Șterge toate draft-urile expirate (mai vechi de X zile)
    /// </summary>
    /// <param name="expirationDays">Numărul de zile după care draft-ul expiră</param>
    /// <returns>Numărul de draft-uri șterse</returns>
    Task<int> CleanupExpiredDraftsAsync(int expirationDays = 7);
    
    /// <summary>
    /// Obține lista cu toate draft-urile utilizatorului curent
    /// </summary>
    /// <param name="userId">ID-ul utilizatorului</param>
    /// <returns>Lista de entity IDs cu draft-uri</returns>
    Task<List<Guid>> GetUserDraftsAsync(string userId);
}
