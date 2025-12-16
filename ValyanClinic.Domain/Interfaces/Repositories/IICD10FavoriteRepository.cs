using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru gestionarea ICD-10 Favorite per medic
/// </summary>
public interface IICD10FavoriteRepository
{
    /// <summary>
    /// Obține toate codurile ICD-10 favorite pentru un medic
    /// </summary>
    /// <param name="personalId">ID-ul medicului</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>Lista de coduri ICD-10 favorite, sortate</returns>
    Task<IEnumerable<ICD10Code>> GetFavoritesAsync(
        Guid personalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adaugă un cod ICD-10 la favoritele medicului
    /// </summary>
    /// <param name="personalId">ID-ul medicului</param>
    /// <param name="icd10Id">ID-ul codului ICD-10</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>ID-ul favoritei create (sau existente)</returns>
    Task<Guid> AddFavoriteAsync(
        Guid personalId,
        Guid icd10Id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Șterge un cod ICD-10 din favoritele medicului
    /// </summary>
    /// <param name="personalId">ID-ul medicului</param>
    /// <param name="icd10Id">ID-ul codului ICD-10</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    Task RemoveFavoriteAsync(
        Guid personalId,
        Guid icd10Id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizează ordinea unui cod favorit
    /// </summary>
    /// <param name="personalId">ID-ul medicului</param>
    /// <param name="icd10Id">ID-ul codului ICD-10</param>
    /// <param name="newSortOrder">Noua poziție</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    Task UpdateSortOrderAsync(
        Guid personalId,
        Guid icd10Id,
        int newSortOrder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifică dacă un cod este în favorite pentru un medic
    /// </summary>
    /// <param name="personalId">ID-ul medicului</param>
    /// <param name="icd10Id">ID-ul codului ICD-10</param>
    /// <param name="cancellationToken">Token pentru anulare</param>
    /// <returns>True dacă este favorit</returns>
    Task<bool> IsFavoriteAsync(
        Guid personalId,
        Guid icd10Id,
        CancellationToken cancellationToken = default);
}
