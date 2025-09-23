using ValyanClinic.Domain.Models;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository interface pentru incarcarea departamentelor medicale din baza de date
/// Foloseste stored procedures pentru incarcarea departamentelor medicale din tabela Departamente
/// IMPORTANT: NU sunt enum-uri statice - totul din baza de date pentru flexibilitate
/// </summary>
public interface IDepartamentMedicalRepository
{
    /// <summary>
    /// incarca toate departamentele medicale din tabela Departamente WHERE Tip = 'Medical'
    /// Foloseste sp_Departamente_GetByTip cu @Tip = 'Medical'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetAllDepartamenteMedicaleAsync();
    
    /// <summary>
    /// incarca categoriile medicale (nivelul 1 in ierarhia medicala)
    /// Foloseste sp_Departamente_GetByTip cu @Tip = 'Categorie'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetCategoriiMedicaleAsync();
    
    /// <summary>
    /// incarca specializarile medicale (nivelul 2 in ierarhia medicala)  
    /// Foloseste sp_Departamente_GetByTip cu @Tip = 'Specializare'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSpecializariMedicaleAsync();
    
    /// <summary>
    /// incarca subspecializarile medicale (nivelul 3 in ierarhia medicala)
    /// Foloseste sp_Departamente_GetByTip cu @Tip = 'Subspecializare'
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSubspecializariMedicaleAsync();
    
    /// <summary>
    /// Gaseste un departament medical specific dupa ID
    /// </summary>
    Task<DepartamentMedical?> GetDepartamentMedicalByIdAsync(Guid departamentId);
    
    /// <summary>
    /// incarca specializarile pentru o categorie specifica
    /// Foloseste relatiile din baza de date pentru a gasi specializarile asociate
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSpecializariPentruCategoriaAsync(Guid categorieId);
    
    /// <summary>
    /// incarca subspecializarile pentru o specializare specifica
    /// Foloseste relatiile din baza de date pentru a gasi subspecializarile asociate
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> GetSubspecializariPentruSpecializareaAsync(Guid specializareId);
    
    /// <summary>
    /// Verifica daca un departament medical exista si este activ
    /// </summary>
    Task<bool> DepartamentMedicalExistsAsync(Guid departamentId);
    
    /// <summary>
    /// Cauta departamente medicale dupa nume
    /// </summary>
    Task<IEnumerable<DepartamentMedical>> SearchDepartamenteMedicaleAsync(string searchText);
    
    // Test method for debugging database connectivity issues
    Task<bool> TestDatabaseConnectionAsync();
}
