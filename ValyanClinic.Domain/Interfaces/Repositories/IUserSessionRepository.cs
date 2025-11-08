using ValyanClinic.Domain.Entities.Settings;

namespace ValyanClinic.Domain.Interfaces.Repositories;

public interface IUserSessionRepository
{
  /// <summary>
    /// Obține sesiuni active (legacy method - poate fi deprecated)
    /// </summary>
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    
/// <summary>
    /// Obține sesiuni active cu detalii utilizator (JOIN cu Utilizatori)
    /// </summary>
  Task<IEnumerable<(
     Guid SessionID,
  Guid UtilizatorID,
    string Username,
        string Email,
     string Rol,
    string SessionToken,
     string AdresaIP,
      string? UserAgent,
   string? Dispozitiv,
     DateTime DataCreare,
    DateTime DataUltimaActivitate,
  DateTime DataExpirare,
  bool EsteActiva
    )>> GetActiveSessionsWithDetailsAsync(
Guid? utilizatorId = null,
   bool doarExpiraInCurand = false,
      string sortColumn = "DataUltimaActivitate",
     string sortDirection = "DESC",
CancellationToken cancellationToken = default);
    
    /// <summary>
 /// Obține sesiuni pentru un utilizator specific
 /// </summary>
    Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid utilizatorId, CancellationToken cancellationToken = default);
    
/// <summary>
    /// Creează o sesiune nouă
/// </summary>
    Task<(Guid SessionId, string SessionToken)> CreateAsync(
   Guid utilizatorId, 
      string adresaIp, 
   string userAgent, 
     string dispozitiv, 
CancellationToken cancellationToken = default);
    
  /// <summary>
 /// Actualizează ultima activitate pentru o sesiune
  /// </summary>
  Task<bool> UpdateActivityAsync(string sessionToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Închide o sesiune (logout forțat)
    /// </summary>
    Task<bool> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
 
  /// <summary>
    /// Curăță sesiuni expirate
    /// </summary>
    Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);
  
  /// <summary>
/// Obține statistici sesiuni
    /// </summary>
Task<(int TotalActive, int ExpiraInCurand, int InactiviAzi)> GetStatisticsAsync(
CancellationToken cancellationToken = default);
}
