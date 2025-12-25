namespace ValyanClinic.Application.Authorization.Services;

/// <summary>
/// Serviciu pentru verificarea permisiunilor utilizatorilor.
/// Suportă citirea din baza de date cu caching pentru performanță.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Verifică dacă un rol are o permisiune specifică.
    /// </summary>
    /// <param name="role">Denumirea rolului (Admin, Doctor, etc.)</param>
    /// <param name="permission">Codul permisiunii</param>
    /// <returns>True dacă rolul are permisiunea</returns>
    Task<bool> HasPermissionAsync(string role, string permission);
    
    /// <summary>
    /// Obține toate permisiunile pentru un rol.
    /// </summary>
    /// <param name="role">Denumirea rolului</param>
    /// <returns>Lista de permisiuni</returns>
    Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(string role);
    
    /// <summary>
    /// Invalidează cache-ul de permisiuni pentru un rol specific.
    /// Apelat când se modifică permisiunile unui rol.
    /// </summary>
    /// <param name="role">Denumirea rolului (null pentru toate)</param>
    void InvalidateCache(string? role = null);
}
