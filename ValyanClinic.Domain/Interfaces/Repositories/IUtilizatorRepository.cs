using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru Utilizatori
/// </summary>
public interface IUtilizatorRepository
{
    // Query methods
    Task<(IEnumerable<Utilizator> Items, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? searchText = null,
        string? rol = null,
        bool? esteActiv = null,
        string sortColumn = "Username",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    
    Task<Utilizator?> GetByIdAsync(Guid utilizatorID, CancellationToken cancellationToken = default);
    Task<Utilizator?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Utilizator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        string? searchText = null,
        string? rol = null,
        bool? esteActiv = null,
  CancellationToken cancellationToken = default);
    
    // Command methods
 Task<Guid> CreateAsync(Utilizator utilizator, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(Utilizator utilizator, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid utilizatorID, CancellationToken cancellationToken = default);
    
    // Authentication specific
    Task<bool> ChangePasswordAsync(
   Guid utilizatorID,
        string newPasswordHash,
 string newSalt,
string modificatDe,
  CancellationToken cancellationToken = default);
    
    Task<bool> UpdateUltimaAutentificareAsync(Guid utilizatorID, CancellationToken cancellationToken = default);
    
    Task<(bool Success, string Message)> IncrementIncercariEsuateAsync(
        Guid utilizatorID,
 CancellationToken cancellationToken = default);
    
    Task<bool> SetTokenResetareParolaAsync(
     string email,
        string token,
        DateTime dataExpirare,
        CancellationToken cancellationToken = default);
    
    // Statistics
 Task<Dictionary<string, (int Total, int Activi)>> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
