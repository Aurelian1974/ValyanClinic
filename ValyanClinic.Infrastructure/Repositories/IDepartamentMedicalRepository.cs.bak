using ValyanClinic.Domain.Models;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository interface pentru încărcarea departamentelor medicale din baza de date
/// Foloseste stored procedures pentru încărcarea departamentelor medicale din tabela Departamente
/// IMPORTANT: NU sunt enum-uri statice - totul din baza de date pentru flexibilitate
/// </summary>
public interface IDepartamentMedicalRepository
{
    /// <summary>
    /// Încarcă toate departamentele medicale din tabela Departamente WHERE Tip = 'Medical'
    /// Folosește sp_Departamente_GetByTip cu @Tip = 'Medical'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetAllDepartamenteMedicaleAsync();
    
    /// <summary>
    /// Încarcă categoriile medicale (nivelul 1 în ierarhia medicală)
    /// Folosește sp_Departamente_GetByTip cu @Tip = 'Categorie'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetCategoriiMedicaleAsync();
    
    /// <summary>
    /// Încarcă specializările medicale (nivelul 2 în ierarhia medicală)  
    /// Folosește sp_Departamente_GetByTip cu @Tip = 'Specializare'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSpecializariMedicaleAsync();
    
    /// <summary>
    /// Încarcă subspecializările medicale (nivelul 3 în ierarhia medicală)
    /// Folosește sp_Departamente_GetByTip cu @Tip = 'Subspecializare'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSubspecializariMedicaleAsync();
    
    /// <summary>
    /// Găsește un departament medical specific după ID
    /// </summary>
    Task<DepartamentMedical?> GetDepartamentMedicalByIdAsync(Guid departamentId);
    
    /// <summary>
    /// Încarcă specializările pentru o categorie specifică
    /// Folosește relațiile din baza de date pentru a găsi specializările asociate
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSpecializariPentruCategoriaAsync(Guid categorieId);
    
    /// <summary>
    /// Încarcă subspecializările pentru o specializare specifică
    /// Folosește relațiile din baza de date pentru a găsi subspecializările asociate
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSubspecializariPentruSpecializareaAsync(Guid specializareId);
    
    /// <summary>
    /// Verifică dacă un departament medical există și este activ
    /// </summary>
    Task<bool> DepartamentMedicalExistsAsync(Guid departamentId);
    
    /// <summary>
    /// Caută departamente medicale după nume
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> SearchDepartamenteMedicaleAsync(string searchText);
    
    // Test method for debugging database connectivity issues
    Task<bool> TestDatabaseConnectionAsync();
}
