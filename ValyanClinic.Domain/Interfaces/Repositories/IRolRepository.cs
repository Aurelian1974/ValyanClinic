using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru gestionarea Rolurilor și Permisiunilor.
/// Suportă Policy-Based Authorization cu configurare dinamică din baza de date.
/// </summary>
public interface IRolRepository
{
    #region Roluri CRUD
    
    /// <summary>
    /// Obține un rol după ID.
    /// </summary>
    Task<Rol?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obține lista paginată de roluri.
    /// </summary>
    Task<IEnumerable<Rol>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchText = null,
        bool? esteActiv = null,
        string sortColumn = "OrdineAfisare",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obține numărul total de roluri pentru paginare.
    /// </summary>
    Task<int> GetCountAsync(
        string? searchText = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creează un rol nou.
    /// </summary>
    Task<Guid> CreateAsync(Rol rol, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualizează un rol existent.
    /// </summary>
    Task<bool> UpdateAsync(Rol rol, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Șterge un rol (doar dacă nu este rol de sistem).
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifică dacă denumirea rolului este unică.
    /// </summary>
    Task<bool> CheckUniqueAsync(
        string denumire,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Permisiuni per Rol
    
    /// <summary>
    /// Obține toate permisiunile pentru un rol specific.
    /// </summary>
    Task<IEnumerable<string>> GetPermisiuniForRolAsync(
        Guid rolId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obține permisiunile pentru un rol după denumire (pentru autorizare).
    /// </summary>
    Task<IEnumerable<string>> GetPermisiuniForRolByDenumireAsync(
        string denumireRol,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Setează permisiunile pentru un rol (înlocuiește toate existente).
    /// </summary>
    Task<bool> SetPermisiuniForRolAsync(
        Guid rolId,
        IEnumerable<string> permisiuni,
        string? creatDe = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adaugă o permisiune la un rol.
    /// </summary>
    Task<bool> AddPermisiuneToRolAsync(
        Guid rolId,
        string permisiune,
        string? creatDe = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Șterge o permisiune de la un rol.
    /// </summary>
    Task<bool> RemovePermisiuneFromRolAsync(
        Guid rolId,
        string permisiune,
        CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Definiții Permisiuni
    
    /// <summary>
    /// Obține toate definițiile de permisiuni disponibile.
    /// </summary>
    Task<IEnumerable<PermisiuneDefinitie>> GetAllPermisiuniDefinitiiAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obține categoriile de permisiuni disponibile.
    /// </summary>
    Task<IEnumerable<string>> GetCategoriiPermisiuniAsync(
        CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Helpers
    
    /// <summary>
    /// Obține rolurile pentru dropdown-uri.
    /// </summary>
    Task<IEnumerable<(Guid Id, string Denumire)>> GetDropdownOptionsAsync(
        bool esteActiv = true,
        CancellationToken cancellationToken = default);
    
    #endregion
}
