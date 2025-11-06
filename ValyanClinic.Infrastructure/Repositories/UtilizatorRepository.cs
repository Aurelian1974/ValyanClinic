using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru Utilizatori cu Dapper și Stored Procedures
/// </summary>
public class UtilizatorRepository : BaseRepository, IUtilizatorRepository
{
    public UtilizatorRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<(IEnumerable<Utilizator> Items, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? searchText = null,
        string? rol = null,
        bool? esteActiv = null,
        string sortColumn = "Username",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            Rol = rol,
            EsteActiv = esteActiv,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        // ✅ FIX: Call TWO separate stored procedures
        // sp_Utilizatori_GetAll returns only data (no count)
        // sp_Utilizatori_GetCount returns the total count
        
        using var connection = _connectionFactory.CreateConnection();
    
        // Get paginated items
        var items = await connection.QueryAsync<Utilizator>(
            "sp_Utilizatori_GetAll",
         parameters,
          commandType: System.Data.CommandType.StoredProcedure);

   // Get total count (for pagination)
        var countParameters = new
        {
      SearchText = searchText,
            Rol = rol,
            EsteActiv = esteActiv
        };
 
     var count = await connection.QueryFirstOrDefaultAsync<int>(
"sp_Utilizatori_GetCount",
      countParameters,
     commandType: System.Data.CommandType.StoredProcedure);

return (items, count);
    }

    public async Task<int> GetCountAsync(
        string? searchText = null,
        string? rol = null,
        bool? esteActiv = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchText = searchText,
            Rol = rol,
            EsteActiv = esteActiv
        };

        return await ExecuteScalarAsync<int>("sp_Utilizatori_GetCount", parameters, cancellationToken);
    }

    public async Task<Utilizator?> GetByIdAsync(Guid utilizatorID, CancellationToken cancellationToken = default)
    {
        var parameters = new { UtilizatorID = utilizatorID };
        return await QueryFirstOrDefaultAsync<Utilizator>("sp_Utilizatori_GetById", parameters, cancellationToken);
    }

    public async Task<Utilizator?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var parameters = new { Username = username };
        return await QueryFirstOrDefaultAsync<Utilizator>("sp_Utilizatori_GetByUsername", parameters, cancellationToken);
    }

    public async Task<Utilizator?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var parameters = new { Email = email };
        return await QueryFirstOrDefaultAsync<Utilizator>("sp_Utilizatori_GetByEmail", parameters, cancellationToken);
    }

    public async Task<Guid> CreateAsync(Utilizator utilizator, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            utilizator.PersonalMedicalID,
            utilizator.Username,
            utilizator.Email,
            utilizator.PasswordHash,
            utilizator.Salt,
            utilizator.Rol,
            utilizator.EsteActiv,
            utilizator.CreatDe
        };

        var result = await QueryFirstOrDefaultAsync<Utilizator>("sp_Utilizatori_Create", parameters, cancellationToken);
        return result?.UtilizatorID ?? Guid.Empty;
    }

    public async Task<bool> UpdateAsync(Utilizator utilizator, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            utilizator.UtilizatorID,
            utilizator.Username,
            utilizator.Email,
            utilizator.Rol,
            utilizator.EsteActiv,
            utilizator.ModificatDe
        };

        var result = await QueryFirstOrDefaultAsync<Utilizator>("sp_Utilizatori_Update", parameters, cancellationToken);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid utilizatorID, CancellationToken cancellationToken = default)
    {
        var parameters = new { UtilizatorID = utilizatorID };

        // Soft delete - setează EsteActiv = 0
        var result = await QueryFirstOrDefaultAsync<DeleteResult>("sp_Utilizatori_Delete", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<bool> ChangePasswordAsync(
        Guid utilizatorID,
        string newPasswordHash,
        string newSalt,
        string modificatDe,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            UtilizatorID = utilizatorID,
            NewPasswordHash = newPasswordHash,
            NewSalt = newSalt,
            ModificatDe = modificatDe
        };

        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Utilizatori_ChangePassword", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<bool> UpdateUltimaAutentificareAsync(Guid utilizatorID, CancellationToken cancellationToken = default)
    {
        var parameters = new { UtilizatorID = utilizatorID };

        var result = await QueryFirstOrDefaultAsync<SuccessResult>("sp_Utilizatori_UpdateUltimaAutentificare", parameters, cancellationToken);
        return result?.Success == 1;
    }

    public async Task<(bool Success, string Message)> IncrementIncercariEsuateAsync(
        Guid utilizatorID,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { UtilizatorID = utilizatorID };

        var result = await QueryFirstOrDefaultAsync<IncercariEsuateResult>(
            "sp_Utilizatori_IncrementIncercariEsuate",
            parameters,
            cancellationToken);

        return (result?.Success == 1, result?.Message ?? string.Empty);
    }

    public async Task<bool> SetTokenResetareParolaAsync(
        string email,
        string token,
        DateTime dataExpirare,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            Email = email,
            Token = token,
            DataExpirare = dataExpirare
        };

        var result = await QueryFirstOrDefaultAsync<SuccessResult>(
            "sp_Utilizatori_SetTokenResetareParola",
            parameters,
            cancellationToken);

        return result?.Success == 1;
    }

    public async Task<Dictionary<string, (int Total, int Activi)>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<StatisticResult>("sp_Utilizatori_GetStatistics", cancellationToken: cancellationToken);

        return results.ToDictionary(
            r => r.Categorie,
            r => (r.Numar, r.Activi)
        );
    }

    #region DTOs pentru maparea rezultatelor

    private class SuccessResult
    {
        public int Success { get; set; }
        public string? Message { get; set; }
    }

    private class DeleteResult
    {
        public int Success { get; set; }
    }

    private class IncercariEsuateResult
    {
        public int Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private class StatisticResult
    {
        public string Categorie { get; set; } = string.Empty;
        public int Numar { get; set; }
        public int Activi { get; set; }
    }

    #endregion
}
