using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru ICD10Code - Clasificarea Internationala a Bolilor
/// Returnează entități Domain (NU DTOs) conform Clean Architecture
/// </summary>
public interface IICD10Repository
{
    /// <summary>Caută coduri ICD-10 folosind sp_ICD10_Search</summary>
    Task<IEnumerable<ICD10Code>> SearchAsync(string searchTerm, int maxResults = 50);
    
    /// <summary>Obține detalii cod ICD-10 după ID</summary>
    Task<ICD10Code?> GetByIdAsync(Guid icd10Id);
    
    /// <summary>Obține detalii cod ICD-10 după cod (ex: I10)</summary>
    Task<ICD10Code?> GetByCodeAsync(string code);
    
   /// <summary>Obține coduri ICD-10 frecvente (IsCommon = true)</summary>
    Task<IEnumerable<ICD10Code>> GetCommonCodesAsync(int maxResults = 100);

    /// <summary>Obține favorite ICD-10 ale utilizatorului</summary>
    Task<IEnumerable<ICD10Code>> GetFavoritesAsync(Guid userId);
    
    /// <summary>Adaugă cod ICD-10 la favorite</summary>
    Task<bool> AddFavoriteAsync(Guid userId, Guid icd10Id);
    
    /// <summary>Elimină cod ICD-10 din favorite</summary>
    Task<bool> RemoveFavoriteAsync(Guid userId, Guid icd10Id);
    
    /// <summary>Obține statistici ICD-10 (tuple simplu)</summary>
    Task<(int TotalCodes, int TranslatedCodes, int CommonCodes, int LeafNodeCodes)> GetStatisticsAsync();
  
    /// <summary>Validează cod ICD-10</summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateCodeAsync(string code);
}
